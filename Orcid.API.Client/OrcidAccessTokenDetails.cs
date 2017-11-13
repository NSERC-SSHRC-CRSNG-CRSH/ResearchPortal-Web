using RestSharp.Deserializers;
using System.Runtime.Serialization;

namespace Orcid.Models
{
    [DataContract ]
    public class OrcidAccessTokenDetails
    {
        [DataMember(Name = "orcid")]
        [DeserializeAs(Name = "orcid")]
        public  string OrcidId { get; set; }

        [DataMember(Name = "Name")]
        [DeserializeAs(Name = "Name")]
        public string Name { get; set; }

        [DataMember(Name = "access_token")]
        [DeserializeAs(Name = "access_token")]
        public string AccessToken { get; set; }

        [DataMember(Name = "token_type")]
        [DeserializeAs(Name = "token_type")]
        public string TokenType { get; set; }

        [DataMember(Name = "refresh_token")]
        [DeserializeAs(Name = "refresh_token")]
        public string RefreshToken { get; set; }

        [DataMember(Name = "expires_in")]
        [DeserializeAs(Name = "expires_in")]
        public string TokenExpiryDate { get; set; }
  
    }
}