using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
//using Microsoft.Advertising.Mobile.UI;
using System.Windows.Navigation;

namespace MonkeyPad
{
    public partial class MainPage : PhoneApplicationPage
    {
        public bool updateLists = true;
        public bool AlreadyAdded = false;
        public bool adAdded = false;
        Style style = (Style)App.Current.Resources["PerformanceProgressBar"];
        ProgressBar bar = new ProgressBar();
        //AdControl adControl2 = null;
        //AldarIT.SuperAds.AdControl adControl = null;
        Google.AdMob.Ads.WindowsPhone7.WPF.BannerAd adControl = null;
        DataTemplate test = new DataTemplate();
        ListBox globalListBoxCopy = new ListBox();

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            test = LayoutRoot.Resources["noteModelTemplate"] as DataTemplate;
            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            this.LayoutUpdated += new EventHandler(updateUIs);
            startLoadingBar();
            if (App.IsTrial())
            {
                /*adControl = new AdControl("e1a0d7a1-5ba5-4395-bdaf-2a1707c25da8", "Image480_80", AdModel.Contextual, true);
                adControl.Width = 480;
                adControl.Height = 80;*/

                adControl = new Google.AdMob.Ads.WindowsPhone7.WPF.BannerAd
                {
                    AdUnitID = "a14d80621cca948"
                };
                //adControl.TestDeviceIDs.Add("
                /*adControl = new AldarIT.SuperAds.AdControl();

                AldarIT.SuperAds.AdProviders.AdmobAdProvider adMobProvider = new AldarIT.SuperAds.AdProviders.AdmobAdProvider();
                AldarIT.SuperAds.AdProviders.MobFoxAdProvider mobFoxProvider = new AldarIT.SuperAds.AdProviders.MobFoxAdProvider();
                AldarIT.SuperAds.AdProviders.SmaatoAdProvider smaatoProvider = new AldarIT.SuperAds.AdProviders.SmaatoAdProvider();

                adMobProvider.PublisherID = "a14d80621cca948";
                mobFoxProvider.PublisherID = "0e3ca65355a3c9b3febf28b85de5c761";
                smaatoProvider.AdSpaceID = 65738354;
                smaatoProvider.PublisherID = 923835548;

                adControl.AdProviders.Add(adMobProvider);
                adControl.AdProviders.Add(mobFoxProvider);
                adControl.AdProviders.Add(smaatoProvider);

                adControl.Height = 75;
                adControl.Stretch = Stretch.Uniform;
                adControl.TestMode = true;*/

            }
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.loading = true;
            }
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.IsTrial() && !adAdded)
            {
                // Ads.IsEnabled = true;
                // Ads.Visibility = System.Windows.Visibility.Visible;
                Grid grid = (Grid)this.LayoutRoot.Children[3];
                grid.Children.Add(adControl);
                System.Windows.Thickness margin = new System.Windows.Thickness(0, 0, 0, 75);
                pivotContainer.Margin = margin;
                adAdded = true;
            }
            IsolatedStorageSettings iss = IsolatedStorageSettings.ApplicationSettings;
            if (iss.Contains("email") & iss.Contains("authToken") && !App.ViewModel.IsLoggedIn)
            {
                App.ViewModel.IsLoggedIn = true;
                App.ViewModel.authToken = (String)iss["authToken"];
                App.ViewModel.email = (String)iss["email"];
                App.ViewModel.firstLoadLogin = true;
                App.ViewModel.HasBeenToLogin = true;
                App.ViewModel.LoadData();
            }
            else if (!App.ViewModel.IsLoggedIn && !App.ViewModel.HasBeenToLogin)
            {
                App.ViewModel.loading = false;
                NavigationService.Navigate(new Uri("/Login.xaml", UriKind.Relative));
            }
            else if (App.ViewModel.HasEnteredLoginInfo && !App.ViewModel.IsDataLoaded && App.ViewModel.authToken != "")
            {
                App.ViewModel.LoadData();
            }
            else if (App.ViewModel.HasEnteredLoginInfo && !App.ViewModel.IsDataLoaded && App.ViewModel.authToken == "")
            {
                App.ViewModel.IsDataLoaded = false;
                App.ViewModel.loading = true;
                App.ViewModel.notes.Clear();
                App.ViewModel.pinned.Clear();
                App.ViewModel.trashed.Clear();
                App.ViewModel.notes = new Models.SortableObservableCollection<Models.noteModel>();
                App.ViewModel.pinned = new Models.SortableObservableCollection<Models.noteModel>();
                App.ViewModel.trashed = new Models.SortableObservableCollection<Models.noteModel>();
                if (App.ViewModel.noteIndex != null && App.ViewModel.noteIndex.Data != null)
                {
                    App.ViewModel.noteIndex.Data.Clear();
                    App.ViewModel.noteIndex.Data = new Models.SortableObservableCollection<Models.noteModel>();
                    App.ViewModel.noteIndex.Count = 0;
                }
                if (App.ViewModel.markNoteIndex != null && App.ViewModel.markNoteIndex.Data != null)
                {
                    App.ViewModel.markNoteIndex.Data.Clear();
                    App.ViewModel.markNoteIndex.Data = new Models.SortableObservableCollection<Models.noteModel>();
                    App.ViewModel.markNoteIndex.Count = 0;
                }
                notesListBox.ItemsSource = null;
                notesListBox.ItemsSource = App.ViewModel.notes;
                pinnedListBox.ItemsSource = null;
                pinnedListBox.ItemsSource = App.ViewModel.pinned;
                trashListBox.ItemsSource = null;
                trashListBox.ItemsSource = App.ViewModel.trashed;
                App.ViewModel.LoadData();
            }
            if (App.ViewModel.IsSorted && App.ViewModel.IsDataLoaded)
            {
                notesListBox.ItemsSource = null;
                notesListBox.ItemsSource = App.ViewModel.notes;
                pinnedListBox.ItemsSource = null;
                pinnedListBox.ItemsSource = App.ViewModel.pinned;
                trashListBox.ItemsSource = null;
                trashListBox.ItemsSource = App.ViewModel.trashed;
                App.ViewModel.IsSorted = false;
            }
            if (App.ViewModel.HasMore)
            {
                ApplicationBarIconButton appButton = (ApplicationBarIconButton)ApplicationBar.Buttons[2];
                appButton.IsEnabled = true;
            }
            else
            {
                ApplicationBarIconButton appButton = (ApplicationBarIconButton)ApplicationBar.Buttons[2];
                appButton.IsEnabled = false;
            }
        }

        public void startLoadingBar()
        {

            if (style == null) { throw new InvalidOperationException("The style was not found."); }
            bar = new ProgressBar
            {
                IsIndeterminate = true,
                Style = style,
                Margin = new Thickness(240, 5, 5, 5),
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Width = 240,
            };
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void notesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox.SelectedIndex > -1)
            {
                Models.noteModel item = App.ViewModel.notes[listBox.SelectedIndex];
                NavigationService.Navigate(new Uri("/notePage.xaml?key=" + item.Key, UriKind.Relative));
                listBox.SelectedIndex = -1;
            }

        }

        private void pinnedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox.SelectedIndex > -1)
            {
                Models.noteModel item = App.ViewModel.pinned[listBox.SelectedIndex];
                NavigationService.Navigate(new Uri("/notePage.xaml?key=" + item.Key, UriKind.Relative));
                listBox.SelectedIndex = -1;
            }
        }

        private void trashedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox.SelectedIndex > -1)
            {
                Models.noteModel item = App.ViewModel.trashed[listBox.SelectedIndex];
                NavigationService.Navigate(new Uri("/notePage.xaml?key=" + item.Key, UriKind.Relative));
                listBox.SelectedIndex = -1;
            }
        }

        private void tagsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox.SelectedIndex > -1)
            {
                Models.tagModel item = App.ViewModel.tags[listBox.SelectedIndex];
                NavigationService.Navigate(new Uri("/tagPage.xaml?name=" + item.tagName, UriKind.Relative));
                listBox.SelectedIndex = -1;
            }
        }

        private void newButton_clicked(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/newPage.xaml", UriKind.Relative));
        }

        private void resetSelection(ListBox listBox)
        {
            listBox.SelectedIndex = -1;
        }

        private void syncButton_clicked(object sender, EventArgs e)
        {
            App.ViewModel.IsDataLoaded = false;
            App.ViewModel.loading = true;
            App.ViewModel.notes.Clear();
            App.ViewModel.pinned.Clear();
            App.ViewModel.trashed.Clear();
            App.ViewModel.notes = new Models.SortableObservableCollection<Models.noteModel>();
            App.ViewModel.pinned = new Models.SortableObservableCollection<Models.noteModel>();
            App.ViewModel.trashed = new Models.SortableObservableCollection<Models.noteModel>();
            if (App.ViewModel.noteIndex != null && App.ViewModel.noteIndex.Data != null)
            {
                App.ViewModel.noteIndex.Data.Clear();
                App.ViewModel.noteIndex.Data = new Models.SortableObservableCollection<Models.noteModel>();
                App.ViewModel.noteIndex.Count = 0;
            }
            if (App.ViewModel.markNoteIndex != null && App.ViewModel.markNoteIndex.Data != null)
            {
                App.ViewModel.markNoteIndex.Data.Clear();
                App.ViewModel.markNoteIndex.Data = new Models.SortableObservableCollection<Models.noteModel>();
                App.ViewModel.markNoteIndex.Count = 0;
            }
            notesListBox.ItemsSource = null;
            notesListBox.ItemsSource = App.ViewModel.notes;
            pinnedListBox.ItemsSource = null;
            pinnedListBox.ItemsSource = App.ViewModel.pinned;
            trashListBox.ItemsSource = null;
            trashListBox.ItemsSource = App.ViewModel.trashed;
            App.ViewModel.LoadData();
        }

        private void fullRefresh_clicked(object sender, EventArgs e)
        {
            App.ViewModel.refreshData();
        }

        private void getButton_clicked(object sender, EventArgs e)
        {
            App.ViewModel.getMoreFromMark();
        }

        private void about_clicked(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/aboutPage.xaml", UriKind.Relative));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string header = (sender as MenuItem).Header.ToString();
            ListBoxItem selectedListBox = this.notesListBox.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
            if (selectedListBox == null)
            {
                return;
            }
            Models.noteModel note = (Models.noteModel)selectedListBox.Content;
            if (header == "Pin")
            {
                if (App.ViewModel.sendUpdateDone)
                {
                    App.ViewModel.sendUpdateDone = false;
                    App.ViewModel.pinItem(note.Key);
                }
            }
            else if (header == "Trash")
            {
                if (App.ViewModel.sendUpdateDone)
                {
                    App.ViewModel.sendUpdateDone = false;
                    App.ViewModel.trashItem(note.Key);
                }
            }
            else if (header == "Email")
            {
                EmailComposeTask emailComposeTask = new EmailComposeTask();
                emailComposeTask.Body = note.Content;
                emailComposeTask.Subject = "[MonkeyPad Note] " + note.DisplayTitle;
                emailComposeTask.Show();
            }

        }

        private void MenuItem2_Click(object sender, RoutedEventArgs e)
        {
            string header = (sender as MenuItem).Header.ToString();
            ListBoxItem selectedListBox = this.pinnedListBox.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
            if (selectedListBox == null)
            {
                return;
            }
            Models.noteModel note = (Models.noteModel)selectedListBox.Content;
            if (header == "Unpin")
            {
                if (App.ViewModel.sendUpdateDone)
                {
                    App.ViewModel.sendUpdateDone = false;
                    App.ViewModel.unpinItem(note.Key);
                }
            }
            else if (header == "Trash")
            {
                if (App.ViewModel.sendUpdateDone)
                {
                    App.ViewModel.sendUpdateDone = false;
                    App.ViewModel.trashItem(note.Key);
                }
            }
            else if (header == "Email")
            {
                EmailComposeTask emailComposeTask = new EmailComposeTask();
                emailComposeTask.Body = note.Content;
                emailComposeTask.Subject = "[MonkeyPad Note] " + note.DisplayTitle;
                emailComposeTask.Show();
            }

        }

        private void MenuItem3_Click(object sender, RoutedEventArgs e)
        {
            string header = (sender as MenuItem).Header.ToString();
            ListBoxItem selectedListBox = this.trashListBox.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
            if (selectedListBox == null)
            {
                return;
            }
            Models.noteModel note = (Models.noteModel)selectedListBox.Content;
            if (header == "Restore")
            {
                if (App.ViewModel.sendUpdateDone)
                {
                    App.ViewModel.sendUpdateDone = false;
                    App.ViewModel.untrashItem(note.Key);
                }
            }
            else if (header == "Email")
            {
                EmailComposeTask emailComposeTask = new EmailComposeTask();
                emailComposeTask.Body = note.Content;
                emailComposeTask.Subject = "[MonkeyPad Note] " + note.DisplayTitle;
                emailComposeTask.Show();
            }

        }

        public void updateUIs(object sender, EventArgs e)
        {
            if (App.ViewModel.IsDataLoaded)
            {

                if (App.ViewModel.HasMore)
                {
                    ApplicationBarIconButton appButton = (ApplicationBarIconButton)ApplicationBar.Buttons[2];
                    appButton.IsEnabled = true;
                    if (updateLists)
                    {
                        notesListBox.ItemsSource = null;
                        notesListBox.ItemsSource = App.ViewModel.notes;
                        pinnedListBox.ItemsSource = null;
                        pinnedListBox.ItemsSource = App.ViewModel.pinned;
                        trashListBox.ItemsSource = null;
                        trashListBox.ItemsSource = App.ViewModel.trashed;
                        globalListBoxCopy.ItemsSource = null;
                        globalListBoxCopy.ItemsSource = App.ViewModel.tags;
                        updateLists = false;
                    }
                }
                else
                {
                    ApplicationBarIconButton appButton = (ApplicationBarIconButton)ApplicationBar.Buttons[2];
                    appButton.IsEnabled = false;
                }
                if (App.ViewModel.IsSorted && App.ViewModel.IsDataLoaded)
                {
                    notesListBox.ItemsSource = null;
                    notesListBox.ItemsSource = App.ViewModel.notes;
                    pinnedListBox.ItemsSource = null;
                    pinnedListBox.ItemsSource = App.ViewModel.pinned;
                    trashListBox.ItemsSource = null;
                    trashListBox.ItemsSource = App.ViewModel.trashed;
                    globalListBoxCopy.ItemsSource = null;
                    globalListBoxCopy.ItemsSource = App.ViewModel.tags;
                    App.ViewModel.IsSorted = false;
                }
            }
            if (!App.ViewModel.loading && App.ViewModel.alreadyAdded)
            {
                LayoutRoot.Children.Remove(bar);
                App.ViewModel.alreadyAdded = false;
            }
            else if (!App.ViewModel.alreadyAdded && App.ViewModel.loading)
            {
                LayoutRoot.Children.Add(bar);
                App.ViewModel.alreadyAdded = true;
            }

            if (App.ViewModel.authToken == "" || App.ViewModel.authToken == null)
            {
                ApplicationBarIconButton appButton = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                appButton.IsEnabled = false;
                ApplicationBarIconButton appButton2 = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                appButton2.IsEnabled = false;
                ApplicationBarMenuItem appMenuItem = (ApplicationBarMenuItem)ApplicationBar.MenuItems[0];
                appMenuItem.IsEnabled = false;
            }
            else
            {
                ApplicationBarIconButton appButton = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                appButton.IsEnabled = true;
                ApplicationBarIconButton appButton2 = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                appButton2.IsEnabled = true;
                ApplicationBarMenuItem appMenuItem = (ApplicationBarMenuItem)ApplicationBar.MenuItems[0];
                appMenuItem.IsEnabled = true;
            }
            if (App.ViewModel.refreshing)
            {
                ApplicationBarIconButton appButton2 = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                appButton2.IsEnabled = false;
                ApplicationBarMenuItem appMenuItem = (ApplicationBarMenuItem)ApplicationBar.MenuItems[0];
                appMenuItem.IsEnabled = false;
            }
            else
            {
                ApplicationBarIconButton appButton2 = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                appButton2.IsEnabled = true;
                ApplicationBarMenuItem appMenuItem = (ApplicationBarMenuItem)ApplicationBar.MenuItems[0];
                appMenuItem.IsEnabled = true;
            }
            if (App.ViewModel.tags != null && pivotContainer.Items.Count == 3)
            {
                showTags();
            }
        }

        private void logout_clicked(object sender, EventArgs e)
        {
            IsolatedStorageSettings iss = IsolatedStorageSettings.ApplicationSettings;
            App.ViewModel.IsDataLoaded = false;
            App.ViewModel.IsLoggedIn = false;
            App.ViewModel.HasBeenToLogin = false;
            App.ViewModel.HasEnteredLoginInfo = false;
            App.ViewModel.loading = false;
            App.ViewModel.authToken = "";
            if (iss.Contains("authToken"))
            {
                iss.Remove("authToken");
            }
            if (iss.Contains("email"))
            {
                iss.Remove("email");
            }
            App.ViewModel.email = "";
            App.ViewModel.base64login = null;
            App.ViewModel.notes.Clear();
            App.ViewModel.pinned.Clear();
            App.ViewModel.trashed.Clear();
            App.ViewModel.tags.Clear();
            App.ViewModel.notes = new Models.SortableObservableCollection<Models.noteModel>();
            App.ViewModel.pinned = new Models.SortableObservableCollection<Models.noteModel>();
            App.ViewModel.trashed = new Models.SortableObservableCollection<Models.noteModel>();
            App.ViewModel.tags = new Models.SortableObservableCollection<Models.tagModel>();
            if (App.ViewModel.noteIndex != null && App.ViewModel.noteIndex.Data != null)
            {
                App.ViewModel.noteIndex.Data.Clear();
                App.ViewModel.noteIndex.Data = new Models.SortableObservableCollection<Models.noteModel>();
                App.ViewModel.noteIndex.Count = 0;
            }
            if (App.ViewModel.markNoteIndex != null && App.ViewModel.markNoteIndex.Data != null)
            {
                App.ViewModel.markNoteIndex.Data.Clear();
                App.ViewModel.markNoteIndex.Data = new Models.SortableObservableCollection<Models.noteModel>();
                App.ViewModel.markNoteIndex.Count = 0;
            }
            notesListBox.ItemsSource = null;
            notesListBox.ItemsSource = App.ViewModel.notes;
            pinnedListBox.ItemsSource = null;
            pinnedListBox.ItemsSource = App.ViewModel.pinned;
            trashListBox.ItemsSource = null;
            trashListBox.ItemsSource = App.ViewModel.trashed;
            globalListBoxCopy.ItemsSource = null;
            globalListBoxCopy.ItemsSource = App.ViewModel.tags;
            ((App)App.Current).RootFrame.Navigate(new Uri("/Login.xaml", UriKind.Relative));
        }

        private void rate_clicked(object sender, EventArgs e)
        {
            MarketplaceDetailTask details = new MarketplaceDetailTask();
            details.ContentIdentifier = null;
            details.Show();
        }

        private void tagButton_clicked(object sender, EventArgs e)
        {

        }

        private void showTags()
        {
            foreach (Models.tagModel tagItem in App.ViewModel.tags)
            {
                tagItem.visible = true;
                if (tagItem.visible)
                {
                    PivotItem newTagPivot = new PivotItem();
                    newTagPivot.Header = "#" + tagItem.tagName.ToLower();
                    newTagPivot.Margin = new Thickness(0, 8, 0, 0);
                    Color grey = new Color();
                    grey.A = 255;
                    grey.R = 241;
                    grey.G = 241;
                    grey.B = 241;
                    newTagPivot.Background = new SolidColorBrush(grey);
                    ListBox newListBox = new ListBox();
                    newListBox.ItemsSource = tagItem.notes;
                    newListBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    newListBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    newListBox.Margin = new Thickness(12, 0, 0, 0);
                    newListBox.Height = 537;
                    newListBox.ItemTemplate = this.Resources["noteModelTemplate"] as DataTemplate;
                    newListBox.ItemContainerStyle = ListBoxItemStyle1;
                    newListBox.SelectionChanged += new SelectionChangedEventHandler(tagSelection);
                    newListBox.Name = tagItem.tagName + "ListBox";
                    newTagPivot.Content = newListBox;
                    globalListBoxCopy = newListBox;
                    pivotContainer.Items.Add(newTagPivot);
                }
            }
        }

        private void tagSelection(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox.SelectedIndex > -1)
            {
                foreach (Models.tagModel tagItem in App.ViewModel.tags)
                {
                    if (tagItem.tagName + "ListBox" == listBox.Name)
                    {
                        NavigationService.Navigate(new Uri("/notePage.xaml?key=" + tagItem.notes[listBox.SelectedIndex].Key + "&type=" + tagItem.tagName, UriKind.Relative));
                        listBox.SelectedIndex = -1;
                    }
                }
            }
        }

        private void settings_clicked(object sender, EventArgs e)
        {

        }


    }
}