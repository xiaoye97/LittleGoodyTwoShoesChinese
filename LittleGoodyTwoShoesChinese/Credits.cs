using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace xiaoye97
{
    public static class Credits
    {
        public static void ShowWarning(Image image)
        {
            GameObject go = new GameObject("CNWarning");
            go.transform.SetParent(image.transform);
            
            var rt = image.transform as RectTransform;
            var warnRT = go.AddComponent<RectTransform>();
            warnRT.localScale = Vector3.one;
            warnRT.localPosition = Vector3.zero;
            var warnImage = go.AddComponent<Image>();
            var tex = ResourceUtils.GetTex("WARNING.png");
            var warnSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            warnImage.sprite = warnSprite;
            warnRT.anchorMin = rt.anchorMin;
            warnRT.anchorMax = rt.anchorMax;
            warnRT.sizeDelta = rt.sizeDelta;
            GameObject.Destroy(image);
        }
    }
}
