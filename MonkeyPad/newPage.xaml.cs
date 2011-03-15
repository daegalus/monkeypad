using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Wintellect.Threading.AsyncProgModel;

namespace MonkeyPad
{
    public partial class newNoteView : PhoneApplicationPage
    {
        Models.noteModel globalNoteObject = null;
        String createdJson = "";
        String returnedJson = "";
        String currentNoteBox = "";
        public bool done = false;
        JsonSerializerSettings jsonSettings = new JsonSerializerSettings();

        public newNoteView()
        {
            InitializeComponent();
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Call the base implementation
            base.OnNavigatedTo(e);
            dateText.Text = DateTime.Now.ToLongDateString();
        }

        private void saveButton_clicked(object sender, EventArgs e)
        {
            if (noteTextBox.Text != "")
            {
                Models.editNoteModel note = new Models.editNoteModel();
                noteTextBox.Text = noteTextBox.Text.Replace('\r', '\n');
                currentNoteBox = noteTextBox.Text;
                note.content = noteTextBox.Text;
                //note.version = currentNote.Version;
                createdJson = JsonConvert.SerializeObject(note, Formatting.None, jsonSettings);
                AsyncEnumerator asyncEnum = new AsyncEnumerator();
                asyncEnum.BeginExecute(sendData(asyncEnum, createdJson), new AsyncCallback((result) =>
                {
                    asyncEnum.EndExecute(result);
                    if (globalNoteObject != null)
                    {
                        processResponse(globalNoteObject);
                    }
                }));
            }
            else
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Note is empty, please enter something."); });
            }
        }

