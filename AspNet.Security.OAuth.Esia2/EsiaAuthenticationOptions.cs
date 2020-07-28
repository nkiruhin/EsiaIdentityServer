using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace AspNet.Security.OAuth.Esia
{
    public class EsiaAuthenticationOptions : OAuthOptions
    {
        public EsiaAuthenticationOptions()
        {
            CallbackPath = new PathString("/signin-esia");
            AuthorizationEndpoint = EsiaAuthenticationDefaults.AuthorizationEndpoint;
            TokenEndpoint = EsiaAuthenticationDefaults.TokenEndpoint;
            UserInformationEndpoint = EsiaAuthenticationDefaults.UserInformationEndpoint;

            Scope.Add(EsiaConstants.UserInformationScope);

            ClaimActions.MapJsonKey(ClaimTypes.DateOfBirth, "birthDate");
            ClaimActions.MapJsonKey(ClaimTypes.Gender, "gender");
            ClaimActions.MapJsonKey(ClaimTypes.GivenName, "firstName");
            ClaimActions.MapJsonKey(ClaimTypes.Surname, "lastName");
            ClaimActions.MapJsonKey(EsiaConstants.TrustedUrn, "trusted");
            ClaimActions.MapJsonKey(EsiaConstants.MiddleNameUrn, "middleName");
            ClaimActions.MapJsonKey(EsiaConstants.BirthPlaceUrn, "birthPlace");
            ClaimActions.MapJsonKey(EsiaConstants.CitizenshipUrn, "citizenship");
            ClaimActions.MapJsonKey(EsiaConstants.SnilsUrn, "snils");
            ClaimActions.MapJsonKey(EsiaConstants.InnUrn, "inn");
            ClaimActions.MapJsonSubKey("sub", "profile", "profile_id");
            //ClaimActions.MapCustomJson(ClaimTypes.Name, ParseName);
            //ClaimActions.MapCustomJson(ClaimTypes.Email, obj => ParseContactInfo(obj, "EML"));
            //ClaimActions.MapCustomJson(ClaimTypes.MobilePhone, obj => ParseContactInfo(obj, "MBT"));
            //ClaimActions.MapCustomJson(ClaimTypes.HomePhone, obj => ParseContactInfo(obj, "PHN"));
            //ClaimActions.MapCustomJson(ClaimTypes.OtherPhone, obj => ParseContactInfo(obj, "CPH"));
        }

        public string AccessType { get; set; } = "online";

        public Guid State { get; } = Guid.NewGuid();

        public string SignServiceEndpoint { get; set; }

        public string RedirectUrlServiceEndpoint { get; set; }

        public string SubClientId { get; set; }

        public X509Certificate2 ClientCertificate { get; set; }

        public bool FetchContactInfo { get; set; } = false;

        public override void Validate()
        {
            try
            {
                base.Validate();
            }
            catch (ArgumentException e) when (e.ParamName == nameof(ClientSecret))
            {
                // Do nothing
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static string ParseName(JObject obj)
        {
            string lastName = obj["lastName"]?.ToString();
            string firstName = obj["firstName"]?.ToString();
            string middleName = obj["middleName"]?.ToString();
            return string.Join(" ", new[] { lastName, firstName, middleName }.Where(x => !string.IsNullOrEmpty(x)));
        }

        private static string ParseContactInfo(JObject obj, string key)
        {
            return obj?.Value<JArray>("elements")?
                .FirstOrDefault(x => x["type"]?.ToString() == key)?["value"]?.ToString();
        }
    }
}
