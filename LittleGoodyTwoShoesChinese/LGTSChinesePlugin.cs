using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Gridly;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace xiaoye97
{
    [BepInPlugin("xiaoye97.LGTSChinesePlugin", "LGTSChinesePlugin", "1.0")]
    public class LGTSChinesePlugin : BaseUnityPlugin
    {
        #region 配置

        public static ConfigEntry<string> MainFontConfig;
        public static ConfigEntry<string> FallbackFontsConfig;

        public static ConfigEntry<bool> DevMode;
        public static ConfigEntry<KeyCode> SwitchChineseHotkey;
        public static ConfigEntry<KeyCode> ReloadExcelHotkey;
        public static ConfigEntry<KeyCode> DumpHotkey;
        public static ConfigEntry<KeyCode> DumpSceneNoTranslatorTMPsHotkey;
        public static ConfigEntry<KeyCode> DumpSceneNoTranslatorTMPsIncludeInactiveHotkey;

        #endregion 配置

        public static string PluginDirPath;
        public static string ChineseExcelPath;
        public static string UIExcelPath;

        public static LGTSDumper LGTSDumper;
        public static LGTSExcelLoader LGTSExcelLoader;
        public static LGTSUIExcelLoader LGTSUIExcelLoader;
        public static FontManager FontManager;
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

        public static Dictionary<string, UILocData> UILocDatas = new Dictionary<string, UILocData>();

        private void Awake()
        {
            logger = Logger;
            Log("LGTSChinesePlugin加载了");
            FileInfo fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            PluginDirPath = fileInfo.DirectoryName;
            ChineseExcelPath = $"{PluginDirPath}/LittleGoodyTwoShoesChinese.xlsx";
            UIExcelPath = $"{PluginDirPath}/UI.xlsx";
            MainFontConfig = Config.Bind<string>("config", "MainFont", "LXGWWenKai-Regular.ttf", "主字体，如果没有主字体，则使用第一个后备字体作为主字体");
            FallbackFontsConfig = Config.Bind<string>("config", "FallbackFonts", "msyh.ttc;arial.ttf", "后备字体列表，使用;分隔，后备字体仅对TextMeshPro生效");
            DevMode = Config.Bind<bool>("dev", "DevMode", false, "如果打开了开发模式,则可以使用快捷键进行一些快捷操作");
            SwitchChineseHotkey = Config.Bind<KeyCode>("dev", "SwitchChineseHotkey", KeyCode.F9, "开关汉化快捷键");
            ReloadExcelHotkey = Config.Bind<KeyCode>("dev", "ReloadExcelHotkey", KeyCode.F10, "重新加载Excel表格快捷键");
            DumpHotkey = Config.Bind<KeyCode>("dev", "DumpHotkey", KeyCode.F11, "导出文本快捷键");
            DumpSceneNoTranslatorTMPsHotkey = Config.Bind<KeyCode>("dev", "DumpSceneNoTranslatorTMPsHotkey", KeyCode.F1, "导出当前场景没有Translator的文本(不包括隐藏的物体)");
            DumpSceneNoTranslatorTMPsIncludeInactiveHotkey = Config.Bind<KeyCode>("dev", "DumpSceneNoTranslatorTMPsIncludeInactiveHotkey", KeyCode.F2, "导出当前场景没有Translator的文本(包括隐藏的物体)");
            LGTSDumper = new LGTSDumper();
            LGTSExcelLoader = new LGTSExcelLoader();
            LGTSUIExcelLoader = new LGTSUIExcelLoader();
            FontManager = new FontManager(true, PluginDirPath, false, true);
            FontManager.Init();
            FontManager.SetMainFont(MainFontConfig.Value, FallbackFontsConfig.Value.Split(';'));
            LoadExcel();
            EnableChinese = true;
            Harmony.CreateAndPatchAll(typeof(LGTSChinesePlugin));
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            var tmps = GameObject.FindObjectsOfType<TextMeshProUGUI>();
            foreach (var tmp in tmps)
            {
                AddTranslator(tmp);
            }
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
                if (Input.GetKeyDown(DumpSceneNoTranslatorTMPsHotkey.Value))
                {
                    DumpSceneNoTranslatorTMPs(false);
                }
                if (Input.GetKeyDown(DumpSceneNoTranslatorTMPsIncludeInactiveHotkey.Value))
                {
                    DumpSceneNoTranslatorTMPs(true);
                }
            }
        }

        public static void LoadExcel()
        {
            Sheets = LGTSExcelLoader.LoadExcel();
            UILocDatas = LGTSUIExcelLoader.LoadExcel();
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
        /// 导出当前场景没有Translator的文本
        /// </summary>
        public static void DumpSceneNoTranslatorTMPs(bool includeInactive)
        {
            try
            {
                Log($"开始搜索当前场景没有翻译组件的TMP列表");
                StringBuilder sb = new StringBuilder();
                var tmps = GameObject.FindObjectsOfType<TextMeshProUGUI>(includeInactive);
                foreach (var tmp in tmps)
                {
                    var translator = tmp.GetComponent<Translator>();
                    if (translator == null)
                    {
                        string data = $"路径:[{tmp.transform.GetPath()}] 文本:[{tmp.text.Replace("\n", "\\n").Replace("\r", "\\r")}]";

                        sb.AppendLine(data);
                        Log(data);
                    }
                }
                string path = $"{PluginDirPath}/当前场景没有翻译组件的TMP列表.txt";
                Log($"将TMP列表保存到:{path}");
                File.WriteAllText(path, sb.ToString());
                Log($"已保存文件{path}");
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        /// <summary>
        /// 为文本组件添加翻译组件
        /// </summary>
        /// <param name="text"></param>
        public static void AddTranslator(TextMeshProUGUI text)
        {
            var translator = text.GetComponent<Translator>();
            if (translator == null)
            {
                var path = text.transform.GetPath();
                if (UILocDatas.TryGetValue(path, out var data))
                {
                    translator = text.gameObject.AddComponent<Translator>();
                    translator.textMeshPro = text;
                    Log($"给[{path}]添加了翻译组件");
                    if (!string.IsNullOrWhiteSpace(data.表名) && !string.IsNullOrWhiteSpace(data.翻译ID))
                    {
                        translator.grid = data.表名;
                        translator.key = data.翻译ID;
                    }
                    else
                    {
                        translator.grid = "LGTSChinesePlugin";
                        translator.key = path;
                    }
                    translator.Refesh();
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TextMeshProUGUI), "OnEnable")]
        public static void TMPFontPatch1(TextMeshProUGUI __instance)
        {
            AddTranslator(__instance);
        }

        /// <summary>
        /// 对获取翻译进行patch
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(GridlyLocal), "GetStringData")]
        public static bool GridlyLocal_GetStringData(string grid, string recordID, ref string __result)
        {
            if (grid == "LGTSChinesePlugin")
            {
                if (UILocDatas.ContainsKey(recordID))
                {
                    if (EnableChinese)
                    {
                        __result = UILocDatas[recordID].直接翻译;
                        Log($"UI:[{recordID}] 取到翻译:[{__result}]");
                    }
                    else
                    {
                        __result = UILocDatas[recordID].原文备注;
                        Log($"UI:[{recordID}] 取到原文:[{__result}]");
                    }
                    return false;
                }
                else
                {
                    Log($"UI:[{recordID}]被标记为了汉化插件翻译UI, 但是没有对应的翻译数据");
                }
            }
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