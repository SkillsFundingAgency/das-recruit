using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Newtonsoft.Json.Linq;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Reports
{
    public class CsvBuilder : ICsvBuilder
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";

        public void WriteCsvToStream(Stream stream, JArray rows, DateTime reportDate, Func<string,ReportDataType> dataTypeResolver)
        {
            var streamWriter = new StreamWriter(stream, Encoding.UTF8);
            
            using (var csv = GetCsvWriter(streamWriter))
            {
                if (rows.Count == 0)
                {
                    WriteNoResults(csv, reportDate);
                }
                else
                {
                    WriteHeader(csv, rows.First);
                    WriteValues(csv, rows, dataTypeResolver);
                }
            }

            streamWriter.Flush();
            stream.Position = 0;
        }

        private static CsvWriter GetCsvWriter(TextWriter streamWriter)
        {
            var csv = new CsvWriter(streamWriter, true);

            csv.Configuration.CultureInfo = CultureInfo.GetCultureInfo("en-GB");

            return csv;
        }

        private void WriteNoResults(CsvWriter csv, DateTime reportDate)
        {
            csv.WriteField("Date");
            csv.WriteField("Total_Number_Of_Applications");
            csv.NextRecord();

            csv.WriteField(reportDate.ToString(DateTimeFormat));
            csv.WriteField(0);
            csv.NextRecord();
        }

        private void WriteHeader(CsvWriter csv, JToken row)
        {
            csv.WriteField("PROTECT");
            for (var i = 1; i < row.Children().Count(); i++)
            {
                csv.WriteField("");
            }
            csv.NextRecord();

            foreach (var field in row.Children())
            {
                if (field is JProperty property)
                {
                    csv.WriteField(property.Name);
                }
            }
            csv.NextRecord();
        }

        private void WriteValues(CsvWriter csv, JArray rows, Func<string, ReportDataType> dataTypeResolver)
        {
            foreach (var row in rows)
            {
                foreach (var field in row.Children())
                {
                    if(field is JProperty property)
                    { 
                        WriteField(csv, property, dataTypeResolver);
                    }
                }
                csv.NextRecord();
            }
        }

        private void WriteField(CsvWriter csv, JProperty field, Func<string, ReportDataType> dataTypeResolver)
        {
            var format = dataTypeResolver(field.Name);

            string value;
            switch (format)
            {
                case ReportDataType.DateType:
                    value = field.Value.Value<DateTime>().ToString(DateFormat);
                    break;
                default:
                    value = field.Value.Value<string>();
                    break;
            }

            csv.WriteField(value);
        }
    }
}
