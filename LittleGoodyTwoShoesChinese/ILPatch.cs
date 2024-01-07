using HarmonyLib;
using LGTS.MiniGames.Chicken;
using LGTS.MiniGames.ChopChop;
using LGTS.MiniGames.Cursed.ChopChop;
using LGTS.MiniGames.Cursed.RApples;
using LGTS.MiniGames.KissTheRat;
using LGTS.MiniGames.RApples;
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

        /// <summary>
        /// 存档中的日期
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(SaveSlot), "Setup")]
        public static IEnumerable<CodeInstruction> SaveSlot_Setup_Patch(IEnumerable<CodeInstruction> instructions)
        {
            instructions = ReplaceIL(instructions, "<b>DATE</b>  ", "<b>日期</b>  ");
            instructions = ReplaceIL(instructions, "<b>TIME</b>  ", "<b>时间</b>  ");
            return instructions;
        }

        /// <summary>
        /// 地图中的地名
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(WorldMap), "Open")]
        public static IEnumerable<CodeInstruction> WorldMap_Open_Patch(IEnumerable<CodeInstruction> instructions)
        {
            instructions = ReplaceIL(instructions, "Kieferberg", "基弗伯格");
            return instructions;
        }

        #region 小游戏

        /// <summary>
        /// 小游戏通用替换
        /// </summary>
        private static IEnumerable<CodeInstruction> MiniGameLoop(IEnumerable<CodeInstruction> instructions)
        {
            instructions = ReplaceIL(instructions, "ROUND {0}", "第{0}回合");
            instructions = ReplaceIL(instructions, "GO!", "上!");
            return instructions;
        }

        /// <summary>
        /// 捡鸡蛋小游戏
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(ChickenFrenzy), "MinigameLoop", MethodType.Enumerator)]
        public static IEnumerable<CodeInstruction> ChickenFrenzy_MinigameLoop_Patch(IEnumerable<CodeInstruction> instructions)
        {
            return MiniGameLoop(instructions);
        }

        /// <summary>
        /// 砍木头小游戏
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(ChopChop), "MinigameLoop", MethodType.Enumerator)]
        public static IEnumerable<CodeInstruction> ChopChop_MinigameLoop_Patch(IEnumerable<CodeInstruction> instructions)
        {
            return MiniGameLoop(instructions);
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(CursedChopChop), "MinigameLoop", MethodType.Enumerator)]
        public static IEnumerable<CodeInstruction> CursedChopChop_MinigameLoop_Patch(IEnumerable<CodeInstruction> instructions)
        {
            return MiniGameLoop(instructions);
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(CursedRainingApples), "MinigameLoop", MethodType.Enumerator)]
        public static IEnumerable<CodeInstruction> CursedRainingApples_MinigameLoop_Patch(IEnumerable<CodeInstruction> instructions)
        {
            return MiniGameLoop(instructions);
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(KissTheRat), "MinigameLoop", MethodType.Enumerator)]
        public static IEnumerable<CodeInstruction> KissTheRat_MinigameLoop_Patch(IEnumerable<CodeInstruction> instructions)
        {
            return MiniGameLoop(instructions);
        }

        /// <summary>
        /// 苹果雨小游戏
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(RainingApples), "MinigameLoop", MethodType.Enumerator)]
        public static IEnumerable<CodeInstruction> RainingApples_MinigameLoop_Patch(IEnumerable<CodeInstruction> instructions)
        {
            return MiniGameLoop(instructions);
        }

        #endregion 小游戏
    }
}