using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.BankHoliday
{
    public class BankHolidayJob
    {
        private readonly ILogger<BankHolidayJob> _logger;
        private readonly IBankHolidayService _bankHolidaysService;

        public BankHolidayJob(ILogger<BankHolidayJob> logger, IBankHolidayService bankHolidaysService)
        {
            _logger = logger;
            _bankHolidaysService = bankHolidaysService;
        }

        public async Task UpdateBankHolidays([TimerTrigger(Schedules.MidnightDaily, RunOnStartup = true)]
            TimerInfo timerInfo, TextWriter log)
        {
            _logger.LogInformation("Starting updating Bank Holidays ReferenceData");

            await _bankHolidaysService.UpdateBankHolidaysAsync();

            _logger.LogInformation("Finished updating Bank Holidays ReferenceData");
        }
    }
}

