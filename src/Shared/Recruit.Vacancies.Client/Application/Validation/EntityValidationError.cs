namespace Esfa.Recruit.Vacancies.Client.Application.Validation;

public class EntityValidationError(long ruleId, string propertyName, string errorMessage, string errorCode)
{
    public long RuleId { get; set; } = ruleId;
    public string PropertyName { get; set; } = propertyName;
    public string ErrorMessage { get; set; } = errorMessage;
    public string ErrorCode { get; set; } = errorCode;
}