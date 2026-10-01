using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PuzzleGame.Runtime.Startup
{
    // Replace the prototype component with a login/profile loader when that service exists.
    // Complete only when data is usable; report 0..1, honor cancellation, and throw on failure.
    public abstract class StartupDataLoader : MonoBehaviour
    {
        public abstract Task LoadAsync(IProgress<float> progress, CancellationToken cancellationToken);
    }
}
