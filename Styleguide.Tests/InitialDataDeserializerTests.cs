using System.Collections.Generic;
using Forte.Styleguide;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Styleguide.Tests
{
    [TestFixture]
    public class InitialDataDeserializerTests
    {
        private InitialDataDeserializer _initialDataDeserializer;
        
        [SetUp]
        public void SetUp()
        {
            _initialDataDeserializer = new InitialDataDeserializer(new JsonSerializerSettings());
        }

        [Test]
        public void Deserialize_ShouldDeserializeDataWithoutTags()
        {
            // Given
            var json = "{\"model\": {}, \"variants\":[{\"name\" : \"Normal\", \"model\": {}}]}";

            // When
            var initialData = _initialDataDeserializer.Deserialize(json);

            // Then
            Assert.NotNull(initialData);
            Assert.NotNull(initialData.Tags);
            Assert.IsEmpty(initialData.Tags);
        }
        
        [TestCaseSource(typeof(TagsTestCase), nameof(TagsTestCase.GetTestCases))]
        public void Deserialize_ShouldDeserializeDataWithTags(TagsTestCase testCase)
        {
            // When
            var initialData = _initialDataDeserializer.Deserialize(testCase.Json);

            // Then
            Assert.NotNull(initialData);
            Assert.AreEqual(testCase.ExpectedValue, initialData.Tags);
        }
        
        [Test]
        public void Deserialize_ShouldDeserializeDataWithoutDisplayName()
        {
            // Given
            var json = "{\"model\": {}, \"variants\":[{\"name\" : \"Normal\", \"model\": {}}]}";

            // When
            var initialData = _initialDataDeserializer.Deserialize(json);

            // Then
            Assert.NotNull(initialData);
            Assert.AreEqual(string.Empty, initialData.DisplayName);
        }
        
        [Test]
        public void Deserialize_ShouldDeserializeDataWithDisplayName()
        {
            // Given
            var json = "{\"displayName\": \"test\", \"model\": {}, \"variants\":[{\"name\" : \"Normal\", \"model\": {}}]}";

            // When
            var initialData = _initialDataDeserializer.Deserialize(json);

            // Then
            Assert.NotNull(initialData);
            Assert.AreEqual("test", initialData.DisplayName);
        }

        public class TagsTestCase
        {
            public string Json { get; }
            public List<string> ExpectedValue { get; }

            public TagsTestCase(string json,List<string> expectedValue)
            {
                Json = json;
                ExpectedValue = expectedValue;
            }

            public static IEnumerable<TagsTestCase> GetTestCases()
            {
                yield return new TagsTestCase(
                    "{\"tags\": [], \"model\": {}, \"variants\":[{\"name\" : \"Normal\", \"model\": {}}]}",
                    new List<string>());

                yield return new TagsTestCase(
                    "{\"tags\": [\"Tag 1\", \"Tag 2\"], \"model\": {}, \"variants\":[{\"name\" : \"Normal\", \"model\": {}}]}",
                    new List<string>() { "Tag 1", "Tag 2" });
            }
        }
    }
}
