using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PuzzleGame.Runtime.UI;
using UnityEngine;

namespace PuzzleGame.Tests
{
    public class DisplaySettingsTests
    {
        [Test]
        public void ResolutionChoicesAreUniqueFitTheMonitorAndIncludeCurrentWindow()
        {
            var result=DisplaySettings.Resolutions(new Vector2Int(1920,1080),new Vector2Int(1110,740),
                new[] {new Resolution {width=1280,height=720},new Resolution {width=1280,height=720},new Resolution {width=3840,height=2160}});
            Assert.That(result.Distinct().Count(), Is.EqualTo(result.Count));
            Assert.That(result, Does.Contain(new Vector2Int(1110,740)));
            Assert.That(result, Does.Contain(new Vector2Int(1920,1080)));
            Assert.That(result.All(r=>r.x<=1920 && r.y<=1080), Is.True);
        }

        [Test]
        public void SettingsRoundTripAndCorruptFileFallsBackToPreviousSave()
        {
            string directory=Path.Combine(Path.GetTempPath(),"puzzle-settings-test-"+Guid.NewGuid().ToString("N"));
            string path=Path.Combine(directory,"settings.json");
            try
            {
                Assert.That(DisplaySettings.Load(path), Is.Null);
                Assert.That(new DisplaySettings {width=1280,height=720,windowed=true}.Save(path), Is.True);
                Assert.That(new DisplaySettings {width=1920,height=1080,windowed=false}.Save(path), Is.True);
                var current=DisplaySettings.Load(path);
                Assert.That(current.width, Is.EqualTo(1920)); Assert.That(current.windowed, Is.False);
                File.WriteAllText(path,"{broken");
                var recovered=DisplaySettings.Load(path);
                Assert.That(recovered.width, Is.EqualTo(1280)); Assert.That(recovered.windowed, Is.True);
                Assert.That(new DisplaySettings {width=-1,height=0}.Save(path), Is.False);
                Assert.That(File.Exists(path+".tmp"), Is.False);
            }
            finally { if(Directory.Exists(directory)) Directory.Delete(directory,true); }
        }
    }
}
