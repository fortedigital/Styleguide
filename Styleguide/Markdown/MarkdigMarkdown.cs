namespace Forte.Styleguide.Markdown
{
    public class MarkdigMarkdown : IMarkdown
    {
        public bool UseMarkdown { get; }

        public MarkdigMarkdown(bool useMarkdown)
        {
            UseMarkdown = useMarkdown;
        }

        public string ToHtml(string content)
        {
            return Markdig.Markdown.ToHtml(content);
        }
    }
}
