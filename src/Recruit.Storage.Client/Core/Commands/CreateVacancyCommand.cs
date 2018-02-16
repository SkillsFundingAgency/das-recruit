using Esfa.Recruit.Storage.Client.Core.Messaging;
using MediatR;
using System;

namespace Esfa.Recruit.Storage.Client.Core.Commands
{
    public class CreateVacancyCommand : ICommand, IRequest
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        
    }
}
