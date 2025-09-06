using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services
{
    public class ClaimService(AppDbContext db)
    {
        private readonly AppDbContext _db = db;

        public async Task<ClaimDto> AddClaimAsync(long carId, DateOnly claimDate, string description, decimal amount)
        {
            var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
            if(!carExists)
            {
                throw new Exception($"Car {carId} not found");
            }

            var claim = new Claim {CarId = carId, ClaimDate = claimDate, Description = description, Amount = amount};
            _db.Claims.Add(claim);
            await _db.SaveChangesAsync();

            return new ClaimDto(claim.Id, claim.CarId, claim.ClaimDate, claim.Description, claim.Amount);
        }

        public async Task<CarHistoryResponse> GetCarHistoryAsync(long carId)
        {
            var car = await _db.Cars
                .Include(c => c.Policies)
                .Include(c => c.Claims)
                .FirstOrDefaultAsync(c=> c.Id == carId);

            if (car == null) 
            {
                throw new KeyNotFoundException($"Car {carId} not found");
            }

            var policies = car.Policies
                .OrderBy(p => p.StartDate)
                .Select(p=> new PolicyPeriod(
                    p.Provider ?? "Unknown",
                    p.StartDate,
                    p.EndDate))
                .ToList();

            var claims = car.Claims
                .OrderBy(c => c.ClaimDate)
                .Select(c => new ClaimDto(
                    c.Id,
                    c.CarId,
                    c.ClaimDate,
                    c.Description,
                    c.Amount))
                .ToList();

            return new CarHistoryResponse(carId, policies, claims);
        }
    }


}
