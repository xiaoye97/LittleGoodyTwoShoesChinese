using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace xiaoye97
{
    public static class Ex
    {
        public static string GetPath(this Transform transform)
        {
            if (transform == null) return "";
            if (transform.parent == null) return transform.name;
            return GetPath(transform.parent) + "/" + transform.name;
        }
    }
}