using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class PostVacancyRequest(Guid id, VacancyDto vacancy) : IPostApiRequest
{
    public string PostUrl => $"vacancies/{id}";
    public object Data { get; set; } = vacancy;
}

public class VacancyDto
{
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
    public DateTime? ReviewDate { get; init; }
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
    
    public static VacancyDto From(Vacancy vacancy, IEncodingService encodingService)
    {
        return new VacancyDto
        {
            AccountLegalEntityId = string.IsNullOrWhiteSpace(vacancy.AccountLegalEntityPublicHashedId) ? null : encodingService.Decode(vacancy.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId),
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
            DisabilityConfident = vacancy.DisabilityConfident == Domain.Entities.DisabilityConfident.Yes,
            AccountId = string.IsNullOrWhiteSpace(vacancy.EmployerAccountId) ? null : encodingService.Decode(vacancy.EmployerAccountId, EncodingType.AccountId),
            Contact = vacancy.EmployerContact ?? vacancy.ProviderContact,
            EmployerDescription = vacancy.EmployerDescription,
            EmployerLocationInformation = vacancy.EmployerLocationInformation,
            EmployerLocationOption = vacancy.EmployerLocationOption,
            EmployerLocations = MapLocations(vacancy),
            EmployerName = vacancy.EmployerName,
            EmployerNameOption = vacancy.EmployerNameOption,
            EmployerRejectedReason = vacancy.EmployerRejectedReason,
            EmployerReviewFieldIndicators = vacancy.EmployerReviewFieldIndicators?.Select(x => new ReviewFieldIndicator() { FieldIdentifier = x.FieldIdentifier, IsChangeRequested = x.IsChangeRequested }).ToList(),
            EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl,
            GeoCodeMethod = vacancy.GeoCodeMethod,
            HasChosenProviderContactDetails = vacancy.HasChosenProviderContactDetails,
            HasOptedToAddQualifications = vacancy.HasOptedToAddQualifications,
            HasSubmittedAdditionalQuestions = vacancy.HasSubmittedAdditionalQuestions,
            LastUpdatedDate = vacancy.LastUpdatedDate,
            LegalEntityName = vacancy.LegalEntityName,
            LiveDate = vacancy.LiveDate,
            NumberOfPositions = vacancy.NumberOfPositions,
            OutcomeDescription = vacancy.OutcomeDescription,
            OwnerType = vacancy.OwnerType,
            ProgrammeId = vacancy.ProgrammeId,
            ProviderReviewFieldIndicators = vacancy.ProviderReviewFieldIndicators?.Select(x => new ReviewFieldIndicator() { FieldIdentifier = x.FieldIdentifier, IsChangeRequested = x.IsChangeRequested }).ToList(),
            Qualifications = vacancy.Qualifications,
            ReviewCount = vacancy.ReviewCount,
            ReviewDate = vacancy.ReviewDate,
            ShortDescription = vacancy.ShortDescription,
            Skills = vacancy.Skills,
            SourceOrigin = vacancy.SourceOrigin,
            SourceType = vacancy.SourceType,
            SourceVacancyReference = vacancy.SourceVacancyReference,
            StartDate = vacancy.StartDate,
            Status = vacancy.Status,
            SubmittedByUserId = vacancy.SubmittedByUser?.UserId,
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

    private static List<Address> MapLocations(Vacancy vacancy)
    {
        return vacancy.EmployerLocationOption switch
        {
            AvailableWhere.OneLocation => vacancy.EmployerLocations,
            AvailableWhere.MultipleLocations => vacancy.EmployerLocations,
            AvailableWhere.AcrossEngland => null,
            null when vacancy.EmployerLocation is not null => [vacancy.EmployerLocation],
            null => null,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public class ReviewFieldIndicator
{
    public string FieldIdentifier { get; set; }
    public bool IsChangeRequested { get; set; }
}