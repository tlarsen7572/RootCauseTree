using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace com.PorcupineSupernova.RootCauseTreeWpf
{
    class VertexTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var nodeType = (Graphing.RootCauseVertexType)value;
            switch (nodeType)
            {
                case Graphing.RootCauseVertexType.SelectedNode:
                    return (Color)App.Current.FindResource("SelectedNode");
                case Graphing.RootCauseVertexType.RootNode:
                    return (Color)App.Current.FindResource("RootNode");
                case Graphing.RootCauseVertexType.FinalChildNode:
                    return (Color)App.Current.FindResource("FinalChildNode");
                case Graphing.RootCauseVertexType.ChildNode:
                default:
                    return (Color)App.Current.FindResource("ChildNode");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class VertexTypeToDarkColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var nodeType = (Graphing.RootCauseVertexType)value;
            switch (nodeType)
            {
                case Graphing.RootCauseVertexType.SelectedNode:
                    return (Color)App.Current.FindResource("SelectedNodeDark");
                case Graphing.RootCauseVertexType.RootNode:
                    return (Color)App.Current.FindResource("RootNodeDark");
                case Graphing.RootCauseVertexType.FinalChildNode:
                    return (Color)App.Current.FindResource("FinalChildNodeDark");
                case Graphing.RootCauseVertexType.ChildNode:
                default:
                    return (Color)App.Current.FindResource("ChildNodeDark");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
