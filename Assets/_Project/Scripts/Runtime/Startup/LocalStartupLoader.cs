using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using UnityEngine;

namespace PuzzleGame.Runtime.Startup
{
    public sealed class LocalStartupLoader : StartupDataLoader
    {
        [SerializeField] private LevelCatalog catalog;
        public void Configure(LevelCatalog levels)=>catalog=levels;
        public override async Task LoadAsync(IProgress<float> progress,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(!catalog)throw new InvalidOperationException("The campaign catalog is missing.");
            GamePreferences.Load();progress?.Report(.15f);await Task.Yield();
            token.ThrowIfCancellationRequested();
            CampaignState.Ensure(catalog);progress?.Report(.3f);await Task.Yield();
            var ids=new HashSet<string>();
            for(int number=1;number<=LevelCatalog.LevelCount;number++)
            {
                token.ThrowIfCancellationRequested();
                var level=catalog.Get(number);
                if(!level || level.Number!=number || !ids.Add(level.LevelId))throw new InvalidOperationException("Invalid campaign level "+number);
                level.CreateBoard();progress?.Report(.3f+.5f*number/LevelCatalog.LevelCount);
                if(number%5==0)await Task.Yield();
            }
            token.ThrowIfCancellationRequested();GameAudio.Ensure();progress?.Report(.95f);await Task.Yield();
            token.ThrowIfCancellationRequested();progress?.Report(1);
        }
    }
}
