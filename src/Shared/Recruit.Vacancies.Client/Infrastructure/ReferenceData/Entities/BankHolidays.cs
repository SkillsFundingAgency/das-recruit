using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Entities
{
    public class BankHolidays
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
            [BsonElement("england-and-wales")]
            public EnglandAndWales EnglandAndWales { get; set; }
            [BsonElement("scotland")]
            public Scotland Scotland { get; set; }
            [BsonElement("northern-ireland")]
            public NorthernIreland NorthernIreland { get; set; }
        }
    }

    
}
