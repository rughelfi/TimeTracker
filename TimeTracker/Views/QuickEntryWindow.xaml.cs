using System.Windows;
using System.Windows.Input;
using TimeTracker.Models;
using TimeTracker.ViewModels;

namespace TimeTracker.Views;

public partial class QuickEntryWindow : Window
{
    public QuickEntryWindow(QuickEntryViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        Loaded += (_, _) =>
        {
            DescriptionBox.Focus();
            App.PlayAlarmSound();
        };
    }

    private void DescriptionBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (DataContext is QuickEntryViewModel vm && vm.IsSuggestionsOpen && vm.FilteredSuggestions.Count > 0)
        {
            if (e.Key == Key.Down)
            {
                e.Handled = true;
                var nextIndex = SuggestionsList.SelectedIndex + 1;
                if (nextIndex >= vm.FilteredSuggestions.Count) nextIndex = 0;
                SuggestionsList.SelectedIndex = nextIndex;
                return;
            }
            else if (e.Key == Key.Up)
            {
                e.Handled = true;
                var prevIndex = SuggestionsList.SelectedIndex - 1;
                if (prevIndex < 0) prevIndex = vm.FilteredSuggestions.Count - 1;
                SuggestionsList.SelectedIndex = prevIndex;
                return;
            }
            else if (e.Key == Key.Tab || (e.Key == Key.Return && SuggestionsList.SelectedItem != null))
            {
                if (SuggestionsList.SelectedItem is ActivitySuggestion suggestion)
                {
                    e.Handled = true;
                    vm.SelectSuggestion(suggestion);
                    return;
                }
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                vm.IsSuggestionsOpen = false;
                return;
            }
        }

        if (e.Key == Key.Escape)
            Close();
    }
}
