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
    public class OrcidController : Controller
    {
        // ORCID Webiste Settings Keys for the ORCID API.
        private const string orcidBaseUrlKey = "ResearchPortal/ORCID/API/BaseUrl";
        private const string orcidClientIdKey = "ResearchPortal/ORCID/API/ClientId";
        private const string orcidClientSecretKey = "ResearchPortal/ORCID/API/ClientSecret";
        private const string orcidClientRequestUriKey = "ResearchPortal/ORCID/API/RequestUri";

        [HttpGet]
        public ActionResult Index()
        {
            CrmUser user = HttpContext.GetUser();

            if (user == null || user == CrmUser.Anonymous)
            {
                // TODO give a better error around not already being signed in.
                return Redirect("/SignIn/");
            }

            IOrganizationService service = HttpContext.GetOrganizationService();

            string orcidBaseUrl = HttpContext.GetWebsite().Settings.Get<string>(orcidBaseUrlKey);
            string orcidClientId = HttpContext.GetWebsite().Settings.Get<string>(orcidClientIdKey);
            string orcidClientSecret = HttpContext.GetWebsite().Settings.Get<string>(orcidClientSecretKey);
            string orcidClientRequestUri = Request.Url.GetLeftPart(UriPartial.Authority) + "/ResearchPortal/Orcid";

            if (!Request.QueryString.AllKeys.Contains("code"))
            {
                // TODO give a better error around not already being signed in.
                return Redirect("/profile/");
            }
            string orcidReturnCode = Request.QueryString["code"];

            if (orcidBaseUrl.EndsWith("/"))
            {
                orcidBaseUrl = orcidBaseUrl.Substring(0, orcidBaseUrl.Length - 1);
            }

            System.Diagnostics.Debug.Assert(orcidClientSecret == "af5d4a77-8061-4c64-bd7e-fbaa31d3a04f");
            System.Diagnostics.Debug.Assert(orcidClientId == "APP-MU2GDO5Z9E42DFNK");

            var client = new RestClient($"{orcidBaseUrl}/oauth/token");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-type", "application/x-www-form-urlencoded");
            request.AddParameter("code", orcidReturnCode);
            request.AddParameter("client_secret", orcidClientSecret);
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("client_id", orcidClientId);
            request.AddParameter("redirect_uri", orcidClientRequestUri);

            IRestResponse<OrcidAccessTokenDetails> response = client.Execute<OrcidAccessTokenDetails>(request);
            
            if (!response.IsSuccessful)
            {
                throw new Exception($"{response.StatusCode}, {response.Content}");
            }

            OrcidAccessTokenDetails accessToken = response.Data;

            Entity contact = new Entity(user.ContactId.LogicalName);
            contact.Id = user.ContactId.Id;
            contact["rp2_orcid"] = accessToken.OrcidId;
            DateTime utcExpiry = DateTime.UtcNow.AddSeconds(Convert.ToInt32(accessToken.TokenExpiryDate));
            contact["rp2_orcidutcexpiry"] = utcExpiry;
            contact["rp2_orcidaccesstoken"] = accessToken.AccessToken;
            contact["rp2_orcid"] = accessToken.OrcidId;
            service.Update(contact);


            return Redirect("/profile/");
        }
    }
}