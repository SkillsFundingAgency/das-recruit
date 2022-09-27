using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AngleSharp.Css;
using CsvHelper;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Reports
{
    public class CsvBuilder : ICsvBuilder
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";

        public void WriteCsvToStream(
            Stream stream,
            JArray rows,
            IEnumerable<KeyValuePair<string, string>> headers,
            Func<string,ReportDataType> dataTypeResolver)
        {
            var streamWriter = new StreamWriter(stream, Encoding.UTF8);
            
            using (var csv = GetCsvWriter(streamWriter))
            {
                if (rows.Count == 0)
                {
                    WriteNoResults(csv, headers);
                }
                else
                {
                    WriteHeader(csv, rows.First, headers);
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

        private void WriteNoResults(CsvWriter csv, IEnumerable<KeyValuePair<string, string>> headers)
        {
            WriteTotalHeader(csv, headers);
        }

        private void WriteTotalHeader(
            CsvWriter csv,
            IEnumerable<KeyValuePair<string, string>> headers)
        {
            headers = headers ?? new KeyValuePair<string, string>[0];
            csv.WriteField("OFFICIAL");
            csv.NextRecord();

            csv.NextRecord();
            csv.NextRecord();

            if (headers.Any())
            {
                foreach (var kvp in headers)
                    csv.WriteField(kvp.Key);
                csv.NextRecord();

                foreach (var kvp in headers)
                    csv.WriteField(kvp.Value);
                csv.NextRecord();
            }
        }

        private void WriteHeader(
            CsvWriter csv,
            JToken row,
            IEnumerable<KeyValuePair<string, string>> headers)
        {
            var fieldCount = row.Children().Count();

            WriteTotalHeader(csv, headers);
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
                case ReportDataType.DateTimeType:
                    value = field.Value.Value<DateTime>().ToString(DateTimeFormat);
                    break;
                case ReportDataType.StringType:
                    value = field.Value.Value<string>();
                    break;
                case ReportDataType.ArrayType:
                    if (!field.Value.Value<JArray>().Any())
                    {
                        value = "";
                        break;
                    }
                    
                    value = field.Value.Value<JArray>()
                        .Select(c => JsonConvert.DeserializeObject<ReviewField>(c.ToString()).FieldIdentifier)
                        .ToDelimitedString("|");
                    break;
                default:
                    throw new NotImplementedException(format.ToString());
            }

            csv.WriteField(value);
        }

        private class ReviewField
        {
            public string FieldIdentifier { get; set; }
        }
    }
}

