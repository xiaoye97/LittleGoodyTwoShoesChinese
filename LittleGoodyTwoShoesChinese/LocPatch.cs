using Gridly;
using HarmonyLib;
using LGTS.Domain.DataManagers;
using LGTS.Graphics;
using LGTS.UI.PopupWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace xiaoye97
{
    /// <summary>
    /// 翻译相关的Patch
    /// </summary>
    public static class LocPatch
    {
        /// <summary>
        /// 翻译弹窗中的文本
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix, HarmonyPatch(typeof(PopupWindowText), "SetContext")]
        public static void PopupWindowText_SetContext_Postfix(PopupWindowText __instance)
        {
            if (LGTSChinesePlugin.EnableChinese)
            {
                TMP_Text text = Traverse.Create(__instance).Field("text").GetValue<TMP_Text>();
                if (LGTSChinesePlugin.UILocDatas2.ContainsKey(text.text))
                {
                    string 翻译文本 = LGTSChinesePlugin.UILocDatas2[text.text];
                    LGTSChinesePlugin.Log($"PopupWindowText获取到翻译,原文:[{text.text}] 翻译:[{翻译文本}]");
                    text.text = 翻译文本;
                }
                else
                {
                    LGTSChinesePlugin.Log($"PopupWindowText没有获取到翻译,原文:[{text.text}]");
                }
            }
        }

        /// <summary>
        /// 翻译人的名字
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(EntityDataManager), "GetEntityAlias")]
        public static void EntityDataManager_GetEntityAlias_Postfix(EntityDataManager __instance, string entityName, ref string __result)
        {
            if (LGTSChinesePlugin.EnableChinese)
            {
                if (LGTSChinesePlugin.NameLocDatas.TryGetValue(entityName, out string alias))
                {
                    LGTSChinesePlugin.Log($"取到了人名翻译, [{entityName}] [{alias}]");
                    __result = alias;
                }
            }
        }

        /// <summary>
        /// 翻译设置中的选择按钮
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="text"></param>
        [HarmonyPostfix, HarmonyPatch(typeof(ShiftableOptions), "AddOption")]
        public static void ShiftableOptions_AddOption_Postfix(ShiftableOptions __instance, string text)
        {
            var panel = __instance.scrollSnap.Panels[__instance.scrollSnap.Panels.Length - 1];
            var tmp = panel.GetComponentInChildren<TextMeshProUGUI>();
            var translator = tmp.GetComponent<Translator>();
            if (translator == null)
            {
                translator = tmp.gameObject.AddComponent<Translator>();
                translator.textMeshPro = tmp;
                translator.grid = "UI";
                translator.key = text;
                LGTSChinesePlugin.Log($"为ShiftableOptions的选项文本组件:[{__instance.transform.GetPath()}]添加了翻译组件, grid:[UI] key:[{text}]");
            }
        }
    }
}