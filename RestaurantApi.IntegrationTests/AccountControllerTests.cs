using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RestaurantApi.IntegrationTests.Helpers;
using RestaurantAPI;
using RestaurantAPI.Entities;
using RestaurantAPI.Models;
using RestaurantAPI.Services;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantApi.IntegrationTests
{
    public class AccountControllerTests : IClassFixture<WebApplicationFactory<Startup>>  
    {
        private HttpClient _client; 
        private Mock<IAccountService> _accountServiceMock = new Mock<IAccountService>();
        //                 /\ typ musi pokrywać się z typem z AccountControllera

        public AccountControllerTests(WebApplicationFactory<Startup> factory)   
        {
            _client = factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var dbContextOptions = services
                            .SingleOrDefault(service => service.ServiceType == typeof(DbContextOptions<RestaurantDbContext>));

                        services.Remove(dbContextOptions);

                        services.AddSingleton<IAccountService>(_accountServiceMock.Object);

                        services.AddDbContext<RestaurantDbContext>(options => options.UseInMemoryDatabase("RestaurantDB"));
                    });
                })
            .CreateClient();
        }

        [Fact]
        public async Task Login_ForRegisteredUser_ReturnsOk()
        {
            // arrange
            _accountServiceMock
                .Setup(e => e.GenerateJwt(It.IsAny<LoginDto>()))
                .Returns("jwt");

            var loginDto = new LoginDto()
            {
                Email = "test@wp.pl",
                Password = "password"
            };

            var httpContent = loginDto.ToJsonHttpContent();

            // act
            var response = await _client.PostAsync("/api/account/login", httpContent);

            //assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }


        [Fact]
        public async Task RegisterUser_ForValidModel_ReturnsOK()
        {
            // arrange
            var registerUser = new RegisterUserDto()
            {
                Email = "test@wp.pl",
                Password = "test123",
                ConfirmPassword = "test123"
            };

            var httpContent = registerUser.ToJsonHttpContent();

            // act
            var response = await _client.PostAsync("/api/account/register", httpContent);

            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task RegisterUser_ForInvalidModel_ReturnsBadRequest()
        {
            // arrange
            var registerUser = new RegisterUserDto()
            {
                Email = "test@wp.pl",
                Password = "test123",
                ConfirmPassword = "test1234"
            };

            var httpContent = registerUser.ToJsonHttpContent();

            // act
            var response = await _client.PostAsync("/api/account/register", httpContent);

            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
