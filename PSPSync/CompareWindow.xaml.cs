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
using System.Windows.Shapes;

namespace PSPSync
{
    /// <summary>
    /// Logika interakcji dla klasy CompareWindow.xaml
    /// </summary>
    public partial class CompareWindow : Window
    {
        readonly Action ifDst;
        readonly Action final;

        public CompareWindow()
        {
            InitializeComponent();
        }

        public CompareWindow(SaveMeta src, SaveMeta dst, string devName, Action toDoIfUserWantsDst, Action finalw) {
            InitializeComponent();
            srcItem.GiveMeta(src);
            dstItem.GiveMeta(dst);
            ifDst = toDoIfUserWantsDst;
            final = finalw;
            title.Content = $"This save slot is already in use in {devName}";
            srcDesc.Content = $"currently in {devName}";
        }

        private void KeepSRC_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            final.Invoke();
            base.OnClosed(e);
        }

        private void KeepDST_Click(object sender, RoutedEventArgs e)
        {
            ifDst.Invoke();
            this.Close();
        }
    }
}
