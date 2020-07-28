using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace AspNet.Security.OAuth.Esia
{
    internal class EsiaClientSecret
    {
        public EsiaClientSecret(EsiaAuthenticationOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));

            //if (Options.ClientCertificate == null)
            //    throw new ArgumentException("Client certificate must be provided.");
        }

        public EsiaAuthenticationOptions Options { get; }

        public string State { get; private set; }
        public string Timestamp { get;  private set; }
        public string Scope { get;  private set; }
        public string Secret { get; private set; }

        public async Task<string> GenerateClientSecretAsync()
        {
            State = Options.State.ToString("D");
            Timestamp = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss zz00");
            Scope = FormatScope(Options.Scope);

            string signMessage = Scope + Timestamp + Options.ClientId + State;
            return await SignMessageAsync(signMessage);
            //Secret = Base64UrlEncoder.Encode(encodedSignature);
            //Secret = EsiaHelpers.Base64UrlEncode(encodedSignature);
            return Secret;
        }
        public async Task<string> GetSecretData(string redirectUri)
        {
            var requestParam = new Dictionary<string, string>
            {
                { "clientId", Options.SubClientId },
                { "redirectUrl", redirectUri }
            };
            HttpClient httpClient = new HttpClient();
            string url = QueryHelpers.AddQueryString(Options.RedirectUrlServiceEndpoint, requestParam);
            //string url = "http://85.233.74.78:8080/esia/v1/redirectUrl?clientId=989fbc1c-9691-4804-b73b-f2e135dc7b0f&redirectUrl=" + HttpUtility.UrlEncode("http://localhost:5000/signin-esia?data=") + data.Values.First();
            Uri uri = new Uri(string.Format(url));
            var authUrl = JsonDocument.Parse(await httpClient.GetStringAsync(uri));
            var authorizationEndpoint = authUrl.RootElement.GetProperty("esiaUrl").GetString();
            var secretData = QueryHelpers.ParseQuery(authorizationEndpoint);
            State = secretData["state"];
            Timestamp = secretData["timestamp"];
            Scope = secretData["scope"];
            return secretData["client_secret"];
        }
        private async Task<string> SignMessageAsync(string message)
        {
            HttpClient httpClient = new HttpClient();
            string url = "http://85.233.74.77:8080/crypto/signPkcs7?client="+ Options.ClientId + "&content=" + HttpUtility.UrlEncode(message).ToUpper();
            Uri uri = new Uri(string.Format(url));
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string data = JsonConvert.SerializeObject(new { client = "577005", content = "test" });
            StringContent content = new StringContent(string.Empty, Encoding.UTF8);
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error");
            }
            string signMessage = await response.Content.ReadAsStringAsync();

            //httpClient.DefaultRequestHeaders.Accept.Clear();
            //httpClient.DefaultRequestHeaders.Accept.Add(
            //new MediaTypeWithQualityHeaderValue("application/json"));
            //var result = await httpClient.PostAsync("http://85.233.74.77:8080/crypto/signPkcs7");
            //var signedCms = new SignedCms(new ContentInfo(message), true);
            //var cmsSigner = new CmsSigner(Options.ClientCertificate);
            //signedCms.ComputeSignature(cmsSigner);
            return signMessage;
        }

        private static string FormatScope(IEnumerable<string> scopes)
        {
            return string.Join(" ", scopes);
        }
    }
}
