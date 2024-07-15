using Newtonsoft.Json;
using NotificationsPush.Tests.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationsPush.Tests.EndPointsToConsume
{
    public class AuthorizationControllerTest
    {
        public async Task< HttpResponseMessage> AuthenticationTest( App app )
        {
            // Arrange
            var data = new
            {
                user = Authentication.user,
                password = Authentication.password,
                type = Authentication.type
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await app._client.PostAsync("/api/authorization/Authentication", content);

            return response;
           

        }
    }
}
