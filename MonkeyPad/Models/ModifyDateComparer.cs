using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
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
    public class ModifyDateComparer : IComparer<Models.noteModel>
    {
        #region IComparer<Models.noteModel> Members
        public int Compare(Models.noteModel x, Models.noteModel y)
        {
            Type pi = x.GetType();
            return decimal.Compare(y.ModifyDate, x.ModifyDate);  // sort by name
        }
        #endregion
    }
}
