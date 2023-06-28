using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantAPI;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantApi.IntegrationTests
{
    public class RestaurantControllerTests : IClassFixture<WebApplicationFactory<Startup>>  // wer 3 z interfejsem
    {
        private HttpClient _client;                                     // <= wer 2

        //public RestaurantControllerTests()                              // <= wer 2 tworzy klienta za każdym przypadkiem testowym
        //{
        //    var factory = new WebApplicationFactory<Startup>();
        //    _client = factory.CreateClient();
        //}

        public RestaurantControllerTests(WebApplicationFactory<Startup> factory)   // <= wer 3 tworzy tylko raz klienta
        {
            _client = factory.CreateClient();
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

            var response = await _client.GetAsync("/api/restaurant?"+ querryParams);

            //Assert

            //response.Should().HaveStatusCode(System.Net.HttpStatusCode.OK);     // <= to samo tylko że moje
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
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
