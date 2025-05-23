﻿namespace Esfa.Recruit.Provider.Web.Configuration
{
    public static class TempDataKeys
    {
        public const string ProviderIdentifier = "Provider";
        public const string ProviderName = "ProviderName";
        public const string IsBlockedProvider = "IsBlockedProvider";
        public const string VacanciesErrorMessage = "Vacancies_ErrorMessage";
        public const string VacanciesInfoMessage = "Vacancies_InfoMessage";
        public const string Skills = "Skills";
        public const string VacancyClosedMessage = "VacancyClosedMessage";
        public const string ApplicationReviewStatusInfoMessage = "ApplicationReviewStatus_InfoMessage";
        public const string ApplicationReviewSuccessStatusInfoMessage = "ApplicationReviewSuccessStatus_InfoMessage";
        public const string ApplicationReviewUnsuccessStatusInfoMessage = "ApplicationReviewUnsuccessStatus_InfoMessage";
        public const string VacancyPreviewInfoMessage = "VacancyPreviewInfoMessage";
        public const string SharedMultipleApplicationsHeader = "SharedMultipleApplicationsHeader_InfoMessage";
        public const string SharedSingleApplicationsHeader = "SharedSingleApplicationsHeader_InfoMessage";
        public const string ApplicationsToUnsuccessfulHeader = "ApplicationsToUnsuccessfulHeader_InfoMessage";
        public const string ApplicationStatusChangedHeader = "ApplicationStatusChangedHeader_InfoMessage";
        public const string SelectedLocations = nameof(SelectedLocations);
        public const string AddedLocation = nameof(AddedLocation);
        public const string Postcode = nameof(Postcode);
        public const string AddLocationReturnPath = nameof(AddLocationReturnPath);
    }
}