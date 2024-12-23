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
            foreach (string saveSubdir in ss)
            {
                if (!parent.device.FileExists(saveSubdir + "/PARAM.SFO"))
                {
                    continue;
                }
                Stream fstream = null;
                try
                {
                    fstream = new MemoryStream();
                    parent.device.DownloadFile(saveSubdir + "/PARAM.SFO", fstream);
                    fstream.Seek(0, SeekOrigin.Begin);
                    SFOReader.SFOFile sfoData = SFOReader.ReadSFO(fstream);

                    MemoryStream imageStream = new MemoryStream();
                    ImageSource imsr = null;
                    if (parent.device.FileExists(saveSubdir + "/ICON0.PNG"))
                    {
                        parent.device.DownloadFile(saveSubdir + "/ICON0.PNG", imageStream);
                        imsr = BitmapFromStream(imageStream);
                    }
                    imageStream.Position = 0;

                    saves.Add(new SaveMeta
                    {
                        name = sfoData.title,
                        info = sfoData.info,
                        info2 = sfoData.info2,
                        directory = saveSubdir,
                        thumbnail = imsr,
                        timeModified = parent.device.GetFileInfo(saveSubdir + "/PARAM.SFO").LastWriteTime.Value
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to read {saveSubdir}: {e.Message}");
                }
                finally
                {
                    if (fstream != null)
                    {
                        fstream.Close();
                    }
                }
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
        }

        public GeneralDeviceSpeed GetDeviceSpeed()
        {
            return GeneralDeviceSpeed.Slow;
        }
    }
}
