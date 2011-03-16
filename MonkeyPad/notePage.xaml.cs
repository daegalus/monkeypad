using System;
using System.Collections.Generic;
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
//using Microsoft.Advertising.Mobile.UI;

namespace MonkeyPad
{
    public partial class noteView : PhoneApplicationPage
    {
        Models.noteModel savedNote = null;
        string list = "";
        bool pinned = false;
        bool trashed = false;
        bool adAdded = false;
        //AdControl adControl2 = null;
        Google.AdMob.Ads.WindowsPhone7.WPF.BannerAd adControl = null;
        public noteView()
        {
            InitializeComponent();
            if (App.IsTrial())
            {
                /*adControl = new AdControl("e1a0d7a1-5ba5-4395-bdaf-2a1707c25da8", "Image480_80", AdModel.Contextual, true);
                adControl.Width = 480;
                adControl.Height = 80;*/
                
                adControl = new Google.AdMob.Ads.WindowsPhone7.WPF.BannerAd {
                    AdUnitID = "a14d80621cca948"
                };
            }
            this.LayoutUpdated += new EventHandler(updateUI);
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Call the base implementation
            base.OnNavigatedTo(e);
            if (App.IsTrial() && !adAdded)
            {
                Grid grid = (Grid)this.LayoutRoot.Children[2];
                grid.Children.Add(adControl);
                scrollViewer1.Height = 497;
                adAdded = true;
            }
            String noteKey = "";
            UpdateLayout();
            bool found = false;
            if(NavigationContext.QueryString.TryGetValue("key", out noteKey))
            {
                foreach (Models.noteModel note in App.ViewModel.notes)
                {
                    if (note.Key == noteKey)
                    {
                        found = true;
                        noteText.Text = note.Content;
                        System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                        date = date.AddSeconds((double)note.CreateDate);
                        dateText.Text = date.ToLongDateString();
                        savedNote = note;
                        list = "notes";
                    }
                }
                if (!found)
                {
                    foreach (Models.noteModel note in App.ViewModel.pinned)
                    {
                        if (note.Key == noteKey)
                        {
                            found = true;
                            noteText.Text = note.Content;
                            System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                            date = date.AddSeconds((double)note.CreateDate);
                            dateText.Text = date.ToLongDateString();
                            savedNote = note;
                            list = "pinned";
                            pinned = true;
                        }
                    }
                    if (!found)
                    {
                        foreach (Models.noteModel note in App.ViewModel.trashed)
                        {
                            if (note.Key == noteKey)
                            {
                                found = true;
                                noteText.Text = note.Content;
                                System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                                date = date.AddSeconds((double)note.CreateDate);
                                dateText.Text = date.ToLongDateString();
                                savedNote = note;
                                list = "trashed";
                                trashed = true;
                            }
                        }
                    }
                }
                found = false;
            }
            else
            {
                noteKey = savedNote.Key;
                foreach (Models.noteModel note in App.ViewModel.notes)
                {
                    if (note.Key == noteKey)
                    {
                        found = true;
                        noteText.Text = note.Content;
                        System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                        date = date.AddSeconds((double)note.CreateDate);
                        dateText.Text = date.ToLongDateString();
                        list = "notes";
                    }
                }
                if (!found)
                {
                    foreach (Models.noteModel note in App.ViewModel.pinned)
                    {
                        if (note.Key == noteKey)
                        {
                            found = true;
                            noteText.Text = note.Content;
                            System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                            date = date.AddSeconds((double)note.CreateDate);
                            dateText.Text = date.ToLongDateString();
                            list = "pinned";
                            pinned = true;
                        }
                    }
                    if (!found)
                    {
                        foreach (Models.noteModel note in App.ViewModel.trashed)
                        {
                            if (note.Key == noteKey)
                            {
                                found = true;
                                noteText.Text = note.Content;
                                System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                                date = date.AddSeconds((double)note.CreateDate);
                                dateText.Text = date.ToLongDateString();
                                list = "trashed";
                                trashed = true;
                            }
                        }
                    }
                }
                found = false;
            }
        }

        private void editButton_clicked(object sender, EventArgs e)
        {
            if (App.ViewModel.sendUpdateDone)
            {
                NavigationService.Navigate(new Uri("/editPage.xaml?key=" + savedNote.Key + "&list=" + list, UriKind.Relative));
            }
        }

        private void pinButton_clicked(object sender, EventArgs e)
        {
            
            if (!pinned && App.ViewModel.sendUpdateDone)
            {
                App.ViewModel.sendUpdateDone = false;
                pinned = true;
                list = "pinned";
                App.ViewModel.pinItem(savedNote.Key);
                ApplicationBarIconButton appButton = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                appButton.Text = "Unpin";
                System.Uri temp = new System.Uri("/icons/appbar.favs.subfrom.rest.png", UriKind.Relative);
                appButton.IconUri = temp;
                appButton.Click -= new EventHandler(pinButton_clicked);
                appButton.Click += new EventHandler(unpinButton_clicked);
            }
        }

