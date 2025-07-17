using System;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web;

public interface IUtility
{
    Task<Vacancy> GetAuthorisedVacancyForEditAsync(VacancyRouteModel vrm, string routeName);
    Task<Vacancy> GetAuthorisedVacancyAsync(VacancyRouteModel vrm, string routeName);
    void CheckAuthorisedAccess(Vacancy vacancy, long ukprn);
    bool VacancyHasCompletedPartOne(Vacancy vacancy);
    bool VacancyHasStartedPartTwo(Vacancy vacancy);
    PartOnePageInfoViewModel GetPartOnePageInfo(Vacancy vacancy);
    Task<ApplicationReview> GetAuthorisedApplicationReviewAsync(ApplicationReviewRouteModel rm);
    Task UpdateEmployerProfile(VacancyEmployerInfoModel vacancyEmployerInfoModel, EmployerProfile profile, Address address, VacancyUser user);
    bool IsTaskListCompleted(Vacancy vacancy);
}
public class Utility(IRecruitVacancyClient vacancyClient, ITaskListValidator taskListValidator) : IUtility
{
    public async Task<Vacancy> GetAuthorisedVacancyForEditAsync(VacancyRouteModel vrm, string routeName)
    {
        var vacancy = await GetAuthorisedVacancyAsync(vrm, routeName);

        CheckCanEdit(vacancy);

        return vacancy;
    }

    public async Task<Vacancy> GetAuthorisedVacancyAsync(VacancyRouteModel vrm, string routeName)
    {
        var vacancy = await vacancyClient.GetVacancyAsync(vrm.VacancyId.GetValueOrDefault());

        CheckAuthorisedAccess(vacancy, vrm.Ukprn);

        return vacancy;
    }

    private void CheckCanEdit(Vacancy vacancy)
    {
        if (!vacancy.CanEdit)
            throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing,
                vacancy.Title));
    }

    public void CheckAuthorisedAccess(Vacancy vacancy, long ukprn)
    {
        if (vacancy.TrainingProvider.Ukprn.Value != ukprn)
            throw new AuthorisationException(string.Format(ExceptionMessages.VacancyUnauthorisedAccessForProvider, ukprn, vacancy.TrainingProvider.Ukprn, vacancy.Title, vacancy.Id));
        if (vacancy.OwnerType != OwnerType.Provider)
            throw new AuthorisationException(string.Format(ExceptionMessages.UserIsNotTheOwner, OwnerType.Provider));
    }

        
    public bool VacancyHasCompletedPartOne(Vacancy vacancy)
    {
        return IsTaskListCompleted(vacancy);
    }

    public bool VacancyHasStartedPartTwo(Vacancy vacancy)
    {
        return !string.IsNullOrWhiteSpace(vacancy.EmployerDescription) ||
               vacancy.ApplicationMethod != null ||
               !string.IsNullOrWhiteSpace(vacancy.ThingsToConsider) ||
               vacancy.ProviderContact != null ||
               vacancy.Qualifications != null ||
               vacancy.Skills != null ||
               !string.IsNullOrWhiteSpace(vacancy.Description) ||
               !string.IsNullOrWhiteSpace(vacancy.ShortDescription);
    }

    public PartOnePageInfoViewModel GetPartOnePageInfo(Vacancy vacancy)
    {
        return new PartOnePageInfoViewModel
        {
            HasCompletedPartOne = VacancyHasCompletedPartOne(vacancy),
            HasStartedPartTwo = VacancyHasStartedPartTwo(vacancy)
        };
    }

    public async Task<ApplicationReview> GetAuthorisedApplicationReviewAsync(ApplicationReviewRouteModel rm)
    {
        var applicationReview = await vacancyClient.GetApplicationReviewAsync(rm.ApplicationReviewId);
        var vacancy = await vacancyClient.GetVacancyAsync(rm.VacancyId.GetValueOrDefault());

        applicationReview.AdditionalQuestion1 = vacancy.AdditionalQuestion1;
        applicationReview.AdditionalQuestion2 = vacancy.AdditionalQuestion2;
        applicationReview.VacancyTitle = vacancy.Title;

        // Set Disability Status based on DisabilityConfident setting
        if (applicationReview.Application != null)
            applicationReview.Application.DisabilityStatus = vacancy.DisabilityConfident == DisabilityConfident.Yes
                ? applicationReview.Application.DisabilityStatus
                : ApplicationReviewDisabilityStatus.Unknown;

        try
        {
            CheckAuthorisedAccess(vacancy, rm.Ukprn);
            return applicationReview;
        }
        catch (Exception)
        {
            throw new AuthorisationException(string.Format(ExceptionMessages.ApplicationReviewUnauthorisedAccessForProvider, rm.Ukprn, 
                vacancy.TrainingProvider.Ukprn, rm.ApplicationReviewId,vacancy.Id));
        }                    
    }

    public async Task UpdateEmployerProfile(VacancyEmployerInfoModel employerInfoModel, 
        EmployerProfile employerProfile, Address address, VacancyUser user)
    {
        var updateProfile = false;
        if (string.IsNullOrEmpty(employerProfile.AccountLegalEntityPublicHashedId) && !string.IsNullOrEmpty(employerInfoModel?.AccountLegalEntityPublicHashedId)) 
        {
            updateProfile = true;
            employerProfile.AccountLegalEntityPublicHashedId = employerInfoModel.AccountLegalEntityPublicHashedId;
        }
        if (employerInfoModel != null && employerInfoModel.EmployerIdentityOption == EmployerIdentityOption.NewTradingName)
        {
            updateProfile = true;
            employerProfile.TradingName = employerInfoModel.NewTradingName;
        }
        if (address != null)
        {
            updateProfile = true;
            employerProfile.OtherLocations.Add(address);
        }
        if (updateProfile)    
        {
            await vacancyClient.UpdateEmployerProfileAsync(employerProfile, user);
        }
    }
    
    [Obsolete("Use an instance of ITaskListValidator instead")]
    public bool IsTaskListCompleted(Vacancy vacancy)
    {
        return vacancy is not null && taskListValidator.IsCompleteAsync(vacancy, ProviderTaskListSectionFlags.All).Result;
    }
}