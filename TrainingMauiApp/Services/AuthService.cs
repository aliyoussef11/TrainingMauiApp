using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using System.Web;
using System.Runtime.InteropServices;

namespace DotNetMauiApp.Services
{
    public class AuthService
    {
        private const string CallbackUrl = "myapp://callback";
        private const string Auth0ClientId = "YU4Dk68WnthD9GUmB2lXZdkoA5l8HU9l";
        private const string Auth0Domain = "dev-algybd55zd12gxzy.us.auth0.com";
        private const string API = "https://dev-algybd55zd12gxzy.us.auth0.com/api/v2/";
        private const string TokenEndpoint = $"https://{Auth0Domain}/oauth/token";

        public async Task AuthenticateAsync()
        {
            // Generate PKCE verifier and challenge
            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(codeVerifier);
            await SecureStorage.SetAsync("pkce_verifier", codeVerifier);

            string authUrl =
                $"https://{Auth0Domain}/authorize" +
                $"?client_id={Auth0ClientId}" +
                "&response_type=code" +
                "&scope=openid%20profile%20email" +
                $"&redirect_uri={CallbackUrl}" +
                $"&audience={API}" +
                $"&code_challenge={codeChallenge}" +
                "&code_challenge_method=S256" +
                "&prompt=login";

            try
            {
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    await Launcher.Default.OpenAsync(authUrl);
                    //await HandleWindowsAuthAsync(authUrl);
                }
                else
                {
                    await HandleNonWindowsAuthAsync(authUrl);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Auth failed: {ex.Message}");
            }
        }


//        private async Task HandleWindowsAuthAsync(string authUrl)
//        {
//#if WINDOWS
//            // 1) Build the URIs
//            var authUri = new Uri(authUrl);
//                // this will be something like "myapp://callback" — it must exactly match your manifest
//                var callbackUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri();

//                try
//                {
//                    // 2) Fire the broker
//                    var result = await WebAuthenticationBroker.AuthenticateAsync(
//                        WebAuthenticationOptions.None,
//                        authUri,
//                        callbackUri);

//                    // 3) Inspect the result
//                    switch (result.ResponseStatus)
//                    {
//                        case WebAuthenticationStatus.Success:
//                            // result.ResponseData is something like "myapp://callback?code=XYZ"
//                            var responseUri = new Uri(result.ResponseData);
//                            await HandleCallbackAsync(responseUri);
//                            break;

//                        case WebAuthenticationStatus.UserCancel:
//                            Debug.WriteLine("Authentication canceled by user.");
//                            break;

//                        case WebAuthenticationStatus.ErrorHttp:
//                            Debug.WriteLine($"HTTP error during auth: {result.ResponseErrorDetail}");
//                            break;
//                    }
//                }
//                catch (COMException comEx)
//                {
//                    // This will at least show you the HRESULT rather than a blank message
//                    Debug.WriteLine($"WebAuthBroker COM error 0x{comEx.HResult:X}: {comEx.Message}");
//                }
//                catch (Exception ex)
//                {
//                    Debug.WriteLine($"Unexpected error in WebAuthBroker: {ex}");
//                }
//#endif
//        }



        private async Task HandleNonWindowsAuthAsync(string authUrl)
        {
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(authUrl),
                new Uri(CallbackUrl));

            var queryParams = new Dictionary<string, string>();
            foreach (var kvp in authResult.Properties)
            {
                queryParams[kvp.Key] = kvp.Value;
            }

            var uriBuilder = new UriBuilder(CallbackUrl)
            {
                Query = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"))
            };

            await HandleCallbackAsync(uriBuilder.Uri);
        }

        public async Task HandleCallbackAsync(Uri callbackUri)
        {
            var queryParams = HttpUtility.ParseQueryString(callbackUri.Query);
            var code = queryParams["code"];

            if (string.IsNullOrEmpty(code))
                throw new InvalidOperationException("No authorization code in callback URI.");

            var codeVerifier = await SecureStorage.GetAsync("pkce_verifier");
            await ExchangeCodeForTokens(code, codeVerifier);
        }

        private async Task ExchangeCodeForTokens(string code, string codeVerifier)
        {
            using var client = new HttpClient();
            var tokenRequest = new
            {
                grant_type = "authorization_code",
                client_id = Auth0ClientId,
                code,
                redirect_uri = CallbackUrl,
                code_verifier = codeVerifier
            };

            var json = JsonSerializer.Serialize(tokenRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(TokenEndpoint, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            var accessToken = root.GetProperty("access_token").GetString();
            var idToken = root.TryGetProperty("id_token", out var idt) ? idt.GetString() : null;

            await SecureStorage.SetAsync("access_token", accessToken);
            if (!string.IsNullOrEmpty(idToken))
                await SecureStorage.SetAsync("id_token", idToken);
        }

        private string GenerateCodeVerifier()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Base64UrlEncode(bytes);
        }

        private string GenerateCodeChallenge(string codeVerifier)
        {
            using var sha256 = SHA256.Create();
            var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Base64UrlEncode(challengeBytes);
        }

        private string Base64UrlEncode(byte[] data)
        {
            return Convert.ToBase64String(data)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}