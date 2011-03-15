using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MonkeyPad.Models
{
    public class noteIndexModelNotes
    {
        public string Key { get; set; }
        public bool Deleted { get; set; }
        public decimal ModifyDate { get; set; }
        public decimal CreateDate { get; set; }
        public int SyncNum { get; set; }
        public int Version { get; set; }
        public int MinVersion { get; set; }
        public string ShareKey { get; set; }
        public string PublishKey { get; set; }
        public string[] SystemTags { get; set; }
        public string[] Tags { get; set; }
    }
}
