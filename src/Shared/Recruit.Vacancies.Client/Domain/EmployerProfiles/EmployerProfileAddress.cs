namespace Esfa.Recruit.Vacancies.Client.Domain.EmployerProfiles;

public record EmployerProfileAddress(int Id,
    string AddressLine1,
    string? AddressLine2,
    string? AddressLine3,
    string? AddressLine4,
    string Postcode,
    double? Latitude,
    double? Longitude);
