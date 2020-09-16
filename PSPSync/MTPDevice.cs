using MediaDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSPSync
{
    public class MTPDevice
    {
        public MediaDevice device;
        public List<MTPSaveDir> saveDirs = new List<MTPSaveDir>();
        public MTPDevice(MediaDevice dev) {
            device = dev;
            device.Connect();
            try
            {
                foreach (string a in device.GetDirectories("/"))
                {
                    if (device.DirectoryExists(a + "/PSP/SAVEDATA"))
                    {
                        saveDirs.Add(new MTPSaveDir(this, a + "/PSP/SAVEDATA", $"[MTP:{device.FriendlyName}{a}] Android (or PSP via MTP)"));
                    }
                    if (device.DirectoryExists(a + "/switch/ppsspp/config/ppsspp/PSP/SAVEDATA"))
                    {
                        saveDirs.Add(new MTPSaveDir(this, a + "/switch/ppsspp/config/ppsspp/PSP/SAVEDATA", $"[MTP:{device.FriendlyName}{a}] Nintendo Switch"));
                    }
                }
                //device.Disconnect();
            }
            catch (System.Runtime.InteropServices.COMException) {
                saveDirs.Clear();
            }
        }

        public void Close() {
            device.Disconnect();
            device.Dispose();
        }

        public static IEnumerable<MediaDevice> ScanMTPs() {
            return MediaDevice.GetDevices();
        }
    }
}
