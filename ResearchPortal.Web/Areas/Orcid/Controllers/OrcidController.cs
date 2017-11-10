using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.AspNet.Identity;
using System.Net.Http;
using System.Net.Http.Headers;
using RestSharp;
using Adxstudio.Xrm.Web;

namespace ResearchPortal.Web.Areas.Orcid
{
    /// <summary>
    /// The Orcid endpoint controller
    /// </summary>
    public class OrcidController : Controller
    {
        // ORCID Webiste Settings Keys for the ORCID API.
        private const string orcidBaseUrlKey = "ResearchPortal/ORCID/API/BaseUrl";
        private const string orcidClientIdKey = "ResearchPortal/ORCID/API/ClientId";
        private const string orcidClientSecretKey = "ResearchPortal/ORCID/API/ClientSecret";
        private const string orcidClientRequestUriKey = "ResearchPortal/ORCID/API/RequestUri";

        protected string OrcidBaseUrl { get; set; }


        protected string OrcidClientId { get; set; }
        protected string OrcidClientSecret { get; set; }
        protected string OrcidClientRequestUri { get; set; }

        protected string UserOrcid { get; set; }
        protected string UserAuthorizationToken { get; set; }


        protected CrmUser xrmUser { get; set; }
        public OrcidController()
        {
            // Get the CRM user from the xrm Portal HttpContext
            xrmUser = HttpContext.GetUser();

            UserOrcid = xrmUser.Entity["rp2_orcid"].ToString();
            UserAuthorizationToken = xrmUser.Entity["rp2_orcidaccesstoken"].ToString();

            OrcidBaseUrl = HttpContext.GetWebsite().Settings.Get<string>(orcidBaseUrlKey);
            if (OrcidBaseUrl.EndsWith("/"))
            {
                OrcidBaseUrl = OrcidBaseUrl.Substring(0, OrcidBaseUrl.Length - 1);
            }

            OrcidClientId = HttpContext.GetWebsite().Settings.Get<string>(orcidClientIdKey);
            OrcidClientSecret = HttpContext.GetWebsite().Settings.Get<string>(orcidClientSecretKey);
            OrcidClientRequestUri = Request.Url.GetLeftPart(UriPartial.Authority) + "/ResearchPortal/Orcid";

        }

        /// <summary>
        /// Indext Get endpoint
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            // if the user is anonymus or null, redirect them to the signin page
            if (xrmUser == null || xrmUser == CrmUser.Anonymous)
            {
                // TODO give a better error around not already being signed in.
                return Redirect("/SignIn/");
            }

            // if the Orcid code parameter does not exist, re-direct the user back to the profile page.
            if (!Request.QueryString.AllKeys.Contains("code"))
            {
                // TODO give a better error around not already being signed in.
                return Redirect("/profile/");
            }
            string orcidReturnCode = Request.QueryString["code"];


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
            OrcidAccessTokenDetails accessToken = response.Data;

            // Create the entity contact to update.
            Entity contact = new Entity(xrmUser.ContactId.LogicalName);
            contact.Id = xrmUser.ContactId.Id;

            contact["rp2_orcid"] = accessToken.OrcidId;
            DateTime utcExpiry = DateTime.UtcNow.AddSeconds(Convert.ToInt32(accessToken.TokenExpiryDate));
            contact["rp2_orcidutcexpiry"] = utcExpiry;
            contact["rp2_orcidaccesstoken"] = accessToken.AccessToken;
            contact["rp2_orcid"] = accessToken.OrcidId;

            string[] names = accessToken.Name.Split(' ');
            if (string.IsNullOrEmpty(xrmUser.FirstName) && names.Length >= 1)
            {
                contact["firstname"] = names.FirstOrDefault();
            }
            if (string.IsNullOrEmpty(xrmUser.LastName) && names.Length >= 2)
            {
                contact["lastname"] = names.LastOrDefault();
            }

            IOrganizationService service = HttpContext.GetOrganizationService();
            service.Update(contact);


            return Redirect("/profile/");
        }

        [Route("ResearchPortal/Orcid/RetrieveOrcidDetails")]
        [HttpGet()]
        public ActionResult RetrieveOrcidDetails()
        {
            //http://localhost:8020/ResearchPortal/Orcid/RetrieveOrcidDetails
            // if the user is anonymus or null, redirect them to the signin page
            if (xrmUser == null || xrmUser == CrmUser.Anonymous)
            {
                // TODO give a better error around not already being signed in.
                return Redirect("/SignIn/");
            }

            // Create a Rest Client API object
            var client = new RestClient($"{OrcidBaseUrl}/oauth/token");
            var request = new RestRequest(Method.GET); // set the method to Post

            // Add the necessary headers
            request.AddHeader("Accept", "application/vnd.orcid+xml");
            request.AddHeader("Authorization", $"Bearer {UserAuthorizationToken}");

            // request the auth token from ORCID, and receive the response back.
            IRestResponse response = client.Execute(request);

            // Create the entity contact to update.
            Entity contact = new Entity(xrmUser.ContactId.LogicalName);
            contact.Id = xrmUser.ContactId.Id;

            contact["description"] = response.Content;

            IOrganizationService service = HttpContext.GetOrganizationService();
            service.Update(contact);

            return Redirect("/profile");
        }
    }
}