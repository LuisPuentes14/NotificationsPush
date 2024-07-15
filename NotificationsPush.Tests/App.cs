using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationsPush.Tests
{
    public class App : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        public readonly HttpClient _client;
        private WebApplicationFactory<Program> webApplicationFactory;      

        public App()
        {
            webApplicationFactory = new WebApplicationFactory<Program>();
            _client = webApplicationFactory .CreateClient();
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
