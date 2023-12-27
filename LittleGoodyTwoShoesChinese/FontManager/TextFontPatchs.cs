using HarmonyLib;
using UnityEngine.UI;

namespace xiaoye97
{
    public class TextFontPatchs
    {
        /// <summary>
        /// 修改字体
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(Text), "OnEnable")]
        public static void FontPatch(Text __instance)
        {
            if (FontManager.MainFont.Font == null || __instance.font == null) return;
            if (__instance.font.name != FontManager.MainFont.Font.name)
            {
                __instance.font = FontManager.MainFont.Font;
            }
        }
    }
}
