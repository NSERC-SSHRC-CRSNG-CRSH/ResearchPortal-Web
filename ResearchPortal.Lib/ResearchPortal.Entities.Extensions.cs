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
        { }
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
                this.SetLookupKeyAttribute(rp2_FundingOpportunity.LogicalName, "rp2_fundingopportunity", "rp2_code", value);
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
                this.SetLookupKeyAttribute(rp2_FundingOpportunity.LogicalName, "rp2_primaryapplicant", "rp2_identifier", value);
            }
        }
    }


    public partial class rp2_award
    {
        public rp2_award(string keyName, object value) : base(EntityLogicalName, keyName, value)
        { }


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
    }
}