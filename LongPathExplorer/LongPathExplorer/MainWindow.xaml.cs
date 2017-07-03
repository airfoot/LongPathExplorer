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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Resources;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows.Media.Animation;
//using System.IO;
using Alphaleonis.Win32.Filesystem;


namespace LongPathExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BuildviewTree();
            comboboxView.SelectedIndex = 0;
        }

        private ObservableCollection<FileFolderInformation> listViewSource;
        private TreeViewItem treeViewItem_GobalPosition;
        private bool bool_EnableFilter = false;
        private List<string> list_FilterFiles = new List<string>();
        private TreeViewItem treeViewItem_GlobalSource = null;
        private TreeViewItem treeViewItem_GlobalDestination = null;
        private ListViewItem listViewItem_GlobalSource = null;
        private ListViewItem listViewItem_GlobalDestination = null;
        private ListView listView_GlobalDestination = null;
        private bool pasteCanExecute = false;
        private bool copy = false;
        private bool cut = false;

        public List<LongPathExplorer.Result> listResult = new List<LongPathExplorer.Result>();
      //  public string ExistingFile { get; set; }

        public bool OverrideAll { get; set; }

    

        public delegate void ProcessbarCallback(int value);
        public event ProcessbarCallback processCallback;

        private void updateProgressbar(int value)
        {
           // progressBar.Value = value;
            progressBar.Dispatcher.Invoke(() => progressBar.Value = value, DispatcherPriority.Render);
        }

        private void treeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            item.Items.Clear();
            DirectoryInfo dir;
            dir = (DirectoryInfo)item.Tag;
            try
             {
                 foreach (DirectoryInfo subDir in dir.GetDirectories())
                 {
                     BitmapImage bitMapImage = new BitmapImage();
                     bitMapImage.BeginInit();
                     bitMapImage.UriSource = new Uri("Images/itemfolder.png", UriKind.Relative);
                     bitMapImage.EndInit();
                     FileFolderInformation filefolderInformation = new FileFolderInformation(subDir.Name, true, bitMapImage);
                     TreeViewItem newItem = new TreeViewItem();
                     newItem.Tag = subDir;
                     newItem.Header = filefolderInformation;
                   //  newItem.MouseLeftButtonDown += treeViewItem_MouseLeftButtonDown;
                     newItem.Items.Add("*");
                     item.Items.Add(newItem);
                 }
             }
             catch
             {
              
             }
            //blow part of code will lift selected item to top line
         //   item.BringIntoView();
            ScrollViewer scroll=null;
            Border scroll_border = VisualTreeHelper.GetChild(treeView, 0) as Border;
            if (scroll_border is Border)
            {
                scroll = scroll_border.Child as ScrollViewer;
            }
            if (scroll != null)
            {
                var point = item.TranslatePoint(new System.Windows.Point(0, -35), scroll);
                scroll.ScrollToVerticalOffset(scroll.ContentVerticalOffset + point.Y);
            }
                       
        }
        //build the root of Treeview
        private void BuildviewTree()
        {
            treeView.Items.Clear();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                TreeViewItem item = new TreeViewItem();
                BitmapImage bitMapImage = new BitmapImage();
                bitMapImage.BeginInit();
                bitMapImage.UriSource = new Uri("Images/diskdrive.png", UriKind.Relative);
                bitMapImage.EndInit();
                FileFolderInformation temp = new FileFolderInformation( drive.ToString(),true,bitMapImage);
                item.Tag = drive.RootDirectory;
                item.Header = temp;
                item.MouseLeftButtonUp += treeViewItem_MouseLeftButtonUp;
                item.Items.Add("*");
                treeView.Items.Add(item);
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //TreeViewItem selectedItem = e.NewValue as TreeViewItem;
            //selectedItem.IsExpanded = true;
            //DirectoryInfo dirInfo = selectedItem.Tag as DirectoryInfo;
            //listViewSource = GetEnumContents(dirInfo);
            //listView.ItemsSource = listViewSource;
            // MessageBox.Show(dirInfo.Name);
        }

        private ObservableCollection<FileFolderInformation> GetEnumContents(DirectoryInfo dir)
        {
            ObservableCollection<FileFolderInformation> enumCollection = new ObservableCollection<FileFolderInformation>();
            try
            {

           
              foreach(DirectoryInfo subDir in dir.GetDirectories())
              {
                BitmapImage folderShowIcon = new BitmapImage();
                folderShowIcon.BeginInit();
                folderShowIcon.UriSource = new Uri("Images/itemfolder.png", UriKind.Relative);
                folderShowIcon.EndInit();
                FileFolderInformation folderInfo = new FileFolderInformation(folderShowIcon,subDir.Name,true,subDir.CreationTime,subDir.LastWriteTime,subDir);
                enumCollection.Add(folderInfo);
              }

              foreach(FileInfo subFile in dir.GetFiles())
              {
                BitmapImage fileShowIcon = GetIcon(subFile.FullName);
                FileFolderInformation fileInfo = new FileFolderInformation(fileShowIcon,subFile.Name,true,subFile.CreationTime,subFile.LastWriteTime,subFile);
                enumCollection.Add(fileInfo);
              }
            }
            catch
            {

            }
            return enumCollection;
        }

        private BitmapImage GetIcon(string fileFullName)
        {

            Bitmap iconBitmap = Bitmap.FromHicon(Shell32.GetFileIcon(fileFullName,Shell32.FileAttributes.LargeIcon));
            BitmapImage bitmapImage = new BitmapImage();
            using (System.IO.MemoryStream memory = new System.IO.MemoryStream())
            {
                iconBitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem listViewItem_Temp = sender as ListViewItem;
            FileFolderInformation fileFolderInformation_Temp = listViewItem_Temp.Tag as FileFolderInformation;
            if(fileFolderInformation_Temp.IsDirectory)
            {
                listViewSource = GetEnumContents(fileFolderInformation_Temp.DirectoryInformation);
                listView.ItemsSource = listViewSource;
                textBoxAddress.Text = fileFolderInformation_Temp.DirectoryInformation.FullName;
                textBoxAddress.Tag = fileFolderInformation_Temp.DirectoryInformation;
                listView.Tag = fileFolderInformation_Temp.DirectoryInformation;

          
                ExpandChosenTreeViewItem(fileFolderInformation_Temp.DirectoryInformation);
                
            }
            if (fileFolderInformation_Temp.IsFile)
            {
                Process.Start(fileFolderInformation_Temp.FileInformation.FullName);
            }
        }


        private void ListViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //ListViewItem listViewItem_Temp = sender as ListViewItem;
            //FileFolderInformation fileFolderInformation_Temp = listViewItem_Temp.Tag as FileFolderInformation;
            //if (fileFolderInformation_Temp.IsDirectory)
            //{
            //    pasteCanExecute = true;
               

            //}
        }

        //Expand the choosed TreeViewItem
        private void ExpandChosenTreeViewItem(DirectoryInfo dirInfo)
        {
           DirectoryInfo dirLoop = dirInfo;
           DirectoryInfo dirHead = null;
           Stack<DirectoryInfo> stackDir = new Stack<DirectoryInfo>();
           TreeViewItem treeViewItem_Expanded=null;
         
            bool bool_Exist = false;

            while (dirLoop.Parent != null)
            { 
                DirectoryInfo dirParent = dirLoop.Parent;
                stackDir.Push(dirParent);
                dirLoop = dirLoop.Parent;

            }

            dirHead = stackDir.Pop();
            foreach( TreeViewItem item in treeView.Items)
            {
                if (item.Tag.Equals(dirHead))
                { 
                    if(!item.IsExpanded)
                    {
                        item.IsExpanded = true;
                    }
                    treeViewItem_Expanded = item;
                    bool_Exist = true;
                    break;

                }
            }
            if(!bool_Exist)
            {
                TreeViewItem newItem = new TreeViewItem();
                BitmapImage bitMapImage = new BitmapImage();
                bitMapImage.BeginInit();
                bitMapImage.UriSource = new Uri("Images/itemfolder.png", UriKind.Relative);
                bitMapImage.EndInit();
                FileFolderInformation filefolderInformation = new FileFolderInformation(dirHead.Name, true, bitMapImage);
                newItem.Tag = dirHead;
                newItem.Header = filefolderInformation;
              //  newItem.MouseLeftButtonDown += treeViewItem_MouseLeftButtonDown;
                newItem.Items.Add("*");
                treeView.Items.Add(newItem);
                newItem.IsExpanded = true;
                treeViewItem_Expanded = newItem;
            }

            foreach(DirectoryInfo dirStack in stackDir)
            {
                foreach (TreeViewItem item_Foreach in treeViewItem_Expanded.Items)
                {
                    if(item_Foreach.Tag is DirectoryInfo)
                    {
                       if( item_Foreach.Tag.Equals(dirStack))
                       {
                           if(!item_Foreach.IsExpanded)
                           {
                               item_Foreach.IsExpanded = true;
                           }
                          
                           treeViewItem_Expanded = item_Foreach;
                       }
               //           string listView_TagFullName = (listView.Tag as DirectoryInfo).FullName;
               // string dirSource_FullName = dirSource.FullName;
               //if(String.Equals(listView_TagFullName,dirSource_FullName,StringComparison.CurrentCultureIgnoreCase))
               //{
                        if(item_Foreach.Tag.Equals(dirInfo))
                        {
                            if(item_Foreach.IsExpanded)
                            {
                                item_Foreach.IsExpanded = false;
                                item_Foreach.IsExpanded = true;
                            }
                        }

                    }
                }
               
               
            } 

        }

        private void treeViewItem_Collapsed(object sender, RoutedEventArgs e)
        {
            TreeViewItem collapsed_Item = (TreeViewItem)e.OriginalSource;
            //item.Items.Clear();
           // treeViewItem_GobalPosition = collapsed_Item;
            collapsed_Item.IsExpanded = false;
            //DirectoryInfo dirInfo = collapsed_Item.Tag as DirectoryInfo;
            //listViewSource = GetEnumContents(dirInfo);
            //listView.ItemsSource = listViewSource;
        }

        private void treeViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            TreeViewItem selectedItem = sender as TreeViewItem;
            
           // TreeViewItem selectedItem = e.NewValue as TreeViewItem;
            selectedItem.IsExpanded = true;
            DirectoryInfo dirInfo = selectedItem.Tag as DirectoryInfo;
            listViewSource = GetEnumContents(dirInfo);
            listView.ItemsSource = listViewSource;
            listView.Tag = dirInfo;
            textBoxAddress.Text = dirInfo.FullName;
            textBoxAddress.Tag = dirInfo;
            selectedItem.Focus();
            e.Handled = true;
        }

        private void EanbleFilter(object sender, RoutedEventArgs e)
        {
            bool_EnableFilter = true;
            textBox_FilterFiles.Clear();
            gridFilter.Visibility = System.Windows.Visibility.Visible;
        }

        private void CancelFilterHandler(object sender, RoutedEventArgs e)
        {
            bool_EnableFilter = false;
            gridFilter.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void TextChanged_ReadFilterFiles(object sender, TextChangedEventArgs e)
        {
            list_FilterFiles.Clear();
            int lineCount = textBox_FilterFiles.LineCount;
            
            for(int line = 0; line < lineCount;line++)
            {
                string lineContext = textBox_FilterFiles.GetLineText(line).Trim();
                list_FilterFiles.Add(lineContext);
            }
        }

        private bool VerifyFilterFile(string fileName)
        {
            foreach(string file in list_FilterFiles)
            {
                if(string.Compare(file,fileName,true) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
          //  string s = sender.GetType().ToString();
            TreeViewItem selectedItem = sender as TreeViewItem;
            selectedItem.Focus();
            e.Handled = true;
           // MessageBox.Show(selectedItem.Tag.ToString());
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GetUIObject(e, true, false);
            if(treeViewItem_GlobalSource!=null)
            {
                treeViewItem_GlobalSource.IsExpanded = true;
                DirectoryInfo dirInfo = treeViewItem_GlobalSource.Tag as DirectoryInfo;
                listViewSource = GetEnumContents(dirInfo);
                listView.ItemsSource = listViewSource;
                listView.Tag = dirInfo;
                textBoxAddress.Text = dirInfo.FullName;
                textBoxAddress.Tag = dirInfo;
                treeViewItem_GlobalSource.Focus();
                e.Handled = true;
            }
            if(listViewItem_GlobalSource!=null)
            {
                ListViewItem listViewItem_Temp = listViewItem_GlobalSource;
                FileFolderInformation fileFolderInformation_Temp = listViewItem_Temp.Tag as FileFolderInformation;
                if (fileFolderInformation_Temp.IsDirectory)
                {
                    listViewSource = GetEnumContents(fileFolderInformation_Temp.DirectoryInformation);
                    listView.ItemsSource = listViewSource;
                    textBoxAddress.Text = fileFolderInformation_Temp.DirectoryInformation.FullName;
                    textBoxAddress.Tag = fileFolderInformation_Temp.DirectoryInformation;
                    listView.Tag = fileFolderInformation_Temp.DirectoryInformation;


                    ExpandChosenTreeViewItem(fileFolderInformation_Temp.DirectoryInformation);

                } 
                if(fileFolderInformation_Temp.IsFile)
                {
                    Process.Start(fileFolderInformation_Temp.FileInformation.FullName);
                }
            }

        }

        private void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            GetUIObject(e, true, false);
            pasteCanExecute = true;
            copy = true;
            cut = false;

        }

        private void CutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GetUIObject(e, true, false);
            pasteCanExecute = true;
            cut = true;
            copy = false;
        }

        private void PasteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            lableStatus.Content = "Processing";
           // progressBar.Value = 0;
            progressBar.Dispatcher.Invoke(() => progressBar.Value = 0, DispatcherPriority.Normal);
            //progressBar.Dispatcher.Invoke(() => progressBar.Visibility = System.Windows.Visibility.Visible, DispatcherPriority.Normal);
            progressBar.Visibility = System.Windows.Visibility.Visible;
            processCallback += updateProgressbar;
            listResult.Clear();
            GetUIObject(e, false, true);
            if(cut)
            { 
              CopyorCut(true);
            }
            if(copy)
            {
                CopyorCut(false);
            }
         processCallback -= updateProgressbar;
         
         progressBar.Visibility = System.Windows.Visibility.Hidden;
        // progressBar.Dispatcher.Invoke(() => progressBar.Visibility = System.Windows.Visibility.Hidden, DispatcherPriority.Normal);
         refreshUI();
         treeViewItem_GlobalSource = null;
         treeViewItem_GlobalDestination = null;
         listViewItem_GlobalSource = null;
         listViewItem_GlobalDestination = null;
         listView_GlobalDestination = null;
         pasteCanExecute = false;
         copy = false;
         cut = false;
         lableStatus.Content = "Idle";
        }

        private void refreshUI()
        {

            if(copy)
            {
                if(treeViewItem_GlobalSource != null)
                {
                    
                }
                if(treeViewItem_GlobalDestination != null)
                {
                    if(treeViewItem_GlobalDestination.IsExpanded)
                    {
                        treeViewItem_GlobalDestination.IsExpanded = false;
                        treeViewItem_GlobalDestination.IsExpanded = true;
                    }
                }
                if(listView_GlobalDestination!=null)
                {
                    if(textBoxAddress.Tag != null)
                    {
                        DirectoryInfo dirInfo = textBoxAddress.Tag as DirectoryInfo;
                        listViewSource = GetEnumContents(dirInfo);
                        listView.ItemsSource = listViewSource;
                        ExpandChosenTreeViewItem(dirInfo);
                    }
                  // DirectoryInfo dir = new DirectoryInfo(textBoxAddress.Text)
                }
            }
        }

        private void PasteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = pasteCanExecute;

        }

        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GetUIObject(e, true, false);
           try
           { 
            if (listViewItem_GlobalSource != null)
            {
                FileFolderInformation info = listViewItem_GlobalSource.Tag as FileFolderInformation;
                if (info.IsDirectory)
                {
                    DirectoryInfo dirSource = info.DirectoryInformation;
                    dirSource.Delete(true);
                    ExpandChosenTreeViewItem(dirSource.Parent);
                }
                else if (info.IsFile)
                {
                    FileInfo fileSource = info.FileInformation;
                    fileSource.Delete();

                }
                listViewSource.Remove(info);
                
            }
            if (treeViewItem_GlobalSource != null)
            {
                DirectoryInfo dirSource = treeViewItem_GlobalSource.Tag as DirectoryInfo;
                dirSource.Delete(true);
                TreeViewItem parent = treeViewItem_GlobalSource.Parent as TreeViewItem;
                parent.Items.Remove(treeViewItem_GlobalSource);
               // parent.RemoveLogicalChild()
             //  parent.RemoveLogicalChild(treeViewItem_GlobalSource);
                string listView_TagFullName = (listView.Tag as DirectoryInfo).FullName;
                string dirSource_FullName = dirSource.FullName;
               if(String.Equals(listView_TagFullName,dirSource_FullName,StringComparison.CurrentCultureIgnoreCase))
               {
                   DirectoryInfo dir = dirSource.Parent;
                   listViewSource = GetEnumContents(dir);
                   listView.ItemsSource = listViewSource;
                    textBoxAddress.Text = dir.FullName;
                   textBoxAddress.Tag = dir;
                   listView.Tag = dir;
                   ExpandChosenTreeViewItem(dir);
               }
               // treeViewItem_GlobalSource.
                //DirectoryInfo dir = new DirectoryInfo(strPath);
                //listViewSource = GetEnumContents(dir);
                //listView.ItemsSource = listViewSource;
                //// textBoxAddress.Text = dir.FullName;
                //textBoxAddress.Tag = dir;
                //listView.Tag = dir;
                //ExpandChosenTreeViewItem(dir);

            }
           }catch(Exception delException)
           {
               MessageBox.Show("Failed:" + delException.Message);
           }
        }

        private void GetUIObject(ExecutedRoutedEventArgs e,bool source,bool destination)
        {
         
            if(source)
            { 
              if (string.Equals(e.OriginalSource.GetType().ToString(), "System.Windows.Controls.ListViewItem", StringComparison.CurrentCultureIgnoreCase))
              {
                 listViewItem_GlobalSource = e.OriginalSource as ListViewItem;
                 treeViewItem_GlobalSource = null;
                // DirectoryInfo dd = listViewItem_GlobalSource.Tag;
                 
              }
              else if (string.Equals(e.OriginalSource.GetType().ToString(), "System.Windows.Controls.TreeViewItem", StringComparison.CurrentCultureIgnoreCase))
              {
                treeViewItem_GlobalSource = e.OriginalSource as TreeViewItem;
                listViewItem_GlobalSource = null;
                //string aa = (treeViewItem_GlobalSource.Tag as DirectoryInfo).Name;
              }

            }

            if(destination)
            {
                if (string.Equals(e.OriginalSource.GetType().ToString(), "System.Windows.Controls.ListViewItem", StringComparison.CurrentCultureIgnoreCase))
                {
                    listViewItem_GlobalDestination = e.OriginalSource as ListViewItem;
                    treeViewItem_GlobalDestination = null;
                    listView_GlobalDestination = null; 
                }
                else if (string.Equals(e.OriginalSource.GetType().ToString(), "System.Windows.Controls.TreeViewItem", StringComparison.CurrentCultureIgnoreCase))
                {
                    treeViewItem_GlobalDestination = e.OriginalSource as TreeViewItem;
                    listViewItem_GlobalDestination = null;
                    listView_GlobalDestination = null;
                }
                else if (string.Equals(e.OriginalSource.GetType().ToString(), "System.Windows.Controls.ListView", StringComparison.CurrentCultureIgnoreCase))
                {
                    listView_GlobalDestination = e.OriginalSource as ListView;
                    listViewItem_GlobalDestination = null;
                    treeViewItem_GlobalDestination = null;
                }

             }

        }

        private void CopyorCut(bool removeSource)
        {
            DirectoryInfo dirSource = null;
            DirectoryInfo dirDestination = null;
            FileInfo fileSource = null;
            bool copyReturn = false;

            if (listViewItem_GlobalSource != null)
            {
                FileFolderInformation info = listViewItem_GlobalSource.Tag as FileFolderInformation;
                if (info.IsDirectory)
                {
                    dirSource = info.DirectoryInformation;
                    fileSource = null;
                }
                else if (info.IsFile)
                {
                    fileSource = info.FileInformation;
                    dirSource = null;
                }
            }
            if (treeViewItem_GlobalSource != null)
            {
                dirSource = treeViewItem_GlobalSource.Tag as DirectoryInfo;
                fileSource = null;
            }
            if (listViewItem_GlobalDestination != null)
            {
                FileFolderInformation info = listViewItem_GlobalDestination.Tag as FileFolderInformation;
                if (info.IsDirectory)
                {
                    dirDestination = info.DirectoryInformation;
                }
                if (info.IsFile)
                {
                    MessageBox.Show("Destination can't be a file");
                    return;
                }
            }
            if (treeViewItem_GlobalDestination != null)
            {
                dirDestination = treeViewItem_GlobalDestination.Tag as DirectoryInfo;
            }
            if (listView_GlobalDestination != null)
            {
                dirDestination = listView_GlobalDestination.Tag as DirectoryInfo;
            }
            //To solve source is same with destination
            if(dirSource!=null && dirDestination!=null)
            { 
             if(string.Equals(dirSource.Parent.FullName,dirDestination.FullName,StringComparison.CurrentCultureIgnoreCase))
             {
                StringBuilder dirString = new StringBuilder(dirSource.FullName.ToString());
                int i = 1;
                while(true)
                {
                    if(Directory.Exists(dirString.Append(i).ToString()) )
                    {
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }

                dirDestination = Directory.CreateDirectory(dirString.ToString());
             }
             string strSource = dirSource.FullName;
             string strDestination = dirDestination.FullName;
             if(strDestination.IndexOf(strSource)>= 0)
             {
                 MessageBox.Show("Destination Folder can not contain Source folder;");
                 return;

             }
            }

            #region CopyorCut
                if(fileSource!=null)
                {
                    if(processCallback != null)
                    {
                        processCallback(10);
                    }
                    string filePath = dirDestination.FullName + @"\" + fileSource.Name;

                    //if(true)
                    if (string.Equals(fileSource.FullName, filePath, StringComparison.CurrentCultureIgnoreCase))
                    {
                        StringBuilder dirString = new StringBuilder(filePath);
                        int i = 1;
                        while (true)
                        {
                            if (File.Exists(dirString.Append(i).ToString()))
                            {
                                i++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        filePath = dirString.ToString();
                    }
                    //{}
     
                    copyReturn = Copy(fileSource, filePath);
                    if (processCallback != null)
                    {
                        processCallback(80);
                    }
                    if(copyReturn)
                    { 
                     if(removeSource)
                     {
                        fileSource.Delete();
                     }
                    }
                    if (processCallback != null)
                    {
                        processCallback(100);
                    }
                    
                }
                if(dirSource != null)
                {
                   

                    int sub = dirSource.FullName.Length - dirSource.Name.Length;
                    if(!bool_EnableFilter)
                    {
                        
                        try
                        {
                            DirectoryInfo dirReturn = null;
                            var enumCollect = dirSource.EnumerateDirectories(DirectoryEnumerationOptions.Folders | DirectoryEnumerationOptions.Recursive);
                            if(enumCollect != null)
                            {
                            foreach (DirectoryInfo directoryinfoSource in enumCollect)
                            {
                                
                                string dirPath = dirDestination.FullName + directoryinfoSource.FullName.Substring(sub-1);
                       
                                if (!Directory.Exists(dirPath))
                                {
                                    dirReturn = Directory.CreateDirectory(dirPath);
                                    if(dirReturn != null)
                                    {
                                        Result dirResult = new Result(directoryinfoSource.FullName, dirReturn.FullName, "Success");
                                        listResult.Add(dirResult);
                                    }
                                    else
                                    {
                                        Result dirResult = new Result(directoryinfoSource.FullName, dirReturn.FullName, "Failed");
                                        listResult.Add(dirResult);

                                    }
                                }
                            }
                            }
                            string selectFolder = dirDestination.FullName + @"\" + dirSource.Name;
                            if (!Directory.Exists(selectFolder))
                            {
                                dirReturn = Directory.CreateDirectory(selectFolder);
                                if (dirReturn != null)
                                {
                                    Result dirResult = new Result(selectFolder, dirReturn.FullName, "Success");
                                    listResult.Add(dirResult);
                                }
                                else
                                {
                                    Result dirResult = new Result(selectFolder, dirReturn.FullName, "Failed");
                                    listResult.Add(dirResult);

                                }
                            }

                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Failed:" + e.Message);
                        }
                    }
                 
                    try
                    {
                        long countAllFiles = dirSource.CountFileSystemObjects(DirectoryEnumerationOptions.Files | DirectoryEnumerationOptions.Recursive);
                        float intCount = 1;
                       
                       foreach(FileInfo fileinfoSource in dirSource.EnumerateFiles(DirectoryEnumerationOptions.Files | DirectoryEnumerationOptions.Recursive))
                        {
                            int value = (int)((intCount / countAllFiles)*100);
                            intCount++;
                            
                            if (processCallback != null)
                            {
                                processCallback(value);
                            }
                             
                          string filePath = dirDestination.FullName + fileinfoSource.FullName.Substring(sub-1);
                         copyReturn = Copy(fileinfoSource, filePath);
                           if(copyReturn)
                           { 
                            if(removeSource)
                            {
                               fileinfoSource.Delete();
                            }
                           }
                        }

                        if(removeSource)
                        {
                            dirSource.Delete(true);
                        }
                    }catch(Exception e)
                    {
                       MessageBox.Show("Failed:" + e.Message);
                    }

               
                }


            
            #endregion
        }

        private bool Copy(FileInfo file,string destination)
        {
            bool boolOverride = false;
            if(OverrideAll)
            {
                boolOverride = true;
            }else
            { 
              if (File.Exists(destination))
               {
             
                DialogWindow win = new DialogWindow(file.Name);
                win.Owner = this;
              
                if (win.ShowDialog() == true)
                 {
                    boolOverride = true;
                 }
                }
             }
            try
            {
                if(bool_EnableFilter)
                {
                    if(VerifyFilterFile(file.Name))
                    {
                      FileInfo fileDestination = null;
                      string strPath = Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(destination);
                      if (!Directory.Exists(strPath))
                      {
                          Directory.CreateDirectory(strPath);
                          
                      }
                      fileDestination = file.CopyTo(destination, boolOverride);
                      if(fileDestination != null)
                      {
                          if(string.Equals(file.Name,fileDestination.Name,StringComparison.CurrentCultureIgnoreCase))
                          {
                           Result returnResult = new Result(file.FullName, fileDestination.FullName, "Success");
                           listResult.Add(returnResult);
                          }
                      }else
                      {
                          Result returnResultFailed = new Result(file.FullName, fileDestination.FullName, "Failed");
                          listResult.Add(returnResultFailed);
                      }
                    }
                    return true;
                }
                //file.CopyTo(destination, boolOverride);

                FileInfo fileInfoDestination = null;
                fileInfoDestination = file.CopyTo(destination, boolOverride);
                if (fileInfoDestination != null)
                {
                    if (string.Equals(file.Name, fileInfoDestination.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Result returnResult = new Result(file.FullName, fileInfoDestination.FullName, "Success");
                        listResult.Add(returnResult);
                    }
                }
                else
                {
                    Result returnResultFailed = new Result(file.FullName, fileInfoDestination.FullName, "Failed");
                    listResult.Add(returnResultFailed);
                }
                return true;
                
            }catch(Exception e)
            {
                MessageBox.Show("Failed:" + e.Message);
            }
           //null
            return false;

        }

        private void ListView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListView listView_Temp = sender as ListView;
            listView_Temp.Focus();
            e.Handled = true;

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ResultWindow resultWindow = new ResultWindow(listResult);
            resultWindow.Show();
        }

        private void buttonClick(object sender, RoutedEventArgs e)
        {
            string strPath;
          if(textBoxAddress.Text != null)
          {
              strPath = textBoxAddress.Text;
          }
          else
          {
              MessageBox.Show("Please enter a valid path in Address Box");
              return;
          }

          if (strPath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
          {
              MessageBox.Show("Please enter a valid path in Address Box");
              return;
          }
          strPath = Alphaleonis.Win32.Filesystem.Path.AddTrailingDirectorySeparator(strPath);
          strPath = Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(strPath);
          if(!Directory.Exists(strPath))
          {
              MessageBox.Show(strPath + " is not exist;");
              return;
          }
          DirectoryInfo dir = new DirectoryInfo(strPath);
          listViewSource = GetEnumContents(dir);
          listView.ItemsSource = listViewSource;
         // textBoxAddress.Text = dir.FullName;
          textBoxAddress.Tag = dir;
          listView.Tag = dir;
          ExpandChosenTreeViewItem(dir);


        }

        private void comboboxView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)comboboxView.SelectedItem;
            listView.View = (ViewBase)this.FindResource(selectedItem.Content);
        }
      

    }
  
}
