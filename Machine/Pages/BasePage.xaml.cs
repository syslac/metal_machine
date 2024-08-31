using Android.Systems;
using MetalMachine.ViewModels;

namespace MetalMachine.Pages;

public partial class BasePage : ContentPage
{
	public BasePage(BaseViewModel vm)
	{
		BindingContext = vm;

		InitializeComponent();
	}

	public async void BasePage_Appearing(object sender, EventArgs args) 
	{
		await ((BaseViewModel)BindingContext).OnAppearing();
	}
}