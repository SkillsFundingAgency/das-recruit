namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public interface IAddress
{
    string AddressLine1 { get; set; }
    string AddressLine2 { get; set; }
    string AddressLine3 { get; set; }
    string AddressLine4 { get; set; }
    string Postcode { get; set; }
}