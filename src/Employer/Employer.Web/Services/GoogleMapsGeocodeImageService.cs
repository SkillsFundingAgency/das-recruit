using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class GoogleMapsGeocodeImageService : IGeocodeImageService
    {

        private string _privateKey { get; set; }

        public GoogleMapsGeocodeImageService(string privateKey)
        {
            _privateKey = privateKey;
        }

        public string GetMapImageUrl(string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode))
            {
                return null;
            }
            
            var staticMapsUrl = $"https://maps.googleapis.com/maps/api/staticmap?markers={WebUtility.UrlEncode(postcode)}&size=190x125&zoom=12";
            var url = SignUrl(staticMapsUrl);
            return url;
        }

        public string GetMapImageUrl(string latitude, string longitude)
        {
            if (string.IsNullOrWhiteSpace(latitude) || string.IsNullOrWhiteSpace(longitude))
            {
                return null;
            }

            var staticMapsUrl = $"https://maps.googleapis.com/maps/api/staticmap?markers={latitude},{longitude}&size=190x125&zoom=12";
            var url = SignUrl(staticMapsUrl);
            return url;
        }

        private string SignUrl(string url)
        {
            var privateKeyBytes = Convert.FromBase64String(_privateKey);

            url += "&client=gme-skillsfundingagency";
            var uri = new Uri(url);

            var encoding = new ASCIIEncoding();
            var encodedPathAndQueryBytes = encoding.GetBytes(uri.LocalPath + uri.Query);

            // compute the hash
            var algorithm = new HMACSHA1(privateKeyBytes);
            var hash = algorithm.ComputeHash(encodedPathAndQueryBytes);

            // convert the bytes to string and make url-safe by replacing '+' and '/' characters
            var signature = Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_");

            // Add the signature to the existing URI.
            return uri.Scheme + "://" + uri.Host + uri.LocalPath + uri.Query + "&signature=" + signature;
        }
    }
}
