using HarmonyLib;
using LGTS.UI.SaveMenu;
using LGTS.UI.WorldMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace xiaoye97
{
    public static class ILPatch
    {
        /// <summary>
        /// 在IL中替换文本
        /// </summary>
        public static IEnumerable<CodeInstruction> ReplaceIL(IEnumerable<CodeInstruction> instructions, string target, string i18n)
        {
            bool success = false;
            var list = instructions.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                var ci = list[i];
                if (ci.opcode == OpCodes.Ldstr)
                {
                    if ((string)ci.operand == target)
                    {
                        ci.operand = i18n;
                        success = true;
                    }
                }
            }
            if (!success)
            {
                LGTSChinesePlugin.Log($"汉化插件欲将{target}替换成{i18n}失败，没有找到目标");
            }
            return list.AsEnumerable();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(SaveSlot), "Setup")]
        public static IEnumerable<CodeInstruction> SaveSlot_Setup_Patch(IEnumerable<CodeInstruction> instructions)
        {
            instructions = ReplaceIL(instructions, "<b>DATE</b>  ", "<b>日期</b>  ");
            instructions = ReplaceIL(instructions, "<b>TIME</b>  ", "<b>时间</b>  ");
            return instructions;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(WorldMap), "Open")]
        public static IEnumerable<CodeInstruction> WorldMap_Open_Patch(IEnumerable<CodeInstruction> instructions)
        {
            instructions = ReplaceIL(instructions, "Kieferberg", "基弗伯格");
            return instructions;
        }
    }
}