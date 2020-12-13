using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PSPSync
{
    public class MTPSaveDir : IStorageDevice
    {
        public string dir;
        public MTPDevice parent;
        public string name;

        public MTPSaveDir(MTPDevice pnt, string dr2, string nm2) {
            parent = pnt;
            dir = dr2;
            name = nm2;
        }

        public void DeleteSave(string name)
        {
            foreach (string a in parent.device.GetFiles(dir + "/" + name)) {
                //MessageBox.Show("am bouta delet " + a);
                parent.device.DeleteFile(a);
            }
        }

        public string GetDeviceName()
        {
            return name;
        }

        public bool HasSave(string name)
        {
            return parent.device.DirectoryExists(dir + "/" + name);
        }

        public bool IsConnected()
        {
            return parent.device.IsConnected;
        }

        public NamedStream[] ReadSave(string directory)
        {
            string[] files = parent.device.GetFiles(directory);
            NamedStream[] ret = new NamedStream[files.Length];
            for (int x = 0; x != files.Length; x++)
            {
                string filename = files[x];
                for (int fnw = filename.Length - 1; fnw > 0; fnw--)
                {
                    if (filename[fnw] == '/' || filename[fnw] == '\\')
                    {
                        filename = filename.Substring(fnw);
                        break;
                    }
                }
                MemoryStream file = new MemoryStream();
                parent.device.DownloadFile(files[x], file);
                file.Position = 0;
                ret[x] = new NamedStream(file, filename);
            }
            return ret;
        }

        public List<SaveMeta> ScanSaves()
        {
            if (!this.IsConnected() || !parent.device.DirectoryExists(dir))
            {
                return null;
            }
            List<SaveMeta> saves = new List<SaveMeta>();
            string[] ss = parent.device.GetDirectories(dir);
            foreach (string a in ss)
            {
                if (!parent.device.FileExists(a + "/PARAM.SFO"))
                {
                    continue;
                }
                string title;
                string info;
                string info2;
                byte[] reader = new byte[128];
                MemoryStream b = new MemoryStream();
                parent.device.DownloadFile(a + "/PARAM.SFO", b);
                b.Position = 0;
                b.Seek(0x110, SeekOrigin.Begin);
                b.Read(reader, 0, 128);
                info2 = Encoding.UTF8.GetString(reader);
                b.Seek(0x1230, SeekOrigin.Begin);
                b.Read(reader, 0, 128);
                info = Encoding.UTF8.GetString(reader);
                b.Seek(0x12B0, SeekOrigin.Begin);
                b.Read(reader, 0, 128);
                title = Encoding.UTF8.GetString(reader);
                b.Close();

                MemoryStream imageStream = new MemoryStream();
                ImageSource imsr = null;
                if (parent.device.FileExists(a + "/ICON0.PNG"))
                {
                    parent.device.DownloadFile(a + "/ICON0.PNG", imageStream);
                    imsr = BitmapFromStream(imageStream);
                }
                imageStream.Position = 0;

                saves.Add(new SaveMeta(title, info, info2, a, imsr, parent.device.GetFileInfo(a + "/PARAM.SFO").LastWriteTime.Value));
            }
            return saves;
        }

        public static ImageSource BitmapFromStream(Stream src)
        {

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = src;
            bitmap.EndInit();
            return bitmap;
        }

        public void WriteSave(string directoryName, NamedStream[] files)
        {
            string dr = dir + "/" + directoryName;
            if (!parent.device.DirectoryExists(dr))
            {
                parent.device.CreateDirectory(dr);
            }
            foreach (NamedStream stm in files)
            {
                parent.device.UploadFile(stm.stream, dr + "/" + stm.name);
            }
            Console.WriteLine("done holy shit");
        }

        public GeneralDeviceSpeed GetDeviceSpeed()
        {
            return GeneralDeviceSpeed.Slow;
        }
    }
}
