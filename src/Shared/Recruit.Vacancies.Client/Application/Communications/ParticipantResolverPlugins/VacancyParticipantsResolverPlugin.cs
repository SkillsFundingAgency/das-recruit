using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications.ParticipantResolverPlugins
{
    public class VacancyParticipantsResolverPlugin : IParticipantResolver
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserNotificationPreferencesRepository _userPreferenceRepository;
        private readonly ILogger<VacancyParticipantsResolverPlugin> _logger;

        public string ParticipantResolverName => CommunicationConstants.ParticipantResolverNames.VacancyParticipantsResolverName;

        public VacancyParticipantsResolverPlugin(
            IVacancyRepository vacancyRepository,
            IUserRepository userRepository,
            IUserNotificationPreferencesRepository userPreferenceRepository,
            ILogger<VacancyParticipantsResolverPlugin> logger)
        {
            _userRepository = userRepository;
            _userPreferenceRepository = userPreferenceRepository;
            _vacancyRepository = vacancyRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CommunicationUser>> GetParticipantsAsync(CommunicationRequest request)
        {
            _logger.LogDebug($"Resolving participants for RequestType: '{request.RequestType}'");

            var entityId = request.Entities.Single(e => e.EntityType == CommunicationConstants.EntityTypes.Vacancy).EntityId.ToString();
            if(long.TryParse(entityId, out var vacancyReference) == false)
            {
                return Array.Empty<CommunicationUser>();
            }

            var vacancy = await _vacancyRepository.GetVacancyAsync(vacancyReference);
            List<User> users = null;
            if (vacancy.OwnerType == OwnerType.Employer)
            {
                users = await _userRepository.GetEmployerUsersAsync(vacancy.EmployerAccountId);
            }
            else
            {
                users = await _userRepository.GetProviderUsersAsync(vacancy.TrainingProvider.Ukprn.GetValueOrDefault());
            }
            return ParticipantResolverPluginHelper.ConvertToCommunicationUsers(users, vacancy.SubmittedByUser.UserId);
        }

        public async Task<IEnumerable<CommunicationMessage>> ValidateParticipantAsync(IEnumerable<CommunicationMessage> messages)
        {
            var validCommunicationMessages = new List<CommunicationMessage>();

            foreach (var msg in messages)
            {
                var isStillValid = await IsUserStillAssociatedToCommunicationMessageVacancyAsync(msg);

                if (isStillValid)
                    validCommunicationMessages.Add(msg);
                else
                    _logger.LogInformation($"Unable to validate notification recipients of CommunicationMessage {msg.Id.ToString()}");
            }

            return validCommunicationMessages;
        }

        private async Task<bool> IsUserStillAssociatedToCommunicationMessageVacancyAsync(CommunicationMessage msg)
        {
            var user = await _userRepository.GetAsync(msg.Recipient.UserId);

            if (user == null)
            {
                _logger.LogWarning($"Unable to locate user associated with the recipients of CommunicationMessage {msg.Id.ToString()}");
                return false;
            }

            var vacancyId = msg.DataItems.Single(di => di.Key == CommunicationConstants.DataItemKeys.Vacancy.VacancyReference).Value;

            if (long.TryParse(vacancyId, out var vacancyReference) == false)
            {
                return false;
            }

            var vacancy = await _vacancyRepository.GetVacancyAsync(vacancyReference);

            var isUserStillAssociatedToVacancy = false;
            if (vacancy.OwnerType == OwnerType.Employer)
            {
                isUserStillAssociatedToVacancy = user.EmployerAccountIds.Contains(vacancy.EmployerAccountId);
            }
            else
            {
                isUserStillAssociatedToVacancy = user.Ukprn.HasValue
                                                ? user.Ukprn.Value.Equals(vacancy.TrainingProvider.Ukprn.GetValueOrDefault())
                                                : false;
            }

            if (isUserStillAssociatedToVacancy == false) return false;

            var userPreference = await _userPreferenceRepository.GetAsync(user.IdamsUserId);

            if (Enum.TryParse<NotificationTypes>(msg.RequestType, out var reqNotificationType))
            {
                var hasOptedInToNotificationType = userPreference.NotificationTypes.HasFlag(reqNotificationType);
                return hasOptedInToNotificationType;
            }

            return false;
        }
    }
}