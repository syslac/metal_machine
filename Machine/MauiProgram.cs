using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui;
using MetalMachine.Pages;
using MetalMachine.ViewModels;
using MetalMachine.Services;

namespace MetalMachine;

public static class MauiProgram
{
	private static string _dbPath = "setlist_data.db";
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.RegisterServices()
			.RegisterViewModels()
			.RegisterViews()
			;

#if DEBUG
		builder.Logging.AddDebug();
#endif
		return builder.Build();
	}

	public static MauiAppBuilder RegisterServices(this MauiAppBuilder builder) 
	{
		builder.Services.AddSingleton<IDBManager>(new SQLiteDBManager(_dbPath));
		return builder;
	}
	public static MauiAppBuilder RegisterViews(this MauiAppBuilder builder) 
	{
		builder.Services.AddSingleton<LandingView>();
		return builder;
	}
	public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder) 
	{
		builder.Services.AddSingleton<LandingViewModel>();
		return builder;
	}
}
