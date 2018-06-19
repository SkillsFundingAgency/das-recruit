using Esfa.Recruit.Vacancies.Client.Domain.Messaging;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class ApplicationSubmitCommand : CommandBase
    {
        public Domain.Entities.Application Application { get; set; }
    }
}