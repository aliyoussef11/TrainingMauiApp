using System.Diagnostics;

namespace DotNetMauiApp.Services
{
    public class AuthService
    {
        private const string CallbackUrl = "myapp://callback";

        private const string Auth0ClientId = "YU4Dk68WnthD9GUmB2lXZdkoA5l8HU9l";
        private const string Auth0Domain = "dev-algybd55zd12gxzy.us.auth0.com";

        public async Task AuthenticateAsync()
        {
            string authUrl =
                $"https://{Auth0Domain}/authorize" +
                $"?client_id={Auth0ClientId}" +
                $"&response_type=token" +
                $"&scope=openid%20profile%20email" +
                $"&redirect_uri={CallbackUrl}";

            try
            {
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    // Use browser instead
                    await Launcher.Default.OpenAsync(new Uri(authUrl));
                    return;
                }
                else
                {
                    var authResult = await WebAuthenticator.AuthenticateAsync(
                        new Uri(authUrl),
                        new Uri(CallbackUrl));

                    if (authResult?.Properties != null)
                    {
                        var accessToken = authResult.Properties["access_token"];
                        var idToken = authResult.Properties.ContainsKey("id_token")
                            ? authResult.Properties["id_token"]
                            : null;

                        // Now you have the tokens! You can store them securely.
                        await SecureStorage.SetAsync("access_token", accessToken);
                        if (idToken != null)
                            await SecureStorage.SetAsync("id_token", idToken);
                    }
                }     
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Auth failed: {ex.Message}");
            }
        }
    }
}
