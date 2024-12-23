namespace AdminX.Models
{
    public class BreadcrumbItem
    {
        public string Text { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public Dictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>();

    }

}
