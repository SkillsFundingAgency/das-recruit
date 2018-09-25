using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    /// <summary>
    /// provides a list of words to be ignored when comparing content for similarity
    /// </summary>
    public interface IStopWordListProvider
    {
        IEnumerable<string> GetStopWords();
    }
}
