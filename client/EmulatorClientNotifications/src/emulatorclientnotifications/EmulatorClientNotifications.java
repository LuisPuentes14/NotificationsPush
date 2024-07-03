/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Main.java to edit this template
 */
package emulatorclientnotifications;

import emulatorclientnotifications.utilities.Utilities;
import static emulatorclientnotifications.utilities.Utilities.decrypt;
import static emulatorclientnotifications.utilities.Utilities.encrypt;
import static emulatorclientnotifications.utilities.Utilities.getIvFromBase64String;
import static emulatorclientnotifications.utilities.Utilities.getKeyFromBase64String;

import java.io.IOException;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.Base64;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
import java.util.logging.Level;
import java.util.logging.Logger;
import javax.crypto.Cipher;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

/**
 *
 * @author aleja
 */
public class EmulatorClientNotifications {

    /**
     * @param args the command line arguments
     */
    private static ArrayList<String> terminalsSerial = new ArrayList<String>();
    private static final String JDBC_URL = "jdbc:sqlserver://localhost:1487;databaseName=CAJA_PIURA_POLARIS_CLOUD";
    private static final String USERNAME = "sa";
    private static final String PASSWORD = "12345";

    public static void main(String[] args) throws Exception {

        SQLServerConnection();
        
//       loadTerminals();

        for (String terminalSerial : terminalsSerial) {
            SignalRConnection signalRConnection = new SignalRConnection(terminalSerial);
            signalRConnection.run();           
        }

    }

    private static void SQLServerConnection() {
        Connection connection = null;
        Statement statement = null;
        ResultSet resultSet = null;

        try {
            // Establish the connection
            connection = DriverManager.getConnection(JDBC_URL, USERNAME, PASSWORD);

            // Create a statement object
            statement = (Statement) connection.createStatement();

            // Execute a query
            String sql = "SELECT TOP 100 ter_serial FROM terminal";
            resultSet = statement.executeQuery(sql);

            // Process the result set
            while (resultSet.next()) {
                // Retrieve data by column name
                //int id = resultSet.getInt("id");
                String name = resultSet.getString("ter_serial");
                terminalsSerial.add(name);
                
                // Display values
                //System.out.print("ID: " + id);
                System.out.println(", Name: " + name);
            }
        } catch (SQLException e) {
            e.printStackTrace();
        } finally {
            // Close the resources
            try {
                if (resultSet != null) resultSet.close();
                if (statement != null) statement.close();
                if (connection != null) connection.close();
            } catch (SQLException e) {
                e.printStackTrace();
            }
        }

    }


}
