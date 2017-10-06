using Google.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VolebniPrukaz.DialogModels
{
    [Serializable]
    public class AddressDM
    {
        public string City { get; set; }

        public string Zip { get; set; }

        public string Street { get; set; }

        public string HouseNumber { get; set; }

        public string Country { get; set; }
    }

    public static class AddressHelpers
    {
        public static AddressDM MapGeocodeToAddressDM(this Result geocodeResult)
        {
            var address = new AddressDM
            {
                City = geocodeResult.address_components.FirstOrDefault(a => a.types.Contains("locality") || a.types.Contains("sublocality"))?.short_name,
                Street = geocodeResult.address_components.FirstOrDefault(a => a.types.Contains("route"))?.long_name,
                HouseNumber = geocodeResult.address_components.FirstOrDefault(a => a.types.Contains("street_number"))?.long_name,
                Zip = geocodeResult.address_components.FirstOrDefault(a => a.types.Contains("postal_code"))?.long_name?.Replace(" ", ""),
                Country = geocodeResult.address_components.FirstOrDefault(a => a.types.Contains("country"))?.long_name
            };

            if (string.IsNullOrEmpty(address.Street))
            {
                address.Street = geocodeResult.address_components.FirstOrDefault(a => a.types.Contains("locality"))?.long_name;
                address.HouseNumber = geocodeResult.address_components.FirstOrDefault(a => a.types.Contains("premise"))?.long_name;
            }

            if (string.IsNullOrEmpty(address.HouseNumber))
                address.HouseNumber = geocodeResult.address_components.FirstOrDefault(a => a.types.Contains("premise"))?.long_name;

            return address;
        }

        public static string ToAddressString(this AddressDM address)
        {
            return $"{address.Street} {address.HouseNumber}, {address.Zip} {address.City}, {address.Country}";
        }
    }
}