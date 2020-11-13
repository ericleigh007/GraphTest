// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// <ConverterSnippet>
using Microsoft.Graph;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace GraphTest.Converters
{
    /*
     * This class returns a unicode character based upon the facets that are present and valid in the
     * drive item.  This is experimental and I'm not sure it's quite the right way to do this.
     * To use it, make sure to pass the entire object {Binding .,} NOT a class member {Binding Memeber}.
    */
    class ClassToItemIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var driveItem = (value as DriveItem);
            if (driveItem == null) return " ";
            // Any icond types that overlap should be listed more unique first (photo, then file, for instance)
            if (driveItem?.Audio != null) return "\ue8d6";   // Audio icon
            if (driveItem?.Photo != null) return "\ue8b9";   // photo icon
            if (driveItem?.Video != null) return "\ue714";   // Video icon
            if (driveItem?.Image != null) return "\ue7c3";   // Image icon
            if (driveItem?.Folder != null) return "\ue8b7";  // Folder icon
            if (driveItem?.File != null) return "\ue8a5";    // file icon
            return " ";  // neither
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
// </ConverterSnippet>
