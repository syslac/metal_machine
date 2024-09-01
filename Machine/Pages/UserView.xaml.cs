using MetalMachine.ViewModels;

namespace MetalMachine.Pages;

public partial class UserView : BasePage
{
	public UserView(UserViewModel vm) : base(vm)
	{
		BindingContext = vm;

		InitializeComponent();
	}
}