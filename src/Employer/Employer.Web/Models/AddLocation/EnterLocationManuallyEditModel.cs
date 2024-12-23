using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Models.AddLocation;

public class EnterLocationManuallyEditModel: AddLocationJourneyModel
{
    public string AddressLine1 { get; init; }
    public string AddressLine2 { get; init; }
    public string City { get; init; }
    public string County { get; init; }
    public string Postcode { get; init; }

    public Address ToDomain()
    {
        return new Address
        {
            AddressLine1 = AddressLine1,
            AddressLine2 = AddressLine2,
            AddressLine3 = City,
            AddressLine4 = County,
            Postcode = Postcode
        };
    }
}