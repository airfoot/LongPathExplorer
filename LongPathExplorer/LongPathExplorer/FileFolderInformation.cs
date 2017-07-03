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
using Alphaleonis.Win32.Filesystem;

namespace LongPathExplorer
{
    //Provide relative Folder/File information
    public class FileFolderInformation : INotifyPropertyChanged
    {
        public BitmapImage ShowIcon { get; set; }
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged(new PropertyChangedEventArgs("name"));
            }
        }
        public bool IsDirectory { get; set; }
        public bool IsFile { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastModifyTime { get; set; }
        public DirectoryInfo DirectoryInformation { get; set; }
        public FileInfo FileInformation { get; set; }

        public FileFolderInformation() { }

        public FileFolderInformation(String name, bool isDirectory, BitmapImage showIcon)
        {
            Name = name;
            IsDirectory = isDirectory;
            ShowIcon = showIcon;
        }

        public FileFolderInformation(BitmapImage showIcon, string dirName, bool isDirectory, DateTime creationTime, DateTime lastModifyTime, DirectoryInfo dirInfo)
        {
            ShowIcon = showIcon;
            Name = dirName;
            IsDirectory = isDirectory;
            CreationTime = creationTime;
            LastModifyTime = lastModifyTime;
            DirectoryInformation = dirInfo;
        }

        public FileFolderInformation(BitmapImage showIcon, string fileName, bool isFile, DateTime creationTime, DateTime lastModifyTime, FileInfo fileInfo)
        {
            ShowIcon = showIcon;
            Name = fileName;
            IsFile = isFile;
            CreationTime = creationTime;
            LastModifyTime = lastModifyTime;
            FileInformation = fileInfo;
        }

        public override string ToString()
        {
            return Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }


    }
}
