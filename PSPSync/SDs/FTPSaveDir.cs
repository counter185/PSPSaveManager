using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Animation;

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
            NamedStream[] ret = new NamedStream[files.Length];
            for (int x = 0; x != files.Length; x++)
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
                MemoryStream file = client.Download(files[x]);
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
            foreach (string dr2 in ss)
            {
                string a = mainDir + dr2;
                if (!client.FileExists(a + "/PARAM.SFO") || !client.FileExists(a + "/ICON0.PNG"))
                {
                    continue;
                }
                string title;
                string info;
                string info2;
                byte[] reader = new byte[128];
                MemoryStream b = client.Download(a + "/PARAM.SFO");
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

                MemoryStream imageStream = client.Download(a + "/ICON0.PNG");
                Console.WriteLine(a);
                imageStream.Position = 0;

                saves.Add(new SaveMeta(title, info, info2, a, MTPSaveDir.BitmapFromStream(imageStream), new System.DateTime(0)));
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
            Console.WriteLine("done holy shit");
        }
    }
}
