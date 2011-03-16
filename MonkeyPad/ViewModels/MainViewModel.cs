using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Wintellect.Threading;
using Wintellect.Threading.AsyncProgModel;


namespace MonkeyPad
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            this.notes = new Models.SortableObservableCollection<Models.noteModel>();
            this.pinned = new Models.SortableObservableCollection<Models.noteModel>();
            this.trashed = new Models.SortableObservableCollection<Models.noteModel>();
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
        }

        /* Lists and Collections */
        public Models.SortableObservableCollection<Models.noteModel> notes { get; set; }
        public Models.SortableObservableCollection<Models.noteModel> pinned { get; set; }
        public Models.SortableObservableCollection<Models.noteModel> trashed { get; set; }
        
        /* Other Variables */
		public bool IsDataLoaded { get; set; }
		public bool IsLoggedIn { get; set; }
        public bool HasBeenToLogin { get; set; }
        public bool HasEnteredLoginInfo { get; set; }
        public bool IsSorted { get; set; }
        public bool HasMore { get; set; }
        public bool refreshUpdate { get; set; }
        public bool firstLoadLogin { get; set; }
        public bool loading { get; set; }
        public bool alreadyAdded { get; set; }
        public bool sendUpdateDone = true;
        JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
        public int counter = 0;

        /* Note Variables */
        public string returnedJson = "";
        public Models.noteIndexModel noteIndex { get; set; }
        public Models.noteIndexModel markNoteIndex { get; set; }
        public Models.noteIndexModel noteRefreshIndex { get; set; }

        /* Login Info */
        public string authToken { get; set; }
        public string email = "";
		public string base64login = "";

        /* Date Arrays */
        public string[] Months = { "Invalid Month", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        public string[] MonthAbr = { "Inv", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        public void LoadData()
        {
            try
            {
                if (firstLoadLogin)
                {
                    AsyncEnumerator asyncEnum = new AsyncEnumerator();

                    IsolatedStorageSettings iss = IsolatedStorageSettings.ApplicationSettings;
                    if ((!iss.Contains("authToken") || authToken == "") && HasEnteredLoginInfo && !IsLoggedIn)
                    {
                        asyncEnum.BeginExecute(login(asyncEnum), new AsyncCallback((result) =>
                        {
                            asyncEnum.EndExecute(result);
                            AsyncEnumerator asyncEnum2 = new AsyncEnumerator();
                            asyncEnum2.BeginExecute(getData(asyncEnum2), new AsyncCallback((result2) =>
                            {
                                asyncEnum2.EndExecute(result2);
                                AsyncEnumerator asyncEnum3 = new AsyncEnumerator();
                                asyncEnum3.BeginExecute(updateData(asyncEnum3), new AsyncCallback((result3) =>
                                {
                                    try
                                    {
                                        asyncEnum3.EndExecute(result3);
                                        loading = false;
                                        updateUI();
                                    }
                                    catch (InvalidOperationException e)
                                    {

                                    }
                                }));
                            }));
                        }));
                    }
                    else
                    {
                        asyncEnum.BeginExecute(getData(asyncEnum), new AsyncCallback((result) =>
                        {
                            asyncEnum.EndExecute(result);
                            AsyncEnumerator asyncEnum2 = new AsyncEnumerator();
                            asyncEnum2.BeginExecute(updateData(asyncEnum2), new AsyncCallback((result2) =>
                            {
                                asyncEnum2.EndExecute(result2);
                                loading = false;
                                updateUI();
                            }));
                        }));
                    }
                }
            }
            catch (Exception)
            {

            }
        
        }

        private IEnumerator<Int32> login(AsyncEnumerator asyncEnum)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://simple-note.appspot.com/api/login");
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            request.BeginGetRequestStream(asyncEnum.End(), null);
            asyncEnum.SyncContext = null;
            yield return 1;

            Stream requestStream = request.EndGetRequestStream(asyncEnum.DequeueAsyncResult());
            requestStream.Write(Encoding.UTF8.GetBytes(base64login), 0, base64login.Length);
            requestStream.Close();
            request.BeginGetResponse(asyncEnum.End(), null);
            asyncEnum.SyncContext = null;
            yield return 1;

            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.EndGetResponse(asyncEnum.DequeueAsyncResult());
                Stream responseText = response.GetResponseStream();
                StreamReader sr = new StreamReader(responseText);
                authToken = sr.ReadToEnd();
                sr.Close();
                responseText.Close();
                response.Close();

                this.IsLoggedIn = true;
                IsolatedStorageSettings iss = IsolatedStorageSettings.ApplicationSettings;
                if (iss.Contains("authToken"))
                {
                    iss["authToken"] = authToken;
                }
                else
                {
                    iss.Add("authToken", authToken);
                }
                if (iss.Contains("email"))
                {
                    iss["email"] = email;
                }
                else
                {
                    iss.Add("email", email);
                }
                iss.Save();
                if (authToken == "" || authToken == null)
                {
                    throw new LoginFailedException();
                }
            }
            catch (LoginFailedException e)
            {
                e.ToString();
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
                    MessageBox.Show("Login information incorrect or token is outdated. Please re-enter it.");
                    ((App)App.Current).RootFrame.Navigate(new Uri("/Login.xaml", UriKind.Relative));
                });
                yield break;
            }
            catch (WebException e)
            {
                if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized)
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
                        MessageBox.Show("Login information incorrect or token is outdated. Please re-enter it.");
                        ((App)App.Current).RootFrame.Navigate(new Uri("/Login.xaml", UriKind.Relative));
                    });
                    yield break;
                }
                else
                {
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error logging in. Please check that you are connected and try again."); App.ViewModel.loading = false; });
                    App.ViewModel.loading = false;
                    App.ViewModel.IsDataLoaded = false;
                    yield break;
                }
            }
            catch (IOException e)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("IO Exception: " + e.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
                App.ViewModel.loading = false;
                App.ViewModel.IsDataLoaded = false;
                yield break;
            }
            catch (Exception e)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Exception: " + e.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    // An unhandled exception has occurred; break into the debugger
                    System.Diagnostics.Debugger.Break();
                }
                App.ViewModel.loading = false;
                App.ViewModel.IsDataLoaded = false;
                yield break;
            }
        }
		
        private IEnumerator<Int32> getData(AsyncEnumerator asyncEnum)
        {
            String URL = "https://simple-note.appspot.com/api2/index?length=50&auth=" + authToken + "&email=" + email;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            //request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "GET";
            request.BeginGetResponse(asyncEnum.End(), null);
            asyncEnum.SyncContext = null;
            yield return 1;

            HttpWebResponse response = null;
            string result = "";
            bool loginfailure = false;
            try
            {
                if (authToken == "" || authToken == null)
                {
                    loginfailure = true;
                }
                response = (HttpWebResponse)request.EndGetResponse(asyncEnum.DequeueAsyncResult());
                Stream responseText = response.GetResponseStream();
                StreamReader sr = new StreamReader(responseText);
                result = sr.ReadToEnd();
                sr.Close();
                responseText.Close();
                response.Close();
            }
            catch (WebException ee)
            {
                if(loginfailure)
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
                        App temp = ((App)App.Current);
                        if (((App)App.Current).RootFrame.CurrentSource.OriginalString != "/Login.xaml")
                        {
                            ((App)App.Current).RootFrame.Navigate(new Uri("/Login.xaml", UriKind.Relative));
                        }
                    });
                    yield break;
                }
                else if (ee is WebException)
                {
                    if (((HttpWebResponse)(((WebException)ee).Response)).StatusCode == HttpStatusCode.Unauthorized)
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
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error retrieving note index. Please check that you are connected and try again."); App.ViewModel.loading = false; });
                        App.ViewModel.loading = false;
                        App.ViewModel.IsDataLoaded = false;
                        yield break;
                    }
                }
                else
                {
                    throw;
                }
            }
            catch (IOException e)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("IO Exception: " + e.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
                App.ViewModel.loading = false;
                App.ViewModel.IsDataLoaded = false;
                yield break;
            }
            catch (Exception e)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Exception: " + e.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    // An unhandled exception has occurred; break into the debugger
                    System.Diagnostics.Debugger.Break();
                }
                App.ViewModel.loading = false;
                App.ViewModel.IsDataLoaded = false;
                yield break;
            }

            this.noteIndex = JsonConvert.DeserializeObject<Models.noteIndexModel>(result);
            this.IsDataLoaded = true;
            if (noteIndex != null && noteIndex.Mark != null)
            {
                HasMore = true;
            }
            else
            {
                HasMore = false;
            }
            //updateData(asyncEnum);
        }

        private IEnumerator<Int32> updateData(AsyncEnumerator asyncEnum)
        {
            if ((noteIndex != null) && (noteIndex.Data != null))
            {
                foreach (Models.noteModel item in noteIndex.Data)
                {
                    String URL = "https://simple-note.appspot.com/api2/data/" + item.Key + "?auth=" + App.ViewModel.authToken + "&email=" + App.ViewModel.email;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                    //request.ContentType = "application/x-www-form-urlencoded";
                    request.Method = "GET";
                    request.BeginGetResponse(asyncEnum.End(), null);
                    asyncEnum.SyncContext = null;
                    yield return 1;

                    string result = "";
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asyncEnum.DequeueAsyncResult());
                        Stream responseText = response.GetResponseStream();
                        StreamReader sr = new StreamReader(responseText);
                        result = sr.ReadToEnd();
                        sr.Close();
                        responseText.Close();
                        response.Close();

                        Models.noteModel noteObject = new Models.noteModel();
                        noteObject = JsonConvert.DeserializeObject<Models.noteModel>(result);

                        System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                        String _title = "", _text = "", _temptext="";
                        String[] firstLines = noteObject.Content.Trim().Split('\n');
                        if (firstLines[0].Length >= 23)
                        {
                            _title = firstLines[0].Substring(0, 23) + "...";
                        }
                        else
                        {
                            _title = firstLines[0];
                        }
                        _temptext = noteObject.Content.Replace('\n', ' ');
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
                        date = date.AddSeconds((double)item.ModifyDate);

                        item.Content = noteObject.Content;

                        if (refreshUpdate)
                        {
                            object[] array = new object[2];
                            array[0] = item;
                            array[1] = date;
                            ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((item2) => { item2.DisplayTitle = _title; }), item);
                            ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((item2) => { item2.DisplayContent = _text; }), item);
                            ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel, System.DateTime>((item2, date2) => { item2.DisplayDate = App.ViewModel.MonthAbr[date2.Month] + " " + date2.Day; }), array);
                        }
                        else
                        {
                            item.DisplayTitle = _title;
                            item.DisplayContent = _text;
                            item.DisplayDate = App.ViewModel.MonthAbr[date.Month] + " " + date.Day;
                        }

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
                            asyncEnum.Cancel(true);
                            yield break;
                        }
                        else
                        {
                            ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error retrieving a note. Please check that you are connected and try again."); App.ViewModel.loading = false; });
                            asyncEnum.Cancel(true);
                            App.ViewModel.loading = false;
                            yield break;
                        }
                    }
                    catch (IOException e)
                    {
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("IO Exception: " + e.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
                        App.ViewModel.loading = false;
                        yield break;
                    }
                    catch (Exception e)
                    {
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Exception: " + e.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            // An unhandled exception has occurred; break into the debugger
                            System.Diagnostics.Debugger.Break();
                        }
                        App.ViewModel.loading = false;
                        yield break;
                    }
                }
            }
            else
            {
                asyncEnum.Cancel(true);
            }
        }

        private void updateUI()
        {
            if (noteIndex != null && noteIndex.Data != null)
            {
                foreach (Models.noteModel item in noteIndex.Data)
                {
                    if (item.SystemTags.Length > 0)
                    {
                        if (item.SystemTags[0] == "pinned")
                        {
                            ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.pinned.Add(aItem); }), item);
                        }

                    }
                    else if (item.Deleted)
                    {
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.trashed.Add(aItem); }), item);
                    }
                    else
                    {
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.notes.Add(aItem); }), item);
                    }
                }
                sortAll();
            }
        }

        private void updateVisualInfo(Models.SortableObservableCollection<Models.noteModel> noteList)
        {
            foreach (Models.noteModel item in noteList)
            {
                System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                String _title = "", _text = "", _temptext = "";
                String[] firstLines = item.Content.Trim().Split('\n');
                if (firstLines[0].Length >= 25)
                {
                    _title = firstLines[0].Substring(0, 23) + "...";
                }
                else
                {
                    _title = firstLines[0];
                }
                _temptext = item.Content.Replace('\n', ' ');
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
                date = date.AddSeconds((double)item.ModifyDate);
                object[] array = new object[2];
                array[0] = item;
                array[1] = date;
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((item2) => { item2.DisplayTitle = _title; }), item);
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((item2) => { item2.DisplayContent = _text; }), item);
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel, System.DateTime>((item2, date2) => { item2.DisplayDate = App.ViewModel.MonthAbr[date2.Month] + " " + date2.Day; }), array);
            }
        }

		public void clearLists()
		{
			((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { this.notes.Clear(); });
			((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { this.pinned.Clear(); });
			((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { this.trashed.Clear(); });
            ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { this.notes = new Models.SortableObservableCollection<Models.noteModel>(); });
            ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { this.pinned = new Models.SortableObservableCollection<Models.noteModel>(); });
            ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { this.trashed = new Models.SortableObservableCollection<Models.noteModel>(); });
            if (noteIndex != null && noteIndex.Data != null)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { this.noteIndex.Data.Clear(); });
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { this.noteIndex.Data = new Models.SortableObservableCollection<Models.noteModel>(); this.noteIndex.Count = 0; });
            }
            if (markNoteIndex != null && markNoteIndex.Data != null)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { this.markNoteIndex.Data.Clear(); });
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { this.markNoteIndex.Data = new Models.SortableObservableCollection<Models.noteModel>(); this.markNoteIndex.Count = 0; });
            }
		}

        public void sortAll()
        {
            if (notes != null && notes.Count > 0)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { App.ViewModel.notes.Sort(new Models.ModifyDateComparer()); });
                IsSorted = true;
            }
            if (pinned != null && pinned.Count > 0)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { App.ViewModel.pinned.Sort(new Models.ModifyDateComparer()); });
                IsSorted = true;
            }
            if (trashed != null && trashed.Count > 0)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { App.ViewModel.trashed.Sort(new Models.ModifyDateComparer()); });
                IsSorted = true;
            }
            if (noteIndex != null && noteIndex.Data != null && noteIndex.Data.Count > 0)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { App.ViewModel.noteIndex.Data.Sort(new Models.ModifyDateComparer()); });
                IsSorted = true;
            }
            //((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => {  });
            

        }

        public void pinItem(string Key)
        {
            bool found = false;
            Models.noteModel workingItem = null;
            foreach (Models.noteModel item in notes)
            {
                if (item.Key == Key)
                {
                    found = true;
                    item.SystemTags = new string[1] {"pinned"};
                    workingItem = item;
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.pinned.Add(aItem); }), item);
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.notes.Remove(aItem); }), item);
                    break;
                }
            }

            if (!found)
            {
                foreach (Models.noteModel item in trashed)
                {
                    if (item.Key == Key)
                    {
                        found = true;
                        item.SystemTags = new string[1] { "pinned" };
                        workingItem = item;
                        //((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.pinned.Add(aItem); }), item);
                        //((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.trashed.Remove(aItem); }), item);
                        break;
                    }
                }
            }
            Models.editNoteModel pinUpdate = new Models.editNoteModel();
            //pinUpdate.content = workingItem.Content;
            pinUpdate.version = workingItem.Version;
            pinUpdate.systemtags = workingItem.SystemTags;
            pinUpdate.deleted = workingItem.Deleted;

            string createdJson = JsonConvert.SerializeObject(pinUpdate, Formatting.None, jsonSettings);
            AsyncEnumerator asyncEnum = new AsyncEnumerator();
            asyncEnum.BeginExecute(sendUpdateData(asyncEnum, createdJson, workingItem.Key), new AsyncCallback((result) =>
            {
                asyncEnum.EndExecute(result);
                Models.noteModel noteObject = JsonConvert.DeserializeObject<Models.noteModel>(returnedJson);
                processResponse(noteObject, workingItem);
                App.ViewModel.sendUpdateDone = true;
            }));
        }

        public void trashItem(string Key)
        {
            bool found = false;
            Models.noteModel workingItem = null;
            foreach (Models.noteModel item in notes)
            {
                if (item.Key == Key)
                {
                    found = true;
                    item.Deleted = true;
                    workingItem = item;
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.trashed.Add(aItem); }), item);
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.notes.Remove(aItem); }), item);
                    break;
                }
            }

            if (!found)
            {
                foreach (Models.noteModel item in pinned)
                {
                    if (item.Key == Key)
                    {
                        found = true;
                        item.Deleted = true;
                        workingItem = item;
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.trashed.Add(aItem); }), item);
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.pinned.Remove(aItem); }), item);
                        break;
                    }
                }
            }
            Models.editNoteModel trashUpdate = new Models.editNoteModel();
            //trashUpdate.content = workingItem.Content;
            trashUpdate.version = workingItem.Version;
            trashUpdate.systemtags = workingItem.SystemTags;
            trashUpdate.deleted = workingItem.Deleted;

            string createdJson = JsonConvert.SerializeObject(trashUpdate, Formatting.None, jsonSettings);
            AsyncEnumerator asyncEnum = new AsyncEnumerator();
            asyncEnum.BeginExecute(sendUpdateData(asyncEnum, createdJson, workingItem.Key), new AsyncCallback((result) =>
            {
                asyncEnum.EndExecute(result);
                Models.noteModel noteObject = JsonConvert.DeserializeObject<Models.noteModel>(returnedJson);
                processResponse(noteObject, workingItem);
                App.ViewModel.sendUpdateDone = true;
            }));
        }

        public void untrashItem(string Key)
        {
            //bool found = false;
            Models.noteModel workingItem = null;
            foreach (Models.noteModel item in trashed)
            {
                if (item.Key == Key)
                {
                    //found = true;
                    item.Deleted = false;
                    workingItem = item;
                    if (item.SystemTags.Length > 0 && item.SystemTags[0] == "pinned")
                    {
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.pinned.Add(aItem); }), item);
                    }
                    else
                    {
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.notes.Add(aItem); }), item);
                    }
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.trashed.Remove(aItem); }), item);
                    break;
                }
            }
            Models.editNoteModel trashUpdate = new Models.editNoteModel();
            //trashUpdate.content = workingItem.Content;
            trashUpdate.version = workingItem.Version;
            trashUpdate.systemtags = workingItem.SystemTags;
            trashUpdate.deleted = workingItem.Deleted;

            string createdJson = JsonConvert.SerializeObject(trashUpdate, Formatting.None, jsonSettings);
            AsyncEnumerator asyncEnum = new AsyncEnumerator();
            asyncEnum.BeginExecute(sendUpdateData(asyncEnum, createdJson, workingItem.Key), new AsyncCallback((result) =>
            {
                asyncEnum.EndExecute(result);
                Models.noteModel noteObject = JsonConvert.DeserializeObject<Models.noteModel>(returnedJson);
                processResponse(noteObject, workingItem);
                App.ViewModel.sendUpdateDone = true;
            }));
        }

        public void unpinItem(string Key)
        {
            //bool found = false;
            Models.noteModel workingItem = null;
            foreach (Models.noteModel item in pinned)
            {
                if (item.Key == Key)
                {
                    //found = true;
                    item.SystemTags = new string[0];
                    workingItem = item;
                    if (item.Deleted)
                    {
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.trashed.Add(aItem); }), item);
                    }
                    else
                    {
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.notes.Add(aItem); }), item);
                    }
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.pinned.Remove(aItem); }), item);
                    break;
                }
            }
            Models.editNoteModel pinUpdate = new Models.editNoteModel();
            //pinUpdate.content = workingItem.Content;
            pinUpdate.version = workingItem.Version;
            pinUpdate.systemtags = workingItem.SystemTags;
            pinUpdate.deleted = workingItem.Deleted;

            string createdJson = JsonConvert.SerializeObject(pinUpdate, Formatting.None, jsonSettings);
            AsyncEnumerator asyncEnum = new AsyncEnumerator();
            asyncEnum.BeginExecute(sendUpdateData(asyncEnum,createdJson,workingItem.Key), new AsyncCallback((result) =>
            {
                asyncEnum.EndExecute(result);
                Models.noteModel noteObject = JsonConvert.DeserializeObject<Models.noteModel>(returnedJson);
                processResponse(noteObject, workingItem);
                App.ViewModel.sendUpdateDone = true;
            }));           
        }

        private IEnumerator<Int32> sendUpdateData(AsyncEnumerator asyncEnum, string jsonBody, string Key)
        {
            sendUpdateDone = false;
            String URL = "https://simple-note.appspot.com/api2/data/" + Key + "?auth=" + App.ViewModel.authToken + "&email=" + App.ViewModel.email;
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
                }
                else
                {
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error syncing note update. Please check that you are connected and try again."); App.ViewModel.loading = false; });
                    yield break;
                }
                asyncEnum.Cancel(true);
            }
            catch (IOException e)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("IO Exception: " + e.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
                yield break;
            }
            catch (Exception e)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Exception: " + e.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    // An unhandled exception has occurred; break into the debugger
                    System.Diagnostics.Debugger.Break();
                }
                yield break;
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
                }
                else
                {
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error syncing note update. Please check that you are connected and try again."); App.ViewModel.loading = false; });
                    yield break;
                }
                asyncEnum.Cancel(true);
            }
            catch (IOException e)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("IO Exception: " + e.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
                yield break;
            }
            catch (Exception e)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Exception: " + e.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    // An unhandled exception has occurred; break into the debugger
                    System.Diagnostics.Debugger.Break();
                }
                yield break;
            }
            Stream responseText = response.GetResponseStream();
            StreamReader sr = new StreamReader(responseText);
            returnedJson = sr.ReadToEnd();
            sr.Close();
            responseText.Close();
            response.Close();

        }

        private void processResponse(Models.noteModel noteObject, Models.noteModel currentNote)
        {
            try
            {

                if (noteObject.Content != null)
                {
                    currentNote.Content = noteObject.Content;
                }
                currentNote.Deleted = noteObject.Deleted;
                currentNote.MinVersion = noteObject.MinVersion;
                currentNote.SyncNum = noteObject.SyncNum;
                currentNote.Tags = noteObject.Tags;
                currentNote.SystemTags = noteObject.SystemTags;
                currentNote.Version = noteObject.Version;
                currentNote.ModifyDate = noteObject.ModifyDate;
                if (noteObject.Content != null)
                {
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
                            if (firstLines[0].Length >= 25)
                            {
                                _title = firstLines[0].Substring(0, 25) + "...";
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
                }
                App.ViewModel.sortAll();
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
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error updating note. Please check that you are connected and try again."); App.ViewModel.loading = false; });
                }
            }
            catch (IOException e)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("IO Exception: " + e.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
            }
            catch (Exception e)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Exception: " + e.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    // An unhandled exception has occurred; break into the debugger
                    System.Diagnostics.Debugger.Break();
                }
            }
        }

        public void getMoreFromMark()
        {
            AsyncEnumerator asyncEnum = new AsyncEnumerator();
            asyncEnum.BeginExecute(getMarkData(asyncEnum), new AsyncCallback((result) =>
            {
                asyncEnum.EndExecute(result);
                AsyncEnumerator asyncEnum2 = new AsyncEnumerator();
                asyncEnum2.BeginExecute(updateMarkData(asyncEnum2), new AsyncCallback((result2) =>
                {
                    asyncEnum2.EndExecute(result2);
                    //clearLists();
                    updateMarkUI();
                }));
            }));
        }

        private IEnumerator<Int32> getMarkData(AsyncEnumerator asyncEnum)
        {
            String URL = "https://simple-note.appspot.com/api2/index?length=5&mark=" + noteIndex.Mark + "&auth=" + authToken + "&email=" + email;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            //request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "GET";
            request.BeginGetResponse(asyncEnum.End(), null);
            asyncEnum.SyncContext = null;
            yield return 1;

            HttpWebResponse response = null;
            string result = "";
            try
            {
                response = (HttpWebResponse)request.EndGetResponse(asyncEnum.DequeueAsyncResult());
                Stream responseText = response.GetResponseStream();
                StreamReader sr = new StreamReader(responseText);
                result = sr.ReadToEnd();
                sr.Close();
                responseText.Close();
                response.Close();
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
                    ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error retrieving note index. Please check that you are connected and try again."); App.ViewModel.loading = false; });
                    yield break;
                }
                asyncEnum.Cancel(true);
            }
            catch (IOException e)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("IO Exception: " + e.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
                yield break;
            }
            catch (Exception e)
            {
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Exception: " + e.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    // An unhandled exception has occurred; break into the debugger
                    System.Diagnostics.Debugger.Break();
                }
                yield break;
            }
            this.markNoteIndex = JsonConvert.DeserializeObject<Models.noteIndexModel>(result);
        }

        private IEnumerator<Int32> updateMarkData(AsyncEnumerator asyncEnum)
        {
            if ((markNoteIndex != null) && (markNoteIndex.Data != null))
            {
                foreach (Models.noteModel item in markNoteIndex.Data)
                {
                    String URL = "https://simple-note.appspot.com/api2/data/" + item.Key + "?auth=" + App.ViewModel.authToken + "&email=" + App.ViewModel.email;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                    //request.ContentType = "application/x-www-form-urlencoded";
                    request.Method = "GET";
                    request.BeginGetResponse(asyncEnum.End(), null);
                    asyncEnum.SyncContext = null;
                    yield return 1;

                    string result = "";
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asyncEnum.DequeueAsyncResult());
                        Stream responseText = response.GetResponseStream();
                        StreamReader sr = new StreamReader(responseText);
                        result = sr.ReadToEnd();
                        sr.Close();
                        responseText.Close();
                        response.Close();

                        Models.noteModel noteObject = new Models.noteModel();
                        noteObject = JsonConvert.DeserializeObject<Models.noteModel>(result);

                        System.DateTime date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                        String _title = "", _text = "", _temptext = "";
                        String[] firstLines = noteObject.Content.Trim().Split('\n');
                        if (firstLines[0].Length >= 25)
                        {
                            _title = firstLines[0].Substring(0, 25) + "...";
                        }
                        else
                        {
                            _title = firstLines[0];
                        }
                        _temptext = noteObject.Content.Replace('\n', ' ');
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
                        date = date.AddSeconds((double)item.ModifyDate);

                        item.Content = noteObject.Content;
                        if (refreshUpdate)
                        {
                            object[] array = new object[2];
                            array[0] = item;
                            array[1] = date;
                            ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((item2) => { item2.DisplayTitle = _title; }), item);
                            ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((item2) => { item2.DisplayContent = _text; }), item);
                            ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel, System.DateTime>((item2, date2) => { item2.DisplayDate = App.ViewModel.MonthAbr[date2.Month] + " " + date2.Day; }), array);

                        }
                        else
                        {
                            item.DisplayTitle = _title;
                            item.DisplayContent = _text;
                            item.DisplayDate = App.ViewModel.MonthAbr[date.Month] + " " + date.Day;
                        }
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
                            ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error retrieving a note. Please check that you are connected and try again."); App.ViewModel.loading = false; });
                            yield break;
                        }
                        asyncEnum.Cancel(true);
                    }
                    catch (IOException e)
                    {
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("IO Exception: " + e.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
                        yield break;
                    }
                    catch (Exception e)
                    {
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(() => { MessageBox.Show("Exception: " + e.Message + " \n\n If you get this message, report it to me ASAP"); App.ViewModel.loading = false; });
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            // An unhandled exception has occurred; break into the debugger
                            System.Diagnostics.Debugger.Break();
                        }
                        yield break;
                    }
                }
                noteIndex.Data.Concat(markNoteIndex.Data);
                if (noteIndex.Mark == markNoteIndex.Mark || markNoteIndex.Mark == null)
                {
                    noteIndex.Mark = null;
                    HasMore = false;
                }
                else
                {
                    noteIndex.Mark = markNoteIndex.Mark;
                    HasMore = true;
                }
            }
            else
            {
                asyncEnum.Cancel(true);
            }
        }

        private void updateMarkUI()
        {
            if (markNoteIndex != null && markNoteIndex.Data != null)
            {
                foreach (Models.noteModel item in markNoteIndex.Data)
                {
                    if (item.SystemTags.Length > 0)
                    {
                        if (item.SystemTags[0] == "pinned")
                        {
                            ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.pinned.Add(aItem); }), item);
                        }

                    }
                    else if (item.Deleted)
                    {
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.trashed.Add(aItem); }), item);
                    }
                    else
                    {
                        ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<Models.noteModel>((aItem) => { this.notes.Add(aItem); }), item);
                    }
                }
                sortAll();
            }
        }

        public void refreshData()
        {
            if (!loading)
            {
                loading = true;
                refreshUpdate = true;
                AsyncEnumerator asyncEnum = new AsyncEnumerator();
                asyncEnum.BeginExecute(updateData(asyncEnum), new AsyncCallback((result) =>
                {
                    asyncEnum.EndExecute(result);
                    //updateUI();
                    IsSorted = true;
                    loading = false;
                    refreshUpdate = false;
                }));
            }
        }

        public void refreshAll()
        {
            if (!loading)
            {
                loading = true;
                clearLists();

                AsyncEnumerator asyncEnum = new AsyncEnumerator();
                asyncEnum.BeginExecute(getData(asyncEnum), new AsyncCallback((result) =>
                {
                    asyncEnum.EndExecute(result);
                    AsyncEnumerator asyncEnum2 = new AsyncEnumerator();
                    asyncEnum2.BeginExecute(updateData(asyncEnum2), new AsyncCallback((result2) =>
                    {
                        asyncEnum2.EndExecute(result2);
                        loading = false;
                        updateUI();
                    }));
                }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}