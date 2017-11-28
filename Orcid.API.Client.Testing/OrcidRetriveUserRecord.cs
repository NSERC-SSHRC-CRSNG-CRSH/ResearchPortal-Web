using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orcid.Models;
using System.Linq;
namespace Orcid.API.Client.Testing
{
    [TestClass]
    public class OrcidRetriveUserRecord
    {
        [TestMethod]
        public void SaveClientSecretAndId()
        {
            // Uncomment and Enter you client id and secret
            // run the test then undo your changes.
            // this is to help ensure that credentials are not stored in source control.
            //Properties.Settings.Default.OrcidClientId = "";
            //Properties.Settings.Default.OrcidClientSecret = "";
            //Properties.Settings.Default.UserAuthorizatoinToken = "";
            //Properties.Settings.Default.UserOrcidId = "";
            // Properties.Settings.Default.Save();
        }

        [TestMethod]
        public void RetrieveRecord()
        {
            OrcidClient client = new OrcidClient(Properties.Settings.Default.OrcidAPIBaseUrl, Properties.Settings.Default.UserAuthorizatoinToken, Properties.Settings.Default.UserOrcidId);

            record userRecord = client.GetUserRecord();
            Assert.IsNotNull(userRecord);
        }

        [TestMethod]
        public void RetrieveEmployments()
        {
            OrcidClient client = new OrcidClient(Properties.Settings.Default.OrcidAPIBaseUrl, Properties.Settings.Default.UserAuthorizatoinToken, Properties.Settings.Default.UserOrcidId);

            record userRecord = client.GetUserRecord();
            Assert.IsNotNull(userRecord);
            educations userEmployments = client.GetUserOrcidData<educations>(userRecord.activitiessummary.educations.path);

            Assert.IsNotNull(userEmployments);

        }

        [TestMethod]
        public void RetrieveEducations()
        {
            OrcidClient client = new OrcidClient(Properties.Settings.Default.OrcidAPIBaseUrl, Properties.Settings.Default.UserAuthorizatoinToken, Properties.Settings.Default.UserOrcidId);

            record userRecord = client.GetUserRecord();
            Assert.IsNotNull(userRecord);
            employments userEmployments = client.GetUserOrcidData<employments>(userRecord.activitiessummary.employments.path);

            Assert.IsNotNull(userEmployments);

        }

        [TestMethod]
        public void FuzzyDate()
        {
            string xml = @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?><activities:educations path='/0000-0002-0455-4284/educations' xmlns:funding='http://www.orcid.org/ns/funding' xmlns:education='http://www.orcid.org/ns/education' xmlns:common='http://www.orcid.org/ns/common' xmlns:work='http://www.orcid.org/ns/work' xmlns:activities='http://www.orcid.org/ns/activities' xmlns:peer-review='http://www.orcid.org/ns/peer-review' xmlns:employment='http://www.orcid.org/ns/employment'><common:last-modified-date>2017-11-15T17:32:50.602Z</common:last-modified-date><education:education-summary put-code='29200' path='/0000-0002-0455-4284/education/29200' visibility='public'><common:created-date>2017-11-15T17:32:50.602Z</common:created-date><common:last-modified-date>2017-11-15T17:32:50.602Z</common:last-modified-date><common:source><common:source-orcid><common:uri>http://sandbox.orcid.org/0000-0002-0455-4284</common:uri><common:path>0000-0002-0455-4284</common:path><common:host>sandbox.orcid.org</common:host></common:source-orcid><common:source-name>Shawn Lautebach</common:source-name></common:source><education:department-name>Computer Science</education:department-name><education:role-title>Bachelor</education:role-title><common:start-date><common:year>2000</common:year><common:month>09</common:month><common:day>01</common:day></common:start-date><common:end-date><common:year>2005</common:year><common:month>12</common:month><common:day>31</common:day></common:end-date><education:organization><common:name>Carleton University</common:name><common:address><common:city>Ottawa</common:city><common:region>ON</common:region><common:country>CA</common:country></common:address><common:disambiguated-organization><common:disambiguated-organization-identifier>6339</common:disambiguated-organization-identifier><common:disambiguation-source>RINGGOLD</common:disambiguation-source></common:disambiguated-organization></education:organization></education:education-summary></activities:educations>";

            educations educations = Extensions.DeserializeXml<educations>(xml);
            educationsummary es1 = educations.educationsummary.FirstOrDefault();
            DateTime? start = es1.startdate.ToDateTime();
            Assert.IsNotNull(start);
        }
    }
}
