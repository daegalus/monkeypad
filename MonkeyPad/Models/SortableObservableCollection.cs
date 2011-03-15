using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
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
    public class SortableObservableCollection<T> : ObservableCollection<T>
    {
        public void Sort()   
        {   
            this.Sort(0, Count, null);   
        }   
        public void Sort(IComparer<T> comparer)
        {
            this.Sort(0, Count, comparer);
        }
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            (Items as List<T>).Sort(index, count, comparer);
        }
    }
}
