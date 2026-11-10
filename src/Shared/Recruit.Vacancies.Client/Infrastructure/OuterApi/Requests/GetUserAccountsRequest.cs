using System.Web;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetUserAccountsRequest(string userId, string email) : IGetApiRequest
{
    private readonly string _email = HttpUtility.UrlEncode(email);

    public string GetUrl => $"accountusers/{userId}/accounts?email={_email}";
}