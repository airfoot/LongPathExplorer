using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace LongPathExplorer
{
    class CustomListView : ViewBase
    {
        private DataTemplate itemTemplate;
        public DataTemplate ItemTemplate
        {
            get { return itemTemplate; }
            set { itemTemplate = value; }
        }
        protected override object DefaultStyleKey
        {
            get { return new ComponentResourceKey(GetType(), "CustomListView"); }
        }

        protected override object ItemContainerDefaultStyleKey
        {
            get { return new ComponentResourceKey(GetType(), "CustomListViewItem"); }
        }       
    }
}
