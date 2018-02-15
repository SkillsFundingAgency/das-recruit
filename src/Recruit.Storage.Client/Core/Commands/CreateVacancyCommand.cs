using Esfa.Recruit.Storage.Client.Core.Messaging;
using Esfa.Recruit.Storage.Client.Core.Responses;
using MediatR;
using System;

namespace Esfa.Recruit.Storage.Client.Core.Commands
{
    public class CreateVacancyCommand : ICommand<CreateVacancyCommandResponse>, IRequest<CreateVacancyCommandResponse>
    {
        public string Title { get; set; }
    }
}