        private void unpinButton_clicked(object sender, EventArgs e)
        {
            if (pinned && App.ViewModel.sendUpdateDone)
            {
                App.ViewModel.sendUpdateDone = false;
                pinned = false;
                if (savedNote.Deleted == true)
                {
                    list = "trashed";
                }
                else
                {
                    list = "notes";
                }
                App.ViewModel.unpinItem(savedNote.Key);
                ApplicationBarIconButton appButton = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                appButton.Text = "Pin";
                System.Uri temp = new System.Uri("/icons/appbar.favs.addto.rest.png", UriKind.Relative);
                appButton.IconUri = temp;
                appButton.Click -= new EventHandler(unpinButton_clicked);
                appButton.Click += new EventHandler(pinButton_clicked);
            }
        }

        private void trashButton_clicked(object sender, EventArgs e)
        {
            if (!trashed && App.ViewModel.sendUpdateDone)
            {
                App.ViewModel.sendUpdateDone = false;
                trashed = true;
                list = "trashed";
                App.ViewModel.trashItem(savedNote.Key);
                ApplicationBarIconButton appButton = (ApplicationBarIconButton)ApplicationBar.Buttons[2];
                appButton.Text = "Restore";
                System.Uri temp = new System.Uri("/icons/appbar.undelete.rest.png", UriKind.Relative);
                appButton.IconUri = temp;
                appButton.Click -= new EventHandler(trashButton_clicked);
                appButton.Click += new EventHandler(untrashButton_clicked);
                ApplicationBarIconButton appButton2 = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                appButton2.IsEnabled = false;
                ApplicationBarIconButton appButton3 = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                appButton3.IsEnabled = false;
            }
        }

        private void untrashButton_clicked(object sender, EventArgs e)
        {
            if (trashed && App.ViewModel.sendUpdateDone)
            {
                App.ViewModel.sendUpdateDone = false;
                trashed = false;
                if (savedNote.SystemTags != null && savedNote.SystemTags.Length > 1 && savedNote.SystemTags[0] != "pinned")
                {
                    list = "pinned";
                }
                else
                {
                    list = "notes";
                }
                App.ViewModel.untrashItem(savedNote.Key);
                ApplicationBarIconButton appButton = (ApplicationBarIconButton)ApplicationBar.Buttons[2];
                appButton.Text = "Trash";
                System.Uri temp = new System.Uri("/icons/appbar.delete.rest.png", UriKind.Relative);
                appButton.IconUri = temp;
                appButton.Click -= new EventHandler(untrashButton_clicked);
                appButton.Click += new EventHandler(trashButton_clicked);
                ApplicationBarIconButton appButton2 = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                appButton2.IsEnabled = true;
                ApplicationBarIconButton appButton3 = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                appButton3.IsEnabled = true;
            }
        }

        private void updateUI(object sender, EventArgs e)
        {
            
            if (savedNote != null && savedNote.SystemTags.Length > 0)
            {
                ApplicationBarIconButton appButton = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                appButton.Text = "Unpin";
                var test = appButton.IconUri;
                System.Uri temp = new System.Uri("/icons/appbar.favs.subfrom.rest.png", UriKind.Relative);
                appButton.IconUri = temp;
                appButton.Click -= new EventHandler(pinButton_clicked);
                appButton.Click += new EventHandler(unpinButton_clicked);
            }
            else
            {
                ApplicationBarIconButton appButton = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                appButton.Text = "Pin";
                System.Uri temp = new System.Uri("/icons/appbar.favs.addto.rest.png", UriKind.Relative);
                appButton.IconUri = temp;
                appButton.Click -= new EventHandler(unpinButton_clicked);
                appButton.Click += new EventHandler(pinButton_clicked);
            }

            if (savedNote != null && savedNote.Deleted)
            {
                ApplicationBarIconButton appButton = (ApplicationBarIconButton)ApplicationBar.Buttons[2];
                appButton.Text = "Restore";
                var test = appButton.IconUri;
                System.Uri temp = new System.Uri("/icons/appbar.undelete.rest.png", UriKind.Relative);
                appButton.IconUri = temp;
                appButton.Click -= new EventHandler(trashButton_clicked);
                appButton.Click += new EventHandler(untrashButton_clicked);
                ApplicationBarIconButton appButton2 = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                appButton2.IsEnabled = false;
                ApplicationBarIconButton appButton3 = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                appButton3.IsEnabled = false;

            }
            else
            {
                ApplicationBarIconButton appButton = (ApplicationBarIconButton)ApplicationBar.Buttons[2];
                appButton.Text = "Trash";
                System.Uri temp = new System.Uri("/icons/appbar.delete.rest.png", UriKind.Relative);
                appButton.IconUri = temp;
                appButton.Click -= new EventHandler(untrashButton_clicked);
                appButton.Click += new EventHandler(trashButton_clicked);
                ApplicationBarIconButton appButton2 = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                appButton2.IsEnabled = true;
                ApplicationBarIconButton appButton3 = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                appButton3.IsEnabled = true;
            }
        }
                      
    }
}
