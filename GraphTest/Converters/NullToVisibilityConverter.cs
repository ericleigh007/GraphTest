// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// <ConverterSnippet>
using Microsoft.Graph;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace GraphTest.Converters
{
    class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
// </ConverterSnippet>
