using Microsoft.AspNetCore.Mvc;
using Moq;
using NotificationsPush.Controllers;
using NotificationsPush.DTO.Request;
using NotificationsPush.DTO.Response;
using NotificationsPush.Models.Local;
using NotificationsPush.Services.Interfaces;

namespace NotificationsPush.Tests.Controllers
{
    public class AuthorizationControllerTest
    {

        private readonly Mock<IAuthenticationService> _mockAuthenticationService;
        private readonly AuthorizationController _controller;

        public AuthorizationControllerTest() {
            _mockAuthenticationService = new Mock<IAuthenticationService>();
            _controller = new AuthorizationController(_mockAuthenticationService.Object);
        }

        [Fact]
        public async void AuthenticationTest()
        {
            // Arrange

            User user = new User();
            user.user = "alejandro";
            user.password = "dsf3w345r43gfdgdfg";
            user.type = "USER";

            UserRequest request = new UserRequest();
            request.user = user.user;
            request.password = user.password;
            request.type = user.type;


            UserAuthenticated userAuthenticated = new UserAuthenticated();
            userAuthenticated.status = true;
            userAuthenticated.message = "proceso exito.";
            userAuthenticated.token = "token";
            userAuthenticated.minutesExpiresToken = "560";

            _mockAuthenticationService.Setup(service => service.Authentication(It.Is<User>(u =>
                u.user == user.user &&
                u.password == user.password &&
                u.type == user.type))).ReturnsAsync(userAuthenticated);


            // Act
            var result = await _controller.Authentication(request);


            // Assert
            Assert.NotNull(result);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var genericResponse = Assert.IsType<GenericResponse<object>>(okResult.Value);

            Assert.Equal(userAuthenticated.status, genericResponse.status);
            Assert.Equal(userAuthenticated.message, genericResponse.message);
        }
    }
}
