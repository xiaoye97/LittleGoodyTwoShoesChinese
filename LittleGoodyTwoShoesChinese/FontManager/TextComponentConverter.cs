using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace xiaoye97
{
    public static class TextComponentConverter
    {
        /// <summary>
        /// 删除Text组件并创建代替的TMP组件
        /// </summary>
        /// <param name="textComponent"></param>
        public static TextMeshProUGUI ConvertTextToTMP(Text textComponent)
        {
            Transform transform = textComponent.transform;
            var text = textComponent.text;
            var fontSize = textComponent.fontSize;
            var alignment = textComponent.alignment;
            var color = textComponent.color;
            var raycastTarget = textComponent.raycastTarget;
            GameObject.DestroyImmediate(textComponent);
            var tmp = transform.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Baseline;
            tmp.horizontalAlignment = HAlignment(alignment);
            tmp.verticalAlignment = VAlignment(alignment);
            tmp.color = color;
            tmp.raycastTarget = raycastTarget;
            return tmp;
        }

        /// <summary>
        /// 从Text创建一个相同的TMP放到一起
        /// </summary>
        /// <param name="textComponent"></param>
        public static TextMeshProUGUI CreateTMPFormText(Text textComponent)
        {
            GameObject go = new GameObject($"{textComponent.name}_TMP");
            go.transform.SetParent(textComponent.transform.parent);
            var textrt = textComponent.transform as RectTransform;
            var tmprt = go.AddComponent<RectTransform>();
            tmprt.position = textrt.position;
            tmprt.rotation = textrt.rotation;
            tmprt.sizeDelta = textrt.sizeDelta;
            tmprt.anchoredPosition = textrt.anchoredPosition;
            tmprt.anchorMin = textrt.anchorMin;
            tmprt.anchorMax = textrt.anchorMax;

            var text = textComponent.text;
            var fontSize = textComponent.fontSize;
            var alignment = textComponent.alignment;
            var color = textComponent.color;
            var raycastTarget = textComponent.raycastTarget;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Baseline;
            tmp.horizontalAlignment = HAlignment(alignment);
            tmp.verticalAlignment = VAlignment(alignment);
            tmp.color = color;
            tmp.raycastTarget = raycastTarget;
            return tmp;
        }

        public static HorizontalAlignmentOptions HAlignment(TextAnchor textAnchor)
        {
            int i = (int)textAnchor;
            if (i == 0 || i == 3 || i == 6) return HorizontalAlignmentOptions.Left;
            if (i == 1 || i == 4 || i == 7) return HorizontalAlignmentOptions.Center;
            if (i == 2 || i == 5 || i == 8) return HorizontalAlignmentOptions.Right;
            return HorizontalAlignmentOptions.Center;
        }

        public static VerticalAlignmentOptions VAlignment(TextAnchor textAnchor)
        {
            int i = (int)textAnchor;
            if (i == 0 || i == 1 || i == 2) return VerticalAlignmentOptions.Top;
            if (i == 3 || i == 4 || i == 5) return VerticalAlignmentOptions.Middle;
            if (i == 6 || i == 7 || i == 8) return VerticalAlignmentOptions.Bottom;
            return VerticalAlignmentOptions.Middle;
        }
    }
}
