namespace Esfa.Recruit.Qa.Web.Models.WithdrawVacancy
{
    public enum PostFindVacancyEditModelResultType
    {
        NotFound,
        AlreadyClosed,
        CanClose,
        NotLive
    }

    public class PostFindVacancyEditModelResult
    {
        public long? VacancyReference { get; set; }
        public PostFindVacancyEditModelResultType ResultType { get; set; }
    }
}
