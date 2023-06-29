using RestaurantAPI.Models.Validators;
using RestaurantAPI.Models;
using Xunit;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using FluentValidation.TestHelper;
using System.Collections.Generic;

namespace RestaurantApi.IntegrationTests.Validators
{
    public class RegisterUserDtoValidatorTests
    {
        private readonly RestaurantDbContext _dbContext;

        public RegisterUserDtoValidatorTests()
        {
            var builder = new DbContextOptionsBuilder<RestaurantDbContext>();
            builder.UseInMemoryDatabase("TestDb");

            _dbContext = new RestaurantDbContext(builder.Options);
            Seed();
        }

        public void Seed()
        {
            var testUser = new List<User>()
            {
                new User
                {
                    Email = "test2@wp.pl"
                },
                new User
                {
                    Email = "test3@wp.pl"
                },
            };

            _dbContext.Users.AddRange(testUser);
            _dbContext.SaveChanges();
        }

        [Fact]
        public void Validate_ForCorrectModel_ReturnsSuccess()
        {
            // arrange
            var model = new RegisterUserDto()
            {
                Email = "test@wp.pl",
                Password = "hasło123",
                ConfirmPassword = "hasło123"
            };

            var validator = new RegisterUserDtoValidator(_dbContext);

            // act
            var result = validator.TestValidate(model);

            // assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_ForIncorrectModel_ReturnsError()
        {
            // arrange
            var model = new RegisterUserDto()
            {
                Email = "test2@wp.pl",
                Password = "hasło123",
                ConfirmPassword = "hasło123"
            };

            var validator = new RegisterUserDtoValidator(_dbContext);

            // act
            var result = validator.TestValidate(model);

            // assert
            result.ShouldHaveAnyValidationError();
        }

    }
}
