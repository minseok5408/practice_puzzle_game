using System;
using System.IO;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Editor
{
    public static class SettingsSetup
    {
        private const string Root = "Assets/_Project";
        private static readonly Color Ink = new Color32(103,57,107,255);
        private static readonly Color Cream = new Color32(255,249,238,255);
        private static readonly Color Pink = new Color32(226,77,151,255);
        private static readonly Color Purple = new Color32(140,95,170,255);
        private static TMP_FontAsset font;
        private static Sprite rounded;

        [MenuItem("Puzzle Game/Add Display Settings")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(BuildCommands.GameScene);
            Install(UnityEngine.Object.FindFirstObjectByType<HUDView>().gameObject);
            PlayerSettings.resizableWindow = true;
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("SETTINGS READY: button, ESC, resolution, windowed toggle, pause/resume and quit.");
        }

        public static void Install(GameObject canvas)
        {
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "/UI/Fonts/CandyBody.asset");
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/UI/Theme/Rounded.png");
            if (!font || !rounded) throw new InvalidOperationException("Apply the candy theme before adding settings.");
            const string characters = "설정 화면 해상도 창모드 체크를 해제하면 전체 화면으로 전환됩니다 크기를 선택하세요 적용 계속하기 게임 종료"
                + "화면 설정을 적용했어요 화면은 변경됐지만 설정을 저장하지 못했어요 ESC x0123456789.";
            if (!font.TryAddCharacters(characters,out string missing)) throw new InvalidOperationException("Missing settings glyphs: " + missing);
            foreach (var texture in font.atlasTextures)
                if (!AssetDatabase.Contains(texture)) AssetDatabase.AddObjectToAsset(texture,font);
            EditorUtility.SetDirty(font);
            var previous = canvas.GetComponent<SettingsPopup>();
            if (previous) UnityEngine.Object.DestroyImmediate(previous);
            var sidebar = canvas.transform.Find("ScoreCard");
            foreach (var old in new[] { canvas.transform.Find("SettingsOverlay"), sidebar.Find("SettingsButton") })
                if (old) UnityEngine.Object.DestroyImmediate(old.gameObject);
            Button open = Button("SettingsButton",sidebar,.635f,.025f,.925f,.095f,"설정",Purple);
            canvas.GetComponent<CandyLayout>().ConfigureSettings((RectTransform)open.transform);

            RectTransform overlay = Rect("SettingsOverlay",canvas.transform,0,0,1,1);
            overlay.gameObject.AddComponent<Image>().color = new Color(.24f,.10f,.32f,.76f);
            RectTransform card = Panel("SettingsCard",overlay,.5f,.5f,.5f,.5f,Cream);
            card.sizeDelta = new Vector2(560,530);
            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(.22f,.06f,.3f,.3f); shadow.effectDistance = new Vector2(0,-8);
            Text("Title",card,.08f,.82f,.70f,.95f,"설정",42,Ink,TextAlignmentOptions.Left);
            Text("Shortcut",card,.75f,.85f,.92f,.92f,"ESC",19,Purple,TextAlignmentOptions.Right);
            Panel("Rule",card,.08f,.79f,.92f,.794f,new Color32(233,207,226,255));
            Text("DisplayLabel",card,.08f,.71f,.92f,.77f,"화면",19,Purple,TextAlignmentOptions.Left);
            Text("ResolutionLabel",card,.08f,.59f,.37f,.70f,"해상도",25,Ink,TextAlignmentOptions.Left);

            Sprite checkmark = Glyph("SettingsCheck",false), arrow = Glyph("SettingsArrow",true);
            var dropdownObject = TMP_DefaultControls.CreateDropdown(new TMP_DefaultControls.Resources {
                standard=rounded, background=rounded, inputField=rounded, knob=rounded, checkmark=checkmark, dropdown=arrow, mask=rounded
            });
            dropdownObject.name="Resolution"; dropdownObject.transform.SetParent(card,false);
            Place((RectTransform)dropdownObject.transform,.40f,.60f,.92f,.70f);
            var dropdown=dropdownObject.GetComponent<TMP_Dropdown>();
            dropdown.GetComponent<Image>().color=new Color32(238,222,239,255);
            dropdown.template.GetComponent<Image>().color=Cream;
            dropdown.template.sizeDelta=new Vector2(0,220);
            var content=(RectTransform)dropdown.template.Find("Viewport/Content");
            content.sizeDelta=new Vector2(0,48);
            var item=(RectTransform)content.Find("Item"); item.sizeDelta=new Vector2(0,44);
            item.Find("Item Background").GetComponent<Image>().color=new Color32(244,231,243,255);
            item.Find("Item Checkmark").GetComponent<Image>().color=Pink;
            dropdownObject.transform.Find("Arrow").GetComponent<Image>().color=Purple;
            foreach(var text in dropdownObject.GetComponentsInChildren<TMP_Text>(true))
            {
                text.font=font; text.color=Ink; text.fontSize=23;
                text.enableAutoSizing=true; text.fontSizeMin=14; text.fontSizeMax=23; text.raycastTarget=false;
            }
            dropdown.ClearOptions(); dropdown.AddOptions(new System.Collections.Generic.List<string> {"1920 x 1080"});
            SetColors(dropdown);

            RectTransform toggleRoot=Rect("Windowed",card,.08f,.46f,.92f,.56f);
            toggleRoot.gameObject.AddComponent<Image>().color=Color.clear;
            RectTransform square=Panel("Box",toggleRoot,0,.12f,0,.88f,new Color32(211,181,211,255));
            square.pivot=new Vector2(0,.5f);
            square.sizeDelta=new Vector2(40,0);
            square.GetComponent<Image>().sprite=null;
            Panel("Inset",square,.09f,.09f,.91f,.91f,Color.white).GetComponent<Image>().sprite=null;
            RectTransform tick=Panel("Checkmark",square,.13f,.13f,.87f,.87f,Purple);
            var checkImage=tick.GetComponent<Image>(); checkImage.sprite=checkmark; checkImage.type=Image.Type.Simple;
            Text("Label",toggleRoot,.13f,0,1,1,"창모드",25,Ink,TextAlignmentOptions.Left);
            Toggle toggle=toggleRoot.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic=square.GetComponent<Image>(); toggle.graphic=checkImage; toggle.isOn=false; SetColors(toggle);
            Text("WindowHint",card,.08f,.38f,.92f,.445f,"체크를 해제하면 전체 화면으로 전환됩니다.",17,Purple,TextAlignmentOptions.Left);
            TMP_Text status=Text("Status",card,.08f,.30f,.92f,.355f,"화면에 맞는 크기를 선택하세요.",17,Ink,TextAlignmentOptions.Center);
            Button apply=Button("Apply",card,.08f,.175f,.92f,.275f,"적용",Pink);
            Button close=Button("Resume",card,.08f,.045f,.49f,.135f,"계속하기",Purple);
            Button quit=Button("Quit",card,.52f,.045f,.92f,.135f,"게임 종료",new Color32(187,106,140,255));
            canvas.AddComponent<SettingsPopup>().Configure(UnityEngine.Object.FindFirstObjectByType<BoardController>(),overlay.gameObject,
                card,open,close,apply,quit,dropdown,toggle,status);
            overlay.gameObject.SetActive(false);
            canvas.GetComponent<CandyLayout>().ApplyLayout();
        }

        private static Sprite Glyph(string name,bool arrow)
        {
            string path=Root+"/UI/Theme/"+name+".png";
            if(!File.Exists(path))
            {
                var texture=new Texture2D(32,32,TextureFormat.RGBA32,false);
                Vector2 a=arrow?new Vector2(6,21):new Vector2(5,16);
                Vector2 b=arrow?new Vector2(16,11):new Vector2(13,8);
                Vector2 c=arrow?new Vector2(26,21):new Vector2(27,25);
                for(int y=0;y<32;y++) for(int x=0;x<32;x++)
                {
                    Vector2 p=new Vector2(x+.5f,y+.5f);
                    float distance=Mathf.Min(Distance(p,a,b),Distance(p,b,c));
                    texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(2.4f-distance)));
                }
                texture.Apply(); File.WriteAllBytes(path,texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
            }
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite; importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false; importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static float Distance(Vector2 p,Vector2 a,Vector2 b) => Vector2.Distance(p,a+(b-a)*Mathf.Clamp01(Vector2.Dot(p-a,b-a)/(b-a).sqrMagnitude));
        private static RectTransform Rect(string name,Transform parent,float x0,float y0,float x1,float y1)
        {
            var result=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;
            result.SetParent(parent,false); Place(result,x0,y0,x1,y1); return result;
        }
        private static void Place(RectTransform rect,float x0,float y0,float x1,float y1)
        { rect.anchorMin=new Vector2(x0,y0); rect.anchorMax=new Vector2(x1,y1); rect.offsetMin=rect.offsetMax=Vector2.zero; }
        private static RectTransform Panel(string name,Transform parent,float x0,float y0,float x1,float y1,Color color)
        {
            RectTransform rect=Rect(name,parent,x0,y0,x1,y1); var image=rect.gameObject.AddComponent<Image>();
            image.sprite=rounded; image.type=Image.Type.Sliced; image.color=color; image.raycastTarget=false; return rect;
        }
        private static TMP_Text Text(string name,Transform parent,float x0,float y0,float x1,float y1,string value,float size,Color color,TextAlignmentOptions alignment)
        {
            var text=Rect(name,parent,x0,y0,x1,y1).gameObject.AddComponent<TextMeshProUGUI>();
            text.font=font; text.text=value; text.fontSize=size; text.color=color; text.alignment=alignment;
            text.enableAutoSizing=true; text.fontSizeMin=12; text.fontSizeMax=size; text.raycastTarget=false;
            text.textWrappingMode=TextWrappingModes.NoWrap; return text;
        }
        private static Button Button(string name,Transform parent,float x0,float y0,float x1,float y1,string label,Color color)
        {
            var rect=Panel(name,parent,x0,y0,x1,y1,color); var image=rect.GetComponent<Image>(); image.raycastTarget=true;
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; SetColors(button);
            Text("Label",rect,.04f,.08f,.96f,.94f,label,23,Color.white,TextAlignmentOptions.Center);
            rect.gameObject.AddComponent<CandyButtonMotion>(); return button;
        }
        private static void SetColors(Selectable selectable)
        {
            var colors=selectable.colors; colors.highlightedColor=new Color(1,.92f,.98f); colors.selectedColor=colors.highlightedColor;
            colors.pressedColor=new Color(.85f,.70f,.87f); colors.disabledColor=new Color(.85f,.73f,.84f,.5f); selectable.colors=colors;
        }
    }
}
