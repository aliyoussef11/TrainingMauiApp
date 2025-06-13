using Microsoft.Identity.Client;

namespace TrainingMauiApp.Services
{
    public class MicrosoftAuthService
    {
        private static IPublicClientApplication _pca;
        private static string[] _scopes = { "openid", "profile", "offline_access" };

        public static void Init(string clientId, string tenantId)
        {
            _pca = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
                #if ANDROID
                .WithRedirectUri($"msal{clientId}://auth")
                #else
                .WithRedirectUri("http://localhost")
                 #endif
                .Build();
        }

        public static async Task<AuthenticationResult> SignInAsync()
        {
            AuthenticationResult result;
            var accounts = await _pca.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            try
            {
                // Try silent first
                return result = await _pca
                    .AcquireTokenSilent(_scopes, firstAccount)
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                // No token in cache or expired—prompt the user
                return result = await _pca
                    .AcquireTokenInteractive(_scopes)
                    //.WithParentActivityOrWindow(App.ParentWindow)
                    .ExecuteAsync();
            }
        }
    }
}
