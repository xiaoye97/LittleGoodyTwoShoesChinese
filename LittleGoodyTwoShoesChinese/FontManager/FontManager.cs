using System;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using HarmonyLib;

namespace xiaoye97
{
    /// <summary>
    /// 字体管理器
    /// </summary>
    public class FontManager
    {
        public List<FontData> Fonts;
        public Dictionary<string, FontData> FontDict;

        /// <summary>
        /// 是否搜索系统字体
        /// </summary>
        public bool SearchSystemFont;

        /// <summary>
        /// 自定义的搜索文件夹
        /// </summary>
        public string CustomFontDirPath;

        public bool PatchTextFont;
        public bool PatchTextMeshProFont;

        /// <summary>
        /// 进行字体替换时使用的主要字体
        /// </summary>
        public static FontData MainFont;

        public FontManager()
        {
        }

        public FontManager(bool searchSystemFont, string customFontDirPath, bool patchTextFont, bool patchTextMeshProFont)
        {
            PatchTextFont = patchTextFont;
            PatchTextMeshProFont = patchTextMeshProFont;
            SearchSystemFont = searchSystemFont;
            CustomFontDirPath = customFontDirPath;
        }

        public void Init()
        {
            Debug.Log("[UnityFontLoader]FontManager开始初始化");
            Fonts = new List<FontData>();
            FontDict = new Dictionary<string, FontData>();
            if (SearchSystemFont)
            {
                LoadSystemFonts();
            }
            if (Directory.Exists(CustomFontDirPath))
            {
                LoadFonts(CustomFontDirPath);
            }
            if (PatchTextFont)
            {
                Harmony.CreateAndPatchAll(typeof(TextFontPatchs));
            }
            if (PatchTextMeshProFont)
            {
                Harmony.CreateAndPatchAll(typeof(TMPFontPatchs));
            }
            Debug.Log("[UnityFontLoader]FontManager初始化完毕");
        }

        /// <summary>
        /// 设置主字体
        /// </summary>
        /// <param name="mainFontName">主字体,如果不知道用什么,就填msyh.ttc</param>
        /// <param name="fallbackFontNames">后备字体,如果不知道用什么,就填arial.ttf</param>
        public void SetMainFont(string mainFontName, string[] fallbackFontNames)
        {
            if (FontDict.ContainsKey(mainFontName))
            {
                MainFont = FontDict[mainFontName];
                Debug.Log($"[UnityFontLoader]将{mainFontName}作为主字体");
            }
            else
            {
                Debug.Log($"[UnityFontLoader]找不到后主字体 {mainFontName}，忽略");
            }
            if (fallbackFontNames.Length > 0)
            {
                foreach (string fontName in fallbackFontNames)
                {
                    if (FontDict.ContainsKey(fontName))
                    {
                        if (MainFont == null)
                        {
                            MainFont = FontDict[fontName];
                            Debug.Log($"[UnityFontLoader]将后备字体{fontName}作为主字体");
                        }
                        else
                        {
                            if (MainFont.TMPFont.fallbackFontAssetTable == null)
                            {
                                MainFont.TMPFont.fallbackFontAssetTable = new List<TMP_FontAsset>();
                            }
                            MainFont.TMPFont.fallbackFontAssetTable.Add(FontDict[fontName].TMPFont);
                            Debug.Log($"[UnityFontLoader]将后备字体{fontName}添加到后备字体列表");
                        }
                    }
                    else
                    {
                        Debug.Log($"[UnityFontLoader]找不到后备字体 {fontName}，忽略");
                    }
                }
            }
            if (MainFont == null)
            {
                if (Fonts.Count > 0)
                {
                    MainFont = Fonts[0];
                    Debug.Log($"[UnityFontLoader]由于既没有主字体，也没有后备字体，所以将扫描到的第一个字体[{Fonts[0].FontFileName}]作为主字体，其他字体作为后备字体");
                    for (int i = 1; i < Fonts.Count; i++)
                    {
                        if (MainFont.TMPFont.fallbackFontAssetTable == null)
                        {
                            MainFont.TMPFont.fallbackFontAssetTable = new List<TMP_FontAsset>();
                        }
                        MainFont.TMPFont.fallbackFontAssetTable.Add(Fonts[i].TMPFont);
                        Debug.Log($"[UnityFontLoader]将字体{Fonts[i].TMPFont}添加到后备字体列表");
                    }
                }
            }
        }

        public void LoadFonts(string dirPath)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            // 查找文件夹下的所有字体
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            int count = 0;
            if (dir.Exists)
            {
                List<string> paths = new List<string>();
                paths.AddRange(dir.GetFiles("*.ttf").Select(f => f.FullName));
                paths.AddRange(dir.GetFiles("*.otf").Select(f => f.FullName));

                foreach (var path in paths)
                {
                    try
                    {
                        FontData font = new FontData(path);
                        Fonts.Add(font);
                        FontDict[font.FontFileName] = font;
                        Debug.Log($"[UnityFontLoader]加载了字体:{font.FontFileName}，路径:{path}");
                        count++;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[UnityFontLoader]加载字体文件 {path} 时出现异常，忽略此文件:\n{e}");
                    }
                }
            }
            else
            {
                Debug.Log($"[UnityFontLoader]目标字体文件夹不存在 {dir}");
            }
            sw.Stop();
            Debug.Log($"从[{dir}]加载了{count}个字体, 耗时{sw.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// 加载系统字体
        /// </summary>
        public void LoadSystemFonts()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var paths = Font.GetPathsToOSFonts();
            int count = 0;
            foreach (var path in paths)
            {
                try
                {
                    FontData font = new FontData(path);
                    Fonts.Add(font);
                    FontDict[font.FontFileName] = font;
                    Debug.Log($"[UnityFontLoader]加载了字体:{font.FontFileName}，路径:{path}");
                    count++;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UnityFontLoader]加载字体文件 {path} 时出现异常，忽略此文件:\n{e}");
                }
            }

            sw.Stop();
            Debug.Log($"[UnityFontLoader]从系统目录加载了{count}个字体, 耗时{sw.ElapsedMilliseconds}ms");
        }
    }
}