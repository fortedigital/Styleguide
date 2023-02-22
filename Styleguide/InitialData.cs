namespace Forte.Styleguide
{
    public class InitialData
    {
        public string DisplayName { get; } = string.Empty;
        public IEnumerable<string> Tags { get; }

        public InitialData()
        {
            Tags = new List<string>();
        }

        public InitialData(string displayName, IEnumerable<string> tags)
        {
            Tags = tags;
            DisplayName = displayName;
        }
    }
}
