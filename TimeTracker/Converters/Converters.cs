using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TimeTracker.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true
            ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x47, 0x57))   // urgent red
            : new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x00, 0xD4, 0xFF));  // accent cyan

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool v = value switch
        {
            bool b => b,
            int i => i > 0,
            _ => value != null
        };
        if (parameter is string s && s == "Invert") v = !v;
        return v ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
