using Moq;
using NotificationsPush.Controllers;
using NotificationsPush.Models.Local;
using NotificationsPush.Services.Interfaces;


namespace NotificationsPush.Tests.Controllers
{
    public class NotificationControllerTest
    {
        private readonly Mock<INotificationsService> _mockAuthenticationService;
        private readonly NotificationController _controller;

        public NotificationControllerTest()
        {
            _mockAuthenticationService = new Mock<INotificationsService>();
            _controller = new NotificationController(_mockAuthenticationService.Object);
        }

        [Fact]
        public async void GetNotitificationsPendingTest()
        {
            // Arrange

            List<NotificationPending> notificationPendings = new List<NotificationPending>() {
                new NotificationPending (){
                    icon = "icono en base64",
                    picture = "imagen en base 63",
                    description = "descripcion de prueba",
                    title= "titulo de prueba" }

            };

            _mockAuthenticationService.Setup(Services => Services.GetNotitificationsPending("123456790")).ReturnsAsync(notificationPendings);

            // Act
            var result = _controller.GetNotitificationsPending("123456790");

            // Assert
            Assert.NotNull(result);

        }

        [Fact]
        public async void SendNotificationTest()
        {
            // Arrange

            SentTerminalsStatus sentTerminalsStatus = new SentTerminalsStatus();
            sentTerminalsStatus.terminalNotSend = new List<string>() { "1234567890" };
            sentTerminalsStatus.terminalSend = new List<string>() { "0987654321" };

            SendNotification sendNotification = new SendNotification();

            sendNotification.terminal_serial = new List<string>() { "1234567890", "0987654321" };
            sendNotification.icon = "ICOCNO BASE64";
            sendNotification.picture = "IMAGEN BASE 64";
            sendNotification.title = "TITULO DE PRUEBA";
            sendNotification.description = "DECRIPCION DE PRUEBA";
            sendNotification.notification_id = 0;

            _mockAuthenticationService.Setup(Services => Services.SendNotification(sendNotification)).ReturnsAsync(sentTerminalsStatus);

            // Act
            var result = _controller.GetNotitificationsPending("123456790");

            // Assert
            Assert.NotNull(result);

        }

    }
}
