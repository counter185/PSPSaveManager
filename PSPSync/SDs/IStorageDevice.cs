using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PSPSync
{
    public enum GeneralDeviceSpeed { 
        Fast, Slow
    }
    public class SaveMeta {
        public string name;
        public string info;
        public string info2;
        public string directory;
        public ImageSource thumbnail;
        public DateTime timeModified;

        public SaveMeta(string nm2, string if2, string if3, string dir2, ImageSource th2, DateTime mod) {
            name = nm2;
            info = if2;
            info2 = if3;
            directory = dir2;
            thumbnail = th2;
            timeModified = mod;
        }
    }

    public struct NamedStream {
        public Stream stream;
        public string name;
        public NamedStream(Stream st, string nm) {
            stream = st;
            name = nm;
        }
    }

    public interface IStorageDevice
    {
        bool IsConnected();
        string GetDeviceName();
        bool HasSave(string name);
        List<SaveMeta> ScanSaves();
        void DeleteSave(string name);
        NamedStream[] ReadSave(string directory);
        void WriteSave(string directoryName, NamedStream[] files);
        GeneralDeviceSpeed GetDeviceSpeed();
    }
}
