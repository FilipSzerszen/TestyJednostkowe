using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RestaurantAPI;
using RestaurantAPI.Controllers;
using RestaurantAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RestaurantApi.IntegrationTests
{
    public class StartupTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly List<Type> _controllerTtypes;
        private readonly WebApplicationFactory<Startup> _factory;

        public StartupTests(WebApplicationFactory<Startup> factory)
        {
            _controllerTtypes = typeof(Startup)
                .Assembly
                .GetTypes()
                .Where(c => c.IsSubclassOf(typeof(ControllerBase)))
                .ToList();

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    _controllerTtypes.ForEach(c => services.AddScoped(c));
                });
            });
        }

        [Fact]
        public void ConfigureServices_ForControllers_RegisterAllDependencies()
        {
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();

            var controller = scope.ServiceProvider.GetService<AccountController>(); // dla pojedynczego kontrolera

            _controllerTtypes.ForEach(c =>
            {
                var controller = scope.ServiceProvider.GetService(c);
                controller.Should().NotBeNull();
            });

            
        }
    }
}
