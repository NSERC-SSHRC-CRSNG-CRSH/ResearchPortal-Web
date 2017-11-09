using System.Web.Mvc;

namespace ResearchPortal.Web.Areas.Orcid
{
    public class OrcidAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Orcid";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Orcid_default",
                "Orcid/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );

            context.MapRoute(
              "ResearchPortal_Orcid",
              "ResearchPortal/Orcid",
              new { pageless = true, area = "ResearchPortal", controller = "Orcid", action = "Index" });

        }
    }
}