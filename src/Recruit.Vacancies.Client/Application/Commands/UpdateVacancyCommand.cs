using Esfa.Recruit.Storage.Client.Domain.Entities;
using Esfa.Recruit.Storage.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Storage.Client.Application.Commands
{
    public class UpdateVacancyCommand : ICommand, IRequest
    {
        public Vacancy Vacancy { get; set; }
    }
}
