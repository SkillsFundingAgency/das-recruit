﻿using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Queries.ManageNotifications.GetEmployerNotificationPreferences;

public record GetEmployerNotificationPreferencesQuery(string IdamsId) : IRequest<GetEmployerNotificationPreferencesQueryResult>;