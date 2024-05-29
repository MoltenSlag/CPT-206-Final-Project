using Avalonia.Controls;

namespace Avalonia_Test_1.Views;

public partial class ErrorWindow : Window
{
	//Simple error window
	//Can't use MessageBox because that's Windows exclusive
	//So I made my own
	public ErrorWindow()
	{
		InitializeComponent();
	}

	public ErrorWindow(string errorMessage)
	{
		InitializeComponent();
		error.Text = errorMessage;
	}
}
