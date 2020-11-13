// Together with changes in the .csproj file, should allow us to use both the Microsoft.Graph.Beta library (which the
// Windows community toolkit graph controls depend on for now, and the Release library Microsoft.Graph.
// See the .csproj file for more (We're doing the opposite of the article)
// Source: https://github.com/microsoftgraph/msgraph-beta-sdk-dotnet

extern alias ReleaseLib;

using Microsoft.Toolkit.Graph.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Graph;
using ReleaseGraph = ReleaseLib.Microsoft.Graph;
using Windows.System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GraphTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public enum sortKeyEnum
        { 
            Name = 0,
            Date = 1,
            Size = 2
        };
        private sortKeyEnum currentSortKey = sortKeyEnum.Name;

        private GraphServiceClient graphClient;

        private IDriveItemChildrenCollectionPage driveItems;

        private IDriveItemSearchCollectionPage searchItems;

        private bool searchInProgress = false;

        private readonly int ITEM_COUNT = 50;

        public MainPage()
        {
            this.InitializeComponent();

            Loaded += MainPage_Loaded;

            // Load OAuth settings
            var oauthSettings = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView("OAuth");
            var appId = oauthSettings.GetString("AppId");
            var scopes = oauthSettings.GetString("Scopes");

            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(scopes))
            {
                Debug.WriteLine("Could not load OAuth Settings from resource file.");
            }
            else
            {
                // Configure MSAL provider  
                MsalProvider.ClientId = appId;
                MsalProvider.Scopes = new ScopeSet(scopes.Split(' '));

                // Handle auth state change
                ProviderManager.Instance.ProviderUpdated += ProviderUpdated;
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
        }
        // </ConstructorSnippet>

        // <ProviderUpdatedSnippet>
        private void ProviderUpdated(object sender, ProviderUpdatedEventArgs e)
        {
            var globalProvider = ProviderManager.Instance.GlobalProvider;
            SetAuthState(globalProvider != null && globalProvider.State == ProviderState.SignedIn);
        }
        // </ProviderUpdatedSnippet>

        // <SetAuthStateSnippet>
        private void SetAuthState(bool isAuthenticated)
        {
            (Windows.UI.Xaml.Application.Current as App).IsAuthenticated = isAuthenticated;
            if (isAuthenticated)
            {
                GetDirectoryButton.IsEnabled = true;
                LoggedInTextBlock.Text = "Logged In";

                graphClient = ProviderManager.Instance.GlobalProvider.Graph;
            }
        }
        // </SetAuthStateSnippet>

        private async void GetDirectoryButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var sortKeyName = currentSortKey.ToString();

            try
            {
                // Get the events
                driveItems = await graphClient.Me.Drive.Root.Children.Request()
                    //                    .Select("parent,name,size,lastModifiedDateTime,webUrl")
                    .OrderBy($"{sortKeyName} ASC").Top(ITEM_COUNT)
                    .GetAsync();


                FileList.ItemsSource = driveItems.CurrentPage.ToList();

                // page iterator is really a poor name for this as it gets called for each item.
                // we return false to pause iteration.
                /*
                 * pageIterator = ReleaseGraph.PageIterator<DriveItem>.CreatePageIterator(graphClient, driveItems, (d) =>
                                {
                                    Debug.WriteLine($"currently on {d.Name}");
                                    if (ITEM_COUNT == ++itemsIterated )
                                        return false;

                                    return true;
                                });
                */

                NextPageButton.IsEnabled = (driveItems.NextPageRequest != null);
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                Debug.WriteLine($"Exception getting Files: {ex.Message}");
            }
        }

        private async void NextPageButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (searchInProgress)
            {
                searchItems = await searchItems.NextPageRequest.GetAsync();
                NextPageButton.IsEnabled = (searchItems.NextPageRequest != null);
                FileList.ItemsSource = searchItems.CurrentPage;
            }
            else
            { 
                driveItems = await driveItems.NextPageRequest.GetAsync();
                NextPageButton.IsEnabled = (driveItems.NextPageRequest != null);
                FileList.ItemsSource = driveItems.CurrentPage;
            }
        }

        private void SearchButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SearchTextBox.IsEnabled = true;
        }

        private void BackToDirectoryButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            searchInProgress = false;

            FileList.ItemsSource = driveItems;
        }

        private async void SearchTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if ( e.Key == VirtualKey.Enter)
            {
                string searchString = (sender as TextBox).Text;

                if (searchString.Length == 0)
                {
                    return;
                }

                searchInProgress = true;

                searchItems = await graphClient.Me.Drive.Root.Search(searchString).Request()
                    .Top(ITEM_COUNT)
                    .OrderBy($"{currentSortKey} ASC")
                    .GetAsync();

                FileList.ItemsSource = searchItems.CurrentPage;

                NextPageButton.IsEnabled = (searchItems.NextPageRequest != null);
            }
        }
    }
}
