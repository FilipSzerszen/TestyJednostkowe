using FluentAssertions;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RestaurantApi.IntegrationTests.Helpers;
using RestaurantAPI;
using RestaurantAPI.Entities;
using RestaurantAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantApi.IntegrationTests
{
    public class RestaurantControllerTests2_bazaInMemory : IClassFixture<WebApplicationFactory<Startup>>  // wer 3 z interfejsem
    {
        private HttpClient _client;                                     // <= wer 2

        //public RestaurantControllerTests()                              // <= wer 2 tworzy klienta za każdym przypadkiem testowym
        //{
        //    var factory = new WebApplicationFactory<Startup>();
        //    _client = factory.CreateClient();
        //}

        private WebApplicationFactory<Startup> _factory;

        public RestaurantControllerTests2_bazaInMemory(WebApplicationFactory<Startup> factory)   // <= wer 3 tworzy tylko raz klienta
        {
            _factory = factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var dbContextOptions = services     // odnajdujemy dbContext w servisach i usuwamy go
                            .SingleOrDefault(service => service.ServiceType == typeof(DbContextOptions<RestaurantDbContext>));

                        services.Remove(dbContextOptions);

                        services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();         // rejestracja usług pozwalających 
                        services.AddMvc(options => options.Filters.Add(new FakeUserFilter()));  //pominąć autoryzację

                        // w zamian za niego dodajemy swój dbContext ---vvv
                        services.AddDbContext<RestaurantDbContext>(options => options.UseInMemoryDatabase("RestaurantDB"));
                    });
                });
                _client = _factory.CreateClient();
        }

        private void SeedRestaurant(Restaurant restaurant) {
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetService<RestaurantDbContext>();

            _dbContext.Restaurants.Add(restaurant);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task Delete_ForNonRestaurantOwner_ReturnsForbidden()
        {
            // arrange
            var restaurant = new Restaurant()
            {
                CreatedById = 500
            };

            SeedRestaurant(restaurant);

            // act
            var response = await _client.DeleteAsync("/api/restaurant/" + restaurant.Id);

            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }


        [Fact]
        public async Task Delete_ForRestaurantOwner_ReturnsNoContent()
        {
            // arrange
            var restaurant = new Restaurant()
            {
                CreatedById = 1,
                Name = "Test"
            };

            SeedRestaurant(restaurant);

            // act
            var response = await _client.DeleteAsync("/api/restaurant/" + restaurant.Id);

            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_ForNonExistingRestaurant_ReturnsNotFound()
        {
            // arrange
            // act
            var response = await _client.DeleteAsync("/api/restaurant/900");

            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }


        [Theory]
        [InlineData("PageNumber=1&PageSize=5")]
        [InlineData("PageNumber=5&PageSize=10")]
        [InlineData("PageNumber=13&PageSize=15")]
        public async Task GetAll_WithQuerryParamters_ReturnsOkResult(string querryParams)
        {
            //Arrange

            //var factory = new WebApplicationFactory<Startup>();       // <= wer 1
            //var client = factory.CreateClient();

            //Act

            var response = await _client.GetAsync("/api/restaurant?" + querryParams);

            //Assert

            //response.Should().HaveStatusCode(System.Net.HttpStatusCode.OK);     // <= to samo tylko że moje
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }


        [Fact]
        public async Task CreateRestaurant_WithValidModel_ReturnsCreatedStatus()
        {
            // arrange
            var model = new CreateRestaurantDto()
            {

                Name = "Pizza Hut",
                Description = "jakiś opis restauracji",
                Category = "fastfood",
                HasDelivery = true,
                ContactEmail = "pizza@hut.pl",
                ContactNumber = "111-222-333",
                City = "Wrocław",
                Street = "Zelwerowicza 20"
            };

            var httpContent = model.ToJsonHttpContent();

            // act
            var response = await _client.PostAsync("/api/restaurant", httpContent);

            //assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateRestaurant_WithInvalidModel_ReturnsBadRequest()
        {
            // arrange
            var model = new CreateRestaurantDto()
            {
                Name = "Nazwa dłuższa niż dwadzieściapięć znaków",
                Description = "opis restauracji",
                Category = "ff",
                HasDelivery = true,
                ContactEmail = "pizza@hut.pl",
                ContactNumber = "111-222-333",
                City = "Wrocław",
                Street = "Zelwerowicza 20"
            };

            var httpContent = model.ToJsonHttpContent();

            // act
            var response = await _client.PostAsync("/api/restaurant", httpContent);

            //assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }


        [Theory]
        [InlineData("PageNumber=1&PageSize=2")]
        [InlineData("PageNumber=52&PageSize=22")]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetAll_WithInvalidQuerryParams_ReturnsBadRequest(string querryParams)
        {
            //Arrange

            //var factory = new WebApplicationFactory<Startup>();       // <= wer 1
            //var client = factory.CreateClient();

            //Act

            var response = await _client.GetAsync("/api/restaurant?" + querryParams);

            //Assert

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

    }
}
