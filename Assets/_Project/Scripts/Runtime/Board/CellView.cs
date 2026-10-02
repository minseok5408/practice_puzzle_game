using PuzzleGame.Core.Board;
using UnityEngine;

namespace PuzzleGame.Runtime.Board
{
    public sealed class CellView : MonoBehaviour
    {
        [SerializeField] private Sprite[] frostStages;
        private SpriteRenderer iceBack, iceFront;
        private Vector3 iceScale;
        public GridPosition Position { get; private set; }
        public int FrostHealth { get; private set; }
        public Sprite FrostSprite=>iceFront?iceFront.sprite:null;
        public void ConfigureFrost(Sprite[] stages)=>frostStages=stages;

        public void ShowFrost(int health)
        {
            FrostHealth=health;
            if(health==0 && !iceBack)return;
            if(!iceBack)
            {
                iceBack=MakeLayer("IceBack",0);
                iceFront=MakeLayer("IceGlass",5);
            }
            iceBack.gameObject.SetActive(health>0);iceFront.gameObject.SetActive(health>0);
            if(health==0)return;
            var sprite=frostStages[Mathf.Clamp(health-1,0,frostStages.Length-1)];
            iceBack.sprite=iceFront.sprite=sprite;
            // Fit one rounded footprint to every cell instead of using the artwork's transparent padding.
            iceScale=new Vector3(.95f/sprite.bounds.size.x,.95f/sprite.bounds.size.y,1);
            iceBack.transform.localScale=iceFront.transform.localScale=iceScale;
            iceBack.color=Color.white;
            iceFront.color=new Color(1,1,1,FrontAlpha(health));
        }

        public void DamageProgress(int remainingHealth,float progress,bool reduced)
        {
            if(!iceBack)return;
            float t=Mathf.Clamp01(progress);
            float fade=remainingHealth==0?1-t:1;
            iceBack.color=new Color(1,1,1,fade);
            iceFront.color=new Color(1,1,1,FrontAlpha(FrostHealth)*fade);
            float scale=reduced?1:remainingHealth==0?1+.055f*t:1+.018f*Mathf.Sin(t*Mathf.PI);
            iceBack.transform.localScale=iceFront.transform.localScale=iceScale*scale;
        }

        private SpriteRenderer MakeLayer(string name,int order)
        {
            var layer=new GameObject(name).AddComponent<SpriteRenderer>();layer.transform.SetParent(transform,false);
            layer.sharedMaterial=GetComponent<SpriteRenderer>().sharedMaterial;layer.sortingOrder=order;return layer;
        }
        private static float FrontAlpha(int health)=>health>=3?.48f:health==2?.30f:.23f;
        public void Initialize(GridPosition position)
        {
            Position=position;
            ShowItemTarget(false);
            name=$"Cell_{position.X}_{position.Y}";
        }
        public void ShowItemTarget(bool selected)
        {
            GetComponent<SpriteRenderer>().color=selected?new Color32(255,219,145,255):
                (Position.X+Position.Y)%2==0?new Color32(165,140,196,255):new Color32(184,159,211,255);
        }
    }
}
