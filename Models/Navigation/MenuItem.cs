namespace DitibStasbourg.Models.Navigation
{
    public class MenuItem
    {
        public string Title { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? RequiredClaim { get; set; }
        public List<MenuItem> Children { get; set; } = new();

        // Helper to check if this item or any of its children match the current route
        public bool IsActive(string currentController, string currentAction)
        {
            if (Controller == currentController && Action == currentAction) return true;
            if (Controller == currentController && string.IsNullOrEmpty(Action)) return true; // Parent matches controller
            return Children.Any(c => c.IsActive(currentController, currentAction));
        }
    }
}
