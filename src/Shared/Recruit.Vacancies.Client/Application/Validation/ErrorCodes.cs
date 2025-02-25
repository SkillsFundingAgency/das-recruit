namespace Esfa.Recruit.Vacancies.Client.Application.Validation
{
    public static class ErrorCodes
    {
        public const string TrainingNotExist = "260";
        public const string TrainingExpiryDate = "26";
        public const string TrainingExpiryDateMustExist = "27";
        public const string TrainingProviderUkprnNotEmpty = "101";
        public const string TrainingProviderUkprnMustBeCorrectLength = "99";
        public const string TrainingProviderMustExist = "102";
        public const string TrainingProviderMustNotBeBlocked = "103";
        public const string TrainingProviderMustHaveEmployerPermission = "104";
        public const string TrainingProviderMustBeMainOrEmployerProfile = "105";
    }
}
