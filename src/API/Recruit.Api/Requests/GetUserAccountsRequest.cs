using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using System.Web;

namespace SFA.DAS.Recruit.Api.Requests;

public class GetUserAccountsRequest : IGetApiRequest
{
    private readonly string _userId;
    private readonly string _email;

    public GetUserAccountsRequest(string baseUrl, string userId, string email)
    {
        BaseUrl = baseUrl;
        _userId = HttpUtility.UrlEncode(userId);
        _email = HttpUtility.UrlEncode(email);
    }

    public string GetUrl => $"{BaseUrl}/accountusers/{_userId}/accounts?email={_email}";
    public string BaseUrl { get; }
}