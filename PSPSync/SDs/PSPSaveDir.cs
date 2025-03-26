using System;
using System.Collections.Generic;
using System.Data;
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
            string[] subdirs = Directory.GetDirectories(mainDir);
            foreach (string saveSubdir in subdirs) {
                if (!File.Exists(saveSubdir + "/PARAM.SFO")) {
                    continue;
                }
                Stream fstream = null;
                try
                {
                    fstream = new FileStream(saveSubdir + "/PARAM.SFO", FileMode.Open, FileAccess.Read, FileShare.Read);
                    SFOReader.SFOFile sfoData = SFOReader.ReadSFO(fstream);

                    saves.Add(new SaveMeta
                    {
                        name = sfoData.title,
                        info = sfoData.info,
                        info2 = sfoData.info2,
                        directory = saveSubdir,
                        thumbnail = File.Exists(saveSubdir + "/ICON0.PNG") ? BitmapFromUri(new Uri(saveSubdir + "/ICON0.PNG")) : null,
                        timeModified = File.GetLastWriteTime(saveSubdir + "/PARAM.SFO")
                    });
                } catch (Exception e)
                {
                    Console.WriteLine($"Failed to read {saveSubdir}: {e.Message}");
                } finally
                {
                    if (fstream != null)
                    {
                        fstream.Close();
                    }
                }
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
            const int BUFFER_SIZE = int.MaxValue;
            string dr = mainDir + directoryName;
            if (!Directory.Exists(dr))
            {
                Directory.CreateDirectory(dr);
            }
            foreach (NamedStream file in files) {
                FileStream a = File.Create(dr + "/" + file.name);
                int offset = 0;
                long bytesRemaining = file.stream.Length;
                byte[] readBuffer = new byte[Math.Min(bytesRemaining, BUFFER_SIZE)];
                while (bytesRemaining > 0)
                {
                    int bytesRed = file.stream.Read(readBuffer, offset, (int)Math.Min(bytesRemaining, BUFFER_SIZE));
                    a.Write(readBuffer, offset, bytesRed);
                    bytesRemaining -= bytesRed;
                    offset += bytesRed;
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
