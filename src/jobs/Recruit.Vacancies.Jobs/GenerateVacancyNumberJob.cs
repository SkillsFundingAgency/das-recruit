// using System;
// using System.IO;
// using System.Threading.Tasks;
// using Microsoft.Azure.WebJobs;
// using Microsoft.Extensions.Logging;

// namespace Esfa.Recruit.Vacancies.Jobs
// {
//     public class GenerateVacancyNumberJob
//     {
//         private readonly ILogger<GenerateVacancyNumberJob> _logger;

//         public GenerateVacancyNumberJob(ILogger<GenerateVacancyNumberJob> logger)
//         {
//             _logger = logger;
//         }

//         public async Task DoSomethingOnATimer([TimerTrigger("0/10 * * * * *", RunOnStartup = false)] TimerInfo timerInfo, TextWriter log)
//         {
//             _logger.LogInformation("Using logger to log......");
//             Console.WriteLine("We're doing something");
//             await Task.CompletedTask;
//         }

//     }
// }