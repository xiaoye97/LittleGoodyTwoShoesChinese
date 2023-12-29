using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace xiaoye97
{
    public class TextFontPatchs
    {
        public static void ChangeFont(Text text, Font font = null)
        {
            if (text == null) return;
            if (font == null)
            {
                font = FontManager.MainFont.Font;
            }
            if (font == null || text.font == null) return;
            if (text.font.name != font.name)
            {
                text.font = font;
            }
        }

        /// <summary>
        /// 修改字体
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(Text), "OnEnable")]
        public static void FontPatch(Text __instance)
        {
            ChangeFont(__instance);
        }
    }
}