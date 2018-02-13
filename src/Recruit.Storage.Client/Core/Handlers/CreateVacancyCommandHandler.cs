using Esfa.Recruit.Storage.Client.Core.Commands;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Storage.Client.Core.Handlers
{
    public class CreateVacancyCommandHandler : IRequestHandler<CreateVacancyCommand>
    {
        public Task Handle(CreateVacancyCommand message, CancellationToken cancellationToken)
        {
            //magic happens here

            return Task.CompletedTask;
        }
    }
}
