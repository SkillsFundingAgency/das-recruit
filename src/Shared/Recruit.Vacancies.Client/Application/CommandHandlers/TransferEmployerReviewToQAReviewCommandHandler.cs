using System;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;

public class TransferEmployerReviewToQAReviewCommandHandler(
    ILogger<TransferEmployerReviewToQAReviewCommandHandler> logger,
    IVacancyRepository vacancyRepository,
    IMessaging messaging,
    ITimeProvider timeProvider)
    : IRequestHandler<TransferEmployerReviewToQAReviewCommand, Unit>
{
    public const string VacancyNotFoundExceptionMessageFormat = "Vacancy {0} not found";
    public const string InvalidStateExceptionMessageFormat = "Unable to transfer vacancy {0} due to vacancy having a status of {1}.";
    public const string MissingReferenceNumberExceptionMessageFormat = "Cannot submit vacancy {0} without a vacancy reference number";

    public async Task<Unit> Handle(TransferEmployerReviewToQAReviewCommand message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Transferring Employer review Vacancy to QA review {vacancyId}.", message.VacancyId);

        var vacancy = await vacancyRepository.GetVacancyAsync(message.VacancyId);
            
        if (vacancy == null) 
            throw new ArgumentException(string.Format(VacancyNotFoundExceptionMessageFormat, message.VacancyId));

        if (vacancy.VacancyReference.HasValue == false)
            throw new InvalidOperationException(string.Format(MissingReferenceNumberExceptionMessageFormat, vacancy.Id));

        if(vacancy.Status != VacancyStatus.Review)
            throw new InvalidOperationException(string.Format(InvalidStateExceptionMessageFormat, vacancy.Id, vacancy.Status));

        var now = timeProvider.Now;

        var vacancyUser = new VacancyUser
        {
            UserId = message.UserRef.ToString(),
            Email = message.UserEmailAddress,
            Name = message.UserName
        };

        vacancy.Status = VacancyStatus.Submitted;
        vacancy.SubmittedDate = now;
        vacancy.SubmittedByUser = vacancyUser;
        vacancy.LastUpdatedDate = now;

        await vacancyRepository.UpdateAsync(vacancy);

        await messaging.PublishEvent(new VacancySubmittedEvent
        {
            EmployerAccountId = vacancy.EmployerAccountId,
            VacancyId = vacancy.Id,
            VacancyReference = vacancy.VacancyReference.Value
        });
        return Unit.Value;
    }
}