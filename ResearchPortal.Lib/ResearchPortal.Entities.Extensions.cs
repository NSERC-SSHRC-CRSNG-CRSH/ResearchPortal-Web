using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ResearchPortal;
namespace ResearchPortal.Entities
{
    public partial class rp2_employment
    {
        public rp2_employment(string keyName, object value) : base(EntityLogicalName, keyName, value)
        { }
    }

    public partial class Account
    {
        public Account(string keyName, object value) : base(EntityLogicalName, keyName, value)
        { }
      
    }
    public partial class Contact
    {
        public Contact(string keyName, object value) : base(EntityLogicalName, keyName, value)
        {

        }

        public string Last_FirstName
        {
            get
            {
                return FullName;
            }
            set
            {
                var names = value.Split(',').Select(s => s.Trim());
                if (names.Count() == 0)
                {
                    LastName = "";
                    FirstName = "";
                }
                if (names.Count() >= 1)
                {
                    LastName = names.FirstOrDefault();
                }
                if (names.Count() >= 2)
                {
                    FirstName = names.ElementAt(1);
                }
            }
        }
    }
    public partial class rp2_fundingopportunity
    {
        public rp2_fundingopportunity(string keyName, object value) : base(EntityLogicalName, keyName, value)
        { }

    }
    public partial class rp2_fundingcycle
    {
        public rp2_fundingcycle(string keyName, object value) : base(EntityLogicalName, keyName, value)
        { }


        public string rp2_FundingOpportunity_Code
        {
            get
            {
                return this.GetLookupKeyAttribute("rp2_fundingopportunity", "rp2_code").ToString();
            }
            set
            {
                this.SetLookupKeyAttribute(ResearchPortal.Entities.rp2_fundingopportunity.EntityLogicalName, "rp2_fundingopportunity", "rp2_code", value);
            }
        }
    }

    public partial class rp2_application
    {
        public rp2_application(string keyName, object value) : base(EntityLogicalName, keyName, value)
        { }


        public string rp2_PrimaryAppilcant_Identifier
        {
            get
            {
                return this.GetLookupKeyAttribute("rp2_primaryapplicant", "rp2_identifier").ToString();
            }
            set
            {
                this.SetLookupKeyAttribute(Contact.EntityLogicalName, "rp2_primaryapplicant", "rp2_identifier", value);
            }
        }
        public string rp2_Organization_Code
        {
            get
            {
                return this.GetLookupKeyAttribute("rp2_administratingorganization", "rp2_code").ToString();
            }
            set
            {   this.SetLookupKeyAttribute(ResearchPortal.Entities.Account.EntityLogicalName, "rp2_administratingorganization", "rp2_code", value);
            }
        }

        public string rp2_FundingOpportunity_Code
        {
            get
            {
                return this.GetLookupKeyAttribute("rp2_fundingopportunity", "rp2_code").ToString();
            }
            set
            {
                this.SetLookupKeyAttribute(ResearchPortal.Entities.rp2_fundingopportunity.EntityLogicalName, "rp2_fundingopportunity", "rp2_code", value);
            }
        }
        public string rp2_FundingCycle_Code
        {
            get
            {
                return this.GetLookupKeyAttribute("rp2_fundingcycle", "rp2_code").ToString();
            }
            set
            {
                this.SetLookupKeyAttribute(ResearchPortal.Entities.rp2_fundingcycle.EntityLogicalName, "rp2_fundingcycle", "rp2_code", value);
            }
        }
    }


    public partial class rp2_award
    {
        public rp2_award(string keyName, object value) : base(EntityLogicalName, keyName, value)
        { }
        /// <summary>
        /// The short unique identifier that is assigned to the application
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("rp2_identifier")]
        public string rp2_Identifier
        {
            get
            {
                return this.GetAttributeValue<string>("rp2_identifier");
            }
            set
            {
                this.OnPropertyChanging("rp2_Identifier");
                this.SetAttributeValue("rp2_identifier", value);
                this.OnPropertyChanged("rp2_Identifier");
            }
        }

        public string rp2_Application_Identifier
        {
            get
            {
                return this.GetLookupKeyAttribute("rp2_application", "rp2_identifier").ToString();
            }
            set
            {
                this.SetLookupKeyAttribute(rp2_application.EntityLogicalName, "rp2_application", "rp2_identifier", value);
            }
        }
        public string rp2_PrimaryAwardee_Identifier
        {
            get
            {
                return this.GetLookupKeyAttribute("rp2_primaryawardee", "rp2_identifier").ToString();
            }
            set
            {
                this.SetLookupKeyAttribute(Contact.EntityLogicalName, "rp2_primaryawardee", "rp2_identifier", value);
            }
        }
        public string rp2_Organization_Code
        {
            get
            {
                return this.GetLookupKeyAttribute("rp2_organization", "rp2_code").ToString();
            }
            set
            {
                this.SetLookupKeyAttribute(ResearchPortal.Entities.Account.EntityLogicalName, "rp2_organization", "rp2_code", value);
            }
        }
    }
}