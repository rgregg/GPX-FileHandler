//----------------------------------------------------------------------------------------------
//    Copyright 2014 Microsoft Corporation
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//----------------------------------------------------------------------------------------------

using System;
using System.IdentityModel.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using MVCO365Demo.Models;
using MVCO365Demo.Utils;
using Owin;
using System.Diagnostics;

namespace MVCO365Demo
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    Authority = AuthHelper.Default.GenerateAuthorityUrl(),
                    ClientId = AuthHelper.Default.ClientId,                    
					ClientSecret = AuthHelper.Default.ClientAppKey,
                    ResponseType = "code id_token",
                    Resource = AuthHelper.Default.MicrosoftGraphResourceUri,
                    PostLogoutRedirectUri = "/",
                    TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                    {
                        // instead of using the default validation (validating against a single issuer value, as we do in line of business apps (single tenant apps)), 
                        // we turn off validation
                        //
                        // NOTE:
                        // * In a multitenant scenario you can never validate against a fixed issuer string, as every tenant will send a different one.
                        // * If you don’t care about validating tenants, as is the case for apps giving access to 1st party resources, you just turn off validation.
                        // * If you do care about validating tenants, think of the case in which your app sells access to premium content and you want to limit access only to the tenant that paid a fee, 
                        //       you still need to turn off the default validation but you do need to add logic that compares the incoming issuer to a list of tenants that paid you, 
                        //       and block access if that’s not the case.
                        // * Refer to the following sample for a custom validation logic: https://github.com/AzureADSamples/WebApp-WebAPI-MultiTenant-OpenIdConnect-DotNet

                        ValidateIssuer = false
                    },
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        SecurityTokenValidated = (context) =>
                        {
                            // If your authentication logic is based on users then add your logic here
                            return Task.FromResult(0);
                        },
                        AuthenticationFailed = (context) =>
                        {
                            // Pass in the context back to the app
                            string message = Uri.EscapeDataString(context.Exception.Message);
                            context.OwinContext.Response.Redirect("/Home/Error?msg=" + message);
                            context.HandleResponse(); // Suppress the exception
                            return Task.FromResult(0);
                        },
                        AuthorizationCodeReceived = async (context) =>
                        {
                            var code = context.Code;

                            var authHelper = AuthHelper.Default;

                            string signInUserId = context.AuthenticationTicket.Identity.FindFirst(AuthHelper.ObjectIdentifierClaim).Value;

                            var authContext = authHelper.GetAuthContext("common", signInUserId);
                            var appCredential = authHelper.GetClientCredential();

                            // Returns an accessToken with aud: https://graph.microsoft.com, and a refreshToken
                            AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(
                                authorizationCode: context.Code,
                                redirectUri: new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), 
                                clientCredential: appCredential, 
                                resource: authHelper.MicrosoftGraphResourceUri);



                            var authContext2 = authHelper.GetAuthContext(result.TenantId, signInUserId);

                            try
                            {
                                var tokenResult = await authContext2.AcquireTokenSilentAsync(authHelper.MicrosoftGraphResourceUri,
                                    appCredential, 
                                    new UserIdentifier(signInUserId, UserIdentifierType.UniqueId));
                                Debug.WriteLine($"Slient token: {tokenResult.AccessToken}");
                            }
                            catch (Exception ex)
                            {
                                // Exception is thrown indicating I can’t acquire a token silently

                                Debug.WriteLine($"Life is unexpected: {ex.Message}");
                            }
                        },

                        RedirectToIdentityProvider = (context) =>
                        {
                            // This ensures that the address used for sign in and sign out is picked up dynamically from the request
                            // this allows you to deploy your app (to Azure Web Sites, for example)without having to change settings
                            // Remember that the base URL of the address used here must be provisioned in Azure AD beforehand.
                            string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                            context.ProtocolMessage.RedirectUri = appBaseUrl + "/";
                            context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;

                            // Save the form in the cookie to prevent it from getting lost in the login redirect
                            FormDataCookie cookie = new FormDataCookie(AuthHelper.SavedFormDataName);
                            cookie.SaveRequestFormToCookie();

                            return Task.FromResult(0);
                        }
                    }
                });
        }
    }
}
