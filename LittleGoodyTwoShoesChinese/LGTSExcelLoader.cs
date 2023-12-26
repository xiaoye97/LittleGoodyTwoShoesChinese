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

        public Dictionary<string, SheetData> LoadExcel()
        {
            if (LGTSSheetNames == null)
            {
                RefreshLGTSSheetNams();
            }
            int count = 0;
            Dictionary<string, SheetData> Sheets = new Dictionary<string, SheetData>();
            var sheetNames = MiniExcel.GetSheetNames(LGTSChinesePlugin.ExcelPath);
            foreach (var sheetName in sheetNames)
            {
                // 如果不是游戏中有的表, 则跳过
                if (!LGTSSheetNames.Contains(sheetName)) continue;
                var dataTable = MiniExcel.QueryAsDataTable(LGTSChinesePlugin.ExcelPath, sheetName: sheetName);
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
    }
}