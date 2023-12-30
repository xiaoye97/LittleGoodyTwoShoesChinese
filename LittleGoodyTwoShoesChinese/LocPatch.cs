using Gridly;
using HarmonyLib;
using LGTS.Domain.DataManagers;
using LGTS.Domain.Entities;
using LGTS.Graphics;
using LGTS.UI.PopupWindows;
using LGTS.UI.Quests;
using LGTS.UI.Shops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

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
                if (LGTSChinesePlugin.UILocDatasEx.ContainsKey(text.text))
                {
                    string 翻译文本 = LGTSChinesePlugin.UILocDatasEx[text.text];
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
        //[HarmonyPostfix, HarmonyPatch(typeof(EntityDataManager), "GetEntityAlias")]
        public static void EntityDataManager_GetEntityAlias_Postfix(EntityDataManager __instance, string entityName, ref string __result)
        {
            if (LGTSChinesePlugin.EnableChinese)
            {
                Entity entity = EntityDataManager.Get(entityName);
                bool hasLoc = false;
                if (entity != null)
                {
                    string alias = entity.Alias;
                    // 如果alias等于名字, 说明没有alias, 使用原名查找翻译
                    if (alias == entityName)
                    {
                        if (LGTSChinesePlugin.NameLocDatas.TryGetValue(entityName, out string locAlias))
                        {
                            if (!string.IsNullOrWhiteSpace(locAlias))
                            {
                                LGTSChinesePlugin.Log($"取到了人名翻译, Name:[{entityName}] Alias:[{alias}] 翻译名:[{locAlias}]");
                                __result = locAlias;
                                hasLoc = true;
                            }
                        }
                    }
                    // 如果有alias, 则以这个alias来查找翻译
                    else
                    {
                        if (LGTSChinesePlugin.NameLocDatas.TryGetValue(alias, out string locAlias))
                        {
                            if (!string.IsNullOrWhiteSpace(locAlias))
                            {
                                LGTSChinesePlugin.Log($"取到了人名翻译, Name:[{entityName}] Alias:[{alias}] 翻译名:[{locAlias}]");
                                __result = locAlias;
                                hasLoc = true;
                            }
                        }
                    }
                    if (!hasLoc)
                    {
                        LGTSChinesePlugin.Log($"没有取到人名翻译, Name:[{entityName}] Alias:[{alias}]");
                    }
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

        /// <summary>
        /// 地图菜单中, 任务类型的翻译
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix, HarmonyPatch(typeof(QuestLog), "Awake")]
        public static void QuestLog_SetupNewEntry_Postfix(QuestLog __instance)
        {
            var Story = __instance.questTypeConfigs[QuestGoal.QuestType.Story];
            Story.questTypeInfo = "故事事件";
            __instance.questTypeConfigs[QuestGoal.QuestType.Story] = Story;
            var Romantic = __instance.questTypeConfigs[QuestGoal.QuestType.Romantic];
            Romantic.questTypeInfo = "浪漫约会";
            __instance.questTypeConfigs[QuestGoal.QuestType.Romantic] = Romantic;
            var Task = __instance.questTypeConfigs[QuestGoal.QuestType.Task];
            Task.questTypeInfo = "任务事件";
            __instance.questTypeConfigs[QuestGoal.QuestType.Task] = Task;
            var Small = __instance.questTypeConfigs[QuestGoal.QuestType.Small];
            Small.questTypeInfo = "小目标";
            __instance.questTypeConfigs[QuestGoal.QuestType.Small] = Small;
        }

        /// <summary>
        /// 商店名字的翻译
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(Shop), "Start")]
        public static void Shop_Start(Shop __instance)
        {
            if (LGTSChinesePlugin.EnableChinese)
            {
                Shop.ShopID shopID = Traverse.Create(__instance).Field("shopID").GetValue<Shop.ShopID>();
                if (shopID == Shop.ShopID.LebShop)
                {
                    var go = GameObject.Find("Shop/ShopUI/Banner/Text (TMP)");
                    if (go != null)
                    {
                        go.GetComponent<TextMeshProUGUI>().text = "斯佩尔特的面包店";
                    }
                }
            }
        }
    }
}