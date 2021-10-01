using MediatR;

namespace SFA.DAS.Recruit.Api.Queries
{
    public class GetApplicantsQuery : IRequest<GetApplicantsResponse>
    {
        public long VacancyReference { get; }
        public string ApplicantApplicationOutcomeFilter { get; }

        public GetApplicantsQuery(long vacancyReference, string applicantApplicationOutcomeFilter)
        {
            VacancyReference = vacancyReference;
            ApplicantApplicationOutcomeFilter = applicantApplicationOutcomeFilter;
        }
    }
}