using PuzzleGame.Runtime.Services;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    // All decoration stays above the result copy and never intercepts button input.
    public sealed class ResultPresentation : MonoBehaviour
    {
        private const int ParticleLimit=20;
        private readonly Image[] particles=new Image[ParticleLimit];
        private CanvasGroup group;
        private RectTransform card, field;
        private Image badge;
        private Sprite victory, retry, crown;
        private float began;
        private int particleCount;
        public bool IsAnimating { get; private set; }
        public int ActiveConfettiCount { get; private set; }
        public Sprite BadgeSprite=>badge ? badge.sprite : null;

        public void Configure(Image emblem,Sprite success,Sprite failure,Sprite completion,Sprite[] candies)
        {
            card=(RectTransform)transform;badge=emblem;victory=success;retry=failure;crown=completion;
            group=GetComponent<CanvasGroup>();if(!group)group=gameObject.AddComponent<CanvasGroup>();
            badge.color=Color.white;badge.type=Image.Type.Simple;badge.preserveAspect=true;badge.raycastTarget=false;
            foreach(var shadow in badge.GetComponents<Shadow>())shadow.enabled=false;
            var rect=badge.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,1);rect.pivot=new Vector2(.5f,1);
            rect.anchoredPosition=new Vector2(0,-20);rect.sizeDelta=new Vector2(160,126);
            if(field)return;
            field=(RectTransform)new GameObject("CandyConfetti",typeof(RectTransform),typeof(RectMask2D)).transform;
            field.SetParent(card,false);field.anchorMin=new Vector2(0,1);field.anchorMax=Vector2.one;field.pivot=new Vector2(.5f,1);
            field.offsetMin=new Vector2(16,-152);field.offsetMax=new Vector2(-16,-12);
            field.SetSiblingIndex(badge.transform.GetSiblingIndex());
            for(int i=0;i<particles.Length;i++)
            {
                var image=new GameObject("Candy",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image)).GetComponent<Image>();
                image.transform.SetParent(field,false);image.sprite=candies!=null && candies.Length>0?candies[i%candies.Length]:null;
                image.preserveAspect=true;image.raycastTarget=false;image.rectTransform.sizeDelta=Vector2.one*(14+i%3*3);
                image.gameObject.SetActive(false);particles[i]=image;
            }
        }

        public void Present(bool won,bool final)
        {
            badge.sprite=won?(final?crown:victory):retry;
            began=Time.unscaledTime;particleCount=won?(final?20:12):0;
            IsAnimating=true;
            if(GamePreferences.Current.reducedEffects)Settle();else Animate(0);
        }

        public void Settle()
        {
            IsAnimating=false;ActiveConfettiCount=0;
            if(group)group.alpha=1;
            if(card)card.localScale=Vector3.one;
            if(badge){badge.transform.localScale=Vector3.one;badge.transform.localRotation=Quaternion.identity;}
            foreach(var particle in particles)if(particle)particle.gameObject.SetActive(false);
        }

        private void OnDisable()=>Settle();
        private void Update()
        {
            if(!IsAnimating)return;
            if(GamePreferences.Current.reducedEffects){Settle();return;}
            float age=Time.unscaledTime-began;
            if(age>=1.15f){Settle();return;}
            Animate(age);
        }

        private void Animate(float age)
        {
            float enter=Mathf.Clamp01(age/.24f),ease=1-Mathf.Pow(1-enter,3);
            group.alpha=Mathf.Lerp(.3f,1,ease);card.localScale=Vector3.one*Mathf.Lerp(.96f,1,ease);
            float badgeT=Mathf.Clamp01(age/.42f);
            badge.transform.localScale=Vector3.one*(Mathf.Lerp(.84f,1,Mathf.SmoothStep(0,1,badgeT))+.045f*Mathf.Sin(badgeT*Mathf.PI));
            badge.transform.localRotation=Quaternion.Euler(0,0,-5*(1-badgeT));
            ActiveConfettiCount=0;
            for(int i=0;i<particles.Length;i++)
            {
                float t=(age-.04f*(i%4))/.9f;
                bool visible=i<particleCount && particles[i].sprite && t>0 && t<1;
                particles[i].gameObject.SetActive(visible);if(!visible)continue;
                ActiveConfettiCount++;
                float side=i%2==0?-1:1;
                float travel=(.25f+.07f*(i%5))*field.rect.width;
                particles[i].rectTransform.anchoredPosition=new Vector2(side*Mathf.Lerp(20,travel,t),5+75*t-110*t*t+(i%3-1)*14);
                particles[i].rectTransform.localRotation=Quaternion.Euler(0,0,side*(i*23+180*t));
                particles[i].color=new Color(1,1,1,Mathf.Min(1,t*8)*Mathf.Clamp01((1-t)*4));
            }
        }
    }
}
