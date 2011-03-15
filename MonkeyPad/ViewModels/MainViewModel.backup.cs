using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
//using Hammock;
//using Hammock.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;


namespace MonkeyPad
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            this.Items = new ObservableCollection<ItemViewModel>();
        }

        public ObservableCollection<ItemViewModel> Items { get; private set; }
        public ObservableCollection<Models.noteModel> notes { get; private set; }
        private bool _checkboxclicked = false;
        public string authToken { get; private set; }
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        public Models.noteIndexModel noteIndex { get; private set; }
        //public Models.noteModel[] notes { get; private set; }
        public String email = "yulian@kuncheff.com";

        public bool CheckBoxClicked
        { 
            get
            {
                return _checkboxclicked;
            }
            set
            {
                _checkboxclicked = value;
                NotifyPropertyChanged("checkBox");
            }
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        static public string EncodeTo64(string toEncode)
		{
			byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
			string returnValue = Convert.ToBase64String(toEncodeAsBytes);
			return returnValue; 
		}

        public void LoadData()
        {
            string loginInfo = "email=yulian@kuncheff.com&password=3817iln3";
            byte[] base64login = Encoding.UTF8.GetBytes(EncodeTo64(loginInfo));

            /*RestClient client = new RestClient
            {
                Authority = "https://simple-note.appspot.com",
                VersionPath = "api",
                HasElevatedPermissions = true
            };
            client.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            RestRequest request = new RestRequest
            {
                Path = "login"
            };
            request.AddPostContent(base64login);

            client.BeginRequest(request, new RestCallback((restRequest, restResponse, userState) =>
                {

                    authToken = restResponse.Content;
                    Deployment.Current.Dispatcher.BeginInvoke(new Action(getNotes));

                }));*/

            /*WebClient client = new WebClient();
            client.UploadStringCompleted += new UploadStringCompletedEventHandler(getNotes);
            client.Headers["Content-Type"] = "application/x-www-form-urlencoded";
            client.Encoding = Encoding.UTF8;

            client.UploadStringAsync(new Uri("https://simple-note.appspot.com/api/login"), "POST", EncodeTo64(loginInfo));*/

            var client = new RestClient("https://simple-note.appspot.com");
            client.UserAgent = "MonkeyPad 0.1";
            var test = client.DefaultParameters;

            var request = new RestRequest("api/login", Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddBody(EncodeTo64(loginInfo));
            

            client.ExecuteAsync(request, (response) => 
            {
                authToken = response.Content;
                getNotes();
            });

            this.IsDataLoaded = true;
        }

        private void getNotes(/*object sender, UploadStringCompletedEventArgs e*/)
        {
            //authToken = e.Result;
            var test = 1;
            /*WebClient client2 = new WebClient();
            client2.UploadStringCompleted += new UploadStringCompletedEventHandler(getNoteText);
            client2.Headers["Content-Type"] = "application/x-www-form-urlencoded";
            client2.Encoding = Encoding.UTF8;

            client2.UploadStringAsync(new Uri("https://simpdwle-note.appspot.com/api2/index"), "GET", "length=20&auth="+authToken+"&email="+email);*/

            
            /*RestClient client2 = new RestClient
            {
                Authority = "https://simple-note.appspot.com",
                VersionPath = "api2",
                HasElevatedPermissions = true
            };
            client2.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            RestRequest request2 = new RestRequest
            {
                Path = "index"
            };
            request2.AddParameter("length", "20");
            request2.AddParameter("auth", authToken);
            request2.AddParameter("email", email);

            
            client2.BeginRequest(request2, new RestCallback((restRequest, restResponse, userState) =>
            {
                this.noteIndex = JsonConvert.DeserializeObject<Models.noteIndexModel>(restResponse.Content);
                Deployment.Current.Dispatcher.BeginInvoke(new Action(getNoteText));

                               
            }));*/
            
            
        }

        private void getNoteText(/*object sender, UploadStringCompletedEventArgs e*/)
        {
            //this.noteIndex = JsonConvert.DeserializeObject<Models.noteIndexModel>(e.Result);
            
            Models.noteIndexModelNotes[] noTextNotes = noteIndex.Notes;

            int counter = 0;
            foreach (Models.noteIndexModelNotes note in noTextNotes)
            {
                
                /*WebClient client3 = new WebClient();
                client3.UploadStringCompleted += new UploadStringCompletedEventHandler(addNotes);
                client3.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                client3.Encoding = Encoding.UTF8;

                client3.UploadStringAsync(new Uri("https://simple-note.appspot.com/api2/data/"+note.Key+"?auth=" + authToken + "&email=" + email), "GET", "");*/
                
                /*RestClient client3 = new RestClient
                {
                    Authority = "https://simple-note.appspot.com",
                    VersionPath = "api2",
                    HasElevatedPermissions = true
                };
                client3.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                RestRequest request3 = new RestRequest
                {
                    Path = note.Key
                };
                request3.AddParameter("auth", authToken);
                request3.AddParameter("email", email);


                client3.BeginRequest(request3, new RestCallback((restRequest, restResponse, userState) =>
                {
                    Models.noteModel noteObject = null;
                    noteObject = JsonConvert.DeserializeObject<Models.noteModel>(restResponse.Content);
                    notes.Add(noteObject);
                    addNotesToView(noteObject);
                    counter++;
                }));*/
                 
                             
            }

            
        }

        private void addNotes(object sender, UploadStringCompletedEventArgs e)
        {
            Models.noteModel noteObject = null;
            noteObject = JsonConvert.DeserializeObject<Models.noteModel>(e.Result);
            notes.Add(noteObject);
            addNotesToView(noteObject);
        }

        private void addNotesToView(Models.noteModel noteObject)
        {
                System.DateTime date= new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                date = date.AddSeconds(noteObject.CreateDate);
                ((App)App.Current).RootFrame.Dispatcher.BeginInvoke(new Action<ItemViewModel>(item =>
                {
                    this.Items.Add(item);
                }), new ItemViewModel
                {
                    Title = noteObject.Content,
                    Text = noteObject.Content,
                    Date = date.ToShortDateString(),
                    CheckBox = true
                });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

/* Code Snippets for future Use.

---Dispatcher add---

*/