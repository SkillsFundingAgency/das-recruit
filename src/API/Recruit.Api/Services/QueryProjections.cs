using System;
using System.Collections.Generic;

namespace SFA.DAS.Recruit.Api.Services
{
    internal struct QueryViewType
    {
        public string TypeName { get; }
        private readonly string _idFormatString;

        private QueryViewType(string name, string formatString)
        {
            TypeName = name;
            _idFormatString = formatString;
        }

        public string GetIdValue(object[] args)
        {
            return string.Format(_idFormatString, args);
        }

        public string GetIdValue(string value)
        {
            return string.Format(_idFormatString, value);
        }

        public string GetIdValue(long value)
        {
            return string.Format(_idFormatString, value);
        }

        public string GetIdValue()
        {
            return _idFormatString;
        }

        public static QueryViewType EmployerDashboard => new QueryViewType(nameof(EmployerDashboard), "EmployerDashboard_{0}");
        public static QueryViewType ProviderDashboard => new QueryViewType(nameof(ProviderDashboard), "ProviderDashboard_{0}");
        public static QueryViewType EditVacancyInfo => new QueryViewType(nameof(EditVacancyInfo), "EditVacancyInfo_{0}");
        public static QueryViewType LiveVacancy => new QueryViewType(nameof(LiveVacancy), "LiveVacancy_{0}");
        public static QueryViewType VacancyApplications => new QueryViewType(nameof(VacancyApplications), "VacancyApplications_{0}");
        public static QueryViewType QaDashboard => new QueryViewType(nameof(QaDashboard), "QaDashboard");
        public static QueryViewType ClosedVacancy => new QueryViewType(nameof(ClosedVacancy), "ClosedVacancy_{0}");
        public static QueryViewType VacancyAnalyticsSummary => new QueryViewType(nameof(VacancyAnalyticsSummary), "VacancyAnalyticsSummary_{0}");
        public static QueryViewType BlockedEmployerOrganisations => new QueryViewType(nameof(BlockedEmployerOrganisations), "BlockedEmployerOrganisations");
        public static QueryViewType BlockedProviderOrganisations => new QueryViewType(nameof(BlockedProviderOrganisations), "BlockedProviderOrganisations");
    }

    public abstract class QueryProjectionBase
    {
        protected QueryProjectionBase(string viewType)
        {
            ViewType = viewType;
        }

        public string Id { get; set; }
        public string ViewType { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public enum VacancyStatus
    {
        Draft,
        Review,
        Rejected,
        Submitted,
        Referred,
        Live,
        Closed,
        Approved
    }

    public enum TrainingType
    {
        Standard = 0,
        Framework = 1
    }

    public enum ApplicationMethod
    {
        ThroughFindAnApprenticeship,
        ThroughExternalApplicationSite,
        ThroughFindATraineeship
    }

    public enum ProgrammeLevel
    {

        Unknown = 0,
        Intermediate = 2,
        Advanced = 3,
        Higher = 4,
        FoundationDegree = 5,
        Degree = 6,
        Masters = 7
    }

    public enum DurationUnit
    {
        Month,
        Year
    }

    public class BlockedOrganisationSummary
    {
        public string BlockedOrganisationId { get; set; }
        public DateTime BlockedDate { get; set; }
        public string BlockedByUser { get; set; }
    }

    public class VacancySummaryProjection
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public long? VacancyReference { get; set; }
        public long? LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string EmployerAccountId { get; set; }
        public string EmployerName { get; set; }
        public long? Ukprn { get; set; }
        public DateTime? CreatedDate { get; set; }
        public VacancyStatus Status { get; set; }
        public DateTime? ClosingDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public int? Duration { get; set; }
        public DurationUnit DurationUnit { get; internal set; }
        public ApplicationMethod? ApplicationMethod { get; set; }
        public string ProgrammeId { get; set; }
        public DateTime? StartDate { get; set; }
        public string TrainingTitle { get; set; }
        public TrainingType TrainingType { get; set; }
        public ProgrammeLevel TrainingLevel { get; set; }
        public int NoOfNewApplications { get; set; }
        public int NoOfSuccessfulApplications { get; set; }
        public int NoOfUnsuccessfulApplications { get; set; }
    }

    public class EmployerDashboard : QueryProjectionBase
    {
        public EmployerDashboard() : base(QueryViewType.EmployerDashboard.TypeName)
        {
        }

        public IEnumerable<VacancySummaryProjection> Vacancies { get; set; }
    }

    public class ProviderDashboard : QueryProjectionBase
    {
        public ProviderDashboard() : base(QueryViewType.ProviderDashboard.TypeName)
        {
        }

        public IEnumerable<VacancySummaryProjection> Vacancies { get; set; }
    }

    public enum ApplicationReviewStatus
    {
        New,
        Successful,
        Unsuccessful,
        Interviewing,
        InReview
    }

    public enum ApplicationReviewDisabilityStatus
    {
        Unknown = 0,
        No,
        PreferNotToSay,
        Yes
    }

    public class VacancyApplication
    {
        public Guid CandidateId { get; set; }
        public DateTime SubmittedDate { get; set; }
        public ApplicationReviewStatus Status { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Guid ApplicationReviewId { get; set; }
        public ApplicationReviewDisabilityStatus DisabilityStatus { get; set; }
        public bool IsWithdrawn { get; set; }
    }

    public class VacancyApplications : QueryProjectionBase
    {
        public VacancyApplications() : base(QueryViewType.VacancyApplications.TypeName)
        {
        }

        public long VacancyReference { get; set; }
        public List<VacancyApplication> Applications { get; set; }
    }

    public class BlockedEmployerOrganisations : QueryProjectionBase
    {
        public BlockedEmployerOrganisations() : base(QueryViewType.BlockedEmployerOrganisations.TypeName)
        {}

        public IEnumerable<BlockedOrganisationSummary> Data { get; set; }
    }

    public class BlockedProviderOrganisations : QueryProjectionBase
    {
        public BlockedProviderOrganisations() : base(QueryViewType.BlockedProviderOrganisations.TypeName)
        {}

        public IEnumerable<BlockedOrganisationSummary> Data { get; set; }
    }
}