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
        }
    }

}
