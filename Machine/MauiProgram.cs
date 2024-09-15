using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui;
using MetalMachine.Pages;
using MetalMachine.ViewModels;
using MetalMachine.Services;
using CommunityToolkit.Mvvm.Messaging;
using Java.Sql;

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
		MauiApp toRet = builder.Build();
		LoadSetlistAsset(toRet).ConfigureAwait(false);
		return toRet;
	}

	public static MauiAppBuilder RegisterServices(this MauiAppBuilder builder) 
	{
		IDBManager dbManager = new SQLiteDBManager(_dbPath);
		builder.Services.AddSingleton<IDBManager>(dbManager);
		builder.Services.AddSingleton<IGeocoding>(Geocoding.Default);
		builder.Services.AddSingleton<IPreferences>(Preferences.Default);
		builder.Services.AddSingleton<IConcertProvider>(new ApiConcertProvider());
		builder.Services.AddSingleton<IMessenger>(new WeakReferenceMessenger());

		// double registration, because the ones in builder.Services will be 
		// auto-resolved in ViewModels, but what if I need it elsewhere in an
		// independent class? Need this DependencyService
		// Important to have a single instance registered in both, and not to
		// accidentally register two different
		DependencyService.RegisterSingleton<IGeocoding>(Geocoding.Default);
		DependencyService.RegisterSingleton<IDBManager>(dbManager);
		return builder;
	}
	public static MauiAppBuilder RegisterViews(this MauiAppBuilder builder) 
	{
		builder.Services.AddSingleton<LandingView>();
		builder.Services.AddSingleton<UserView>();
		builder.Services.AddSingleton<MaintenanceView>();
		return builder;
	}
	public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder) 
	{
		builder.Services.AddSingleton<LandingViewModel>();
		builder.Services.AddSingleton<UserViewModel>();
		builder.Services.AddSingleton<MaintenanceViewModel>();
		return builder;
	}

	public static async Task LoadSetlistAsset(MauiApp app)
	{
		using var stream = await FileSystem.OpenAppPackageFileAsync("setlist.fm");
		using var reader = new StreamReader(stream);

		var contents = reader.ReadToEnd();
		app.Services.GetService<IConcertProvider>()?.Init(contents ?? String.Empty);
	}
}
