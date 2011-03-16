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
using System.Windows.Data;
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
    public partial class editView : PhoneApplicationPage
    {
		Models.noteModel currentNote = null;
        String createdJson = "";
        String returnedJson = "";
        String currentNoteBox = "";
        public bool done = false;
        JsonSerializerSettings jsonSettings = new JsonSerializerSettings();

        public editView()
        {
            InitializeComponent();
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Call the base implementation
            base.OnNavigatedTo(e);
			var test = e.Uri;
            string noteKey = "";
            string list = "";
            if(NavigationContext.QueryString.TryGetValue("key", out noteKey) && NavigationContext.QueryString.TryGetValue("list", out list))
            {
                if (list == "notes")
                {
                    foreach (Models.noteModel note in App.ViewModel.notes)
                    {
                        if (note.Key == noteKey)
                        {
                            noteTextBox.Text = note.Content;
                            System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                            date = date.AddSeconds((double)note.CreateDate);
                            dateText.Text = date.ToLongDateString();
                            currentNote = note;
                        }
                    }
                }
                else if (list == "pinned")
                {
                    foreach (Models.noteModel note in App.ViewModel.pinned)
                    {
                        if (note.Key == noteKey)
                        {
                            noteTextBox.Text = note.Content;
                            System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                            date = date.AddSeconds((double)note.CreateDate);
                            dateText.Text = date.ToLongDateString();
                            currentNote = note;
                        }
                    }
                }
                else if (list == "trashed")
                {
                    foreach (Models.noteModel note in App.ViewModel.trashed)
                    {
                        if (note.Key == noteKey)
                        {
                            noteTextBox.Text = note.Content;
                            System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                            date = date.AddSeconds((double)note.CreateDate);
                            dateText.Text = date.ToLongDateString();
                            currentNote = note;
                        }
                    }
                }
                /*else
                {
                    Boolean found = false;
                    if(!found)
                    {
                        foreach (Models.noteModel note in App.ViewModel.notes)
                        {
                            if (note.Key == noteKey)
                            {
                                found = true;
                                noteTextBox.Text = note.Content;
                                System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                                date = date.AddSeconds((double)note.CreateDate);
                                dateText.Text = date.ToLongDateString();
                                currentNote = note;
                            }
                        }
                    }
                    if(!found)
                    {
                        foreach (Models.noteModel note in App.ViewModel.pinned)
                        {
                            if (note.Key == noteKey)
                            {
                                found = true;
                                noteTextBox.Text = note.Content;
                                System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                                date = date.AddSeconds((double)note.CreateDate);
                                dateText.Text = date.ToLongDateString();
                                currentNote = note;
                            }
                        }
                    }
                    if(!found)
                    {
                        foreach (Models.noteModel note in App.ViewModel.trashed)
                        {
                            if (note.Key == noteKey)
                            {
                                found = true;
                                noteTextBox.Text = note.Content;
                                System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                                date = date.AddSeconds((double)note.CreateDate);
                                dateText.Text = date.ToLongDateString();
                                currentNote = note;
                            }
                        }
                    }
                }*/
            }
        }

        private void saveButton_clicked(object sender, EventArgs e)
        {

            if (currentNote.Content != noteTextBox.Text && App.ViewModel.sendUpdateDone)
            {
                Models.editNoteModel note = new Models.editNoteModel();
                noteTextBox.Text = noteTextBox.Text.Replace('\r', '\n');
                currentNoteBox = noteTextBox.Text;
                note.content = noteTextBox.Text;
                note.version = currentNote.Version;
                note.deleted = currentNote.Deleted;
                note.systemtags = currentNote.SystemTags;
                createdJson = JsonConvert.SerializeObject(note, Formatting.None, jsonSettings);
                AsyncEnumerator asyncEnum = new AsyncEnumerator();
                asyncEnum.BeginExecute(sendData(asyncEnum, createdJson), new AsyncCallback((result) =>
                {
                    asyncEnum.EndExecute(result);
                    Models.noteModel noteObject = JsonConvert.DeserializeObject<Models.noteModel>(returnedJson);
                    processResponse(noteObject);
                }));
            }
        }

        private IEnumerator<Int32> sendData(AsyncEnumerator asyncEnum, string jsonBody)
        {
            String URL = "https://simple-note.appspot.com/api2/data/" + currentNote.Key + "?auth=" + App.ViewModel.authToken + "&email=" + App.ViewModel.email;
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
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error syncing note update. Please check that you are connected and try again."); });
                    yield break;
                }
            }
            asyncEnum.SyncContext = null;
            yield return 1;

            Stream requestStream = request.EndGetRequestStream(asyncEnum.DequeueAsyncResult());

            requestStream.Write(Encoding.UTF8.GetBytes(jsonBody), 0, jsonBody.Length);
            requestStream.Close();
            request.BeginGetResponse(asyncEnum.End(), null);
            asyncEnum.SyncContext = null;
            yield return 1;

            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.EndGetResponse(asyncEnum.DequeueAsyncResult());
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
            Stream responseText = response.GetResponseStream();
            StreamReader sr = new StreamReader(responseText);
            returnedJson = sr.ReadToEnd();
            sr.Close();
            responseText.Close();
            response.Close();

        }

        private void cancelButton_clicked(object sender, EventArgs e)
        {
            ((App)App.Current).RootFrame.GoBack();
        }

        private void processResponse(Models.noteModel noteObject)
        {
            try
            {

                if (noteObject.Content != null)
                {
                    currentNote.Content = noteObject.Content;
                }
                else
                {
                    currentNote.Content = currentNoteBox;
                }
                currentNote.Deleted = noteObject.Deleted;
                currentNote.MinVersion = noteObject.MinVersion;
                currentNote.SyncNum = noteObject.SyncNum;
                currentNote.Tags = noteObject.Tags;
                currentNote.SystemTags = noteObject.SystemTags;
                currentNote.Version = noteObject.Version;
                currentNote.ModifyDate = noteObject.ModifyDate;
                Models.SortableObservableCollection<Models.noteModel> collection = null;
                if (currentNote.SystemTags.Length != 0)
                {
                    if (currentNote.SystemTags[0] == "pinned")
                    {
                        collection = App.ViewModel.pinned;
                    }
                }
                else if (currentNote.Deleted == true)
                {
                    collection = App.ViewModel.trashed;
                }
                else
                {
                    collection = App.ViewModel.notes;
                }

                foreach (Models.noteModel item in collection)
                {
                    if (item.Key == currentNote.Key)
                    {
                        item.ModifyDate = currentNote.ModifyDate;
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
                        item.Content = currentNote.Content;
                        item.Deleted = currentNote.Deleted;
                        item.MinVersion = currentNote.MinVersion;
                        item.ModifyDate = currentNote.ModifyDate;
                        item.PublishKey = currentNote.PublishKey;
                        item.ShareKey = currentNote.ShareKey;
                        item.SyncNum = currentNote.SyncNum;
                        item.SystemTags = currentNote.SystemTags;
                        item.Tags = currentNote.Tags;
                        item.Version = currentNote.Version;
                        object[] array = new object[2];
                        array[0] = item;
                        array[1] = date;
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((item2) => { item2.DisplayTitle = _title; }), item);
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((item2) => { item2.DisplayContent = _text; }), item);
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel, System.DateTime>((item2, date2) => { item2.DisplayDate = App.ViewModel.MonthAbr[date2.Month] + " " + date2.Day; }), array);
                    }
                }
                App.ViewModel.sortAll();
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { ((App)App.Current).RootFrame.GoBack(); });


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
            catch (NullReferenceException eee)
            {
                
            }
        }
    }
}
