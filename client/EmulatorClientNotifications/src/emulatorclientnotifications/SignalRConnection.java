/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package emulatorclientnotifications;

import com.google.gson.Gson;
import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;
import emulatorclientnotifications.models.AutenthicationResponse;
import static emulatorclientnotifications.utilities.Utilities.decrypt;
import static emulatorclientnotifications.utilities.Utilities.encrypt;
import static emulatorclientnotifications.utilities.Utilities.getIvFromBase64String;
import static emulatorclientnotifications.utilities.Utilities.getKeyFromBase64String;
import io.reactivex.rxjava3.functions.Action;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URI;
import java.net.URL;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.net.http.HttpResponse.BodyHandlers;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
import java.util.logging.Level;
import java.util.logging.Logger;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;
import okhttp3.OkHttpClient;

/**
 *
 * @author aleja
 */
public class SignalRConnection extends Thread {

    private boolean is_stopped_manually = false;
    private String TEMRINAL_SERIAL;
    private int MINUTES_BEFORE_TOKEN_EXPIRATION = 1;
    private String TOKEN;
    private String HUB_URL = "http://localhost:5400/notificationsHub?serial=@serialTerminal&access_token=@token";
    private String HUB_URL_END;
    private String MINUTES_EXPIRES_TOKEN;
    private final String RECEIVE_EVENT = "ReceiveMessage";
    private final int BASE_RECONNECT_DELAY = 5000; // 5 segundos
    private String url = "http://localhost:5400/api/authorization/Authentication";
    private HttpURLConnection connection = null;
    private HubConnection hubConnection;
    private String url_notifications_pending = "http://localhost:5400/api/Notification/GetNotitificationsPending/";

    ScheduledExecutorService scheduler = Executors.newScheduledThreadPool(1);
    Runnable task = new Runnable() {
        @Override
        public void run() {
            System.out.println("actualizando token");
            try {
                stopToHub();
                if (authentication()) {
                    getNotificationsPending();
                    startSignalR();
                }
            } catch (IOException ex) {
                System.err.println("token no actualizado");
            } catch (InterruptedException ex) {
                Logger.getLogger(SignalRConnection.class.getName()).log(Level.SEVERE, null, ex);
            }
            System.out.println("token actualizado");

        }

    };

    public void programHour() {
        scheduler.scheduleAtFixedRate(task, Integer.parseInt(MINUTES_EXPIRES_TOKEN) - MINUTES_BEFORE_TOKEN_EXPIRATION, Integer.parseInt(MINUTES_EXPIRES_TOKEN) - MINUTES_BEFORE_TOKEN_EXPIRATION, TimeUnit.MINUTES);
    }

    public SignalRConnection(String terminalSerial) {
        this.TEMRINAL_SERIAL = terminalSerial;
    }

    @Override
    public void run() {
        try {
            if (authentication()) {
                getNotificationsPending();
                startSignalR();
                programHour();
            }
        } catch (IOException ex) {
            Logger.getLogger(SignalRConnection.class.getName()).log(Level.SEVERE, null, ex);
        } catch (InterruptedException ex) {
            Logger.getLogger(SignalRConnection.class.getName()).log(Level.SEVERE, null, ex);
        }
    }    

    private void startSignalR() {

        hubConnection = HubConnectionBuilder.create(HUB_URL_END).build();

        hubConnection.on(RECEIVE_EVENT, (message) -> {
            System.out.println("Mensaje recibido para el terminal " + TEMRINAL_SERIAL + " : " + message);
            //  generateNotification(message);
        }, String.class);

        connectToHub();

        hubConnection.onClosed(exception -> {           
            // si genero error intentea conectarse
            // detencion manual
            if (is_stopped_manually) {
                is_stopped_manually = false;
                System.out.println("Conexión detenida");               
            } else {
                System.out.println("Conexión detenida por error.");
                System.out.println("Intentando reconectar...");
                connectToHub();
            }

        });
    }

    private void connectToHub() {
        try {
            hubConnection.start().blockingAwait();
            System.out.println("Servicio SignalR Activo");
        } catch (Exception e) {
            System.out.println("Error al conectar: " + e.getMessage());
            System.out.println("Volviendo a intertar reconectar.");
            scheduleReconnect();
        }
    }

    private void stopToHub() {
        try {
            is_stopped_manually = true;
            hubConnection.stop().blockingAwait();           
            System.out.println("Servicio SignalR tenido");
        } catch (Exception e) {
            System.out.println("Error al desconectarse: " + e.getMessage());
        }
    }

