using System;
using System.Globalization;
using System.Windows.Data;

namespace MvvmTools.Converters
{
    public class ProjectDisplayNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var name = (string)values[0];
                var isProject = (bool)values[1];
                if (isProject)
                    return name;
                return name + " (solution - inherited by projects)";
            }
            catch
            {
                return "?";
            }
            
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
