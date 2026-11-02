using System.Web;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetUserAccountsRequest : IGetApiRequest
{
    private readonly string _userId;
    private readonly string _email;

    public GetUserAccountsRequest(string userId, string email)
    {
        _userId = userId;
        _email = HttpUtility.UrlEncode(email);
    }

    public string GetUrl => $"accountusers/{_userId}/accounts?email={_email}";
}