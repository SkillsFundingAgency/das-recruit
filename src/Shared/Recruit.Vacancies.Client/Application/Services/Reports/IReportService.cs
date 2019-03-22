﻿using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Services.Reports
{
    public interface IReportService
    {
        Task GenerateReportAsync(Guid reportId);
        void WriteReportAsCsv(Stream stream, Report report);
        Task IncrementReportDownloadCountAsync(Guid reportId);
    }
}