using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Microsoft.Phone.Controls;

namespace MonkeyPad
{
    public partial class Login : PhoneApplicationPage
    {
        public Login()
        {
            InitializeComponent();
            
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Call the base implementation
            base.OnNavigatedTo(e);
            App.ViewModel.HasBeenToLogin = true;
        }

        private void login_clicked(object sender, System.Windows.RoutedEventArgs e)
        {
        	App.ViewModel.email = UsernameBox.Text.Trim();
			
			string loginInfo = "email="+App.ViewModel.email+"&password="+PasswordBox.Password;
            App.ViewModel.base64login = EncodeTo64(loginInfo);
			App.ViewModel.clearLists();
            App.ViewModel.HasBeenToLogin = true;
            App.ViewModel.HasEnteredLoginInfo = true;
            App.ViewModel.firstLoadLogin = true;
            App.ViewModel.IsDataLoaded = false;
            App.ViewModel.loading = true;
            //((App)App.Current).RootFrame.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
			((App)App.Current).RootFrame.GoBack();
        }
		
		static public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
            string returnValue = Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        private void exitApp_clicked(object sender, RoutedEventArgs e)
        {
            //App.Quit();
        }
    }
}
