using System;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel.Activation;
using DotNetMauiApp.Services;
using System.Diagnostics;

namespace TrainingMauiApp.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            this.InitializeComponent();
        }

        // This hooks your MAUI App builder.
        protected override MauiApp CreateMauiApp()
            => MauiProgram.CreateMauiApp();

        // WinUI calls this for all launches, including protocol activations.
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            // args.Arguments contains the full URI when launched via protocol.
            var raw = args.Arguments;
            if (!string.IsNullOrEmpty(raw) && raw.StartsWith("myapp://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var uri = new Uri(raw);
                    var authSvc = this.Services.GetService<AuthService>();
                    if (authSvc != null)
                    {
                        _ = authSvc.HandleCallbackAsync(uri);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error handling protocol URI: {ex}");
                }
            }
        }
    }
}
