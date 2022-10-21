namespace Forte.Styleguide
{
    public interface IMarkdown
    {
        bool UseMarkdown { get; }
        string ToHtml(string content);
    }
}