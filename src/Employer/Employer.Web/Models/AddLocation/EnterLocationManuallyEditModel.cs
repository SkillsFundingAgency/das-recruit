using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Humanizer;

namespace Esfa.Recruit.Employer.Web.Models.AddLocation;

public class EnterLocationManuallyEditModel: AddLocationJourneyModel
{
    public string AddressLine1 { get; init; }
    public string AddressLine2 { get; init; }
    public string City { get; init; }
    public string Postcode { get; init; }

    public Address ToDomain()
    {
        return new Address
        {
            AddressLine1 = AddressLine1?.Humanize(LetterCasing.Title),
            AddressLine2 = AddressLine2?.Humanize(LetterCasing.Title),
            AddressLine3 = City?.Humanize(LetterCasing.Title),
            Postcode = Postcode?.ToUpper()
        };
    }
}