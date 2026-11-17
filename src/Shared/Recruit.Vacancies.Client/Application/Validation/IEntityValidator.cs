namespace Esfa.Recruit.Vacancies.Client.Application.Validation;

public interface IEntityValidator<in T, in TRules>
{
    void ValidateAndThrow(T entity, TRules rules);

    EntityValidationResult Validate(T entity, TRules rules);
}