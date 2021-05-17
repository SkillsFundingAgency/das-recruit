namespace Esfa.Recruit.Shared.Web.ViewModels
{
    public class ErrorMessages
    {
        public const string VacancyCannotBeViewed = "The advert '{0}' can't be viewed.";
        public const string VacancyNotAvailableForEditing = "The advert '{0}' has been edited by one of your colleagues. Your changes have not been saved.";
        public const string VacancyNotAvailableForClosing = "The advert '{0}' cannot be closed as it is not currently published.";
        public const string VacancyNotAvailableForCloning = "The advert '{0}' cannot be cloned.";
        public const string VacancyNotSubmittedSuccessfully = "The advert '{0}' has not been submitted successfully.";
        public const string VacancyDatesCannotBeEdited = "The dates of advert '{0}' can't be edited.";
        public const string CannotCloneVacancyWithSameDates = "The advert '{0}' has a closing date or start date that is in the past.";
        public const string VacancyNotAvailableForReject = "The advert '{0}' cannot be rejected.";
    }
}
