using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Esfa.Recruit.Shared.Web.Middleware
{
    /// <summary>
    /// Custom Exception Handler Middleware. Handles RecruitException errors without logging an error and redirects to the exceptionHandlingPath
    /// </summary>
    public class RecruitExceptionHandlerMiddleware
    {
        //This class is modified version of: https://github.com/aspnet/AspNetCore/blob/0c9eda1f4a551df52f83c7a9b378b4ae8f02e0b5/src/Middleware/Diagnostics/src/ExceptionHandler/ExceptionHandlerMiddleware.cs

        private readonly RequestDelegate _next;
        private readonly string _exceptionHandlingPath;
        private readonly Func<object, Task> _clearCacheHeadersDelegate;

        public RecruitExceptionHandlerMiddleware(RequestDelegate next, string exceptionHandlingPath)
        {
            _next = next;
            _exceptionHandlingPath = exceptionHandlingPath;
            _clearCacheHeadersDelegate = ClearCacheHeaders;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (RecruitException ex)
            {
                if (context.Response.HasStarted)
                {
                    throw;
                }

                PathString originalPath = context.Request.Path;
                
                context.Request.Path = _exceptionHandlingPath;
                
                try
                {
                    context.Response.Clear();
                    var exceptionHandlerFeature = new ExceptionHandlerFeature()
                    {
                        Error = ex,
                        Path = originalPath.Value,
                    };
                    context.Features.Set<IExceptionHandlerFeature>(exceptionHandlerFeature);
                    context.Features.Set<IExceptionHandlerPathFeature>(exceptionHandlerFeature);
                    context.Response.StatusCode = 500;
                    context.Response.OnStarting(_clearCacheHeadersDelegate, context.Response);

                    await _next(context);
                }
                finally
                {
                    context.Request.Path = originalPath;
                }
            }
        }

        private Task ClearCacheHeaders(object state)
        {
            var response = (HttpResponse)state;
            response.Headers[HeaderNames.CacheControl] = "no-cache";
            response.Headers[HeaderNames.Pragma] = "no-cache";
            response.Headers[HeaderNames.Expires] = "-1";
            response.Headers.Remove(HeaderNames.ETag);
            return Task.CompletedTask;
        }
    }
}