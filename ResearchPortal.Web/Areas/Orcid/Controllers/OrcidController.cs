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
using Orcid.API.Client;
using Orcid.Models;

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
        protected string OrcidApiBaseUrl { get; set; }


        protected string OrcidClientId { get; set; }
        protected string OrcidClientSecret { get; set; }
        protected string OrcidClientRequestUri { get; set; }

        protected string UserOrcid { get; set; }
        protected string UserAuthorizationToken { get; set; }
        protected IOrganizationService service { get; set; }

        protected CrmUser xrmUser { get; set; }

        protected OrcidClient OrcidClient { get; set; }

        public void InitializeOrcidController()
        {

            OrcidBaseUrl = HttpContext.GetWebsite().Settings.Get<string>(orcidBaseUrlKey);
            if (OrcidBaseUrl.EndsWith("/"))
            {
                OrcidBaseUrl = OrcidBaseUrl.Substring(0, OrcidBaseUrl.Length - 1);
            }

            OrcidClientId = HttpContext.GetWebsite().Settings.Get<string>(orcidClientIdKey);
            OrcidClientSecret = HttpContext.GetWebsite().Settings.Get<string>(orcidClientSecretKey);
            OrcidClientRequestUri = Request.Url.GetLeftPart(UriPartial.Authority) + "/ResearchPortal/Orcid";

            OrcidApiBaseUrl = "https://api.sandbox.orcid.org/v2.0";

            // Get the CRM user from the xrm Portal HttpContext
            xrmUser = HttpContext.GetUser();

            service = HttpContext.GetOrganizationService();

            Entity userContact = service.Retrieve(xrmUser.ContactId.LogicalName, xrmUser.ContactId.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("rp2_orcid", "rp2_orcidaccesstoken"));
            if (userContact.Contains("rp2_orcid"))
            {
                UserOrcid = userContact["rp2_orcid"].ToString();
            }

            if (userContact.Contains("rp2_orcidaccesstoken"))
            {
                UserAuthorizationToken = userContact["rp2_orcidaccesstoken"].ToString();
            }


            OrcidClient = new OrcidClient(OrcidApiBaseUrl, UserAuthorizationToken, UserOrcid);

        }

        /// <summary>
        /// Indext Get endpoint
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            InitializeOrcidController();
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

            OrcidAccessTokenDetails accessToken = OrcidClient.GetAccessToken(OrcidBaseUrl, orcidReturnCode, OrcidClientSecret, OrcidClientId, OrcidClientRequestUri);

            // Create the entity contact to update.
            Entity contact = new Entity(xrmUser.ContactId.LogicalName);
            contact.Id = xrmUser.ContactId.Id;

            contact["rp2_orcid"] = accessToken.OrcidId;
            DateTime utcExpiry = DateTime.UtcNow.AddSeconds(Convert.ToInt32(accessToken.TokenExpiryDate));
            contact["rp2_orcidutcexpiry"] = utcExpiry;
            contact["rp2_orcidaccesstoken"] = accessToken.AccessToken;
            contact["rp2_orcid"] = accessToken.OrcidId;

            record record = OrcidClient.GetUserRecord();

            
            string[] names = accessToken.Name.Split(' ');
            if (string.IsNullOrEmpty(xrmUser.FirstName) && names.Length >= 1)
            {
                contact["firstname"] = names.FirstOrDefault();
            }
            if (string.IsNullOrEmpty(xrmUser.LastName) && names.Length >= 2)
            {
                contact["lastname"] = names.LastOrDefault();
            }

            service.Update(contact);

            return Redirect("/profile/");
        }

        [Route("ResearchPortal/Orcid/RetrieveOrcidDetails")]
        [HttpGet()]
        public ActionResult RetrieveOrcidDetails()
        {
            InitializeOrcidController();
            // if the user is anonymus or null, redirect them to the signin page
            if (xrmUser == null || xrmUser == CrmUser.Anonymous)
            {
                // TODO give a better error around not already being signed in.
                return Redirect("/SignIn/");
            }

            // Create the entity contact to update.
            Entity contact = new Entity(xrmUser.ContactId.LogicalName);
            contact.Id = xrmUser.ContactId.Id;
            service = HttpContext.GetOrganizationService();
            service.Update(contact);

            return Redirect("/profile");
        }
    }
}