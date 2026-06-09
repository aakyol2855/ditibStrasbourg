using System.Collections.Concurrent;

namespace DitibStasbourg.Services.Implementations
{
    public class ImportProgressTracker
    {
        private readonly ConcurrentDictionary<string, int> _progress = new();

        public void SetProgress(string key, int percent)
        {
            _progress[key] = percent;
        }

        public int GetProgress(string key)
        {
            return _progress.TryGetValue(key, out var val) ? val : 0;
        }

        public void ClearProgress(string key)
        {
            _progress.TryRemove(key, out _);
        }
    }
}
