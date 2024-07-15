using Newtonsoft.Json;
using NotificationsPush.Tests.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationsPush.Tests.EndPointsToConsume
{
    public class NotificationControllerTest
    {
        public async Task<HttpResponseMessage> GetNotitificationsPendingTest(App app, string token)
        {           
            app._client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");           
            var response = await app._client.GetAsync("/api/Notification/GetNotitificationsPending/9220253140");

            return response;
        }

        public async Task<HttpResponseMessage> SendNotificationTest(App app, string token)
        {

            var data = new
            {
                terminal_serial = new string[] { "9220110941", "9220253052" } ,
                icon = "ICOCNO BASE64",
                picture = "IMAGEN BASE 64",
                title = "TITULO DE PRUEBA",
                description = "DECRIPCION DE PRUEBA",
                notification_id = 0,
            };


            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            app._client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
          
            var response = await app._client.PostAsync("/api/Notification/SendNotification", content);

            return response;
        }

    }
}
