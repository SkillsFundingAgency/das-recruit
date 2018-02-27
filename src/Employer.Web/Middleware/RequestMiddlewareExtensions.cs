using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Builder
{
    public static class RequestMiddlewareExtensions
    {
        public static IApplicationBuilder UseRootRedirect(this IApplicationBuilder builder, string redirectUrl)
        {
            return builder.Use(async (context, next) =>
            {
                if (context.Request.Path == "/")
                {
                    context.Response.Redirect(redirectUrl, true);
                    await Task.CompletedTask;
                }
                else
                {
                    await next.Invoke();
                }
            });
        }
    }
}