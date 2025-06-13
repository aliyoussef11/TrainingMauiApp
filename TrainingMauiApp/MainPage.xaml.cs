using TrainingMauiApp.Services;

namespace TrainingMauiApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var authService = new AuthService();
            await authService.AuthenticateAsync();

            var accessToken = await SecureStorage.GetAsync("access_token");
            var idToken = await SecureStorage.GetAsync("id_token");
        }

        private async void OnMicrosoftLoginClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await MicrosoftAuthService.SignInAsync();
                await DisplayAlert("Welcome", $"Logged in as: {result.Account.Username}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Login Failed", ex.Message, "OK");
            }
        }
    }

}
