using MiniExcelLibs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xiaoye97
{
    public class LGTSUIExcelLoader
    {
        public Dictionary<string, UILocData> LoadExcel()
        {
            int count = 0;
            Dictionary<string, UILocData> result = new Dictionary<string, UILocData>();
            var dataTable = MiniExcel.QueryAsDataTable(LGTSChinesePlugin.UIExcelPath, true);
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
                    count++;
                }
            }
            LGTSChinesePlugin.Log($"加载了{count}行UI翻译");
            return result;
        }
    }
}