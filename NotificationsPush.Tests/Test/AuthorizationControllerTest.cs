using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NotificationsPush.Controllers;
using NotificationsPush.DTO.Request;
using NotificationsPush.Models.Local;
using NotificationsPush.Repository.Interfaces;
using NotificationsPush.Tests.Parameters;
using System.Net.Http.Json;
using System.Text;
using NotificationsPush.Tests.EndPointsToConsume;
using Xunit;


namespace NotificationsPush.Tests.Test
{
    public class AuthorizationControllerTest : IClassFixture<App>
    {
        private App _app { get; set; }
      

        public AuthorizationControllerTest(App app)
        {
            _app = app;
        }

        [Fact]
        public async void AuthenticationTest()
        {
            EndPointsToConsume.AuthorizationControllerTest authorizationControllerTest = new EndPointsToConsume.AuthorizationControllerTest();
            var response = await authorizationControllerTest.AuthenticationTest(_app);      
     
            response.EnsureSuccessStatusCode(); // Verifica que el código de estado sea 200-299
            var responseBody = await response.Content.ReadAsStringAsync();

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);
            var value = JsonConvert.DeserializeObject<Dictionary<string, object>>(dictionary["value"].ToString());
            string token = value["token"] == null ? "" : value["token"].ToString();           

            Assert.Contains("token", responseBody);
            Assert.NotEmpty(token);

        }


    }
}
