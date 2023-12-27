using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace xiaoye97
{
    public class FontInfo
    {
        public enum fontType
        { TTF, OTF, FNT, TTC, PFB, PFA, ERR };

        private fontType type = fontType.ERR;

        public fontType Type
        {
            get { return type; }
        }

        private string fullName;

        public string FullName
        {
            get { return fullName; }
        }

        private Dictionary<string, string> threeB2Names; //sns => snl

        public Dictionary<string, string> ThreeB2Names
        {
            get { return threeB2Names; }
        }

        private string familyName;

        public string FamilyName
        {
            get { return familyName; }
        }

        private string version;

        public string Version
        {
            get { return version; }
        }

        private string weight;

        public string Weight
        {
            get { return weight; }
        }

        private string file;

        public string Path
        {
            get { return file; }
        }

        //Convenience dictionary. No hard and fast ASCII rule for doing this.
        private static Dictionary<char, char> brackets = new Dictionary<char, char>
        {
            {'(', ')'},
            {'[', ']'},
            {'{', '}'},
            {'?', '?'}
        };

        private BinaryReader reader;

        public FontInfo(string file)
        {
            this.file = file;
            if (!File.Exists(file)) throw new FileNotFoundException();
            this.reader = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read));
            byte[] magic_num = reader.ReadBytes(4);
            if (file.Substring(file.Length - 4, 4).ToUpper() == ".FNT")
            {
                type = fontType.FNT;
            }
            else if (magic_num.SequenceEqual(new byte[] { 0x00, 0x01, 0x00, 0x00 }))
            {
                type = fontType.TTF;
            }
            else if (magic_num.SequenceEqual(new byte[] { 0x4F, 0x54, 0x54, 0x4F }))
            {
                type = fontType.OTF;
            }
            else if (magic_num.SequenceEqual(new byte[] { 0x74, 0x74, 0x63, 0x66 }))
            {
                type = fontType.TTC;
            }
            else if (magic_num.Take(2).SequenceEqual(new byte[] { 0x80, 0x01 }))
            {
                type = fontType.PFA;
            }
            else if (magic_num.Take(2).SequenceEqual(new byte[] { 0x80, 0x02 }))
            {
                type = fontType.PFB;
            }
        }

        public FontInfo readInfo()
        {
            switch (this.type)
            {
                case fontType.OTF:
                    parseOTF();
                    break;

                case fontType.TTF:
                    parseOTF();
                    break;

                case fontType.PFB:
                    parsePFB();
                    break;

                case fontType.PFA:
                    parsePFB();
                    break;

                case fontType.FNT:
                    parse3B2();
                    break;

                default:
                    break;
            }
            reader.Dispose();
            return this;
        }

        //From: http://www.microsoft.com/typography/otspec/otff.htm
        private void parseOTF()
        {
            //read the OffSet Table (everything after sfnt version)
            UInt16 numTables = reverse(reader.ReadUInt16());
            //Console.WriteLine("numTables: " + numTables);
            UInt16 searchRange = reverse(reader.ReadUInt16());
            UInt16 entrySelector = reverse(reader.ReadUInt16());
            UInt16 rangeShift = reverse(reader.ReadUInt16());
            List<FTable> tables = new List<FTable>(); //could really be array
            FTable_Name ntable = null;

            for (int t = 0; t < numTables; t++)
            {
                tables.Add(FTable.parseTable(reader));
                if (new string(tables.Last().tag) == "name")
                {
                    ntable = new FTable_Name(reader, tables.Last());
                }
            }
            if (ntable.hasRecord(4, 0, 1, 0))
            {
                //Console.WriteLine("Platform 1");
                fullName = ntable.getString(4, 0, 1, 0);
                familyName = ntable.getString(1, 0, 1, 0);
                version = ntable.getString(5, 0, 1, 0);
                weight = ntable.getString(2, 0, 1, 0);
            }
            else if (ntable.hasRecord(4, 0x409, 3, 1))
            {
                //Console.WriteLine("Platform 3");
                fullName = ntable.getString(4, 0x409, 3, 1);
                familyName = ntable.getString(1, 0x409, 3, 1);
                version = ntable.getString(5, 0x409, 3, 1);
                weight = ntable.getString(2, 0x409, 3, 1);
            }
            else
            {
                fullName = "";
                familyName = "";
                version = "";
                weight = "";
            }
        }

        //3B2 Binary fonts only,
        private void parse3B2()
        {
            bool last;
            string sns;
            string snl;
            threeB2Names = new Dictionary<string, string>();
            reader.BaseStream.Seek(0x00, SeekOrigin.Begin);
            do
            {
                last = reader.ReadByte() == 0x00;
                reader.BaseStream.Seek(0x0F, SeekOrigin.Current);
                sns = ASCIIEncoding.UTF8.GetString(reader.ReadBytes(0x10).TakeWhile(b => b != 0).ToArray());
                snl = ASCIIEncoding.UTF8.GetString(reader.ReadBytes(0x30).TakeWhile(b => b != 0).ToArray());
                reader.BaseStream.Seek(0x400 - 0x50, SeekOrigin.Current);
                threeB2Names[sns] = snl;
            }
            while (!last);
        }

        private static void parsePfLine(Dictionary<string, string> dict, string line)
        {
            string key = new string(line.Skip(1).TakeWhile(char.IsLetter).ToArray());
            string value = new string(line.Skip(key.Length + 1).SkipWhile(char.IsWhiteSpace).TakeWhile(c => c != '\n').ToArray());
            if (brackets.ContainsKey(value[0]))
            {
                //Find the last closing bracket and use that as the value
                int close_pos = value.LastIndexOf(brackets[value[0]]) - 1;
                value = new string(value.Skip(1).Take(close_pos).ToArray());
            }
            else
            {
                value = new string(value.TakeWhile(v => v != ' ').ToArray());
            }

            dict[key] = value;
        }

        //temporary hack for now.
        private void parsePFB()
        {
            //go back 2 bytes (we overread on magicnum)
            reader.BaseStream.Seek(2, SeekOrigin.Begin);
            //read block length
            UInt32 block_len = (UInt32)(reader.ReadByte() & 0xFF);
            block_len |= (UInt32)((reader.ReadByte() & 0xFF) << 8);
            block_len |= (UInt32)((reader.ReadByte() & 0xFF) << 16);
            block_len |= (UInt32)((reader.ReadByte() & 0XFF) << 24);
            //read ascii
            string str = ASCIIEncoding.UTF8.GetString(reader.ReadBytes((int)block_len));
            str = str.Replace('\r', '\n');
            Dictionary<string, string> dictPfb = new Dictionary<string, string>();

            parsePfLine(dictPfb, new string(str.Skip(str.IndexOf("/FullName")).TakeWhile(c => c != '\r').ToArray()));
            fullName = dictPfb["FullName"];

            parsePfLine(dictPfb, new string(str.Skip(str.IndexOf("/FamilyName")).TakeWhile(c => c != '\r').ToArray()));
            familyName = dictPfb["FamilyName"];

            parsePfLine(dictPfb, new string(str.Skip(str.IndexOf("/version")).TakeWhile(c => c != '\r').ToArray()));
            version = dictPfb["version"];

            parsePfLine(dictPfb, new string(str.Skip(str.IndexOf("/Weight")).TakeWhile(c => c != '\r').ToArray()));
            weight = dictPfb["Weight"];
        }

        public override string ToString()
        {
            return string.Format("FontInfo:\r\n\tfullName: {0}\r\n\tfamilyName: {1}\r\n\tweight: {2}\r\n\tversion: {3}\r\n", fullName, familyName, weight, version);
        }

        //Functions for converting Motorola (Big) Endian -> Intel (Small) Endian
        public static UInt32 reverse(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        public static UInt16 reverse(UInt16 value)
        {
            return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }
    }

    #region OTF/TTF

    /* Name table */

    internal class FTable_Name
    {
        public FTable entry;
        public UInt16 format;
        public UInt16 count;
        public UInt16 stringOffset;
        public UInt16 langTagCount;
        public List<NameRecord> nameRecords;
        public List<LangTagRecord> langTagRecords;

        public FTable_Name(BinaryReader r, FTable entry)
        {
            nameRecords = new List<NameRecord>();
            this.entry = entry;
            //store current position.
            long start = r.BaseStream.Position;
            //seek the table.
            r.BaseStream.Seek(entry.offset, SeekOrigin.Begin);
            format = FontInfo.reverse(r.ReadUInt16());
            count = FontInfo.reverse(r.ReadUInt16());
            stringOffset = FontInfo.reverse(r.ReadUInt16());
            //read name records...
            for (int i = 0; i < count; i++)
            {
                nameRecords.Add(new NameRecord(r, entry.offset + stringOffset));
                //Console.WriteLine(nameRecords.Last().ToString());
            }
            //Format v1? Then we have lang tags too.
            //Console.WriteLine("FMT: " + format);
            if (format == 1)
            {
                langTagRecords = new List<LangTagRecord>();
                langTagCount = FontInfo.reverse(r.ReadUInt16());
                for (int i = 0; i < langTagCount; i++)
                {
                    langTagRecords.Add(new LangTagRecord(r, entry.offset + stringOffset));
                    //Console.WriteLine(langTagRecords.Last().ToString());
                }
            }
            r.BaseStream.Seek(start, SeekOrigin.Begin);
        }

        // 2 = sub family
        // 3 = Unique ID
        // 4 = full name
        // 5 = version
        // 6 = PS name
        public string getString(UInt16 nameID, UInt16 languageID, UInt16 platformID, UInt16 encodingID)
        {
            var bytes = nameRecords.Find(f => f.languageID == languageID && f.nameID == nameID && f.platformID == platformID && f.encodingID == encodingID).str;
            //reverse the endian of UTF16...
            if (platformID == 3 && (bytes.Length % 2 == 0))
            {
                byte t;
                for (int i = 0; i < bytes.Length; i += 2)
                {
                    t = bytes[i];
                    bytes[i] = bytes[i + 1];
                    bytes[i + 1] = t;
                }
                return UnicodeEncoding.Unicode.GetString(bytes);
            }
            else
            {
                return ASCIIEncoding.UTF8.GetString(bytes);
            }
        }

        public bool hasRecord(UInt16 nameID, UInt16 languageID, UInt16 platformID, UInt16 encodingID)
        {
            return nameRecords.Where(f => f.languageID == languageID && f.nameID == nameID && f.platformID == platformID && f.encodingID == encodingID).Any();
        }

        //Name records...
        public class NameRecord
        {
            public UInt16 platformID;
            public UInt16 encodingID;
            public UInt16 languageID;
            public UInt16 nameID;
            public UInt16 length;
            public UInt16 offset;
            public byte[] str;
            public string Name;

            //accept a BinaryReader and offset for string data.
            public NameRecord(BinaryReader r, long soffset)
            {
                platformID = FontInfo.reverse(r.ReadUInt16());
                encodingID = FontInfo.reverse(r.ReadUInt16());
                languageID = FontInfo.reverse(r.ReadUInt16());
                nameID = FontInfo.reverse(r.ReadUInt16());
                length = FontInfo.reverse(r.ReadUInt16());
                offset = FontInfo.reverse(r.ReadUInt16());
                long ipos = r.BaseStream.Position;
                r.BaseStream.Seek(soffset + offset, SeekOrigin.Begin);
                str = r.ReadBytes(length);
                //seek back to start of next name record.
                r.BaseStream.Seek(ipos, SeekOrigin.Begin);
            }

            public override string ToString()
            {
                return string.Format("NameRecord:\r\n\tpID: {0}\r\n\teID: {1}\r\n\tlID:{2}\r\n\tnID: {3}\r\n\tstring: {4}\r\n",
                                        platformID, encodingID, languageID, nameID, str);
            }
        }

        public class LangTagRecord
        {
            public UInt16 length;
            public UInt16 offset;

            public string str;

            //accept a BinaryReader and offset for string data.
            public LangTagRecord(BinaryReader r, long soffset)
            {
                length = FontInfo.reverse(r.ReadUInt16());
                offset = FontInfo.reverse(r.ReadUInt16());
                long ipos = r.BaseStream.Position;
                r.BaseStream.Seek(soffset + offset, SeekOrigin.Begin);
                str = new string(r.ReadChars(length));
                r.BaseStream.Seek(ipos, SeekOrigin.Begin);
            }

            public override string ToString()
            {
                return string.Format("LangTagRecord:\r\n\tpID: {0}\r\n", str);
            }
        }
    }

    /* Table entries */

    internal class FTable
    {
        public char[] tag;
        public UInt32 checkSum;
        public UInt32 offset;
        public UInt32 length;
        public byte[] payload;

        private FTable()
        { }

        public static FTable parseTable(BinaryReader reader)
        {
            FTable res = new FTable();
            res.tag = reader.ReadChars(4);
            res.checkSum = FontInfo.reverse(reader.ReadUInt32());
            res.offset = FontInfo.reverse(reader.ReadUInt32());
            res.length = FontInfo.reverse(reader.ReadUInt32());
            //res.payload = reader.ReadBytes((int)res.length - 16);
            return res;
        }

        public void readPayload(BinaryReader reader)
        {
            long ipos = reader.BaseStream.Position;
            reader.BaseStream.Seek(this.offset, SeekOrigin.Begin);
            this.payload = reader.ReadBytes((int)this.length);
            reader.BaseStream.Seek(ipos, SeekOrigin.Begin);
        }
    }

    #endregion OTF/TTF
}