    private void scheduleReconnect() {

        try {
            Thread.sleep(BASE_RECONNECT_DELAY);
            connectToHub(); // Intenta reconectar
        } catch (InterruptedException e) {
            System.out.println("Error en el tiempo de espera de reconexión: " + e.getMessage());
        }

    }    
    
    private boolean authentication() throws MalformedURLException, IOException {
        System.out.println("-----------------------------------------------------------------------------------------");
        System.out.println("Autenticando terminal");
        System.out.println("");
        HttpClient client = HttpClient.newHttpClient();
        String data = "{\n"
                + "  \"user\":\"" + TEMRINAL_SERIAL + "\",\n"
                + "  \"password\":\"" + encryptPassword(TEMRINAL_SERIAL + "-" + TEMRINAL_SERIAL) + "\",\n"
                + "  \"type\":\"TERMINAL\"\n"
                + "}"; // Datos JSON para enviar
        HttpRequest request = HttpRequest.newBuilder()
                .uri(URI.create(url))
                .header("Content-Type", "application/json")
                .header("Accept", "application/json")
                .POST(HttpRequest.BodyPublishers.ofString(data)) // Método POST con cuerpo
                .build();

        try {
            HttpResponse<String> response = client.send(request, BodyHandlers.ofString());
            int statusCode = response.statusCode();
            System.out.println("Status Code: " + statusCode);
            String body = response.body();
            System.out.println("Response Body: " + body);

            if (statusCode != 200) {
                System.err.println("Codigo de respuesta en la autenticacion es: " + statusCode);
                return false;
            }

            Gson gson = new Gson();
            AutenthicationResponse autenthicationResponse = gson.fromJson(body, AutenthicationResponse.class);

            System.out.println("Message: " + autenthicationResponse.getMessage());
            //getMessage  System.out.println("Age: " + person.getAge());

            if (autenthicationResponse.getStatus().equals("false")) {
                System.err.println("Teminal con serial " + TEMRINAL_SERIAL + " no autenticado ");

                return false;
            }

            TOKEN = autenthicationResponse.getToken().getToken();
            MINUTES_EXPIRES_TOKEN = autenthicationResponse.getToken().getMinutesExpiresToken();

            HUB_URL_END = HUB_URL;
            HUB_URL_END = HUB_URL_END.replace("@serialTerminal", TEMRINAL_SERIAL);
            HUB_URL_END = HUB_URL_END.replace("@token", TOKEN);

        } catch (InterruptedException e) {
            e.printStackTrace();
            return false;
        }

        System.out.println("-----------------------------------------------------------------------------------------");
        System.out.println("");
        return true;

    }
    
    private void getNotificationsPending() throws IOException, InterruptedException {

        System.out.println("-----------------------------------------------------------------------------------------");
        System.out.println("obteniendo notificaciones pendientes");
        System.out.println("");
        HttpClient client = HttpClient.newHttpClient();
        HttpRequest request = HttpRequest.newBuilder()
                .uri(URI.create(url_notifications_pending + TEMRINAL_SERIAL))
                .header("Content-Type", "application/json")
                .header("Accept", "application/json")
                .header("Authorization", "Bearer " + TOKEN)
                // .POST(HttpRequest.BodyPublishers.ofString(data)) // Método POST con cuerpo
                .build();
        HttpResponse<String> response = client.send(request, BodyHandlers.ofString());
        int statusCode = response.statusCode();
        System.out.println("Status Code: " + statusCode);
        String body = response.body();
        System.out.println("Response Body: " + body);
        System.out.println("");
        System.out.println("-----------------------------------------------------------------------------------------");

    }    

    private String encryptPassword(String input) {
        String algorithm = "AES/CBC/PKCS5Padding";
        String keyString = "r3QXh7vJrhNu2Zo0WL92Y7kM23C4moi2NUs0jgV6j8I=";  // Reemplaza esto con tu clave en texto
        String ivString = "v3bT1CkT3tOb9G5b0oSXYA==";  // Reemplaza esto con tu IV en texto
        String cipherText = "";
        try {

            SecretKeySpec key = getKeyFromBase64String(keyString);
            IvParameterSpec iv = getIvFromBase64String(ivString);

            cipherText = encrypt(algorithm, input, key, iv);
            System.out.println("Encrypted Text: " + cipherText);

        } catch (Exception e) {
            e.printStackTrace();
        }

        return cipherText;

    }

    
}
