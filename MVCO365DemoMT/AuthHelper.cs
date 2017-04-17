/*
 * Markdown File Handler - Sample Code
 * Copyright (c) Microsoft Corporation
 * All rights reserved. 
 * 
 * MIT License
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the ""Software""), to deal in 
 * the Software without restriction, including without limitation the rights to use, 
 * copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
 * Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

namespace MVCO365Demo
{
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Utils;
    using Models;
    using System;
    using System.Diagnostics;
    using System.Configuration;
    using System.Security.Claims;

    public class AuthHelper
    {
        private static AuthHelper StaticAuthHelper = new AuthHelper();
        public static AuthHelper Default
        {
            get
            {
                return StaticAuthHelper;
            }
        }

        public string ClientId { get; private set; }
        public string ClientAppKey { get; private set; }
        public string AuthorityUrlTemplate { get; private set; }
        public string MicrosoftGraphResourceUri { get; private set; }

        private AuthHelper()
        {
            this.ClientId = ConfigurationManager.AppSettings["ida:ClientId"];
            this.ClientAppKey = ConfigurationManager.AppSettings["ida:AppKey"];
            this.AuthorityUrlTemplate = ConfigurationManager.AppSettings["ida:Authority"];
            this.MicrosoftGraphResourceUri = ConfigurationManager.AppSettings["ida:MicrosoftGraphResource"];

            Debug.Assert(!ClientId.Equals("[clientId]"), "You must specify an AAD application client-id before running this project.");
            Debug.Assert(!ClientAppKey.Equals("[secret]"), "You must specify the AAD application app key before running this project.");
        }

        public const string ObjectIdentifierClaim = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public const string SavedFormDataName = "FILEHANDLER_FORMDATA";

        public string GenerateAuthorityUrl(string tenantId = "common")
        {
            return string.Format(AuthorityUrlTemplate, tenantId);
        }

        /// <summary>
        /// Silently retrieve a new access token for the specified resource. If the request fails, null is returned.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task<string> GetUserAccessTokenSilentAsync(string resource, ClaimsPrincipal principal)
        {
            var signInUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userObjectId = principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            var tenantId = principal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

            var authContext = GetAuthContext(tenantId, signInUserId);
            var clientCredential = GetClientCredential();

            try
            {

                var authResult = await authContext.AcquireTokenSilentAsync(
                    resource,
                    clientCredential,
                    new UserIdentifier(userObjectId, UserIdentifierType.UniqueId));

                return authResult.AccessToken;
            }
            catch (AdalSilentTokenAcquisitionException ex)
            {
                Debug.WriteLine($"Error retriving accessToken silently for {signInUserId}: {ex.ToString()}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error occured retriving a tokenAccess for {signInUserId}: {ex.ToString()}");
            }

            return null;
        }

        public ClientCredential GetClientCredential()
        {
            return new ClientCredential(this.ClientId, this.ClientAppKey);
        }

        public AuthenticationContext GetAuthContext(string tenantId, string userUniqueId)
        {
            return new AuthenticationContext(GenerateAuthorityUrl(tenantId), TokenCache.DefaultShared);
        }

        public AuthenticationContext GetAuthContext(ClaimsPrincipal principal)
        {
            var signInUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var userObjectId = principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            var tenantId = principal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

            return GetAuthContext(tenantId, signInUserId);

        }

        public static string GetUserId()
        {
            var claim = System.Security.Claims.ClaimsPrincipal.Current.FindFirst(ObjectIdentifierClaim);
            if (null != claim)
            {
                return claim.Value;
            }

            throw new NullReferenceException("Unable to get the current user ID");
        }
    }
}