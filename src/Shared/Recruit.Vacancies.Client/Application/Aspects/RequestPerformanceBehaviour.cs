using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.Aspects
{
    public class RequestPerformanceBehaviour<TRequest, TResponse>(ILogger<TRequest> logger)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var timer = Stopwatch.StartNew();

            var response = await next();

            timer.Stop();

            var elapsedTime = timer.ElapsedMilliseconds;
            var name = typeof(TRequest).Name;

            logger.LogInformation("Command: {Name}, ElapsedTime: {ElapsedTime} milliseconds", name, elapsedTime);

            return response;
        }
    }
}
