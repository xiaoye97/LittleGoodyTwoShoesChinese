using BepInEx;
using Gridly;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using System.Collections.Generic;
using BepInEx.Logging;

namespace xiaoye97
{
    [BepInPlugin("xiaoye97.LGTSChinesePlugin", "LGTSChinesePlugin", "1.0")]
    public class LGTSChinesePlugin : BaseUnityPlugin
    {
        #region 配置

        public static ConfigEntry<bool> DevMode;
        public static ConfigEntry<KeyCode> SwitchChineseHotkey;
        public static ConfigEntry<KeyCode> ReloadExcelHotkey;
        public static ConfigEntry<KeyCode> DumpHotkey;

        #endregion 配置

        public static string ExcelPath;

        public static LGTSDumper LGTSDumper;
        public static LGTSExcelLoader LGTSExcelLoader;
        private static ManualLogSource logger;

        private static bool _EnableChinese;

        public static bool EnableChinese
        {
            get { return _EnableChinese; }
            set
            {
                if (value != _EnableChinese)
                {
                    _EnableChinese = value;
                    Log($"启用中文:{value}");
                    RefreshAllTranslareText();
                }
            }
        }

        public static Dictionary<string, SheetData> Sheets = new Dictionary<string, SheetData>();

        private void Awake()
        {
            logger = Logger;
            Log("LGTSChinesePlugin加载了");
            ExcelPath = $"{Paths.PluginPath}/LittleGoodyTwoShoesChinese.xlsx";
            DevMode = Config.Bind<bool>("config", "DevMode", false, "如果打开了开发模式,则可以使用快捷键进行一些快捷操作");
            SwitchChineseHotkey = Config.Bind<KeyCode>("config", "SwitchChineseHotkey", KeyCode.F9, "开关汉化快捷键");
            ReloadExcelHotkey = Config.Bind<KeyCode>("config", "ReloadExcelHotkey", KeyCode.F10, "重新加载Excel表格快捷键");
            DumpHotkey = Config.Bind<KeyCode>("config", "DumpHotkey", KeyCode.F11, "导出文本快捷键");
            LGTSDumper = new LGTSDumper();
            LGTSExcelLoader = new LGTSExcelLoader();
            LoadExcel();
            Harmony.CreateAndPatchAll(typeof(LGTSChinesePlugin));
        }

        public static void Log(string msg)
        {
            logger.LogInfo(msg);
        }

        private void Update()
        {
            if (DevMode.Value)
            {
                if (Input.GetKeyDown(SwitchChineseHotkey.Value))
                {
                    EnableChinese = !EnableChinese;
                }
                if (Input.GetKeyDown(ReloadExcelHotkey.Value))
                {
                    LoadExcel();
                }
                if (Input.GetKeyDown(DumpHotkey.Value))
                {
                    LGTSDumper.DumpExcel();
                }
            }
        }

        public static void LoadExcel()
        {
            Sheets = LGTSExcelLoader.LoadExcel();
            RefreshAllTranslareText();
        }

        public static void RefreshAllTranslareText()
        {
            List<TranslareText> list = new List<TranslareText>();
            list.AddRange(UnityEngine.Object.FindObjectsOfType<Translator>());
            list.AddRange(UnityEngine.Object.FindObjectsOfType<SetLanguageImage>());
            foreach (TranslareText translareText in list)
            {
                translareText.Refesh();
            }
        }

        /// <summary>
        /// 对获取翻译进行patch
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(GridlyLocal), "GetStringData")]
        public static bool GridlyLocal_GetStringData(string grid, string recordID, ref string __result)
        {
            if (!_EnableChinese) return true;
            if (Sheets.TryGetValue(grid, out var sheet))
            {
                foreach (var row in sheet.RowDatas)
                {
                    if (row.ID == recordID)
                    {
                        if (!string.IsNullOrWhiteSpace(row.翻译文本))
                        {
                            __result = row.翻译文本;
                            Log("gridId: " + grid + " recordID: " + recordID + " 取到翻译: " + __result);
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}