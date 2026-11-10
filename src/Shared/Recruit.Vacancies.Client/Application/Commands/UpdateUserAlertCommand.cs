using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands;

public class UpdateUserAlertCommand : ICommand, IRequest<Unit>
{
    public string IdamsUserId { get; set; }
    public AlertType AlertType { get; set; }
    public DateTime DismissedOn { get; set; }
    public string DfEUserId { get; set; }
}