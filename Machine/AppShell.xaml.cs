using MetalMachine.Pages;
using MetalMachine.ViewModels;

namespace MetalMachine;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
	}

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

		RegisterRoutes();
		NavigateToLandingPage();
    }

	public static void RegisterRoutes() 
	{
		Routing.RegisterRoute(typeof(LandingViewModel).Name, typeof(LandingView));
		Routing.RegisterRoute(typeof(UserViewModel).Name, typeof(UserView));
	} 
	public static async void NavigateToLandingPage() 
	{
		await Shell.Current.GoToAsync($"//{typeof(LandingViewModel).Name}");
	}
}
