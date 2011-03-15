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
    public class editNoteModel
    {
        
        public int version { get; set; }
        public bool deleted { get; set; }
        public string content { get; set; }
        public string[] systemtags { get; set; }

    }
    

}
