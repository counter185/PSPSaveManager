﻿using System;
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

namespace PSPSync
{
    /// <summary>
    /// Logika interakcji dla klasy SaveListItem.xaml
    /// </summary>
    public partial class SaveListItem : UserControl
    {
        public SaveListItem()
        {
            InitializeComponent();
        }
        public SaveListItem(SaveMeta b) {
            InitializeComponent();
            GiveMeta(b);
        }

        public void GiveMeta(SaveMeta a) {
            if (a != null)
            {
                this.GameName.Content = a.name;
                this.GameInfo.Content = a.info;
                this.GameInfo2.Content = a.info2;
                this.GameDate.Content = a.timeModified.ToString();
                if (a.thumbnail != null)
                {
                    this.MissingIcon.Visibility = Visibility.Hidden;
                    this.GamePic.Source = a.thumbnail;
                }
            }
            else {
                this.GameName.Content = "Corrupted data (no PARAM.SFO)";
                this.GameInfo.Content = "";
                this.GameInfo2.Content = "";
                this.GameDate.Content = "???";
            }
        }
    }
}
