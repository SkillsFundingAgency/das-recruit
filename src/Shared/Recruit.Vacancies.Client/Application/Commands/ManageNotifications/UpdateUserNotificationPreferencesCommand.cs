using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands.ManageNotifications;

public record UpdateUserNotificationPreferencesCommand(Guid Id, NotificationPreferences NotificationPreferences) : IRequest;