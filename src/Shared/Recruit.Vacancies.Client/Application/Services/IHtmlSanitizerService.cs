namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IHtmlSanitizerService
    {
        string Sanitize(string html);
    }
}