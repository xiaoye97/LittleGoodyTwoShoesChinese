using TMPro;
using HarmonyLib;
using UnityEngine;

namespace xiaoye97
{
    public class TMPFontPatchs
    {
        public static void ChangeFont(TMP_Text text)
        {
            if (text.font.name != FontManager.MainFont.TMPFont.name)
            {
                //Debug.Log($"将{text.name}的字体从[{text.font.name}]替换为[{FontManager.MainFont.TMPFont.name}]");
                text.font = FontManager.MainFont.TMPFont;
            }
        }
        /// <summary>
        /// 修改TMP字体
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(TextMeshProUGUI), "OnEnable")]
        public static void TMPFontPatch(TextMeshProUGUI __instance)
        {
            if (FontManager.MainFont.TMPFont == null) return;
            ChangeFont(__instance);
        }

        /// <summary>
        /// 修改TMP字体
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(TextMeshPro), "OnEnable")]
        public static void TMPFontPatch2(TextMeshPro __instance)
        {
            if (FontManager.MainFont.TMPFont == null) return;
            ChangeFont(__instance);
        }

        /// <summary>
        /// 修改TMP字体
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(TMP_Text), "PopulateTextProcessingArray")]
        public static void TMPFontPatch3(TMP_Text __instance)
        {
            if (FontManager.MainFont.TMPFont == null) return;
            ChangeFont(__instance);
        }

        /// <summary>
        /// 如果有不显示的文本，则设置显示方式为溢出
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(TextMeshProUGUI), "InternalUpdate")]
        public static void TMPFontPatch3(TextMeshProUGUI __instance)
        {
            if (FontManager.MainFont.TMPFont == null) return;
            if (__instance.font == FontManager.MainFont.TMPFont)
            {
                if (__instance.overflowMode != TextOverflowModes.Overflow)
                {
                    if (__instance.preferredWidth > 1 && __instance.bounds.extents == Vector3.zero)
                    {
                        __instance.overflowMode = TextOverflowModes.Overflow;
                    }
                }
            }
        }
    }
}