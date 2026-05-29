namespace DitibStasbourg.Models.ViewModels
{
    public class ImportResultViewModel
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public bool IsSuccess => FailureCount == 0;
    }
}
