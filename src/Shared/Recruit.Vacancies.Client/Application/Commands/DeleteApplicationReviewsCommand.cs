using System;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class DeleteApplicationReviewsCommand : ICommand, IRequest
    {
        public Guid CandidateId { get; set; }
    }
}
