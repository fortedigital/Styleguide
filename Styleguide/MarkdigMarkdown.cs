using Markdig;

namespace Forte.Styleguide
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
            return Markdown.ToHtml(content);
        }
    }
}