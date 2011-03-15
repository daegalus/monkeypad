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
    public class noteIndexModel
    {
        public int Count { get; set; }
        public SortableObservableCollection<noteModel> Data { get; set; }
        public string Mark { get; set; }
    }
}
