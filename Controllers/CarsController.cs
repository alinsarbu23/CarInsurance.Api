using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarInsurance.Api.Controllers;

[ApiController]
[Route("api")]
public class CarsController(CarService service) : ControllerBase
{
    private readonly CarService _service = service;

    [HttpGet("cars")]
    public async Task<ActionResult<List<CarDto>>> GetCars()
        => Ok(await _service.ListCarsAsync());

    [HttpGet("cars/{carId:long}/insurance-valid")]
    public async Task<ActionResult<InsuranceValidityResponse>> IsInsuranceValid(long carId, [FromQuery] string date)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        //checking the interval for date 
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (parsedDate > today.AddYears(1) || parsedDate < new DateOnly(1900, 1, 1))
            return BadRequest($"Date must be between 1900-01-01 and {today.AddYears(1):yyyy-MM-dd}");

        try
        {
            var valid = await _service.IsInsuranceValidAsync(carId, parsedDate);
            return Ok(new InsuranceValidityResponse(carId, parsedDate.ToString("yyyy-MM-dd"), valid));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
