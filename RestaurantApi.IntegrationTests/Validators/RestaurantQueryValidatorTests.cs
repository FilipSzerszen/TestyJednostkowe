using FluentAssertions;
using FluentValidation.TestHelper;
using RestaurantAPI.Entities;
using RestaurantAPI.Models;
using RestaurantAPI.Models.Validators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RestaurantApi.IntegrationTests.Validators
{
    public class RestaurantQueryValidatorTests
    {
        public static IEnumerable<object[]> GetSampleValidData()
        {
            var list = new List<RestaurantQuery>()
            {
                new RestaurantQuery()
                {
                    PageNumber = 6,
                    PageSize = 5,
                },
                new RestaurantQuery()
                {
                    PageNumber = 22,
                    PageSize = 10,
                },
                new RestaurantQuery()
                {
                    PageNumber = 17,
                    PageSize = 15,
                },
                new RestaurantQuery()
                {
                    PageNumber = 1,
                    PageSize = 10,
                    SortBy = nameof(Restaurant.Name)
                },
                new RestaurantQuery()
                {
                    PageNumber = 12,
                    PageSize = 15,
                    SortBy = nameof(Restaurant.Category)
                }
            };

            return list.Select(o => new object[] { o });
        }

        public static IEnumerable<object[]> GetSampleInvalidData()
        {
            var list = new List<RestaurantQuery>()
            {
                new RestaurantQuery()
                {
                    PageNumber = 0,
                    PageSize = 2,
                },
                new RestaurantQuery()
                {
                    PageNumber = 22,
                    PageSize = 14,
                },
                new RestaurantQuery()
                {
                    PageNumber = 17,
                    PageSize = 45,
                },
                new RestaurantQuery()
                {
                    PageNumber = 1,
                    PageSize = 10,
                    SortBy = nameof(Restaurant.ContactEmail)
                },
                new RestaurantQuery()
                {
                    PageNumber = 12,
                    PageSize = 15,
                    SortBy = nameof(Restaurant.Address)
                }
            };

            return list.Select(o => new object[] { o });
        }

        [Theory]
        [MemberData(nameof(GetSampleValidData))]
        public void Validate_ForCorrectModel_ReturnsSuccess(RestaurantQuery model)
        {
            // arrange
            var validator = new RestaurantQueryValidator();

            // act
            var result = validator.TestValidate(model);

            // assert
            result.ShouldNotHaveAnyValidationErrors();
        }


        [Theory]
        [MemberData(nameof(GetSampleInvalidData))]
        public void Validate_ForIncorrectModel_ReturnsError(RestaurantQuery model)
        {
            // arrange
            var validator = new RestaurantQueryValidator();

            // act
            var result = validator.TestValidate(model);

            // assert
            result.ShouldHaveAnyValidationError();
        }
    }
}
