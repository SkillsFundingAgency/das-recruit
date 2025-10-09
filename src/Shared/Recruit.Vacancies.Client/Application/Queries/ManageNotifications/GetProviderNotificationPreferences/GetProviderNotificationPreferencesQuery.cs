﻿using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Queries.ManageNotifications.GetProviderNotificationPreferences;

public record GetProviderNotificationPreferencesQuery(string DfEUserId) : IRequest<GetProviderNotificationPreferencesQueryResult>;