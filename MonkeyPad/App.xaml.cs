using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace MonkeyPad
{
    public partial class App : Application
    {
        private static MainViewModel viewModel = null;

        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static MainViewModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (viewModel == null)
                    viewModel = new MainViewModel();

                return viewModel;
            }
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        public bool isAppATrial = false;
        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            // Note that exceptions thrown by ApplicationBarItem.Click will not get caught here.
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();
        }

        public static bool IsTrial()
        {
            var license = new Microsoft.Phone.Marketplace.LicenseInformation();
            return license.IsTrial();
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            
                IsolatedStorageSettings iss = IsolatedStorageSettings.ApplicationSettings;
                if (iss.Contains("noteIndex"))
                {
                    App.ViewModel.noteIndex = (Models.noteIndexModel)iss["noteIndex"];
                    if (iss.Contains("vmNotes"))
                    {
                        App.ViewModel.notes = (Models.SortableObservableCollection<Models.noteModel>)iss["vmNotes"];
                    }
                    if (iss.Contains("vmPinned"))
                    {
                        App.ViewModel.pinned = (Models.SortableObservableCollection<Models.noteModel>)iss["vmPinned"];
                    }
                    if (iss.Contains("vmTrashed"))
                    {
                        App.ViewModel.trashed = (Models.SortableObservableCollection<Models.noteModel>)iss["vmTrashed"];
                    }
                    App.ViewModel.IsDataLoaded = true;
                }
                if (iss.Contains("authToken") && iss.Contains("email"))
                {
                    App.ViewModel.IsLoggedIn = true;
                    App.ViewModel.authToken = (String)iss["authToken"];
                    App.ViewModel.email = (String)iss["email"];
                    App.ViewModel.firstLoadLogin = true;
                    App.ViewModel.HasBeenToLogin = true;
                    App.ViewModel.HasEnteredLoginInfo = true;
                    App.ViewModel.loading = false;
                    App.ViewModel.alreadyAdded = false;
                }
            
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            var store = PhoneApplicationService.Current.State;
            if (store.ContainsKey("MainViewModel"))
            {
                viewModel = store["MainViewModel"] as MainViewModel;
            }
            //viewModel.IsSorted = true;
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            var store = PhoneApplicationService.Current.State;
            if (store.ContainsKey("MainViewModel"))
            {
                store["MainViewModel"] = viewModel;
            }
            else
            {
                store.Add("MainViewModel", viewModel);
            }
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            IsolatedStorageSettings iss = IsolatedStorageSettings.ApplicationSettings;
            if (iss.Contains("noteIndex") && App.ViewModel.noteIndex != null && App.ViewModel.noteIndex.Data != null && App.ViewModel.noteIndex.Data.Count > 0)
            {
                iss["noteIndex"] = App.ViewModel.noteIndex;
            }
            else if (App.ViewModel.noteIndex != null && App.ViewModel.noteIndex.Data != null && !iss.Contains("noteIndex"))
            {
                iss.Add("noteIndex", App.ViewModel.noteIndex);
            }
            if (iss.Contains("vmNotes") && App.ViewModel.notes != null && App.ViewModel.notes.Count > 0)
            {
                iss["vmNotes"] = App.ViewModel.notes;
            }
            else if (App.ViewModel.notes != null && !iss.Contains("vmNotes"))
            {
                iss.Add("vmNotes", App.ViewModel.notes);
            }
            if (iss.Contains("vmPinned") && App.ViewModel.pinned != null && App.ViewModel.pinned.Count > 0)
            {
                iss["vmPinned"] = App.ViewModel.pinned;
            }
            else if (App.ViewModel.pinned != null && !iss.Contains("vmPinned"))
            {
                iss.Add("vmPinned", App.ViewModel.pinned);
            }
            if (iss.Contains("vmTrashed") && App.ViewModel.trashed != null && App.ViewModel.trashed.Count > 0)
            {
                iss["vmTrashed"] = App.ViewModel.trashed;
            }
            else if (App.ViewModel.trashed != null && !iss.Contains("vmTrashed"))
            {
                iss.Add("vmTrashed", App.ViewModel.trashed);
            }
            if (App.ViewModel.authToken != "" && iss.Contains("authToken"))
            {
                iss["authToken"] = App.ViewModel.authToken;
            }
            else if (App.ViewModel.authToken != "" && !iss.Contains("authToken"))
            {
                iss.Add("authToken", App.ViewModel.authToken);
            }
            if (App.ViewModel.email != "" && iss.Contains("email"))
            {
                iss["email"] = App.ViewModel.email;
            }
            else if (App.ViewModel.email != "" && !iss.Contains("email"))
            {
                iss.Add("email", App.ViewModel.email);
            }
            iss.Save();
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        /*private class QuitException : Exception { }

        public static void Quit()
        {
            throw new QuitException();
        }*/

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Exception: " + e.ExceptionObject.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}