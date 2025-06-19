using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Newtonsoft.Json;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview;

public class VacancyReviewDto
{
    public Guid Id { get; init; }
    public long VacancyReference { get; init; }
    public required string VacancyTitle { get; init; }
    public required DateTime CreatedDate { get; init; }
    public required DateTime SlaDeadLine { get; init; }
    public DateTime? ReviewedDate { get; init; }
    public required string Status { get; set; }
    public byte SubmissionCount { get; init; }
    public string ReviewedByUserEmail { get; init; }
    public required string SubmittedByUserEmail { get; init; }
    public DateTime? ClosedDate { get; init; }
    public string ManualOutcome { get; set; }
    public string ManualQaComment { get; init; }
    public required List<string> ManualQaFieldIndicators { get; init; }
    public string AutomatedQaOutcome { get; set; }
    public string AutomatedQaOutcomeIndicators { get; init; }
    public required List<string> DismissedAutomatedQaOutcomeIndicators { get; init; }
    public required List<string> UpdatedFieldIdentifiers { get; init; }
    public string OwnerType { get; set; }
    public required string VacancySnapshot { get; set; }
    public long Ukprn { get; set; }
    public long AccountId { get; set; }
    public long AccountLegalEntityID { get; set; }

    public static VacancyReviewDto MapVacancyReviewDto(Domain.Entities.VacancyReview source, IEncodingService encodingService)
    {
        return new VacancyReviewDto
        {
            
            Id = source.Id,
            VacancyReference = source.VacancyReference,
            VacancyTitle = source.Title,
            CreatedDate = source.CreatedDate!.Value,
            SlaDeadLine = source.SlaDeadline!.Value,
            ReviewedDate = source.ReviewedDate,
            Status = source.Status.ToString(),
            SubmissionCount = (byte)source.SubmissionCount,
            ReviewedByUserEmail = source.ReviewedByUser?.Email,
            SubmittedByUserEmail = source.SubmittedByUser.Email,
            ClosedDate = source.ClosedDate,
            ManualOutcome = source.ManualOutcome?.ToString(),
            ManualQaComment = source.ManualQaComment,
            ManualQaFieldIndicators =source.ManualQaFieldIndicators!=null ? source.ManualQaFieldIndicators.Where(c=>c.IsChangeRequested)
                .Select(c=>c.ToString()).ToList() : [],
            AutomatedQaOutcome = source.AutomatedQaOutcome?.Decision.ToString(),
            AutomatedQaOutcomeIndicators = source.AutomatedQaOutcomeIndicators?.FirstOrDefault()?.IsReferred.ToString(),
            DismissedAutomatedQaOutcomeIndicators = source.DismissedAutomatedQaOutcomeIndicators,
            UpdatedFieldIdentifiers = source.UpdatedFieldIdentifiers,
            VacancySnapshot = JsonConvert.SerializeObject(source.VacancySnapshot),
            OwnerType = source.VacancySnapshot.OwnerType.ToString(),
            AccountId = encodingService.Decode(source.VacancySnapshot.EmployerAccountId, EncodingType.AccountId),
            Ukprn = source.VacancySnapshot.TrainingProvider.Ukprn!.Value,
            AccountLegalEntityID = encodingService.Decode(source.VacancySnapshot.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId)
        };
    }

    

    public static explicit operator Domain.Entities.VacancyReview(VacancyReviewDto source)
    {
        if (source == null)
        {
            return null;
        }
        
        return new Domain.Entities.VacancyReview
        {
            Id = source.Id,
            VacancyReference = source.VacancyReference,
            Title = source.VacancyTitle,
            CreatedDate = source.CreatedDate,
            SlaDeadline = source.SlaDeadLine,
            ReviewedDate = source.ReviewedDate,
            Status = Enum.Parse<ReviewStatus>(source.Status),
            SubmissionCount = source.SubmissionCount,
            ReviewedByUser = new VacancyUser{Email = source.ReviewedByUserEmail},
            SubmittedByUser = new VacancyUser{Email = source.SubmittedByUserEmail },
            ClosedDate = source.ClosedDate,
            ManualOutcome = Enum.Parse<ManualQaOutcome>(source.ManualOutcome),
            ManualQaComment = source.ManualQaComment,
            ManualQaFieldIndicators = source.ManualQaFieldIndicators.Select(c=>new ManualQaFieldIndicator{IsChangeRequested = true, FieldIdentifier = c}).ToList(),
            AutomatedQaOutcome = new RuleSetOutcome{Decision =  Enum.Parse<RuleSetDecision>(source.AutomatedQaOutcome)},
            AutomatedQaOutcomeIndicators = new List<RuleOutcomeIndicator>{new()
            {
                IsReferred = !string.IsNullOrEmpty(source.AutomatedQaOutcomeIndicators),
                RuleOutcomeId = Guid.NewGuid()
            }},
            DismissedAutomatedQaOutcomeIndicators = source.DismissedAutomatedQaOutcomeIndicators,
            UpdatedFieldIdentifiers = source.UpdatedFieldIdentifiers,
            VacancySnapshot = JsonConvert.DeserializeObject<Vacancy>(source.VacancySnapshot)
        };
    }
}