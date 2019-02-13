using Microsoft.WindowsAzure.Storage.Table;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.TableStore
{
    public class QueryEntity : TableEntity
    {
        public QueryEntity(string viewType, string documentId, string jsonData) : base(viewType, documentId)
        {
            JsonData = jsonData;
        }

        public QueryEntity() { }
        public string JsonData { get; set; }
    }
}