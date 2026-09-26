using Microsoft.Extensions.Logging;
using UniConnect.MAUI.Services;
using UniConnect.MAUI.ViewModels;
using UniConnect.MAUI.Views;

namespace UniConnect.MAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // ApiService holds the auth token/current user, so it must be a Singleton -
            // one shared instance for the whole app's lifetime, not a new one per page.
            builder.Services.AddSingleton<ApiService>();

            // Pages + ViewModels are Transient - a fresh instance each time you navigate
            // to them, which is the standard MAUI pattern.
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<LoginViewModel>();

            builder.Services.AddTransient<SignUpPage>();
            builder.Services.AddTransient<SignUpViewModel>();

            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<DashboardViewModel>();

            builder.Services.AddTransient<NewProjectPage>();
            builder.Services.AddTransient<NewProjectViewModel>();

            builder.Services.AddTransient<TaskBoardPage>();
            builder.Services.AddTransient<TaskBoardViewModel>();

            builder.Services.AddTransient<ChatPage>();
            builder.Services.AddTransient<ChatViewModel>();

            return builder.Build();
        }
    }
}
