using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CreateReportCommandHandler : IRequestHandler<CreateReportCommand>
    {
        private readonly ILogger<CreateReportCommandHandler> _logger;
        private readonly IReportRepository _repository;
        private readonly ITimeProvider _timeProvider;
        private readonly IReportsQueue _reportQueue;

        public CreateReportCommandHandler(
            ILogger<CreateReportCommandHandler> logger,
            IReportRepository repository,
            ITimeProvider timeProvider,
            IReportsQueue reportQueue)
        {
            _logger = logger;
            _repository = repository;
            _timeProvider = timeProvider;
            _reportQueue = reportQueue;
        }

        public async Task Handle(CreateReportCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating report '{reportType}' with parameters '{reportParameters}' requested by {userId}", message.ReportType, message.Parameters, message.RequestedBy.UserId);

            var now = _timeProvider.Now;

            var report = new Report(
                message.ReportId,
                message.Owner,
                ReportStatus.New,
                message.ReportName,
                message.ReportType,
                message.Parameters,
                message.RequestedBy,
                now);

            await _repository.CreateAsync(report);

            await _reportQueue.AddAsync(report.Id);

            _logger.LogInformation("Finished create report '{reportType}' with parameters '{reportParameters}' requested by {userId}", message.ReportType, message.Parameters, message.RequestedBy.UserId);
        }
    }
}
