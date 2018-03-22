namespace Esfa.Recruit.Vacancies.Client.Application.Validation
{
    public interface IEntityValidator<T, TRules>
    {
        void ValidateAndThrow(T entity, TRules validationsToRun);
    }
}