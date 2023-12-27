using TMPro;
using System;
using System.IO;
using UnityEngine;

namespace xiaoye97
{
    [Serializable]
    public class FontData
    {
        public string DisplayName;
        public string FamilyName;
        public string FontPath;
        public string FontFileName;
        public Font Font;
        public TMP_FontAsset TMPFont;

        public FontData()
        {
        }

        public FontData(string fontPath)
        {
            FontPath = fontPath;
            FileInfo file = new FileInfo(fontPath);
            FontFileName = file.Name;
            Font = new Font(FontPath);
            TMPFont = TMP_FontAsset.CreateFontAsset(Font);
            TMPFont.name = FontFileName;
            Font.name = FontFileName;
            DisplayName = FontFileName;
            FontInfo fontInfo = new FontInfo(FontPath);
            fontInfo.readInfo();
            if (!string.IsNullOrWhiteSpace(fontInfo.FullName))
            {
                DisplayName = fontInfo.FullName;
            }
            if (!string.IsNullOrWhiteSpace(fontInfo.FamilyName))
            {
                FamilyName = fontInfo.FamilyName;
            }
        }
    }
}
