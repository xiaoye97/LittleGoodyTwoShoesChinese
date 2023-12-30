using Gridly;
using MiniExcelLibs;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace xiaoye97
{
    /// <summary>
    /// 用于加载Excel
    /// </summary>
    public class LGTSExcelLoader
    {
        public static List<string> LGTSSheetNames;

        private void RefreshLGTSSheetNams()
        {
            LGTSSheetNames = new List<string>();
            foreach (var grid in Project.singleton.grids)
            {
                LGTSSheetNames.Add(grid.nameGrid);
            }
        }

        public Dictionary<string, SheetData> LoadMainExcel()
        {
            LGTSChinesePlugin.Log($"开始加载文本翻译");
            if (LGTSSheetNames == null)
            {
                RefreshLGTSSheetNams();
            }
            int count = 0;
            Dictionary<string, SheetData> Sheets = new Dictionary<string, SheetData>();
            var sheetNames = MiniExcel.GetSheetNames(LGTSChinesePlugin.ChineseExcelPath);
            foreach (var sheetName in sheetNames)
            {
                // 如果不是游戏中有的表, 则跳过
                if (!LGTSSheetNames.Contains(sheetName)) continue;
                var dataTable = MiniExcel.QueryAsDataTable(LGTSChinesePlugin.ChineseExcelPath, sheetName: sheetName);
                SheetData sheet = new SheetData();
                sheet.SheetName = sheetName;
                foreach (DataRow row in dataTable.Rows)
                {
                    RowData rowData = new RowData();
                    rowData.ID = row["ID"].ToString();
                    rowData.原文 = row["原文"].ToString();
                    rowData.翻译文本 = row["翻译文本"].ToString();
                    rowData.标记 = row["标记"].ToString();
                    sheet.RowDatas.Add(rowData);
                    count++;
                }
                Sheets[sheetName] = sheet;
            }
            LGTSChinesePlugin.Log($"加载了{count}行翻译");
            return Sheets;
        }

        /// <summary>
        /// 加载UI翻译表
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, UILocData> LoadUIExcel()
        {
            LGTSChinesePlugin.Log($"开始加载UI翻译");
            int count = 0;
            Dictionary<string, UILocData> result = new Dictionary<string, UILocData>();
            var dataTable = MiniExcel.QueryAsDataTable(LGTSChinesePlugin.ChineseExcelPath, sheetName: "UI翻译表");
            foreach (DataRow row in dataTable.Rows)
            {
                UILocData locData = new UILocData();
                locData.UI路径 = row[0].ToString().Replace("\n", "");
                if (!string.IsNullOrWhiteSpace(locData.UI路径))
                {
                    locData.原文备注 = row[1].ToString().Replace("\n", "");
                    locData.直接翻译 = row[2].ToString();
                    locData.表名 = row[3].ToString().Replace("\n", "");
                    locData.翻译ID = row[4].ToString().Replace("\n", "");
                    result[locData.UI路径] = locData;
                    //LGTSChinesePlugin.Log($"UI路径:[{locData.UI路径}] 原文备注:[{locData.原文备注}] 直接翻译:[{locData.直接翻译}] 表名:[{locData.表名}] 翻译ID:[{locData.翻译ID}]");
                    count++;
                }
            }
            LGTSChinesePlugin.Log($"加载了{count}行UI翻译");
            return result;
        }

        /// <summary>
        /// 加载UI补充翻译表
        /// </summary>
        public Dictionary<string, string> LoadUIExcelEx()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            LGTSChinesePlugin.Log($"开始加载UI补充翻译");
            var dataTable = MiniExcel.QueryAsDataTable(LGTSChinesePlugin.ChineseExcelPath, sheetName: "UI补充翻译表");
            foreach (DataRow row in dataTable.Rows)
            {
                string ID = row[0].ToString().Replace("\n", "");
                string 翻译文本 = row[1].ToString();
                result[ID] = 翻译文本;
            }
            LGTSChinesePlugin.Log($"加载了{result.Count}行UI补充翻译");
            return result;
        }

        /// <summary>
        /// 加载UI补充翻译表2
        /// </summary>
        public Dictionary<string, string> LoadUIExcelEx2()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            LGTSChinesePlugin.Log($"开始加载UI补充翻译2");
            var dataTable = MiniExcel.QueryAsDataTable(LGTSChinesePlugin.ChineseExcelPath, sheetName: "UI补充翻译表2");
            foreach (DataRow row in dataTable.Rows)
            {
                string 原文 = row[0].ToString();
                string 翻译文本 = row[1].ToString();
                result[原文] = 翻译文本;
            }
            LGTSChinesePlugin.Log($"加载了{result.Count}行UI补充翻译2");
            return result;
        }

        /// <summary>
        /// 加载人名表
        /// </summary>
        public Dictionary<string, string> LoadNameExcel()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            LGTSChinesePlugin.Log($"开始加载人名翻译");
            var dataTable = MiniExcel.QueryAsDataTable(LGTSChinesePlugin.ChineseExcelPath, sheetName: "人名表");
            foreach (DataRow row in dataTable.Rows)
            {
                string name = row[0].ToString().Replace("\n", "");
                string 翻译名字 = row[2].ToString();
                result[name] = 翻译名字;
            }
            LGTSChinesePlugin.Log($"加载了{result.Count}行人名翻译");
            return result;
        }
    }
}