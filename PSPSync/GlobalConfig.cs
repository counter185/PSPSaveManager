using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PSPSync
{
    public struct SavePath{
        public string name;
        public string path;
        public SavePath(string nm2, string pt2) {
            name = nm2;
            path = pt2;
        }
        public SavePath(bool why) {
            name = "";
            path = "";
        }
    }

    public static class GlobalConfig
    {
        public static List<SavePath> paths = new List<SavePath>();

        public static void LoadConfig() {
            if (File.Exists("savepaths.txt")) {
                foreach (string a in File.ReadAllLines("savepaths.txt")) {
                    if (!a.Contains(';')) {
                        continue;
                    }
                    SavePath newpath = new SavePath(true);
                    bool readingintopath = false;
                    foreach (char b in a) {
                        if (b == ';')
                        {
                            readingintopath = true;
                        }
                        else {
                            if (readingintopath)
                            {
                                newpath.path += b;
                            }
                            else {
                                newpath.name += b;
                            }
                        }
                    }
                    paths.Add(newpath);
                }
            }
        }

        public static void SaveConfig() {
            string[] write = new string[paths.Count];
            for (int x = 0; x != paths.Count; x++) {
                write[x] = paths[x].name + ";" + paths[x].path;
            }
            File.WriteAllLines("savepaths.txt", write);
        }
    }
}
