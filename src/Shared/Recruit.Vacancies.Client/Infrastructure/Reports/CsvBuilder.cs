using System;
using System.IO;
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
            
            using (var csv = new CsvWriter(streamWriter, true))
            {
                csv.WriteField("Date");
                csv.WriteField("Total_Number_Of_Applications");
                csv.NextRecord();

                csv.WriteField(reportDate.ToString(DateTimeFormat));
                csv.WriteField(rows.Count);
                csv.NextRecord();

                if (rows.Count > 0)
                {
                    WriteHeader(csv, rows.First);
                    WriteValues(csv, rows, dataTypeResolver);
                }
            }

            streamWriter.Flush();
            stream.Position = 0;
        }

        private void WriteHeader(CsvWriter csv, JToken row)
        {
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
