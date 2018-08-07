using System;

namespace McbeevCommerceBot.Models
{
    public class KenticoRestServiceSettings
    {
        public string RestUserName { get; set; }
        public string RestUserPassword { get; set; }
        public string SiteUrlBase { get; set; }

        public KenticoRestServiceSettings()
        {

        }

        public KenticoRestServiceSettings(string siteUrlBase, string userName, string userPassword)
        {
            SiteUrlBase = siteUrlBase;
            RestUserName = userName;
            RestUserPassword = userPassword;
        }
    }
}
