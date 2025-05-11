using System.Globalization;
using System.Windows.Data;

namespace GrpsOverAllExamplesClients.Converters
{
    internal class ClientConnectedDisconnectedTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value)
            {
                return "Stop Client server";
            }

            return "Start Client server";
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
