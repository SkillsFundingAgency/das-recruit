using System.Collections.Generic;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays;

public class Event
{
    public string Title { get; set; }
    public string Date { get; set; }
    public string Notes { get; set; }
    public bool Bunting { get; set; }

}

public class Data
{
    public string Division { get; set; }
    public List<Event> Events { get; set; }
}

public class BankHolidaysData
{
    [JsonProperty("england-and-wales")]
    public Data EnglandAndWales { get; set; }
    [JsonProperty("scotland")]
    public Data Scotland { get; set; }
    [JsonProperty("northern-ireland")]
    public Data NorthernIreland { get; set; }
}