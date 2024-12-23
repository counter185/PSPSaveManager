using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace PSPSync
{
    public class FTPSaveDir : IStorageDevice
    {
        public FtpClient client;
        public readonly string ip;
        public string mainDir;
        public string thisName;

        public FTPSaveDir(string mainDir2, string ip2, string name) {
            mainDir = mainDir2;
            ip = ip2;
            client = new FtpClient(ip, "anonymous", "pspsavemanager@github.com");
            thisName = name;
        }

        public void DeleteSave(string name)
        {
            string[] files = client.DirectoryListSimple(mainDir + "/" + name);
            for (int x = 0; x < files.Length - 1; x++) {
                string a = files[x];
                client.Delete(mainDir + "/" + name + "/" + a);
            }
            client.DeleteDir(mainDir + "/" + name);
        }

        public string GetDeviceName()
        {
            return $"[{ip}] {thisName}";
        }

        public bool HasSave(string name)
        {
            return client.DirectoryExists(mainDir + "/" + name);
        }

        public bool IsConnected()
        {
            return client.ServerAvailable();
        }

        public NamedStream[] ReadSave(string directory)
        {
            string[] files = client.DirectoryListSimple(directory);
            NamedStream[] ret = new NamedStream[files.Length-1];
            for (int x = 0; x != files.Length-1; x++)
            {
                string filename = mainDir + "/" + files[x];
                for (int fnw = filename.Length - 1; fnw > 0; fnw--)
                {
                    if (filename[fnw] == '/' || filename[fnw] == '\\')
                    {
                        filename = filename.Substring(fnw);
                        break;
                    }
                }
                MemoryStream file = client.Download(directory + "/" + files[x]);
                //Console.WriteLine(directory + "/" + files[x]);
                file.Position = 0;
                ret[x] = new NamedStream(file, filename);
            }
            return ret;
        }

        public List<SaveMeta> ScanSaves()
        {
            if (!IsConnected() || !client.DirectoryExists(mainDir))
            {
                return null;
            }
            List<SaveMeta> saves = new List<SaveMeta>();
            string[] ss = client.DirectoryListSimple(mainDir);
            foreach (string saveSubdir in ss)
            {
                string fullSaveSubdir = mainDir + saveSubdir;
                if (!client.FileExists(fullSaveSubdir + "/PARAM.SFO"))
                {
                    continue;
                }

                Stream fstream = null;
                try
                {
                    fstream = client.Download(fullSaveSubdir + "/PARAM.SFO");
                    fstream.Seek(0, SeekOrigin.Begin);
                    SFOReader.SFOFile sfoData = SFOReader.ReadSFO(fstream);

                    ImageSource thumbnailImg = null;
                    if (client.FileExists(fullSaveSubdir + "/ICON0.PNG"))
                    {
                        MemoryStream imageStream = client.Download(fullSaveSubdir + "/ICON0.PNG");
                        imageStream.Position = 0;
                        thumbnailImg = MTPSaveDir.BitmapFromStream(imageStream);
                    }

                    saves.Add(new SaveMeta
                    {
                        name = sfoData.title,
                        info = sfoData.info,
                        info2 = sfoData.info2,
                        directory = saveSubdir,
                        thumbnail = thumbnailImg,
                        timeModified = new System.DateTime(0)
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

        public void WriteSave(string directoryName, NamedStream[] files)
        {
            string dr = mainDir + "/" + directoryName;
            if (!client.DirectoryExists(dr))
            {
                client.CreateDirectory(dr);
            }
            foreach (NamedStream stm in files)
            {
                client.Upload(dr + "/" + stm.name, stm.stream);
            }
        }

        public GeneralDeviceSpeed GetDeviceSpeed() {
            return GeneralDeviceSpeed.Slow;
        }
    }
}
