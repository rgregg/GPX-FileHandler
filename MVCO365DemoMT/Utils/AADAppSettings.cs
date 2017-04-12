using System;
using System.Configuration;
using System.Security.Claims;

namespace MVCO365Demo.Utils
{
    public static class AADAppSettings
    {

        private static string _clientId = ConfigurationManager.AppSettings["ida:ClientId"] ?? ConfigurationManager.AppSettings["ida:ClientID"];
        private static string _appKey = ConfigurationManager.AppSettings["ida:AppKey"] ?? ConfigurationManager.AppSettings["ida:Password"];

        private static string _graphResourceId = ConfigurationManager.AppSettings["ida:GraphResourceId"];
        private static string _authority = ConfigurationManager.AppSettings["ida:authority"];

        //private static string _consentUri = _authority + "oauth2/authorize?response_type=code&client_id={0}&resource={1}&redirect_uri={2}";
        //private static string _adminConsentUri = _authority +"oauth2/authorize?response_type=code&client_id={0}&resource={1}&redirect_uri={2}&prompt={3}";

        //private static string _discoverySvcResourceId = ConfigurationManager.AppSettings["ida:DiscoverySvcResourceId"];
        //private static string _discoverySvcEndpointUri = ConfigurationManager.AppSettings["ida:DiscoverySvcEndpointUri"];

        public const string SavedFormDataName = "FILEHANDLER_FORMDATA";

        static AADAppSettings()
        {
            System.Diagnostics.Debug.Assert(!_clientId.Equals("[clientId]"), "You must specify an AAD application client-id before running this project.");
            System.Diagnostics.Debug.Assert(!_appKey.Equals("[secret]"), "You must specify the AAD application app key before running this project.");
        }

        public static string ClientId
        {
            get
            {
                return _clientId;
            }
        }

        public static string AppKey
        {
            get
            {
                return _appKey;
            }
        }

        //public static string AuthorizationUri
        //{
        //    get
        //    {
        //        return _authorizationUri;
        //    }
        //}

        public static string Authority
        {
            get
            {
                return _authority;
            }
        }

        public static string AADGraphResourceId
        {
            get
            {
                return _graphResourceId;
            }
        }

        //public static string AdminConsentUri
        //{
        //    get
        //    {
        //        return _adminConsentUri;
        //    }
        //}


        //public static string ConsentUri
        //{
        //    get
        //    {
        //        return _consentUri;
        //    }
        //}
    }
}
