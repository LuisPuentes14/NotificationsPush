
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;


namespace NotificationsPush.Tests.Test
{
    public class NotificationControllerTest : IClassFixture<App>
    {

        private App _app;

        public NotificationControllerTest(App app)
        {
            _app = app;
        }

        [Fact]
        public async void GetNotitificationsPendingTest()
        {
            // Obtenemos el token autenticandonos
            //-------------------------------------------------------------------------------------------------------------------
            EndPointsToConsume.AuthorizationControllerTest authorizationControllerTest = new EndPointsToConsume.AuthorizationControllerTest();
            var response = await authorizationControllerTest.AuthenticationTest(_app);

            response.EnsureSuccessStatusCode(); // Verifica que el código de estado sea 200-299
            var responseBody = await response.Content.ReadAsStringAsync();

            Assert.NotEmpty(responseBody);
            Assert.NotNull(responseBody);

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);
            var value = JsonConvert.DeserializeObject<Dictionary<string, object>>(dictionary["value"].ToString());
            string token = value["token"] == null ? "" : value["token"].ToString();

            Assert.Contains("token", responseBody);
            Assert.NotEmpty(token);
            //-------------------------------------------------------------------------------------------------------------------

            // Realizamos test de obtenemos las notificaciones pendientes
            //-------------------------------------------------------------------------------------------------------------------
            EndPointsToConsume.NotificationControllerTest notificationControllerTest = new EndPointsToConsume.NotificationControllerTest();
            response = await notificationControllerTest.GetNotitificationsPendingTest(_app, token);


            response.EnsureSuccessStatusCode(); // Verifica que el código de estado sea 200-299
            responseBody = await response.Content.ReadAsStringAsync();

            Assert.NotEmpty(responseBody);
            Assert.NotNull(responseBody);

            dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);
            string status = dictionary["status"].ToString();

            Assert.True(bool.Parse(status));
            //-------------------------------------------------------------------------------------------------------------------

        }


        [Fact]
        public async void SendNotificationTest() {


            // Obtenemos el token autenticandonos
            //-------------------------------------------------------------------------------------------------------------------
            EndPointsToConsume.AuthorizationControllerTest authorizationControllerTest = new EndPointsToConsume.AuthorizationControllerTest();
            var response = await authorizationControllerTest.AuthenticationTest(_app);

            response.EnsureSuccessStatusCode(); // Verifica que el código de estado sea 200-299
            var responseBody = await response.Content.ReadAsStringAsync();

            Assert.NotEmpty(responseBody);
            Assert.NotNull(responseBody);

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);
            var value = JsonConvert.DeserializeObject<Dictionary<string, object>>(dictionary["value"].ToString());
            string token = value["token"] == null ? "" : value["token"].ToString();

            Assert.Contains("token", responseBody);
            Assert.NotEmpty(token);
            //-------------------------------------------------------------------------------------------------------------------

            // Realizamos test de envio de notificaciones
            //-------------------------------------------------------------------------------------------------------------------
            EndPointsToConsume.NotificationControllerTest notificationControllerTest = new EndPointsToConsume.NotificationControllerTest();
            response = await notificationControllerTest.SendNotificationTest(_app, token);


            response.EnsureSuccessStatusCode(); // Verifica que el código de estado sea 200-299
            responseBody = await response.Content.ReadAsStringAsync();

            Assert.NotEmpty(responseBody);
            Assert.NotNull(responseBody);

            dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);
            string status = dictionary["status"].ToString();

            Assert.True(bool.Parse(status));
            //-------------------------------------------------------------------------------------------------------------------


        }

    }
}
