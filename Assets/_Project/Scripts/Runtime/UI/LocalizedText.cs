using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEngine;

namespace PuzzleGame.Runtime.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string key;
        public void Bind(string value) { key = value; Refresh(); }
        private void OnEnable() { GamePreferences.Changed += Refresh; Refresh(); }
        private void OnDisable() => GamePreferences.Changed -= Refresh;
        private void Refresh() { if (!string.IsNullOrEmpty(key)) GetComponent<TMP_Text>().text = Localization.Get(key); }
    }
}
