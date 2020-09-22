using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PSPSync
{
    /// <summary>
    /// Logika interakcji dla klasy Config.xaml
    /// </summary>
    public partial class Config : Window
    {

        public Config()
        {
            InitializeComponent();
            LoadPaths();
        }

        public void LoadPaths() {
            PathList.Items.Clear();
            foreach (SavePath a in GlobalConfig.paths) {
                PathList.Items.Add(a.name + " | " + a.path);
            }
        }

        private void AddPath_Click(object sender, RoutedEventArgs e)
        {
            if (PathName.Text == String.Empty || PathPath.Text == String.Empty) {
                return;
            }
            string path = PathPath.Text.Replace("\\", "/");
            if (!path.EndsWith("/")) {
                path += "/";
            }
            GlobalConfig.paths.Add(new SavePath(PathName.Text, path));
            GlobalConfig.SaveConfig();
            LoadPaths();
            PathName.Text = String.Empty;
            PathPath.Text = String.Empty;
        }

        private void DelSelected_Click(object sender, RoutedEventArgs e)
        {
            if (PathList.SelectedIndex != -1)
            {
                GlobalConfig.paths.RemoveAt(PathList.SelectedIndex);
                GlobalConfig.SaveConfig();
                LoadPaths();
            }
            else {
                System.Windows.MessageBox.Show("Select a path to delete");
            }
        }

        private void ThreeDot_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog a = new OpenFileDialog();
            a.ValidateNames = false;
            a.CheckFileExists = false;
            a.CheckPathExists = true;
            a.FileName = "Select a folder";
            a.ShowDialog();
            PathPath.Text = a.FileName.Substring(0, a.FileName.Length - 15);
            a.Dispose();
        }
    }
}
