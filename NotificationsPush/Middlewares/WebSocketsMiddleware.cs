namespace signalR.Middleware
{
    /// <summary>
    /// Encargado de gestionar las peticion hacia el Hub (WebSocket)
    /// </summary>
    public class WebSocketsMiddleware
    {
        private readonly RequestDelegate _next;

        public WebSocketsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var request = httpContext.Request;

            // valida que la peticion hacia el websocket llege el parametro user y access_token 

            if (request.Path.StartsWithSegments("/notificationsHub", StringComparison.OrdinalIgnoreCase)&&
                !request.Query.TryGetValue("serial", out var user))
            {
                httpContext.Response.StatusCode = 400;
                return;
            }

            if (request.Path.StartsWithSegments("/notificationsHub", StringComparison.OrdinalIgnoreCase) &&
                request.Query.TryGetValue("access_token", out var accessToken))
            {
                request.Headers.Add("Authorization", $"Bearer {accessToken}");
            }

            await _next(httpContext);
        }
    }
}
