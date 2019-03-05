using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Net.Http.Headers;
using MongoDB.Bson.Serialization.Attributes;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays
{
    public static class MySerializer
    {
        public static string Serialize(this object obj)
        {
            var serializer = new DataContractJsonSerializer(obj.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                return Encoding.Default.GetString(ms.ToArray());
            }
        }
    }

    public class BankHolidays : IReferenceDataItem
    {
        public string Id { get; set; }
        public DateTime LastUpdatedDate { get; set; } 
        public BankHolidaysData Data { get; set; }
        public string Etag { get; set; }

        public class Event
        {
            public string Title { get; set; }
            public string Date { get; set; }
            public string Notes { get; set; }
            public bool Bunting { get; set; }

        }

        public class EnglandAndWales
        {
            public string Division { get; set; }
            public List<Event> Events { get; set; }
        }

        public class Scotland
        {
            public string Division { get; set; }
            public List<Event> Events { get; set; }
        }

        public class NorthernIreland
        {
            public string Division { get; set; }
            public List<Event> Events { get; set; }
        }

        public class BankHolidaysData
        {
            [BsonElement("england-and-wales")]
            public EnglandAndWales EnglandAndWales { get; set; }
            [BsonElement("scotland")]
            public Scotland Scotland { get; set; }
            [BsonElement("northern-ireland")]
            public NorthernIreland NorthernIreland { get; set; }
        }
    }

    
}
