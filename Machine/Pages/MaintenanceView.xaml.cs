using MetalMachine.ViewModels;

namespace MetalMachine.Pages;

public partial class MaintenanceView : BasePage
{
	public MaintenanceView(MaintenanceViewModel vm) : base(vm)
	{
		BindingContext = vm;

		InitializeComponent();
	}
}