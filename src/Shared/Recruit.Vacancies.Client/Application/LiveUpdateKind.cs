using System;
using System.Text.Json.Serialization;

namespace Esfa.Recruit.Vacancies.Client.Application
{
    [Flags, JsonConverter(typeof(JsonStringEnumConverter)), Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum LiveUpdateKind
    {
        None,
        ClosingDate,
        StartDate
    }
}