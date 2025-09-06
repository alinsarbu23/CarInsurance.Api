namespace CarInsurance.Api.Dtos;

public record CarDto(long Id, string Vin, string? Make, string? Model, int Year, long OwnerId, string OwnerName, string? OwnerEmail);
public record InsuranceValidityResponse(long CarId, string Date, bool Valid);
public record ClaimDto(long Id, long CarId, DateOnly ClaimDate, string Description, decimal Amount);
public record CreateClaimRequest(DateOnly ClaimDate, string Description, decimal Amount);
public record PolicyPeriod(string Provider, DateOnly StartDate, DateOnly EndDate);
public record CarHistoryResponse(long CarId, List<PolicyPeriod> Policies, List<ClaimDto> Claims);