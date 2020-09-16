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
            GlobalConfig.paths.Add(new SavePath(PathName.Text, PathPath.Text));
            GlobalConfig.SaveConfig();
            LoadPaths();
        }
    }
}
