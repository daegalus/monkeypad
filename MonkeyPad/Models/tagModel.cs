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
    public class tagModel
    {
        public string tagName;
        public bool visible { get; set; }
        public SortableObservableCollection<Models.noteModel> notes;
    }
}
