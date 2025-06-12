using DotNetMauiApp.Services;

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
    }

}
