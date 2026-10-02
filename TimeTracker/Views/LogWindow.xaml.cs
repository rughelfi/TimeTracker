using System.Windows;
using System.Windows.Input;
using TimeTracker.ViewModels;

namespace TimeTracker.Views;

public partial class LogWindow : Window
{
    public LogWindow(LogViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }
}
