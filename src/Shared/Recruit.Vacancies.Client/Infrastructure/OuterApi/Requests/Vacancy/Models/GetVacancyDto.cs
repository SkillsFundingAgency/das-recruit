using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Vacancy.Models;

public class GetVacancyDto
{
    public Guid Id { get; init; }
    public long? VacancyReference { get; init; }
    public long? AccountId { get; init; }
    public required VacancyStatus Status { get; init; }
    public ApprenticeshipTypes? ApprenticeshipType { get; init; }
    public string? Title { get; init; }
    public OwnerType? OwnerType { get; init; }
    public SourceOrigin? SourceOrigin { get; init; }
    public SourceType? SourceType { get; init; }
    public long? SourceVacancyReference { get; init; }
    public DateTime? ApprovedDate { get; init; }
    public DateTime? CreatedDate { get; init; }
    public DateTime? LastUpdatedDate { get; init; }
    public DateTime? SubmittedDate { get; init; }
    public DateTime? ReviewRequestedDate { get; init; }
    public DateTime? ClosedDate { get; init; }
    public DateTime? DeletedDate { get; init; }
    public DateTime? LiveDate { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? ClosingDate { get; init; }
    public int ReviewCount { get; init; }
    public string? ApplicationUrl { get; init; }
    public ApplicationMethod? ApplicationMethod { get; init; }
    public string? ApplicationInstructions { get; init; }
    public string? ShortDescription { get; init; }
    public string? Description { get; init; }
    public string? AnonymousReason { get; init; }
    public bool? DisabilityConfident { get; init; }
    public ContactDetail? Contact { get; set; }
    public string? EmployerDescription { get; init; }
    public List<Address>? EmployerLocations { get; set; }
    public AvailableWhere? EmployerLocationOption { get; init; }
    public string? EmployerLocationInformation { get; init; }
    public string? EmployerName { get; init; }
    public EmployerNameOption? EmployerNameOption { get; init; }
    public string? EmployerRejectedReason { get; init; }
    public string? LegalEntityName { get; init; }
    public string? EmployerWebsiteUrl { get; init; }
    public GeoCodeMethod? GeoCodeMethod { get; init; }
    public long? AccountLegalEntityId { get; init; }
    public int? NumberOfPositions { get; init; }
    public string? OutcomeDescription { get; init; }
    public string? ProgrammeId { get; init; }
    public List<string>? Skills { get; init; }
    public List<Qualification>? Qualifications { get; set; }
    public string? ThingsToConsider { get; init; }
    public string? TrainingDescription { get; init; }
    public string? AdditionalTrainingDescription { get; init; }
    public TrainingProvider? TrainingProvider { get; init; }
    public Wage? Wage { get; set; }
    public ClosureReason? ClosureReason { get; init; }
    public TransferInfo? TransferInfo { get; init; }
    public string? AdditionalQuestion1 { get; init; }
    public string? AdditionalQuestion2 { get; init; }
    public bool? HasSubmittedAdditionalQuestions { get; init; }
    public bool? HasChosenProviderContactDetails { get; init; }
    public bool? HasOptedToAddQualifications { get; init; }
    public List<ReviewFieldIndicator>? EmployerReviewFieldIndicators { get; init; }
    public List<ReviewFieldIndicator>? ProviderReviewFieldIndicators { get; init; }
    public string? SubmittedByUserId { get; init; }
    public string? ReviewRequestedByUserId { get; set; }
    
    public static Domain.Entities.Vacancy ToDomain(GetVacancyDto vacancy, IEncodingService encodingService)
    {
        return new Domain.Entities.Vacancy
        {
            AccountLegalEntityPublicHashedId = vacancy.AccountLegalEntityId is null ? null : encodingService.Encode(vacancy.AccountLegalEntityId.Value, EncodingType.PublicAccountLegalEntityId), 
            AdditionalQuestion1 = vacancy.AdditionalQuestion1,
            AdditionalQuestion2 = vacancy.AdditionalQuestion2,
            AdditionalTrainingDescription = vacancy.AdditionalTrainingDescription,
            AnonymousReason = vacancy.AnonymousReason,
            ApplicationInstructions = vacancy.ApplicationInstructions,
            ApplicationMethod = vacancy.ApplicationMethod,
            ApplicationUrl = vacancy.ApplicationUrl,
            ApprenticeshipType = vacancy.ApprenticeshipType,
            ApprovedDate = vacancy.ApprovedDate,
            ClosedDate = vacancy.ClosedDate,
            ClosingDate = vacancy.ClosingDate,
            ClosureReason = vacancy.ClosureReason,
            CreatedDate = vacancy.CreatedDate,
            DeletedDate = vacancy.DeletedDate,
            Description = vacancy.Description,
            DisabilityConfident = vacancy.DisabilityConfident is true ? Domain.Entities.DisabilityConfident.Yes : Domain.Entities.DisabilityConfident.No,
            EmployerAccountId = vacancy.AccountId is null ? null : encodingService.Encode(vacancy.AccountId.Value, EncodingType.AccountId),
            EmployerContact = vacancy.OwnerType == Domain.Entities.OwnerType.Employer ? vacancy.Contact : null,
            EmployerDescription = vacancy.EmployerDescription,
            EmployerLocationInformation = vacancy.EmployerLocationInformation,
            EmployerLocationOption = vacancy.EmployerLocationOption,
            EmployerLocations = vacancy.EmployerLocations,
            EmployerName = vacancy.EmployerName,
            EmployerNameOption = vacancy.EmployerNameOption,
            EmployerRejectedReason = vacancy.EmployerRejectedReason,
            EmployerReviewFieldIndicators = vacancy.EmployerReviewFieldIndicators?.Select(x => new EmployerReviewFieldIndicator { FieldIdentifier = x.FieldIdentifier, IsChangeRequested = x.IsChangeRequested }).ToList(),
            EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl,
            GeoCodeMethod = vacancy.GeoCodeMethod,
            HasChosenProviderContactDetails = vacancy.HasChosenProviderContactDetails,
            HasOptedToAddQualifications = vacancy.HasOptedToAddQualifications,
            HasSubmittedAdditionalQuestions = vacancy.HasSubmittedAdditionalQuestions ?? false,
            Id = vacancy.Id,
            IsDeleted = vacancy.DeletedDate is not null,
            LastUpdatedDate = vacancy.LastUpdatedDate,
            LegalEntityName = vacancy.LegalEntityName,
            LiveDate = vacancy.LiveDate,
            NumberOfPositions = vacancy.NumberOfPositions,
            OutcomeDescription = vacancy.OutcomeDescription,
            OwnerType = vacancy.OwnerType!.Value,
            ProgrammeId = vacancy.ProgrammeId,
            ProviderContact = vacancy.OwnerType == Domain.Entities.OwnerType.Provider ? vacancy.Contact : null,
            ProviderReviewFieldIndicators = vacancy.ProviderReviewFieldIndicators?.Select(x => new ProviderReviewFieldIndicator() { FieldIdentifier = x.FieldIdentifier, IsChangeRequested = x.IsChangeRequested }).ToList(),
            Qualifications = vacancy.Qualifications,
            ReviewByUser = new VacancyUser { UserId = vacancy.ReviewRequestedByUserId },
            ReviewCount = vacancy.ReviewCount,
            ReviewDate = vacancy.ReviewRequestedDate,
            ShortDescription = vacancy.ShortDescription,
            Skills = vacancy.Skills,
            SourceOrigin = vacancy.SourceOrigin!.Value,
            SourceType = vacancy.SourceType!.Value,
            SourceVacancyReference = vacancy.SourceVacancyReference,
            StartDate = vacancy.StartDate,
            Status = vacancy.Status,
            SubmittedByUser = new VacancyUser { UserId = vacancy.SubmittedByUserId },
            SubmittedDate = vacancy.SubmittedDate,
            ThingsToConsider = vacancy.ThingsToConsider,
            Title = vacancy.Title,
            TrainingDescription = vacancy.TrainingDescription,
            TrainingProvider = vacancy.TrainingProvider,
            TransferInfo = vacancy.TransferInfo,
            VacancyReference = vacancy.VacancyReference,
            Wage = vacancy.Wage,
        };
    }
}