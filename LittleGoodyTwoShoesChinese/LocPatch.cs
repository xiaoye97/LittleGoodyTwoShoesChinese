using Gridly;
using HarmonyLib;
using LGTS.Cinematics.Subtitles;
using LGTS.Domain.DataManagers;
using LGTS.Graphics;
using LGTS.LGTS_Utils;
using LGTS.MiniGames;
using LGTS.UI.PopupWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace xiaoye97
{
    /// <summary>
    /// 翻译相关的Patch
    /// </summary>
    public static class LocPatch
    {
        /// <summary>
        /// 约会后的人名显示处理
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(PopupLauncher), "LaunchRomancePopup")]
        public static bool PopupLauncher_AddOption_Postfix(PopupLauncher __instance, string loveInterest, Action onAccept)
        {
            if (!LGTSChinesePlugin.EnableChinese) return true;
            int relationship = PlayerDataManager.GetRelationship(loveInterest);
            var ex2data = LGTSChinesePlugin.UILocDatasEx2.Values.Where((d) => d.原文 == loveInterest && d.类型 == "人名").First();
            string 人名 = loveInterest;
            if (ex2data != null)
            {
                人名 = ex2data.翻译文本;
            }
            string text = 人名;
            string text2;
            if (relationship < 5)
            {
                text2 = "<b>" + 人名 + "</b> \n " + LocalizationMediator.GetTranslation("UI", "HEART_ACQUIRED");
            }
            else if (relationship == 7)
            {
                text2 = "<b>" + 人名 + "</b> \n " + LocalizationMediator.GetTranslation("UI", "MAX_LOVE_ACQUIRED");
                text = "MaxBond";
            }
            else
            {
                text2 = "<b>" + 人名 + "</b> \n " + LocalizationMediator.GetTranslation("UI", "STRONG_BOND_ACQUIRED");
                text = "StrongBond";
            }
            __instance.LaunchPopup(PopupLauncher.PopupType.RomanceAlert, new PopupContext(new object[] { text2, text, onAccept }), null, null);
            return false;
        }

        /// <summary>
        /// 翻译设置中的选择按钮
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="text"></param>
        [HarmonyPostfix, HarmonyPatch(typeof(ShiftableOptions), "AddOption")]
        public static void ShiftableOptions_AddOption_Postfix(ShiftableOptions __instance, string text)
        {
            if (!LGTSChinesePlugin.EnableChinese) return;
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

        /// <summary>
        /// 小游戏屏幕中字的颜色
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(MinigameScoreScreen), "SetAnimation")]
        public static void MinigameScoreScreen_SetAnimation_Postfix(MinigameScoreScreen __instance, int grade, int finalScore)
        {
            if (!LGTSChinesePlugin.EnableChinese) return;
            __instance.minigameGradeScreen.scoreText.SetText($"<color=#000000>{finalScore}</color>", true);
            __instance.minigameGradeScreen.gradeText.SetText($"<color=#000000>{__instance.GradeIndexToText(grade)}</color>", true);
        }

        private static List<string> SubtitleTrackMixerCache = new List<string>();

        // 在需要Dump的时候再取消这个注释
        //[HarmonyPostfix, HarmonyPatch(typeof(SubtitleTrackMixer), "ProcessFrame")]
        public static void SubtitleTrackMixer_ProcessFrame_Postfix(SubtitleTrackMixer __instance, object playerData)
        {
            if (!LGTSChinesePlugin.EnableChinese) return;
            TextMeshProUGUI textMeshProUGUI = playerData as TextMeshProUGUI;
            if (textMeshProUGUI != null && !string.IsNullOrWhiteSpace(textMeshProUGUI.text))
            {
                if (!SubtitleTrackMixerCache.Contains(textMeshProUGUI.text))
                {
                    SubtitleTrackMixerCache.Add(textMeshProUGUI.text);
                    LGTSChinesePlugin.Log($"Timeline Subtitle原文:[{textMeshProUGUI.text}]");
                }
            }
        }
    }
}