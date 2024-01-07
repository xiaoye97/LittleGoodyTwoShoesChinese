using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace xiaoye97
{
    public class ImageData
    {
        public string Name;
        public string InstanceID;
        public string Path;
        public string ReplaceMode;
        public Dictionary<int, Texture2D> MipTexDict = new Dictionary<int, Texture2D>();

        public Texture2D GetTex(Texture2D src)
        {
            if (MipTexDict.ContainsKey(src.mipmapCount)) return MipTexDict[src.mipmapCount];
            Texture2D tex = new Texture2D(src.width, src.height, src.format, src.mipmapCount, true);
            var bytes = File.ReadAllBytes(Path);
            tex.LoadImage(bytes);
            tex.name = Path;
            MipTexDict[src.mipmapCount] = tex;
            return tex;
        }
    }
}