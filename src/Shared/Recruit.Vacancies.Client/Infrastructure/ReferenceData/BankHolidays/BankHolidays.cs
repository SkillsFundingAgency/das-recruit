using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays
{    
    public class BankHolidays : IReferenceDataItem
    {
        public string Id { get; set; }
        public DateTime LastUpdatedDate { get; set; } 
        public BankHolidaysData Data { get; set; }
       
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
            [JsonProperty("england-and-wales")]
            [BsonElement("england-and-wales")]
            public EnglandAndWales EnglandAndWales { get; set; }
            [BsonElement("scotland")]
            public Scotland Scotland { get; set; }
            [JsonProperty("northern-ireland")]
            [BsonElement("northern-ireland")]
            public NorthernIreland NorthernIreland { get; set; }
        }
    }

    
}
