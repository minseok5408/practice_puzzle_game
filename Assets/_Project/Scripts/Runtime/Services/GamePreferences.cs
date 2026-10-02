using System;
using PuzzleGame.Runtime.UI;
using UnityEngine;

namespace PuzzleGame.Runtime.Services
{
    public static class GamePreferences
    {
        private static DisplaySettings current;
        public static DisplaySettings Current => current ?? (current = Defaults());
        public static event Action Changed;
        public static bool SaveFailed { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { current = null; Changed = null; SaveFailed = false; }
        private static DisplaySettings Defaults() => new DisplaySettings {
            width = Mathf.Max(640, Screen.width), height = Mathf.Max(480, Screen.height),
            windowed = Screen.fullScreenMode == FullScreenMode.Windowed
        };
        public static void Load()
        {
            current = Application.isEditor ? Defaults() : DisplaySettings.Load(DisplaySettings.SavePath) ?? Defaults();
            Changed?.Invoke();
        }
        public static bool Save()
        {
            SaveFailed = !Application.isEditor && !Current.Save(DisplaySettings.SavePath);
            Changed?.Invoke();
            return !SaveFailed;
        }
    }
}
