using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Reports
{
    public interface ICsvBuilder
    {
        void WriteCsvToStream(Stream stream, JArray rows, DateTime reportDate, Func<string, ReportDataType> formatResolver);
    }
}