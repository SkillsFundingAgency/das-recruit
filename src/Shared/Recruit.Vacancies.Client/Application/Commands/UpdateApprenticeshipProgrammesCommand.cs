﻿using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UpdateApprenticeshipProgrammesCommand : ICommand, IRequest<Unit>
    {
    }
}
