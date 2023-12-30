using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace xiaoye97
{
    public static class FileHelper
    {
        public static Texture2D LoadTexture2D(string path)
        {
            Texture2D tex = new Texture2D(2, 2);
            tex.name = path;
            var bytes = File.ReadAllBytes(path);
            tex.LoadImage(bytes);
            return tex;
        }

        public static Texture2D LoadTexture2D(string path, int mipCount)
        {
            Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, mipCount, true);
            tex.name = path;
            var bytes = File.ReadAllBytes(path);
            tex.LoadImage(bytes);
            return tex;
        }

        public static Sprite LoadSprite(string path, bool setPerUnit = false)
        {
            Sprite sprite = null;
            Texture2D tex = LoadTexture2D(path);
            try
            {
                if (setPerUnit)
                {
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
                }
                else
                {
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }
            catch (Exception ex)
            {
                LGTSChinesePlugin.Log($"{ex}");
            }
            return sprite;
        }

        public static Sprite LoadSprite(string path, int mipCount, bool setPerUnit = false)
        {
            Sprite sprite = null;
            Texture2D tex = LoadTexture2D(path, mipCount);
            try
            {
                if (setPerUnit)
                {
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
                }
                else
                {
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }
            catch (Exception ex)
            {
                LGTSChinesePlugin.Log($"{ex}");
            }
            return sprite;
        }
    }
}
