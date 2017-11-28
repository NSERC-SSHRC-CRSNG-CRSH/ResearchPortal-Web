using System;
using System.Linq;
using System.Web.Mvc;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.AspNet.Identity;
using Orcid.API.Client;
using Orcid.Models;
using ResearchPortal.Entities;
using Microsoft.Xrm.Sdk.Messages;
using System.ServiceModel;
using Adxstudio.Xrm;

namespace ResearchPortal.Web.Areas.Orcid
{
    /// <summary>
    /// The Orcid endpoint controller
    /// </summary>
    public class OrcidController : Controller
    {
        // ORCID Webiste Settings Keys for the ORCID API.
        private const string orcidOAuthUrlKey = "ResearchPortal/ORCID/API/OAuthUrl";
        private const string orcidApiKey = "ResearchPortal/ORCID/API/BaseUrl";
        private const string orcidClientIdKey = "ResearchPortal/ORCID/API/ClientId";
        private const string orcidClientSecretKey = "ResearchPortal/ORCID/API/ClientSecret";
        private const string orcidClientRequestUriKey = "ResearchPortal/ORCID/API/RequestUri";

        protected string OrcidOAuthUrl { get; set; }
        protected string OrcidApiBaseUrl { get; set; }


        protected string OrcidClientId { get; set; }
        protected string OrcidClientSecret { get; set; }
        protected string OrcidClientRequestUri { get; set; }

        protected string UserOrcid { get; set; }
        protected string UserAuthorizationToken { get; set; }
        protected IOrganizationService service { get; set; }

        protected CrmUser xrmUser { get; set; }

        protected OrcidClient OrcidClient { get; set; }

        public bool InitializeOrcidController()
        {
            // Get the CRM user from the xrm Portal HttpContext
            xrmUser = HttpContext.GetUser();
            if (xrmUser == null || xrmUser == CrmUser.Anonymous)
            {
                return false;
            }


            OrcidOAuthUrl = HttpContext.GetWebsite().Settings.Get<string>(orcidOAuthUrlKey);
            if (OrcidOAuthUrl.EndsWith("/"))
            {
                OrcidOAuthUrl = OrcidOAuthUrl.Substring(0, OrcidOAuthUrl.Length - 1);
            }

            OrcidClientId = HttpContext.GetWebsite().Settings.Get<string>(orcidClientIdKey);
            OrcidClientSecret = HttpContext.GetWebsite().Settings.Get<string>(orcidClientSecretKey);
            OrcidClientRequestUri = Request.Url.GetLeftPart(UriPartial.Authority) + "/ResearchPortal/Orcid";

            OrcidApiBaseUrl = HttpContext.GetWebsite().Settings.Get<string>(orcidApiKey);


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
            return true;
        }

        /// <summary>
        /// Indext Get endpoint
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            // if the user is anonymus or null, redirect them to the signin page
            if (!InitializeOrcidController())
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

            OrcidAccessTokenDetails accessToken = OrcidClient.GetAccessToken(OrcidOAuthUrl, orcidReturnCode, OrcidClientSecret, OrcidClientId, OrcidClientRequestUri);

            // Create the entity contact to update.
            Entity contact = new Entity(xrmUser.ContactId.LogicalName);
            contact.Id = xrmUser.ContactId.Id;

            contact["rp2_orcid"] = accessToken.OrcidId;
            DateTime utcExpiry = DateTime.UtcNow.AddSeconds(Convert.ToInt32(accessToken.TokenExpiryDate));
            contact["rp2_orcidutcexpiry"] = utcExpiry;
            contact["rp2_orcidaccesstoken"] = accessToken.AccessToken;
            contact["rp2_orcid"] = accessToken.OrcidId;


            UserAuthorizationToken = accessToken.AccessToken;
            UserOrcid = accessToken.OrcidId;
            OrcidClient = new OrcidClient(OrcidApiBaseUrl, UserAuthorizationToken, UserOrcid);

            record record = OrcidClient.GetUserRecord();

            if (string.IsNullOrEmpty(xrmUser.FirstName) && !string.IsNullOrEmpty(record?.person?.name?.givennames?.Value))
            {
                contact["firstname"] = record.person.name.givennames.Value;
            }
            if (string.IsNullOrEmpty(xrmUser.LastName) && !string.IsNullOrEmpty(record?.person?.name?.familyname?.Value))
            {
                contact["lastname"] = record.person.name.familyname.Value;
            }

            service.Update(contact);

            return Redirect("/profile/");
        }


        /// <summary>
        /// Retrieve the user Orcid Profile
        /// </summary>
        /// <returns></returns>
        [Route("ResearchPortal/Orcid/RetrieveOrcidProfile")]
        [HttpGet()]
        public ActionResult RetrieveOrcidProfile()
        {
            // if the user is anonymus or null, redirect them to the signin page

            if (!InitializeOrcidController())
            {
                // TODO give a better error around not already being signed in.
                return Redirect("/SignIn/");
            }
            if (string.IsNullOrEmpty(UserAuthorizationToken))
            {
                // user authorization token is not defined
            }

            record userRecord = OrcidClient.GetUserRecord();

            // Create the entity contact to update.
            Contact contact = new Contact();
            contact.Id = xrmUser.ContactId.Id;
            contact.rp2_OricdBiography = userRecord?.person?.biography?.content;

            service = HttpContext.GetOrganizationService();

            service.Update(contact);

            if (userRecord.activitiessummary == null)
            {
                return Redirect("/profile/orcid/");
            }
            ExecuteMultipleRequest mRequest = new ExecuteMultipleRequest();
            mRequest.Settings = new ExecuteMultipleSettings();
            mRequest.Settings.ContinueOnError = true;
            mRequest.Settings.ReturnResponses = false;
            mRequest.Requests = new OrganizationRequestCollection();

            employments userEmployments = OrcidClient.GetUserOrcidData<employments>(userRecord.activitiessummary.employments.path);
            if (userEmployments?.employmentsummary != null)
            {
                foreach (employmentsummary es in userEmployments.employmentsummary)
                {
                    rp2_employment employment = new rp2_employment("rp2_sourceidentifier", es.path);

                    employment.rp2_Person = xrmUser.ContactId;
                    employment.rp2_Department = es?.departmentname;
                    employment.rp2_OrganizationNameText = es?.organization?.name;
                    employment.rp2_RoleTitle = es.roletitle;
                    employment.rp2_City = es.organization?.address?.city;
                    employment.rp2_Country = es.organization?.address?.country.ToString();
                    employment.rp2_StateProvince = es.organization?.address?.region;
                    employment.rp2_EndDate = es.enddate?.ToDateTime();
                    employment.rp2_StartDate = es.startdate?.ToDateTime();

                    //service.Create(employment);
                    UpsertRequest cRq = new UpsertRequest();
                    cRq.Target = employment;
                    mRequest.Requests.Add(cRq);
                }
            }
            if (mRequest.Requests.Count > 0)
            {
                ExecuteMultipleResponse response = service.Execute(mRequest) as ExecuteMultipleResponse;
                // TODO validate resonse
                var errorFaults = response.Responses.Where(r => r.Fault != null);
                if (errorFaults.Any())
                {
                    string errorMessages = "{" + string.Join("}, {", errorFaults.Select(f => f.Fault.Message)) + "}";
                    throw new Exception(errorMessages);
                }
            }
            return Redirect("/profile/orcid/");
        }
    }
}