        private IEnumerator<Int32> sendData(AsyncEnumerator asyncEnum, string jsonBody)
        {
            String URL = "https://simple-note.appspot.com/api2/data?auth=" + App.ViewModel.authToken + "&email=" + App.ViewModel.email;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            try
            {
                request.BeginGetRequestStream(asyncEnum.End(), null);
            }
            catch (WebException ee)
            {
                if (((HttpWebResponse)(ee.Response)).StatusCode == HttpStatusCode.Unauthorized)
                {
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() =>
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
                        MessageBox.Show("Login information incorrect. Please re-enter it.");
                        ((App)App.Current).RootFrame.Navigate(new Uri("/Login.xaml", UriKind.Relative));
                    });
                    yield break;
                }
                else
                {
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error updating note. Please check that you are connected and try again."); });
                    yield break;
                }
            }
            asyncEnum.SyncContext = null;
            yield return 1;

            Stream requestStream = null;
            try {
                requestStream = request.EndGetRequestStream(asyncEnum.DequeueAsyncResult());
                requestStream.Write(Encoding.UTF8.GetBytes(jsonBody), 0, jsonBody.Length);
                requestStream.Close();
            }
            catch(NullReferenceException e)
            {
                e.ToString();
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error creating new note. Make sure you are logged in and connected to the internet."); });
                yield break;
            }
            request.BeginGetResponse(asyncEnum.End(), null);
            asyncEnum.SyncContext = null;
            yield return 1;

            HttpWebResponse response = null;
            Stream responseText = null;
            StreamReader sr = null;
            Models.noteModel noteObject = new Models.noteModel();
            try
            {
                response = (HttpWebResponse)request.EndGetResponse(asyncEnum.DequeueAsyncResult());
                responseText = response.GetResponseStream();
                sr = new StreamReader(responseText);
                returnedJson = sr.ReadToEnd();
                sr.Close();
                responseText.Close();
                response.Close();

                noteObject = JsonConvert.DeserializeObject<Models.noteModel>(returnedJson);
            }
            catch (WebException ee)
            {
                if (((HttpWebResponse)(ee.Response)).StatusCode == HttpStatusCode.Unauthorized)
                {
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() =>
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
                        MessageBox.Show("Login information incorrect. Please re-enter it.");
                        ((App)App.Current).RootFrame.Navigate(new Uri("/Login.xaml", UriKind.Relative));
                    });
                    yield break;
                }
                else
                {
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error syncing note update. Please check that you are connected and try again."); });
                    yield break;
                }
            }
            
            URL = "https://simple-note.appspot.com/api2/data/" + noteObject.Key + "?auth=" + App.ViewModel.authToken + "&email=" + App.ViewModel.email;
            request = (HttpWebRequest)WebRequest.Create(URL);
            //request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "GET";
            request.BeginGetResponse(asyncEnum.End(), null);
            asyncEnum.SyncContext = null;
            yield return 1;

            try
            {
                response = (HttpWebResponse)request.EndGetResponse(asyncEnum.DequeueAsyncResult());
                responseText = response.GetResponseStream();
                sr = new StreamReader(responseText);
                string result = sr.ReadToEnd();
                sr.Close();
                responseText.Close();
                response.Close();

                noteObject = JsonConvert.DeserializeObject<Models.noteModel>(result);
                globalNoteObject = noteObject;
            }
            catch (WebException ee)
            {
                if (((HttpWebResponse)(ee.Response)).StatusCode == HttpStatusCode.Unauthorized)
                {
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() =>
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
                        MessageBox.Show("Login information incorrect. Please re-enter it.");
                        ((App)App.Current).RootFrame.Navigate(new Uri("/Login.xaml", UriKind.Relative));
                    });
                    yield break;
                }
                else
                {
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error syncing note update. Please check that you are connected and try again."); });
                    yield break;
                }
            }
        }

        private void cancelButton_clicked(object sender, EventArgs e)
        {
            ((App)App.Current).RootFrame.GoBack();
        }

        private void processResponse(Models.noteModel currentNote)
        {
            try
            {
                if (currentNote.Content != null)
                {
                    currentNote.Content = currentNoteBox;
                }
                Models.SortableObservableCollection<Models.noteModel> collection = App.ViewModel.notes;

                System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                String _title = "", _text = "", _temptext = "";
                String[] firstLines = currentNote.Content.Trim().Split('\n');
                if (firstLines[0].Length >= 23)
                {
                    _title = firstLines[0].Substring(0, 23) + "...";
                }
                else
                {
                    _title = firstLines[0];
                }
                _temptext = currentNote.Content.Replace('\n', ' ');
                if (firstLines[0] != "")
                {
                    _temptext = _temptext.Replace(firstLines[0], "");
                }
                _temptext = _temptext.Trim();
                if (_temptext.Length == 0)
                {
                    _text = " ";
                }
                else if (_temptext.Length > 40)
                {

                    _text = _temptext.Substring(0, 40).Trim() + "...";
                }
                else
                {
                    _text = _temptext;
                }
                date = date.AddSeconds((double)currentNote.ModifyDate);
                object[] array = new object[2];
                array[0] = currentNote;
                array[1] = date;
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((item2) => { item2.DisplayTitle = _title; }), currentNote);
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((item2) => { item2.DisplayContent = _text; }), currentNote);
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel,System.DateTime>((item2,date2) => { item2.DisplayDate = App.ViewModel.MonthAbr[date2.Month] + " " + date2.Day; }), array);
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((item2) =>
                {
                    if (App.ViewModel.notes.Count > 0)
                    {
                        App.ViewModel.notes.Insert(0, item2);
                    }
                    else
                    {
                        App.ViewModel.notes.Add(item2);
                    }
                }), currentNote);
                //App.ViewModel.sortAll();
                App.ViewModel.NotifyPropertyChanged("notes");
                App.ViewModel.NotifyPropertyChanged("pinned");
                App.ViewModel.NotifyPropertyChanged("trashed");
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action(() => ((App)App.Current).RootFrame.GoBack()));
            }
            catch (WebException ee)
            {
                if (((HttpWebResponse)(ee.Response)).StatusCode == HttpStatusCode.Unauthorized)
                {
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() =>
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
                        MessageBox.Show("Login information incorrect. Please re-enter it.");
                        ((App)App.Current).RootFrame.Navigate(new Uri("/Login.xaml", UriKind.Relative));
                    });
                }
                else
                {
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error updating note. Please check that you are connected and try again."); });
                }
            }
        }

        private void setFocus(object sender, RoutedEventArgs e)
        {
            noteTextBox.Focus();
        }
    }
}
