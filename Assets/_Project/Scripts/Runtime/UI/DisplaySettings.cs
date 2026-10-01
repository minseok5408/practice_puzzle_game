using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PuzzleGame.Runtime.UI
{
    [Serializable]
    public sealed class DisplaySettings
    {
        public int version = 1;
        public int width, height;
        public bool windowed;
        public bool IsValid => version == 1 && width >= 640 && height >= 480 && width <= 16384 && height <= 16384;

        public static string SavePath
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                string[] args = Environment.GetCommandLineArgs();
                int index = Array.IndexOf(args, "-puzzleSettingsPath");
                if (index >= 0 && index + 1 < args.Length) return args[index + 1];
#endif
                return Path.Combine(Application.persistentDataPath, "settings.json");
            }
        }

        public static bool RestoreSavedDisplay()
        {
            if (Application.isEditor) return false;
            var saved = Load(SavePath);
            if (saved == null) return false;
            int w = Mathf.Min(saved.width, Mathf.Max(960, Screen.currentResolution.width));
            int h = Mathf.Min(saved.height, Mathf.Max(600, Screen.currentResolution.height));
            var mode = saved.windowed ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow;
            if (Screen.width == w && Screen.height == h && Screen.fullScreenMode == mode) return false;
            Screen.SetResolution(w, h, mode);
            return true;
        }

        public static List<Vector2Int> Resolutions(Vector2Int desktop, Vector2Int current, Resolution[] supported)
        {
            var sizes = new HashSet<Vector2Int>();
            void Add(int w, int h)
            {
                if (w >= 640 && h >= 480 && w <= desktop.x && h <= desktop.y) sizes.Add(new Vector2Int(w,h));
            }
            foreach (var size in new[] { new Vector2Int(960,600), new Vector2Int(1024,768), new Vector2Int(1280,720),
                new Vector2Int(1280,800), new Vector2Int(1366,768), new Vector2Int(1600,900), new Vector2Int(1920,1080),
                new Vector2Int(1920,1200), new Vector2Int(2560,1440), new Vector2Int(2560,1600), new Vector2Int(3840,2160) })
                Add(size.x,size.y);
            foreach (var size in supported) Add(size.width,size.height);
            Add(desktop.x, desktop.y);
            Add(current.x,current.y);
            if (sizes.Count == 0) sizes.Add(new Vector2Int(960,600));
            var result = new List<Vector2Int>(sizes);
            result.Sort((a,b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
            return result;
        }

        public static DisplaySettings Load(string path)
        {
            foreach (string candidate in new[] { path, path + ".bak" })
            {
                try
                {
                    if (!File.Exists(candidate)) continue;
                    var settings = JsonUtility.FromJson<DisplaySettings>(File.ReadAllText(candidate));
                    if (settings != null && settings.IsValid) return settings;
                }
                catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException) { }
            }
            return null;
        }

        public bool Save(string path)
        {
            if (!IsValid) return false;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
                File.WriteAllText(path + ".tmp", JsonUtility.ToJson(this, true));
                if (File.Exists(path)) File.Replace(path + ".tmp", path, path + ".bak");
                else File.Move(path + ".tmp", path);
                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            { Debug.LogWarning("Could not save display settings: " + e.Message); return false; }
        }
    }
}
