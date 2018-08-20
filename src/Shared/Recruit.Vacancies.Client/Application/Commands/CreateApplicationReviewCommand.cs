using Esfa.Recruit.Vacancies.Client.Domain.Messaging;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class CreateApplicationReviewCommand : CommandBase
    {
        public Domain.Entities.Application Application { get; set; }
    }
}