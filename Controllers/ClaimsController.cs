using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;


namespace CarInsurance.Api.Controllers;

[ApiController]
[Route("api/cars/{carId}/[controller]")]
public class ClaimsController(ClaimService claimService):ControllerBase
{
    private readonly ClaimService _claimService = claimService;

    [HttpPost]
    public async Task<ActionResult<ClaimDto>> AddClaim(long carId, [FromBody] CreateClaimRequest request)
    {
        try
        {
            var claim = await _claimService.AddClaimAsync(
                carId,
                request.ClaimDate,
                request.Description,
                request.Amount);

            return CreatedAtAction(nameof(GetHistory), new { carId = carId }, claim);
        }
        catch(KeyNotFoundException e)
        {
            return NotFound(new {message = e.Message});
        }
    }

    [HttpGet("/api/cars/{carId}/history")]
    public async Task<ActionResult<CarHistoryResponse>> GetHistory(long carId)
    {
        try
        {
            var history = await _claimService.GetCarHistoryAsync(carId);
            return Ok(history);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
