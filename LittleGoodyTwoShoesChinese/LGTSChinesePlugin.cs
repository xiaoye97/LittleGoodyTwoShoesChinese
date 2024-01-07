using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Gridly;
using Gridly.Internal;
using HarmonyLib;
using LGTS.Cinematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        public static ConfigEntry<KeyCode> FindAndAddTranslatorHotkey;
        public static ConfigEntry<KeyCode> PauseGameHotkey;
        public static ConfigEntry<KeyCode> DumpHotkey;
        public static ConfigEntry<KeyCode> DumpSceneNoTranslatorTMPsHotkey;
        public static ConfigEntry<KeyCode> DumpSceneNoTranslatorTMPsIncludeInactiveHotkey;

        #endregion 配置

        public static string PluginDirPath;
        public static string ChineseExcelPath;
        public static string ImagesDirPath;

        public static LGTSDumper LGTSDumper;
        public static LGTSExcelLoader LGTSExcelLoader;
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
                    Log($"启用中文:{value}", false);
                    RefreshAllTranslareText();
                }
            }
        }

        public static Dictionary<string, SheetData> Sheets = new Dictionary<string, SheetData>();
        public static Dictionary<string, UILocData> UILocDatas = new Dictionary<string, UILocData>();
        public static Dictionary<string, string> UILocDatasEx = new Dictionary<string, string>();
        public static Dictionary<string, UIEx2Data> UILocDatasEx2 = new Dictionary<string, UIEx2Data>();
        public static Dictionary<string, List<ImageData>> ImageLocDatas = new Dictionary<string, List<ImageData>>();

        private void Awake()
        {
            logger = Logger;
            Log("LGTSChinesePlugin加载了", false);
            FileInfo fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            PluginDirPath = fileInfo.DirectoryName;
            ChineseExcelPath = $"{PluginDirPath}/LittleGoodyTwoShoesChinese.xlsx";
            ImagesDirPath = $"{PluginDirPath}/Images";
            MainFontConfig = Config.Bind<string>("config", "MainFont", "LXGWWenKai-Regular.ttf", "主字体，如果没有主字体，则使用第一个后备字体作为主字体");
            FallbackFontsConfig = Config.Bind<string>("config", "FallbackFonts", "msyh.ttc;arial.ttf", "后备字体列表，使用;分隔，后备字体仅对TextMeshPro生效");
            DevMode = Config.Bind<bool>("dev", "DevMode", false, "如果打开了开发模式,则可以使用快捷键进行一些快捷操作");
            SwitchChineseHotkey = Config.Bind<KeyCode>("dev", "SwitchChineseHotkey", KeyCode.F9, "开关汉化快捷键");
            ReloadExcelHotkey = Config.Bind<KeyCode>("dev", "ReloadExcelHotkey", KeyCode.F10, "重新加载Excel表格快捷键");
            DumpHotkey = Config.Bind<KeyCode>("dev", "DumpHotkey", KeyCode.F11, "导出文本快捷键");
            DumpSceneNoTranslatorTMPsHotkey = Config.Bind<KeyCode>("dev", "DumpSceneNoTranslatorTMPsHotkey", KeyCode.F1, "导出当前场景没有Translator的文本(不包括隐藏的物体)");
            DumpSceneNoTranslatorTMPsIncludeInactiveHotkey = Config.Bind<KeyCode>("dev", "DumpSceneNoTranslatorTMPsIncludeInactiveHotkey", KeyCode.F2, "导出当前场景没有Translator的文本(包括隐藏的物体)");
            FindAndAddTranslatorHotkey = Config.Bind<KeyCode>("dev", "FindAndAddTranslatorHotkey", KeyCode.F3, "开始查找并添加翻译组件快捷键");
            PauseGameHotkey = Config.Bind<KeyCode>("dev", "PauseGameHotkey", KeyCode.F4, "暂停游戏快捷键");
            LGTSDumper = new LGTSDumper();
            LGTSExcelLoader = new LGTSExcelLoader();
            FontManager = new FontManager(true, PluginDirPath, false, true);
            FontManager.Init();
            FontManager.SetMainFont(MainFontConfig.Value, FallbackFontsConfig.Value.Split(';'));
            LoadExcel();
            EnableChinese = true;
            Harmony.CreateAndPatchAll(typeof(LGTSChinesePlugin));
            Harmony.CreateAndPatchAll(typeof(LocPatch));
            Harmony.CreateAndPatchAll(typeof(ILPatch));
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            FindAndAddTranslator();
        }

        public static void Log(string msg, bool onlyShowInDevMode = true)
        {
            if (onlyShowInDevMode && DevMode != null && !DevMode.Value) return;
            logger.LogInfo(msg);
        }

        public static void LogWarning(string msg, bool onlyShowInDevMode = true)
        {
            if (onlyShowInDevMode && DevMode != null && !DevMode.Value) return;
            logger.LogWarning(msg);
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
                    DumpSceneNoTranslators(false);
                }
                if (Input.GetKeyDown(DumpSceneNoTranslatorTMPsIncludeInactiveHotkey.Value))
                {
                    DumpSceneNoTranslators(true);
                }
                if (Input.GetKeyDown(FindAndAddTranslatorHotkey.Value))
                {
                    FindAndAddTranslator();
                }
                if (Input.GetKeyDown(PauseGameHotkey.Value))
                {
                    Time.timeScale = Time.timeScale == 1 ? 0 : 1;
                }
            }
        }

        public static void LoadExcel()
        {
            Sheets = LGTSExcelLoader.LoadMainExcel();
            UILocDatas = LGTSExcelLoader.LoadUIExcel();
            UILocDatasEx = LGTSExcelLoader.LoadUIExcelEx();
            UILocDatasEx2 = LGTSExcelLoader.LoadUIExcelEx2();
            ImageLocDatas = LGTSExcelLoader.LoadImageExcel();
            RefreshAllTranslareText();
            FindAndAddTranslator();
        }

        public static void FindAndAddTranslator()
        {
            Log("开始查找并添加翻译组件");
            int count = 0;
            var tmps = GameObject.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var tmp in tmps)
            {
                if (tmp != null)
                {
                    bool result = AddTranslator(tmp);
                    if (result)
                    {
                        Log($"为TMP文本组件:[{tmp.transform.GetPath()}]添加了翻译组件");
                        count++;
                    }
                }
            }
            Log($"共找到{tmps.Length}个没有翻译组件的文本, 添加了{count}个翻译组件");
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
        public static void DumpSceneNoTranslators(bool includeInactive)
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
                        string data = $"路径:[{tmp.transform.GetPath()}] TMP文本:[{tmp.text.Replace("\n", "\\n").Replace("\r", "\\r")}]";
                        sb.AppendLine(data);
                        //Log(data);
                    }
                }
                string path = $"{PluginDirPath}/当前场景没有翻译组件的TMP列表.txt";
                Log($"将TMP列表保存到:{path}");
                File.WriteAllText(path, sb.ToString());
                Log($"已保存文件{path}");

                Log($"开始搜索当前场景没有翻译组件的Text列表");
                sb.Clear();
                var texts = GameObject.FindObjectsOfType<Text>(includeInactive);
                foreach (var text in texts)
                {
                    var translator = text.GetComponent<Translator>();
                    if (translator == null)
                    {
                        string data = $"路径:[{text.transform.GetPath()}] Text文本:[{text.text.Replace("\n", "\\n").Replace("\r", "\\r")}]";
                        sb.AppendLine(data);
                        //Log(data);
                    }
                }
                string path2 = $"{PluginDirPath}/当前场景没有翻译组件的Text列表.txt";
                Log($"将Text列表保存到:{path2}");
                File.WriteAllText(path2, sb.ToString());
                Log($"已保存文件{path2}");
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
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
                    Log($"UI:[{recordID}]被标记为了汉化插件翻译UI, 但是没有对应的翻译数据", false);
                }
            }
            if (EnableChinese)
            {
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
                // 补充UI翻译
                if (grid == "UI")
                {
                    if (UILocDatasEx.ContainsKey(recordID))
                    {
                        __result = UILocDatasEx[recordID];
                        Log("gridId: " + grid + " recordID: " + recordID + " 取到翻译: " + __result);
                        return false;
                    }
                }
            }
            // 原版的获取翻译
            Record record = Project.singleton.grids.Find((Grid x) => x.nameGrid == grid).records.Find((Record x) => x.recordID == recordID);
            if (record != null)
            {
                foreach (var column in record.columns)
                {
                    try
                    {
                        if (column.columnID == Project.singleton.targetLanguage.languagesSuport.ToString())
                        {
                            __result = column.text;
                            Log("gridId: " + grid + " recordID: " + recordID + " 原版取到翻译: " + __result);
                            return false;
                        }
                    }
                    catch
                    {
                        Debug.Log("cant found: " + recordID + " | code:" + Project.singleton.targetLanguage.languagesSuport.ToString());
                        foreach (LangSupport langSupport in Project.singleton.langSupports)
                        {
                            if (column.columnID == langSupport.languagesSuport.ToString())
                            {
                                __result = column.text;
                                Log("gridId: " + grid + " recordID: " + recordID + " 原版取到翻译: " + __result);
                                return false;
                            }
                        }
                    }
                }
            }
            Log("gridId: " + grid + " recordID: " + recordID + " 没有找到翻译使用recordID");
            __result = recordID;
            return false;
        }

        #region 添加翻译组件

        private static bool AddTranslatorComponent(Transform transform, TextMeshProUGUI tmp, Text text)
        {
            Translator translator;
            var path = transform.GetPath();
            // 防止在Mod中翻译
            if (path.StartsWith("UniverseLibCanvas")) return false;
            bool hasLoc = UILocDatas.TryGetValue(path, out var data);
            if (hasLoc)
            {
                translator = transform.gameObject.AddComponent<Translator>();
                translator.textMeshPro = tmp;
                translator.text = text;
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
                return true;
            }
            else
            {
                // 如果没有翻译, 则在补充UI翻译表2内查找是否可以直接从原文翻译
                if (tmp != null && !string.IsNullOrWhiteSpace(tmp.text))
                {
                    string 原文 = tmp.text.Replace("\n", "\\n").Replace("\r", "\\r");
                    if (UILocDatasEx2.TryGetValue(原文, out var ex2data))
                    {
                        tmp.text = ex2data.翻译文本;
                        Log($"对[{tmp.transform.GetPath()}] 进行无翻译组件翻译, 原文:[{原文}] 翻译文本:[{ex2data.翻译文本}]");
                    }
                }
                if (text != null && !string.IsNullOrWhiteSpace(text.text))
                {
                    string 原文 = text.text.Replace("\n", "\\n").Replace("\r", "\\r"); ;
                    if (UILocDatasEx2.TryGetValue(原文, out var ex2data))
                    {
                        text.text = ex2data.翻译文本;
                        Log($"对[{text.transform.GetPath()}] 进行无翻译组件翻译, 原文:[{原文}] 翻译文本:[{ex2data.翻译文本}]");
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 为文本组件添加翻译组件
        /// </summary>
        /// <param name="tmp"></param>
        public static bool AddTranslator(TextMeshProUGUI tmp)
        {
            if (tmp == null) return false;
            try
            {
                var translator = tmp.GetComponent<Translator>();
                if (translator == null)
                {
                    return AddTranslatorComponent(tmp.transform, tmp, null);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
            return false;
        }

        public static bool AddTranslator(Text text)
        {
            if (text == null) return false;
            try
            {
                var translator = text.GetComponent<Translator>();
                if (translator == null)
                {
                    return AddTranslatorComponent(text.transform, null, text);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Text), "OnEnable")]
        public static void Text_OnEnable_Postfix(Text __instance)
        {
            if (AddTranslator(__instance))
            {
                //Log($"为Text文本组件:[{__instance.transform.GetPath()}]在OnEnable时添加了翻译组件");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TextMeshProUGUI), "OnEnable")]
        public static void TextMeshProUGUI_OnEnable_Postfix(TextMeshProUGUI __instance)
        {
            if (AddTranslator(__instance))
            {
                //Log($"为TMP文本组件:[{__instance.transform.GetPath()}]在OnEnable时添加了翻译组件");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TMP_Text), "text", MethodType.Setter)]
        public static void TMP_Text_text_Postfix(TMP_Text __instance)
        {
            if (AddTranslator(__instance as TextMeshProUGUI))
            {
                //Log($"为TMP文本组件:[{__instance.transform.GetPath()}]在OnEnable时添加了翻译组件");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TimelineDirector), "Play")]
        public static void TimelineDirector_Play_Postfix(TimelineDirector __instance)
        {
            FindAndAddTranslator();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Translator), "OnEnable")]
        public static bool Translator_OnEnable_Postfix(Translator __instance)
        {
            if (__instance.text == null)
            {
                __instance.text = __instance.GetComponent<Text>();
            }
            if (__instance.textMeshPro == null)
            {
                __instance.textMeshPro = __instance.GetComponent<TextMeshProUGUI>();
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Translator), "Refesh")]
        public static bool Translator_Refesh_Postfix(Translator __instance)
        {
            if (__instance.text != null)
            {
                Font defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                TextFontPatchs.ChangeFont(__instance.text, defaultFont);
            }
            TMPFontPatchs.ChangeFont(__instance.textMeshPro);
            return true;
        }

        #endregion 添加翻译组件

        #region 图片翻译

        private static ImageData GetLocImage(string name, string instanceID)
        {
            if (ImageLocDatas.TryGetValue(name, out var locDatas))
            {
                if (locDatas.Count == 1)
                {
                    var locData = locDatas[0];
                    // 如果没有填InstanceID, 则直接使用此图片
                    if (string.IsNullOrWhiteSpace(locData.InstanceID))
                    {
                        return locData;
                    }
                    // 如果填了InstanceID, 则必须一致才能使用
                    else
                    {
                        if (locData.InstanceID == instanceID)
                        {
                            return locData;
                        }
                    }
                }
                // 如果同名图片超过1个, 则必须匹配InstanceID
                else
                {
                    foreach (var locData in locDatas)
                    {
                        if (locData.InstanceID == instanceID)
                        {
                            return locData;
                        }
                    }
                }
            }
            return null;
        }

        public static void ChangeSpriteTexture(Transform t, Texture2D gameTexture)
        {
            var locData = GetLocImage(gameTexture.name, gameTexture.GetInstanceID().ToString());
            if (locData != null)
            {
                var tex = locData.GetTex(gameTexture);
                if (locData.ReplaceMode == "复制")
                {
                    t.gameObject.SetActive(false);
                    var go = GameObject.Instantiate(t.gameObject, t);
                    Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    var image = go.GetComponent<Image>();
                    if (image != null)
                    {
                        image.sprite = sprite;
                        t.GetComponent<Image>().enabled = false;
                    }
                    var sr = go.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = sprite;
                        t.GetComponent<SpriteRenderer>().enabled = false;
                    }
                    go.SetActive(true);
                    t.gameObject.SetActive(true);
                }
                else
                {
                    Graphics.CopyTexture(tex, gameTexture);
                    Log($"图片翻译, 替换了[{t.GetPath()}]的图片, 图片名:[{locData.Name}], InstanceID:[{locData.InstanceID}], 替换图片:[{locData.Path}]");
                }
            }
        }

        public static void ChangeTMPSprite(TMP_SubMeshUI tmpSubMesh)
        {
            if (EnableChinese)
            {
                if (tmpSubMesh.mainTexture == null) return;
                ChangeSpriteTexture(tmpSubMesh.transform, tmpSubMesh.mainTexture as Texture2D);
            }
        }

        public static void ChangeImageSprite(Image image)
        {
            if (EnableChinese)
            {
                if (image.sprite == null || image.sprite.texture == null) return;
                ChangeSpriteTexture(image.transform, image.sprite.texture);
            }
        }

        public static void ChangeSpriteRendererSprite(SpriteRenderer spriteRenderer)
        {
            if (EnableChinese)
            {
                if (spriteRenderer.sprite == null || spriteRenderer.sprite.texture == null) return;
                ChangeSpriteTexture(spriteRenderer.transform, spriteRenderer.sprite.texture);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Image), "OnEnable")]
        public static void Image_OnEnable_Postfix(Image __instance)
        {
            if (__instance.sprite != null && __instance.sprite.name == "WARNING_PHOTOSENSITIVITY")
            {
                Credits.ShowWarning(__instance);
            }
            else
            {
                ChangeImageSprite(__instance);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Image), "sprite", MethodType.Setter)]
        public static void Image_sprite_Postfix(Image __instance)
        {
            ChangeImageSprite(__instance);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SpriteRenderer), "sprite", MethodType.Setter)]
        public static void SpriteRenderer_sprite_Postfix(SpriteRenderer __instance)
        {
            ChangeSpriteRendererSprite(__instance);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TMP_SubMeshUI), "AddSubTextObject")]
        public static void TMP_SubMeshUI_AddSubTextObject_Postfix(TMP_SubMeshUI __instance, TMP_SubMeshUI __result)
        {
            ChangeTMPSprite(__result);
        }

        #endregion 图片翻译
    }
}