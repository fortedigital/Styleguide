namespace Forte.Styleguide.Markdown
{
    public interface IMarkdown
    {
        bool UseMarkdown { get; }
        string ToHtml(string content);
    }
}
