using MetalMachine.ViewModels;

namespace MetalMachine.Pages;

public partial class LandingView : BasePage
{
	public LandingView(LandingViewModel vm) : base(vm)
	{
		BindingContext = vm;
		InitializeComponent();
	}
}