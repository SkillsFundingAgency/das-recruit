using System;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class SaveUserLevyDeclarationCommand : ICommand, IRequest
    {
        public string UserId { get; set; }
        public bool DeclaringAsLevyEmployer { get; set; }
    }
}
