using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    public class UserNotificationPreferencesValidator : AbstractValidator<UserNotificationPreferences>
    {
        public UserNotificationPreferencesValidator()
        {
            When(n => n.NotificationTypes != NotificationTypes.None, () => 
            {
                RuleFor(t => t.NotificationScope)
                    .NotNull()
                    .WithErrorCode("1101")
                    .WithMessage("Choose what emails you’d like to get");
            });
            
            When(n => (n.NotificationTypes & NotificationTypes.ApplicationSubmitted) == NotificationTypes.ApplicationSubmitted, () =>
            {
                RuleFor(s => s.NotificationFrequency)
                    .NotNull()
                    .WithErrorCode("1102")
                    .WithMessage("Choose how often you’d like new application emails");
            });
        }
    }
}