using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    public enum CandyButtonRole { Primary, Secondary, Quiet, Danger }

    // Shared presentation rules for scene UI and dialogs created at runtime.
    public static class CandyUIStyle
    {
        public const float Padding = 24, Gap = 12, ButtonHeight = 48, CloseSize = 40;
        public const float HeaderHeight = 88, TitleSize = 30, BodySize = 22;
        public static readonly Color Ink = new Color32(93, 48, 104, 255);
        public static readonly Color Cream = new Color32(255, 248, 235, 255);
        public static readonly Color Pink = new Color32(218, 58, 140, 255);
        public static readonly Color Purple = new Color32(133, 79, 160, 255);
        public static readonly Color Quiet = new Color32(236, 220, 240, 255);
        public static readonly Color Danger = new Color32(173, 65, 91, 255);

        public static void Button(Button button, CandyButtonRole role, Sprite rounded = null,
            float fontSize = BodySize, bool fitLabel = true)
        {
            if (!button) return;
            var image = button.GetComponent<Image>();
            if (rounded) image.sprite = rounded;
            image.type = Image.Type.Sliced;
            image.color = role == CandyButtonRole.Primary ? Pink : role == CandyButtonRole.Danger ? Danger :
                role == CandyButtonRole.Quiet ? Quiet : Purple;
            image.raycastTarget = true;
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1, .95f, 1);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(.83f, .76f, .86f);
            colors.disabledColor = new Color(.82f, .79f, .84f, .65f);
            colors.colorMultiplier = 1; colors.fadeDuration = .1f; button.colors = colors;
            var label = button.GetComponentInChildren<TMP_Text>(true);
            if (label)
            {
                label.color = role == CandyButtonRole.Quiet ? Ink : Color.white;
                label.fontSize = label.fontSizeMax = fontSize; label.fontSizeMin = Mathf.Min(16, fontSize);
                label.enableAutoSizing = true; label.alignment = TextAlignmentOptions.Center;
                label.raycastTarget = false;
                if (fitLabel)
                {
                    label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
                    label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
                    label.margin = new Vector4(14, 4, 14, 4);
                    label.textWrappingMode = TextWrappingModes.NoWrap;
                }
            }
            Shadow(image, 3);
            if (!button.GetComponent<CandyButtonMotion>()) button.gameObject.AddComponent<CandyButtonMotion>();
        }

        public static void Popup(GameObject overlay, RectTransform card, Sprite rounded)
        {
            Dim(overlay, false);
            var image = card.GetComponent<Image>();
            image.sprite = rounded; image.type = Image.Type.Sliced; image.color = Cream;
            Shadow(image, 6);
        }

        public static void Dim(GameObject overlay, bool nested)
        { overlay.GetComponent<Image>().color = new Color(.22f, .10f, .28f, nested ? .20f : .62f); }

        private static void Shadow(Graphic graphic, float depth)
        {
            // Outline derives from Shadow; keep the keyboard focus outline separate.
            Shadow shadow = null;
            foreach (var candidate in graphic.GetComponents<Shadow>())
                if (!(candidate is Outline)) { shadow = candidate; break; }
            if (!shadow) shadow = graphic.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(.27f, .10f, .30f, .22f);
            shadow.effectDistance = new Vector2(0, -depth); shadow.useGraphicAlpha = true;
        }

        public static void ActionRow(Button button, int column, float bottom = Padding)
        {
            if (!button) return;
            var rect = (RectTransform)button.transform;
            rect.anchorMin = new Vector2(column == 1 ? .5f : 0, 0);
            rect.anchorMax = new Vector2(column == 0 ? .5f : 1, 0);
            rect.offsetMin = new Vector2(column == 1 ? Gap / 2 : Padding, bottom);
            rect.offsetMax = new Vector2(column == 0 ? -Gap / 2 : -Padding, bottom + ButtonHeight);
        }

        public static void FixedHeight(RectTransform rect, float height = ButtonHeight)
        {
            float center = (rect.anchorMin.y + rect.anchorMax.y) * .5f;
            rect.anchorMin = new Vector2(rect.anchorMin.x, center);
            rect.anchorMax = new Vector2(rect.anchorMax.x, center);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public static void ContentWidth(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0, rect.anchorMin.y); rect.anchorMax = new Vector2(1, rect.anchorMax.y);
            rect.offsetMin = new Vector2(Padding, rect.offsetMin.y); rect.offsetMax = new Vector2(-Padding, rect.offsetMax.y);
        }

        public static void Header(TMP_Text title)
        {
            title.fontSize = title.fontSizeMax = TitleSize; title.fontSizeMin = 20;
            title.enableAutoSizing = true; title.color = Ink;
            title.alignment = TextAlignmentOptions.MidlineLeft;
            title.rectTransform.anchorMin = new Vector2(0, 1); title.rectTransform.anchorMax = Vector2.one;
            title.rectTransform.offsetMin = new Vector2(Padding, -Padding - CloseSize);
            title.rectTransform.offsetMax = new Vector2(-Padding - CloseSize - Gap, -Padding);
        }

        public static Button CloseButton(Transform parent, Sprite rounded, UnityEngine.Events.UnityAction close)
        {
            var rect = new GameObject("CloseDialog", typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            var button = rect.GetComponent<Button>(); button.onClick.AddListener(close);
            CloseButton(button, rounded);
            return button;
        }

        public static void CloseButton(Button button, Sprite rounded)
        {
            Button(button, CandyButtonRole.Quiet, rounded);
            var text = button.GetComponentInChildren<TMP_Text>(true); if (text) text.gameObject.SetActive(false);
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
            rect.sizeDelta = Vector2.one * CloseSize; rect.anchoredPosition = new Vector2(-Padding, -Padding);
            if (rect.Find("CloseStroke0")) return;
            for (int i = 0; i < 2; i++)
            {
                var stroke = new GameObject("CloseStroke" + i, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                stroke.SetParent(rect, false); stroke.anchorMin = stroke.anchorMax = new Vector2(.5f, .5f);
                stroke.sizeDelta = new Vector2(17, 2.5f); stroke.localRotation = Quaternion.Euler(0, 0, i == 0 ? 45 : -45);
                var image = stroke.GetComponent<Image>(); image.color = Ink; image.raycastTarget = false;
            }
        }
    }
}
