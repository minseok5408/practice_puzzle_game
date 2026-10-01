using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PuzzleGame.Runtime.Startup
{
    public sealed class PrototypeStartupLoader : StartupDataLoader
    {
        [SerializeField, Min(0)] private float durationSeconds = 5;
        public float DurationSeconds { get => durationSeconds; set => durationSeconds = Mathf.Max(0, value); }

        public override async Task LoadAsync(IProgress<float> progress, CancellationToken cancellationToken)
        {
            // This is temporary simulated work, not a minimum duration imposed by the UI.
            double duration = durationSeconds;
            var clock = Stopwatch.StartNew();
            cancellationToken.ThrowIfCancellationRequested();
            while (clock.Elapsed.TotalSeconds < duration)
            {
                progress?.Report((float)(clock.Elapsed.TotalSeconds / duration));
                await Task.Delay(16, cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(1);
        }
    }
}
