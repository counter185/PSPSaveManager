using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PSPSync
{
    public class PSPSaveDir : IStorageDevice
    {
        readonly string mainDir;
        readonly string devName;

        public bool IsConnected() {
            return Directory.Exists(mainDir);
        }

        public string GetDeviceName() {
            return devName;
        }

        public PSPSaveDir(string directory, string name) {
            mainDir = directory;
            devName = name;
        }

        public void DeleteSave(string name)
        {
            Directory.Delete(mainDir + "/" + name, true);
        }

        public List<SaveMeta> ScanSaves()
        {
            List<SaveMeta> saves = new List<SaveMeta>();
            if (!Directory.Exists(mainDir))
            {
                return null;
            }
            string[] ss = Directory.GetDirectories(mainDir);
            foreach (string a in ss) {
                if (!File.Exists(a + "/PARAM.SFO")) {
                    continue;
                }
                string title;
                string info;
                string info2;
                byte[] reader = new byte[128];
                FileStream b = new FileStream(a + "/PARAM.SFO", FileMode.Open, FileAccess.Read, FileShare.Read);
                b.Seek(0x110, SeekOrigin.Begin);
                b.Read(reader, 0, 128);
                info2 = Encoding.UTF8.GetString(reader);
                b.Seek(0x1230, SeekOrigin.Begin);
                b.Read(reader, 0, 128);
                info = Encoding.UTF8.GetString(reader);
                b.Seek(0x12B0, SeekOrigin.Begin);
                b.Read(reader, 0, 128);
                title = Encoding.UTF8.GetString(reader);
                saves.Add(new SaveMeta(title, info, info2, a, (File.Exists(a+"/ICON0.PNG") ? BitmapFromUri(new Uri(a + "/ICON0.PNG")) : null), File.GetLastWriteTime(a + "/PARAM.SFO")));
                b.Close();
            }
            return saves;
        }

        public bool HasSave(string name) {
            return Directory.Exists(mainDir + "/" + name) && File.Exists(mainDir + "/" + name + "/PARAM.SFO");
        }

        public static ImageSource BitmapFromUri(Uri source)
        {
            
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        public NamedStream[] ReadSave(string directory) {
            string[] files = Directory.GetFiles(directory);
            NamedStream[] ret = new NamedStream[files.Length];
            for (int x = 0; x != files.Length; x++) {
                string filename = files[x];
                for (int fnw = filename.Length - 1; fnw > 0; fnw--) {
                    if (filename[fnw] == '/' || filename[fnw] == '\\') {
                        filename = filename.Substring(fnw);
                        break;
                    }
                }
                ret[x] = new NamedStream(File.Open(files[x], FileMode.Open, FileAccess.Read, FileShare.Read), filename);
            }
            return ret;
        }

        public void WriteSave(string directoryName, NamedStream[] files)
        {
            string dr = mainDir + "/" + directoryName;
            if (!Directory.Exists(dr))
            {
                Directory.CreateDirectory(dr);
            }
            foreach (NamedStream stm in files) {
                FileStream a = File.Create(dr + "/" + stm.name);
                long rem = stm.stream.Length;
                int offset = 0;
                byte[] read = new byte[1000000];
                while (rem > 0) {
                    stm.stream.Read(read, offset, (int)Math.Min(rem, 1000000));
                    a.Write(read, offset, (int)Math.Min(rem, 1000000));
                    rem -= Math.Min(rem, 1000000);
                    offset += 1000000;
                }
                a.Close();
            }
        }

        public GeneralDeviceSpeed GetDeviceSpeed()
        {
            return GeneralDeviceSpeed.Fast;
        }
    }
}
