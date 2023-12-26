using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xiaoye97
{
    public class SheetData
    {
        public string SheetName;
        public List<RowData> RowDatas = new List<RowData>();

        public DataTable ToDataTable()
        {
            DataTable table = new DataTable();
            table.TableName = SheetName;
            table.Columns.Add(new DataColumn("ID"));
            table.Columns.Add(new DataColumn("原文"));
            table.Columns.Add(new DataColumn("翻译文本"));
            table.Columns.Add(new DataColumn("标记"));
            foreach (RowData row in RowDatas)
            {
                table.Rows.Add(row.ID, row.原文, "", row.标记);
            }
            return table;
        }
    }
}
