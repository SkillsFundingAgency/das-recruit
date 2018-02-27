using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace Esfa.Recruit.Vacancies.Jobs
{
    public class GenerateVacancyNumberJob
    {
        public async Task DoSomethingOnATimer([TimerTrigger("0/10 * * * * *", RunOnStartup = false)] TimerInfo timerInfo, TextWriter log)
        {
            Console.WriteLine("We're doing something");
            await Task.CompletedTask;
        }

    }
}