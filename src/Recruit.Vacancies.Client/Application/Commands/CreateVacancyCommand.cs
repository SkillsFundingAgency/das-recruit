using Esfa.Recruit.Storage.Client.Domain.Entities;
using Esfa.Recruit.Storage.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Storage.Client.Application.Commands
{
    public class CreateVacancyCommand : ICommand, IRequest
    {
        public Vacancy Vacancy { get; set; }
    }
}
