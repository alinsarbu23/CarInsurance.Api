using CarInsurance.Api.Controllers;
using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CarInsurance.Api.CarInsurance.Api.Tests;

public class CarsControllerTests
{
    private readonly Mock<CarService> _mockCarService;
    private readonly CarsController _controller;

    public CarsControllerTests()
    {
        _mockCarService = new Mock<CarService>(Mock.Of<AppDbContext>());
        _controller = new CarsController(_mockCarService.Object);
    }

    [Fact]
    public async Task IsInsuranceValid_InvalidDateFormat_ReturnsBadRequest()
    {
        var carId = 1;
        var invalidDate = "invalid-date";

        var result = await _controller.IsInsuranceValid(carId, invalidDate);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid date format. Use YYYY-MM-DD.", badRequestResult.Value);
    }



    [Fact]
    public async Task IsInsuranceValid_DateOutOfRange_ReturnsBadRequest()
    {
        var carId = 1;
        var farFutureDate = "2100-01-01";

        var result = await _controller.IsInsuranceValid(carId, farFutureDate);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("Date must be between", badRequestResult.Value?.ToString());
    }

}