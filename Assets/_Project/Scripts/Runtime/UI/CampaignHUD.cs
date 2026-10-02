using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    public sealed class CampaignHUD : MonoBehaviour
    {
        [SerializeField] private LevelSession session;
        [SerializeField] private SpriteRenderer backdrop;
        [SerializeField] private TMP_Text worldTitle;
        [SerializeField] private Button mapButton;
        [SerializeField] private GameObject[] collectionSlots;
        [SerializeField] private Image[] candyIcons;
        [SerializeField] private TMP_Text[] counts;
        [SerializeField] private Sprite[] candySprites;
        private int displayedWorld;
        private GoalPanel goalPanel;
        [SerializeField] private Sprite goalCheck, goalIce;
        [SerializeField] private Sprite[] itemSprites;
        public void ConfigureItems(Sprite[] sprites)=>itemSprites=sprites;
        public void ConfigureGoalCheck(Sprite check)=>goalCheck=check;
        public void ConfigureGoalIce(Sprite ice)=>goalIce=ice;

        public void Configure(LevelSession level, SpriteRenderer background, TMP_Text worldName, Button map,
            GameObject[] slots, Image[] icons, TMP_Text[] values, Sprite[] candies)
        { session = level; backdrop = background; worldTitle = worldName; mapButton = map; collectionSlots = slots; candyIcons = icons; counts = values; candySprites = candies; }

        private void OnEnable() { session.Changed += Refresh; GamePreferences.Changed += Refresh; mapButton.onClick.AddListener(OpenMap); }
        private void OnDisable() { if (session) session.Changed -= Refresh; GamePreferences.Changed -= Refresh; if (mapButton) mapButton.onClick.RemoveListener(OpenMap); }
        private void Update() { mapButton.interactable = session.Progress != null && !session.Progress.IsResolving; }

        public void OpenMap()
        {
            if (session.Progress == null || session.Progress.IsResolving) return;
            var dialogs=GetComponent<PlayerDialogs>();
            if(dialogs)dialogs.ConfirmAbandon(false,()=>SceneManager.LoadSceneAsync("WorldMap", LoadSceneMode.Single));
            else SceneManager.LoadSceneAsync("WorldMap", LoadSceneMode.Single);
        }

        private void Refresh()
        {
            if (!session.Catalog || session.Progress == null) return;
            int world = session.Definition.World;
            if (displayedWorld != world)
            {
                displayedWorld = world; backdrop.sprite = session.Catalog.Background(world);
                backdrop.sharedMaterial = session.Catalog.BackgroundMaterial(world);
            }
            worldTitle.text = Localization.World(world);
            foreach(var slot in collectionSlots)slot.SetActive(false);
            if(!goalPanel)
            {
                var root=new GameObject("GoalPanel",typeof(RectTransform));root.transform.SetParent(worldTitle.transform.parent,false);
                goalPanel=root.AddComponent<GoalPanel>();goalPanel.Configure(worldTitle.font,mapButton.GetComponent<Image>().sprite,candySprites,goalCheck,goalIce);
            }
            goalPanel.Refresh(session.Progress);
            if(itemSprites!=null && itemSprites.Length==4 && !GetComponent<ItemToolbar>())
                gameObject.AddComponent<ItemToolbar>().Configure(session,FindFirstObjectByType<PuzzleGame.Runtime.Board.BoardController>(),worldTitle.font,mapButton.GetComponent<Image>().sprite,itemSprites);
            if(!GetComponent<HUDTools>())gameObject.AddComponent<HUDTools>().Configure(session,worldTitle.font,mapButton.GetComponent<Image>().sprite);
            GetComponent<CandyLayout>()?.ApplyLayout();
        }
    }
}
