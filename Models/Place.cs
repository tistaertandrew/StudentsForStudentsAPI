using Org.BouncyCastle.Asn1.Ocsp;
using StudentsForStudentsAPI.Models.ViewModels;
using System.Xml.Linq;

namespace StudentsForStudentsAPI.Models
{
    public class Place
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public int PostalCode { get; set; }
        public string Number { get; set; }
        public string Locality { get; set; }

        public static PlaceViewModel? CheckAddress(string fullAddress, IConfiguration config)
        {
            string? street = null;
            string? number = null;
            int postalCode = -1;
            string? locality = null;
            string requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?key={1}&address={0}&sensor=false", Uri.EscapeDataString(fullAddress), config["AppSettings:GoogleApiKey"]);

            using (var client = new HttpClient())
            {
                var resp = client.GetAsync(requestUri).Result;
                var content = resp.Content.ReadAsStringAsync().Result;
                var xml = XDocument.Parse(content);
                var elements = xml.Element("GeocodeResponse")?.Element("result")?.Elements("address_component");

                if (elements == null || !elements.Any()) return null;

                foreach (var element in elements)
                {
                    switch (element.Element("type")!.Value)
                    {
                        case "route":
                            street = element.Element("long_name")!.Value;
                            break;
                        case "street_number":
                            number = element.Element("long_name")!.Value;
                            break;
                        case "locality":
                            locality = element.Element("long_name")!.Value;
                            break;
                        case "postal_code":
                            postalCode = int.Parse(element.Element("long_name")!.Value);
                            break;
                    }
                }

                if (street == null || number == null || postalCode == -1 || locality == null)
                {
                    return null;
                }

                return new PlaceViewModel() { Street = street, Number = number, PostalCode = postalCode, Locality = locality };
            }
        }
    }
}
