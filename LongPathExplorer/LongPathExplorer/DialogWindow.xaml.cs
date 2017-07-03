using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LongPathExplorer
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
       // private string duplicateFile;
        public DialogWindow(string file)
        {  
            InitializeComponent();
            //changTextBlock();
          //  duplicateFile = file;
            textBlock.Text = file;
        }

   
        private void button_OKClicked(object sender, RoutedEventArgs e)
        {
            
            if(checkBox.IsChecked == true)
            {
                var mainWindow = this.Owner as MainWindow;
                mainWindow.OverrideAll = true;
            }
            this.DialogResult = true;

        }

        private void button_CancelClicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
