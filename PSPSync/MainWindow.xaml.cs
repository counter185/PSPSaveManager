using MediaDevices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PSPSync
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<IStorageDevice> storageDevices = new List<IStorageDevice>();
        List<SaveMeta> sd1Saves, sd2Saves;
        bool sd1offline, sd2offline;
        private bool mgrEnabled = true;

        public MainWindow()
        {
            InitializeComponent();
            GlobalConfig.LoadConfig();
            UpdateStorageDeviceList();
            UpdateStorageDeviceItems();
            EventHandlers();
            KeyboardInput();
            
            this.SizeChanged += delegate
            {
                UpdateSizes();
            };
        }

        public void UpdateSizes() {
            double epic = (this.Width - 51) / 2d;
            SD1s.Width = epic - SD1s.Margin.Left;
            StorageDevice1.Width = epic - SD1s.Margin.Left;
            SD2s.Width = epic - SD2s.Margin.Right;
            StorageDevice2.Width = epic - SD2s.Margin.Right;

            Thickness thiccness = SD1toSD2.Margin;
            thiccness.Left = epic;
            SD1toSD2.Margin = thiccness;

            thiccness.Top = SD2toSD1.Margin.Top;
            SD2toSD1.Margin = thiccness;

            thiccness.Top = Sync.Margin.Top;
            Sync.Margin = thiccness;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            UpdateSizes();
            base.OnStateChanged(e);
        }

        public void KeyboardInput() {
            foreach (Control a in new Control[] { SD1s, SD2s, StorageDevice1, StorageDevice2, SD1toSD2, SD2toSD1, SD1Delete, SD2Delete})
            {
                a.PreviewKeyDown += (object sender, KeyEventArgs e) =>
                {
                    Console.WriteLine("Keyboard event: " + mgrEnabled);
                    e.Handled = !mgrEnabled;
                };
            }
        }

        public void EventHandlers() {
            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");

            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();
        }

        public void DeviceInsertedEvent(object sender, EventArrivedEventArgs e) {
            Console.WriteLine("Device inserted");
            Dispatcher.Invoke(delegate
            {
                UpdateStorageDeviceList();
                UpdateStorageDeviceItems();
            });
        }
        public void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("Device removed");
            Dispatcher.Invoke(delegate
            {
                UpdateStorageDeviceList();
                UpdateStorageDeviceItems();
            });
        }

        public void UpdateStorageDeviceList() {
            ScanDrives();
            StorageDevice1.SelectedIndex = -1;
            StorageDevice2.SelectedIndex = -1;
            StorageDevice1.Items.Clear();
            StorageDevice2.Items.Clear();
            foreach (IStorageDevice a in storageDevices) {
                StorageDevice1.Items.Add(a.GetDeviceName());
                StorageDevice2.Items.Add(a.GetDeviceName());
            }
            StorageDevice1.SelectedIndex = 0;
            StorageDevice2.SelectedIndex = 0;
        }

        public void UpdateStorageDeviceItems() {
            SD1s.Items.Clear();
            SD2s.Items.Clear();
            if (StorageDevice1.SelectedIndex != -1)
            {
                sd1Saves = storageDevices[StorageDevice1.SelectedIndex].ScanSaves();
                if (sd1Saves == null)
                {
                    SD1s.Items.Add("Offline... (Directory not found)");
                    SD1s.IsEnabled = false;
                    sd1offline = true;
                }
                else
                {
                    SD1s.IsEnabled = true;
                    sd1offline = false;
                    foreach (SaveMeta a in sd1Saves)
                    {
                        if (a == null)
                        {
                            MessageBox.Show("WHY");
                        }
                        SD1s.Items.Add(new SaveListItem(a));
                    }
                }

            }
            if (StorageDevice2.SelectedIndex != -1)
            {
                sd2Saves = storageDevices[StorageDevice2.SelectedIndex].ScanSaves();
                if (sd2Saves == null)
                {
                    SD2s.Items.Add("Offline... (Directory not found)");
                    SD2s.IsEnabled = false;
                    sd2offline = true;
                }
                else
                {
                    SD2s.IsEnabled = true;
                    sd2offline = false;
                    foreach (SaveMeta a in sd2Saves)
                    {
                        SD2s.Items.Add(new SaveListItem(a));
                    }
                }
            }
        }

        public void ScanDrives() {
            storageDevices.Clear();
            int i = 1;
            foreach (SavePath a in GlobalConfig.paths) {
                storageDevices.Add(new PSPSaveDir(a.path, a.name));
                i++;
            }
            string cmaPath = Environment.GetEnvironmentVariable("USERPROFILE") + "/Documents/PS Vita/PSAVEDATA/";
            if (Directory.Exists(cmaPath)) {
                foreach (string a in Directory.GetDirectories(cmaPath)) {
                    Console.WriteLine(a);
                    storageDevices.Add(new PSPSaveDir(a, $"PS Vita Content Manager [{GetGameID(a)}]"));
                }
            }

            foreach (string a in Directory.GetLogicalDrives()) {

                if (Directory.Exists(a + "/pspemu/PSP/"))
                {
                    storageDevices.Add(new PSPSaveDir(a + "/pspemu/PSP/SAVEDATA/", $"[{a}] PS Vita (SD or USB)"));
                }
                if (Directory.Exists(a + "/PSP/")) {
                    storageDevices.Add(new PSPSaveDir(a + "/PSP/SAVEDATA/", $"[{a}] PSP Memory Stick"));
                }
            }

            foreach (MediaDevice a in MTPDevice.ScanMTPs()) {
                MTPDevice b = new MTPDevice(a);
                if (b.saveDirs.Count == 0)
                {
                    b.Close();
                }
                else
                {
                    foreach (MTPSaveDir c in b.saveDirs)
                    {
                        storageDevices.Add(c);
                    }
                }
            }
        }

        public bool IsOnTheSameDevice() {
            return StorageDevice1.SelectedIndex == StorageDevice2.SelectedIndex;
        }

        public SaveMeta GetMetaFromID(List<SaveMeta> list, string ID) {
            //Console.WriteLine("Looking for " + ID);
            foreach (SaveMeta a in list) {
                //Console.WriteLine("Compare " + a.directory);
                if (a.directory.EndsWith(ID)) {
                    return a;
                }
            }
            return null;
        }

        private void SD1toSD2_Click(object sender, RoutedEventArgs e)
        {
            if (CannotCopy()) {
                return;
            }
            TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            IStorageDevice sd = storageDevices[StorageDevice1.SelectedIndex];
            SaveMeta currentmeta = sd1Saves[SD1s.SelectedIndex];
            NamedStream[] files = sd.ReadSave(currentmeta.directory);
            CopySave(currentmeta, GetMetaFromID(sd2Saves, GetGameID(currentmeta.directory)), files, storageDevices[StorageDevice2.SelectedIndex]);
        }

        public bool CannotCopy() {
            return sd1offline || sd2offline || IsOnTheSameDevice() ||
                StorageDevice2.SelectedIndex == -1 || SD2s.SelectedIndex == -1 ||
                StorageDevice1.SelectedIndex == -1 || SD1s.SelectedIndex == -1;
        }

        private void SD2toSD1_Click(object sender, RoutedEventArgs e)
        {
            if (CannotCopy())
            {
                return;
            }
            TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            IStorageDevice sd = storageDevices[StorageDevice2.SelectedIndex];
            SaveMeta currentmeta = sd2Saves[SD2s.SelectedIndex];
            NamedStream[] files = sd.ReadSave(currentmeta.directory);
            CopySave(currentmeta, GetMetaFromID(sd1Saves, GetGameID(currentmeta.directory)), files, storageDevices[StorageDevice1.SelectedIndex]);
        }

        public void SetMgrEnabled(bool enabled) {
            AllDisabled.Visibility = (enabled ? Visibility.Hidden : Visibility.Visible);
            mgrEnabled = enabled;
            //SD1s.IsEnabled = enabled;
            //SD2s.IsEnabled = enabled;
        }

        public void CopySave(SaveMeta srcMeta, SaveMeta otherMeta, NamedStream[] files, IStorageDevice dest) {
            SetMgrEnabled(false);
            string id = GetGameID(srcMeta.directory);
            Console.WriteLine(dest.HasSave(id) + " " + id);
            if (dest.HasSave(id))
            {
                CompareWindow a = new CompareWindow(otherMeta, srcMeta, dest.GetDeviceName(),
                    delegate
                    {
                        try
                        {
                            dest.DeleteSave(GetGameID(srcMeta.directory));
                            dest.WriteSave(GetGameID(srcMeta.directory), files);
                            MessageBox.Show("Copied");
                        }
                        catch (IOException)
                        {
                            MessageBox.Show("Copy operation failed");
                        }

                        foreach (NamedStream b in files)
                        {
                            b.stream.Dispose();
                        }
                    },
                    delegate
                    {
                        TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        SetMgrEnabled(true);
                        UpdateStorageDeviceItems();
                    });
                a.Show();
                /*MessageBoxResult a = MessageBox.Show($"Destination already has a {GetGameID(srcMeta.directory)} save file. Overwrite?", "Warning", MessageBoxButton.YesNo);
                if (a == MessageBoxResult.No)
                {
                    SD1s.IsEnabled = true;
                    SD2s.IsEnabled = true;
                    return;
                }
                else
                {
                    dest.DeleteSave(GetGameID(srcMeta.directory));
                }*/
            }
            else {
                try
                {
                    dest.WriteSave(GetGameID(srcMeta.directory), files);
                    MessageBox.Show("Copied");
                }
                catch (IOException)
                {
                    MessageBox.Show("Copy operation failed");
                }

                foreach (NamedStream b in files)
                {
                    b.stream.Dispose();
                }
                TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                SetMgrEnabled(true);
                UpdateStorageDeviceItems();
            }
        }

        public static string GetGameID(string directory) {
            while (directory.EndsWith("/") || directory.EndsWith(@"\\")) {
                directory = directory.Substring(0, directory.Length - 1);
            }
            for (int fnw = directory.Length - 1; fnw != 0; fnw--)
            {
                if (directory[fnw] == '/' || directory[fnw] == '\\')
                {
                    return directory.Substring(fnw + 1);
                }
            }
            throw new FormatException();
        }

        private void Sync_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is not implemented yet");
        }

        private void Config_Click(object sender, RoutedEventArgs e)
        {
            Config a = new Config();
            a.Closed += delegate
            {
                UpdateStorageDeviceList();
                UpdateStorageDeviceItems();
            };
            a.Show();
        }

        private void SD1Delete_Click(object sender, RoutedEventArgs e)
        {
            SaveMeta srcMeta = sd1Saves[SD1s.SelectedIndex];
            IStorageDevice sd = storageDevices[StorageDevice1.SelectedIndex];
            MessageBoxResult a = MessageBox.Show($"Delete {GetGameID(srcMeta.directory)} save file from {sd.GetDeviceName()}?", "Warning", MessageBoxButton.YesNo);
            if (a == MessageBoxResult.Yes) {
                sd.DeleteSave(GetGameID(srcMeta.directory));
                MessageBox.Show("Save file deleted");
                UpdateStorageDeviceItems();
            }
        }

        private void SD2Delete_Click(object sender, RoutedEventArgs e)
        {
            SaveMeta srcMeta = sd2Saves[SD2s.SelectedIndex];
            IStorageDevice sd = storageDevices[StorageDevice2.SelectedIndex];
            MessageBoxResult a = MessageBox.Show($"Delete {GetGameID(srcMeta.directory)} save file from {sd.GetDeviceName()}?", "Warning", MessageBoxButton.YesNo);
            if (a == MessageBoxResult.Yes)
            {
                sd.DeleteSave(GetGameID(srcMeta.directory));
                MessageBox.Show("Save file deleted");
                UpdateStorageDeviceItems();
            }
        }

        private void RescanDrives_Click(object sender, RoutedEventArgs e)
        {
            UpdateStorageDeviceList();
            UpdateStorageDeviceItems();
        }

        private void StorageDevice1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StorageDevice1.SelectedIndex != -1)
                UpdateStorageDeviceItems();
        }

        private void StorageDevice2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StorageDevice2.SelectedIndex != -1)
                UpdateStorageDeviceItems();
        }
    }
}