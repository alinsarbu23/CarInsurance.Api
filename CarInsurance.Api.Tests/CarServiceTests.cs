using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using CarInsurance.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarInsurance.Api.CarInsurance.Api.Tests;

public class CarServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly CarService _carService;

    public CarServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>() //used a db in memory with a diff name
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
            .Options;

        _dbContext = new AppDbContext(options);
        _carService = new CarService(_dbContext);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var owner = new Owner { Name = "Test Owner", Email = "test@example.com" };
        var car = new Car { Vin = "TESTVIN123", Make = "Test", Model = "Model", YearOfManufacture = 2020, Owner = owner };
        var policy = new InsurancePolicy
        {
            Car = car,
            Provider = "Test Insurance",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31)
        };

        _dbContext.Owners.Add(owner);
        _dbContext.Cars.Add(car);
        _dbContext.Policies.Add(policy);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task IsInsuranceValidForAnExistingCarAsync()
    {
        var carId = 1;
        var validDate = new DateOnly(2024, 6, 15);

        var result = await _carService.IsInsuranceValidAsync(carId, validDate);

        Assert.True(result);
    }

    [Fact]
    public async Task IsInsuranceValidAInvalidDateAsync()
    {
        var carId = 1;
        var invalidDate = new DateOnly(2025, 1, 1);

        var result = await _carService.IsInsuranceValidAsync(carId, invalidDate);

        Assert.False(result);
    }

    [Fact]
    public async Task IsInsuranceValidCarNotFoundAsync()
    {
        var nonExistentCarId = 999;
        var date = new DateOnly(2024, 6, 15);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _carService.IsInsuranceValidAsync(nonExistentCarId, date));
    }

    [Fact]
    public async Task IsInsuranceValidInvalidDateAsync()
    {
        var carId = 1;
        var futureDate = new DateOnly(2100, 1, 1);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _carService.IsInsuranceValidAsync(carId, futureDate));
    }

    [Fact]
    public async Task IsInsuranceValid_BoundaryDates_ReturnsExpected()
    {
        var carId = 1;
        var startDate = new DateOnly(2024, 1, 1);
        var endDate = new DateOnly(2024, 12, 31);
        var beforeStart = startDate.AddDays(-1);
        var afterEnd = endDate.AddDays(1);

        Assert.True(await _carService.IsInsuranceValidAsync(carId, startDate));
        Assert.True(await _carService.IsInsuranceValidAsync(carId, endDate));
        Assert.False(await _carService.IsInsuranceValidAsync(carId, beforeStart));
        Assert.False(await _carService.IsInsuranceValidAsync(carId, afterEnd));
    }


    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}