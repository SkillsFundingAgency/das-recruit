using Esfa.Recruit.Storage.Client.Core.Commands;
using Esfa.Recruit.Storage.Client.Core.Entities;
using Esfa.Recruit.Storage.Client.Core.Repositories;
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

        private readonly IVacancyRepository _repository;

        public CreateVacancyCommandHandler(IVacancyRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(CreateVacancyCommand message, CancellationToken cancellationToken)
        {
            var vacancy = new MongoVacancy
            {
                Title = message.Title
            };

            await _repository.CreateVacancyAsync(vacancy);
        }
    }
}
