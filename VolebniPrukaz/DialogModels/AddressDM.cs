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
    }

    public static class AddressHelpers
    {
        public static AddressDM MapGeocodeToAddressDM(this Result geocodeResult)
        {
            return new AddressDM
            {
                City = geocodeResult.address_components.FirstOrDefault(a => a.types.Contains("locality") || a.types.Contains("sublocality"))?.short_name,
                Street = geocodeResult.address_components.FirstOrDefault(a => a.types.Contains("route"))?.long_name,
                HouseNumber = geocodeResult.address_components.FirstOrDefault(a => a.types.Contains("street_number"))?.long_name,
                Zip = geocodeResult.address_components.FirstOrDefault(a => a.types.Contains("postal_code"))?.long_name?.Replace(" ", "")
            };
        }

        public static string ToAddressString(this AddressDM address)
        {
            return $"{address.Street} {address.HouseNumber}, {address.Zip} {address.City}, Česká republika";
        }
    }
}