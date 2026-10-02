using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    public static class ProgressWidgets
    {
        public static string Stars(int count) => "<color=#CD891B>" + new string('★', Mathf.Clamp(count, 0, 3))
            + "</color><color=#CDBDD1>" + new string('★', 3 - Mathf.Clamp(count, 0, 3)) + "</color>";

        public static RectTransform Rect(string name, Transform parent)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }

        public static TMP_Text Text(string name, Transform parent, TMP_FontAsset font, string value, float size = 20)
        {
            var text = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font; text.text = value; text.fontSize = text.fontSizeMax = size; text.fontSizeMin = Mathf.Min(16, size);
            text.enableAutoSizing = true; text.color = CandyUIStyle.Ink; text.raycastTarget = false;
            text.alignment = TextAlignmentOptions.Center; return text;
        }

        public static void Height(GameObject item, float height)
        {
            var layout = item.GetComponent<LayoutElement>(); if (!layout) layout = item.AddComponent<LayoutElement>();
            layout.minHeight = layout.preferredHeight = height;
        }

        public static void Rewards(Transform parent, TMP_FontAsset font, Sprite[] sprites, int[] amounts)
        {
            int count = 0; foreach (int amount in amounts) if (amount > 0) count++;
            int index = 0;
            int columns = Mathf.Max(3,count);
            for (int i = 0; i < amounts.Length; i++)
            {
                if (amounts[i] <= 0) continue;
                var slot = Rect(((ItemType)i).ToString(), parent);
                float left=(1f-(float)count/columns)*.5f;
                slot.anchorMin = new Vector2(left+(float)index / columns, 0); slot.anchorMax = new Vector2(left+(float)(index + 1) / columns, 1); index++;
                var icon = Rect("Icon", slot).gameObject.AddComponent<Image>();
                icon.sprite = sprites != null && i < sprites.Length ? sprites[i] : null; icon.preserveAspect = true; icon.raycastTarget = false;
                icon.rectTransform.anchorMin = new Vector2(.05f, .32f); icon.rectTransform.anchorMax = new Vector2(.72f, 1);
                var quantity = Text("Quantity", slot, font, "+" + amounts[i], 22);
                quantity.rectTransform.anchorMin = new Vector2(.70f, .32f); quantity.rectTransform.anchorMax = Vector2.one;
                var label = Text("Name", slot, font, Localization.Get(ItemToolbar.NameKey((ItemType)i) + "Short"), 16);
                label.rectTransform.anchorMax = new Vector2(1, .32f);
            }
        }
    }
}
