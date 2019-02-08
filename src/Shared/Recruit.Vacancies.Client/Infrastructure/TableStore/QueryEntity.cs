using Microsoft.WindowsAzure.Storage.Table;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.TableStore
{
    public class QueryEntity : TableEntity
    {
        public QueryEntity(string viewType, string documentId)
        {
            this.PartitionKey = viewType;
            this.RowKey = documentId;
        }

        public QueryEntity() { }
        public string JsonData { get; set; }
    }
}