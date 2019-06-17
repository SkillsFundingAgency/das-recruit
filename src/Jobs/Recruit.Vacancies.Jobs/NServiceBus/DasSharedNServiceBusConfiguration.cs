using System.Net;

namespace Esfa.Recruit.Vacancies.Jobs.NServiceBus
{
    public class DasSharedNServiceBusConfiguration
    {
        public string ConnectionString { get; set; }

        public string NServiceBusLicense
        {
            get => _decodedNServiceBusLicense ?? (_decodedNServiceBusLicense = WebUtility.HtmlDecode(_nServiceBusLicense));
            set => _nServiceBusLicense = value;
        }

        private string _nServiceBusLicense;
        private string _decodedNServiceBusLicense;
    }

}