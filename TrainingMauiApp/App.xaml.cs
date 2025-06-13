using TrainingMauiApp.Services;

namespace TrainingMauiApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }      
    }
}
