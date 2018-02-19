using Esfa.Recruit.Storage.Client.Core.Entities.VacancyPatches;
using Esfa.Recruit.Storage.Client.Core.Messaging;
using MediatR;
using System;

namespace Esfa.Recruit.Storage.Client.Core.Commands
{
    public class UpsertVacancyCommand : ICommand, IRequest
    {
        public Guid Id { get; set; }
        public IVacancyPatch Patch { get; set; }
    }
}
