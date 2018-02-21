using Esfa.Recruit.Storage.Client.Domain.Entities;
using Esfa.Recruit.Storage.Client.Domain.Messaging;
using MediatR;
using System;

namespace Esfa.Recruit.Storage.Client.Application.Commands
{
    public class CreateVacancyCommand : ICommand, IRequest
    {
        public CreateVacancyCommand(Vacancy vacancy) => Vacancy = vacancy;

        public Vacancy Vacancy { get; }
    }
}
