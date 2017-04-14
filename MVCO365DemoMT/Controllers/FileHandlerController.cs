using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security.OpenIdConnect;
using MVCO365Demo.Models;
using MVCO365Demo.Utils;

namespace MVCO365Demo.Controllers
{
    [Authorize]
    public class FileHandlerController : Controller
    {
        private GPXHelper gpxUtils = new GPXHelper();
        public static readonly string DocumentKey = "XML_DOCUMENT_KEY";

        public async Task<ActionResult> Preview()
        {
            var input = GetActivationParameters();
            var viewModel = await GetGPXViewModelAsync(input, readOnly: true);
            return View(viewModel);
        }

        public async Task<ActionResult> Open()
        {
            var input = GetActivationParameters();
            var viewModel = await GetGPXViewModelAsync(input, readOnly: false);
            return View(viewModel);
        }

        public async Task<ActionResult> Save(string newName)
        {
            //grab all the stuff you need to get a token
            //var signInUserId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            //var userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            //var tenantId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;

            var authContext = AuthHelper.Default.GetAuthContext(ClaimsPrincipal.Current);
            //AuthenticationContext authContext = new AuthenticationContext(AADAppSettings.Authority, InMemoryTokenCache.TokenCacheForUser(signInUserId));

            //change the name in the file
            gpxUtils = Session[DocumentKey] as GPXHelper;
            gpxUtils.setTitle(newName);

            Stream fileStream = null;
            try
            {
                //grab activation parameters (this was set in Open controller)
                ActivationParameters parameters = Session[AuthHelper.SavedFormDataName] as ActivationParameters;

                var token = await AuthHelper.Default.GetUserAccessTokenSilentAsync(AuthHelper.Default.MicrosoftGraphResourceUri, ClaimsPrincipal.Current);

                //create request to write file back to server
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(parameters.FilePut);
                request.Headers.Add("Authorization: bearer " + token);
                request.Method = "PUT";

                //write bytes to stream
                byte[] xmlByteArray = Encoding.Default.GetBytes(gpxUtils.doc.OuterXml);
                fileStream = request.GetRequestStream();
                fileStream.Write(xmlByteArray, 0, xmlByteArray.Length);

                //send file over & respond as the workload does
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return new HttpStatusCodeResult(response.StatusCode);
            }
            catch (AdalException exception)
            {
                //handle token acquisition failure
                if (exception.ErrorCode == AdalError.FailedToAcquireTokenSilently)
                {
                    authContext.TokenCache.Clear();
                }
                return new HttpStatusCodeResult(HttpStatusCode.ExpectationFailed);
            }
            catch (WebException webException)
            {
                //something funky happened in the web response - return as needed
                HttpWebResponse response = (HttpWebResponse)webException.Response;
                return new HttpStatusCodeResult(response.StatusCode);
            }
            finally
            {
                //close the file stream
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }
        }

        private ActivationParameters GetActivationParameters()
        {
            ActivationParameters parameters;

            FormDataCookie cookie = new FormDataCookie(AuthHelper.SavedFormDataName);
            if (Request.Form != null && Request.Form.AllKeys.Count<string>() != 0)
            {
                // get from current request's form data
                parameters = new ActivationParameters(Request.Form);
            }
            else if (cookie.Load() && cookie.IsLoaded && cookie.FormData.AllKeys.Count<string>() > 0)
            {
                // if form data does not exist, it must be because of the sign in redirection, at the time form data is saved in the cookie 
                parameters = new ActivationParameters(cookie.FormData);
                // clear the cookie after using it
                cookie.Clear();
            }
            else
            {
                parameters = (ActivationParameters)Session[AuthHelper.SavedFormDataName];
            }
            return parameters;
        }

        private async Task<GPXFileViewModel> GetGPXViewModelAsync(ActivationParameters parameters, bool readOnly = false)
        {
            if (null == parameters)
            {
                return GPXFileViewModel.GetErrorModel(null, "Activation parameters were missing. Please try to launch the previewer from OneDrive or SharePoint again.");
            }

            string accessToken = await AuthHelper.Default.GetUserAccessTokenSilentAsync(parameters.ResourceId, ClaimsPrincipal.Current);
            if (accessToken == null)
            {
                return GPXFileViewModel.GetErrorModel(parameters, "Unable to retrieve an access token from AAD.");
            }

            // Get file content
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(parameters.FileGet);
                request.Headers.Add("Authorization", $"Bearer {accessToken}");
                Stream responseStream = request.GetResponse().GetResponseStream();

                bool complete = gpxUtils.LoadGPXFromStream(responseStream);
                if (complete)
                {
                    return new GPXFileViewModel(parameters)
                    {
                        Coordinates = gpxUtils.getPointsFromGPX(),
                        ReadOnly = readOnly,
                        SignedInUserName = ClaimsPrincipal.Current.FindFirst(ClaimTypes.GivenName).Value + " " + ClaimsPrincipal.Current.FindFirst(ClaimTypes.Surname).Value,
                        Title = gpxUtils.getTitle()
                   };
                }
                else
                {
                    return GPXFileViewModel.GetErrorModel(parameters, "Unable to parse GPX data file.");
                }
            }
            catch (Exception ex)
            {
                return GPXFileViewModel.GetErrorModel(parameters, ex);
            }
        }

    }
}