namespace Esfa.Recruit.Vacancies.Client.Application.Validation;

public class EntityValidationError
{
    public EntityValidationError(long ruleId, string propertyName, string errorMessage, string errorCode)
    {
        RuleId = ruleId;
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    public long RuleId { get; set; }
    public string PropertyName { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorCode { get; set; }
}