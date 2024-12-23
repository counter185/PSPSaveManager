using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSPSync
{
    public static class SFOReader
    {
        public class SFOFile
        {
            public List<SFOEntry> entries;

            public string title => entries.Where(x => x.key == "TITLE").First().data;
            public string info => entries.Where(x => x.key == "SAVEDATA_DETAIL").First().data;
            public string info2 => entries.Where(x => x.key == "SAVEDATA_TITLE").First().data;
        }

        public class SFOEntry
        {
            public ushort keyOffset;
            public ushort paramFmt;
            public uint paramLen;
            public uint paramMaxLen;
            public uint dataOffset;

            public string key;
            public string data;
        }

        public static SFOFile ReadSFO(Stream inputStream)
        {
            List<SFOEntry> ret;

            using (BinaryReader reader = new BinaryReader(inputStream))
            {
                //string magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
                if (!reader.ReadBytes(4).SequenceEqual(new byte[] { (byte)'\x00', (byte)'P', (byte)'S', (byte)'F' }))
                {
                    throw new InvalidDataException("Invalid SFO file");
                }

                uint sfoVersion = reader.ReadUInt32();
                uint keyTableOffset = reader.ReadUInt32();
                uint dataTableOffset = reader.ReadUInt32();
                uint nIndexTableEntries = reader.ReadUInt32();

                inputStream.Seek(0x14, SeekOrigin.Begin);
                ret = new List<SFOEntry>();
                for (int x = 0; x < nIndexTableEntries; x++)
                {
                    SFOEntry entry = new SFOEntry();
                    entry.keyOffset = reader.ReadUInt16();
                    entry.paramFmt = reader.ReadUInt16();
                    entry.paramLen = reader.ReadUInt32();
                    entry.paramMaxLen = reader.ReadUInt32();
                    entry.dataOffset = reader.ReadUInt32();
                    ret.Add(entry);
                }

                foreach (SFOEntry entry in ret)
                {
                    inputStream.Seek(keyTableOffset + entry.keyOffset, SeekOrigin.Begin);
                    entry.key = ReadNullTerminatedString(reader);
                    inputStream.Seek(dataTableOffset + entry.dataOffset, SeekOrigin.Begin);
                    if (entry.paramFmt == 0x0404)
                    {
                        entry.data = reader.ReadUInt32().ToString();
                    }
                    else if (entry.paramFmt == 0x0004 || entry.paramFmt == 0x0204)
                    {
                        entry.data = Encoding.UTF8.GetString(reader.ReadBytes((int)entry.paramLen)).TrimEnd('\u0000');
                    }
                    else
                    {
                        throw new InvalidDataException("Unknown parameter format: " + entry.paramFmt);
                    }
                }
            }

            return new SFOFile 
            { 
                entries = ret
            };
        }

        private static string ReadNullTerminatedString(BinaryReader reader)
        {
            StringBuilder sb = new StringBuilder();
            char ch;
            while ((ch = reader.ReadChar()) != '\u0000')
            {
                sb.Append(ch);
            }
            return sb.ToString();
        }
    }
}
