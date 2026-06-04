using System.Collections.Generic;

namespace DitibStasbourg.Models.Navigation
{
    public class SidebarViewModel
    {
        public List<MenuItem> MainMenu { get; set; } = new();
        public List<MenuItem> AdminMenu { get; set; } = new();
    }
}
