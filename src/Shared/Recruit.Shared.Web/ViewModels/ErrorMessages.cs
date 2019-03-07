namespace Esfa.Recruit.Shared.Web.ViewModels
{
    public class ErrorMessages
    {
        public const string VacancyCannotBeViewed = "The vacancy '{0}' can't be viewed.";
        public const string VacancyNotAvailableForEditing = "The vacancy '{0}' has been edited by one of your colleagues. Your changes have not been saved.";
        public const string VacancyNotAvailableForClosing = "The vacancy '{0}' cannot be closed as it is not currently published.";
        public const string VacancyNotAvailableForCloning = "The vacancy '{0}' cannot be cloned.";
        public const string VacancyNotSubmittedSuccessfully = "The vacancy '{0}' has not been submitted successfully.";
        public const string VacancyDatesCannotBeEdited = "The dates of vacancy '{0}' can't be edited.";
        public const string CannotCloneVacancyWithSameDates = "The vacancy '{0}' has a closing date or start date that is in the past.";
    }
}
