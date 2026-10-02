using PuzzleGame.Runtime.Services;
using UnityEngine;

namespace PuzzleGame.Runtime.Board
{
    // The hint draws above two cells without moving their candies or changing selection.
    public sealed class BoardHintView : MonoBehaviour
    {
        private Material material;
        private LineRenderer[] lines;
        private float age;
        private void Awake()
        {
            material=new Material(Shader.Find("Sprites/Default"));lines=new LineRenderer[6];
            for(int i=0;i<lines.Length;i++)
            {
                var line=new GameObject("HintLine"+i).AddComponent<LineRenderer>();line.transform.SetParent(transform,false);
                line.sharedMaterial=material;line.useWorldSpace=false;line.sortingOrder=12+i%2;
                line.widthMultiplier=i%2==0?.09f:.032f;line.numCornerVertices=3;line.numCapVertices=3;lines[i]=line;
            }
        }
        public void Show(Vector3 first,Vector3 second)
        {
            gameObject.SetActive(true);age=0;
            Vector3[] outline={new Vector3(-.32f,-.46f),new Vector3(.32f,-.46f),new Vector3(.46f,-.32f),new Vector3(.46f,.32f),new Vector3(.32f,.46f),new Vector3(-.32f,.46f),new Vector3(-.46f,.32f),new Vector3(-.46f,-.32f)};
            for(int cell=0;cell<2;cell++)for(int style=0;style<2;style++)
            {
                var line=lines[cell*2+style];line.loop=true;line.positionCount=outline.Length;
                for(int i=0;i<outline.Length;i++)line.SetPosition(i,outline[i]+(cell==0?first:second));
            }
            Vector3 direction=(second-first).normalized,side=new Vector3(-direction.y,direction.x),center=(first+second)*.5f;
            // Two opposed arrow heads explain the swap, including vertical hints.
            var arrows=new[]{center-direction*.21f+side*.1f,center-direction*.32f,center-direction*.21f-side*.1f,center-direction*.32f,center+direction*.32f,center+direction*.21f+side*.1f,center+direction*.32f,center+direction*.21f-side*.1f};
            for(int i=4;i<6;i++){lines[i].loop=false;lines[i].positionCount=arrows.Length;lines[i].SetPositions(arrows);}
            Sample();
        }
        private void Update(){age+=Time.deltaTime;Sample();}
        private void Sample()
        {
            float alpha=GamePreferences.Current.reducedEffects?1:.88f+.12f*Mathf.Sin(age*3.5f);
            for(int i=0;i<lines.Length;i++)lines[i].startColor=lines[i].endColor=i%2==0?new Color(1,.66f,.08f,alpha):new Color(1,1,.91f,alpha);
        }
        public void Hide()=>gameObject.SetActive(false);
        private void OnDestroy(){if(material)Destroy(material);}
    }
}
