using Gridly;
using Gridly.Internal;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;
using System.Data;

namespace xiaoye97
{
    /// <summary>
    /// 用于Dump原文
    /// </summary>
    public class LGTSDumper
    {
        private OpenXmlConfiguration excelConfig = new OpenXmlConfiguration
        {
            DynamicColumns = new DynamicExcelColumn[] {
                    new DynamicExcelColumn("ID"){Width=30},
                    new DynamicExcelColumn("原文"){Width=70},
                    new DynamicExcelColumn("翻译文本"){Width=70},
                }
        };

        /// <summary>
        /// 将所有原文Dump到Excel
        /// </summary>
        public void DumpExcel()
        {
            DataSet sheets = new DataSet();

            foreach (Grid grid in Project.singleton.grids)
            {
                var sheet = DumpGrid(grid);
                sheets.Tables.Add(sheet.ToDataTable());
            }

            MiniExcel.SaveAs(LGTSChinesePlugin.ChineseExcelPath, sheets, excelType: ExcelType.XLSX, overwriteFile: true, configuration: excelConfig);
        }

        private SheetData DumpGrid(Grid grid)
        {
            SheetData sheet = new SheetData();
            sheet.SheetName = grid.nameGrid;
            foreach (var record in grid.records)
            {
                RowData row = new RowData();
                row.ID = record.recordID;
                foreach (var col in record.columns)
                {
                    if (col.columnID == "enUS")
                    {
                        if (!string.IsNullOrWhiteSpace(col.text))
                        {
                            row.原文 = col.text;
                            row.标记 = "新增文本";
                            sheet.RowDatas.Add(row);
                            break;
                        }
                    }
                }
            }
            sheet.RowDatas.Sort((a, b) =>
            {
                return a.ID.CompareTo(b.ID);
            });
            LGTSChinesePlugin.Log($"sheet:{sheet.SheetName} 内容行数:{sheet.RowDatas.Count}");
            return sheet;
        }
    }
}