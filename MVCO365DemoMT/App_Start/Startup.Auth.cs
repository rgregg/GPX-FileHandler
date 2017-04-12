﻿//----------------------------------------------------------------------------------------------
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
                    ClientId = AADAppSettings.ClientId,
                    Authority = AADAppSettings.Authority,

                    // ProtocolValidator = new OpenIdConnectProtocolValidator()
                    // {
                    //    RequireNonce = false,
                    // },
                    Resource = "https://graph.microsoft.com",
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
                        //If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away. 
                        AuthorizationCodeReceived = (context) =>
                        {
                            var code = context.Code;
                            ClientCredential credential = new ClientCredential(AADAppSettings.ClientId, AADAppSettings.AppKey);

                            string tenantId = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
                            string signInUserId = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;

                            var authContext = new AuthenticationContext(AADAppSettings.Authority, 
                                new InMemoryTokenCache(signInUserId));

                            // Get the access token for AAD Graph. Doing this will also initialize the token cache associated with the authentication context
                            // In theory, you could acquire token for any service your application has access to here so that you can initialize the token cache
                            AuthenticationResult result = authContext.AcquireTokenByAuthorizationCode(code, 
                                new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), 
                                credential, 
                                AADAppSettings.AADGraphResourceId);

                            Console.WriteLine($"Authenticated user unique ID: {result.UserInfo.UniqueId}");

                            return Task.FromResult(0);
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
                            FormDataCookie cookie = new FormDataCookie(AADAppSettings.SavedFormDataName);
                            cookie.SaveRequestFormToCookie();

                            return Task.FromResult(0);
                        }

                        // AuthenticationFailed = (context) =>
                        // {
                        //    // Suppress the exception if you don't want to see the error
                        //   // context.HandleResponse();
                        //    return Task.FromResult(0);
                        // }
                    }

                });
        }
    }
}
