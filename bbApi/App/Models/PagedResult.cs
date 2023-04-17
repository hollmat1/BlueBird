 namespace bbApi.App.Models
{
    public class PagedResult<T>
    {
        public string SkipToken { get; set; }
        public int Top { get; set; }
        public IEnumerable<T> CurrentPage { get; set; }
    }
}
