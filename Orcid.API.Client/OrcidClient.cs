using Orcid.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcid.API.Client
{
    public class OrcidClient
    {
        public string OrcidApiBaseUrl { get; set; }
        public string UserAuthorizationToken { get; set; }
        public string UserOrcid { get; set; }
        public OrcidClient(string orcidApiBaseUrl, string userAuthorizationToken, string userOrcid)
        {
            OrcidApiBaseUrl = orcidApiBaseUrl;
            UserAuthorizationToken = userAuthorizationToken;
            UserOrcid = userOrcid;
        }

        public OrcidAccessTokenDetails GetAccessToken(string OrcidBaseUrl, string orcidReturnCode, string OrcidClientSecret,string OrcidClientId, string OrcidClientRequestUri) {


            // Create a Rest Client API object
            var client = new RestClient($"{OrcidBaseUrl}/oauth/token");
            var request = new RestRequest(Method.POST); // set the method to Post

            // Add the necessary headers
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-type", "application/x-www-form-urlencoded");

            // add the parameters
            request.AddParameter("code", orcidReturnCode);
            request.AddParameter("client_secret", OrcidClientSecret);
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("client_id", OrcidClientId);
            request.AddParameter("redirect_uri", OrcidClientRequestUri);

            // request the auth token from ORCID, and receive the response back.
            IRestResponse<OrcidAccessTokenDetails> response = client.Execute<OrcidAccessTokenDetails>(request);

            if (!response.IsSuccessful)
            {
                // if the response is invalid, throw an exception
                // TODO imporove
                throw new Exception($"{response.StatusCode}, {response.Content}");
            }

            // extract the token
            return response.Data;
        }

        /// <summary>
        /// Get the User Orcid Record
        /// </summary>
        /// <param name="UserOrcid"></param>
        /// <returns></returns>
        public Orcid.Models.record GetUserRecord() {

            // Create a Rest Client API object
            var client = new RestClient($"{OrcidApiBaseUrl}/{UserOrcid}/record");
            var request = new RestRequest(Method.GET); // set the method to Post

            // Add the necessary headers
            request.AddHeader("Accept", "application/vnd.orcid+xml");
            request.AddHeader("Authorization", $"Bearer {UserAuthorizationToken}");

            // request the auth token from ORCID, and receive the response back.
            IRestResponse<Orcid.Models.record> response = client.Execute<Orcid.Models.record>(request);

            return response.Data;

        }
    }
}
