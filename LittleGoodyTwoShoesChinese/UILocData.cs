using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xiaoye97
{
    public class UILocData
    {
        public string UI路径;
        public string 原文备注;
        public string 直接翻译;
        public string 表名;
        public string 翻译ID;

        public UILocData()
        {

        }

        public UILocData(string ui路径, string 原文备注, string 直接翻译, string 表名, string 翻译ID)
        {
            UI路径 = ui路径;
            this.原文备注 = 原文备注;
            this.直接翻译 = 直接翻译;
            this.表名 = 表名;
            this.翻译ID = 翻译ID;
        }
    }
}
