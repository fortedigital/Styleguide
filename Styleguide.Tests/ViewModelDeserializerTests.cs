using System.Linq;
using Forte.Styleguide;
using NUnit.Framework;

namespace Styleguide.Tests
{
    [TestFixture]
    public class ViewModelDeserializerTests
    {
        [Test]
        public void GivenJson_Deserialize_ReturnsValidViewModel()
        {
            //given
            var content = @"{
              ""model"" : {
                ""name""  : ""John"",
                ""label"" : ""CEO""
              },
              ""variants"":[
                {
                  ""name"" : ""Vertical layout""
                }
              ]
            }";
            

            //when
            var viewModel = ViewModelDeserializer.Deserialize(typeof(DummyViewModel), content, "Test");
            
            //then
            Assert.AreEqual("Test", viewModel.Name);
            Assert.AreEqual(1, viewModel.Variants.Count());

            var variant = viewModel.Variants.First();
            Assert.AreEqual(typeof(DummyViewModel), variant.Model.GetType());
            Assert.AreEqual("Vertical layout", variant.Name);

            var model = (DummyViewModel) variant.Model;
            Assert.AreEqual("John", model.Name);
            Assert.AreEqual("CEO", model.Label);
            
        }

        [Test]
        public void GivenJsonWithVariants_Deserialize_ReturnsValidViewModel()
        {
            //given
            var content = @"{
              ""model"" : {
                ""name""  : ""John"",
                ""label"" : ""CEO""
              },
              ""variants"":[
                {
                  ""name"" : ""Vertical layout""
                },
                {
                  ""name"" : ""Horizontal layout"",
                  ""model"" : {
                    ""label"": ""CTO""
                  }
                }
              ]
            }";
          
            //when
            var viewModel = ViewModelDeserializer.Deserialize(typeof(DummyViewModel), content, "Test");
            Assert.AreEqual(2, viewModel.Variants.Count());
           
            var verticalVariant = viewModel.Variants.First();
            Assert.AreEqual(typeof(DummyViewModel), verticalVariant.Model.GetType());
            Assert.AreEqual("Vertical layout", verticalVariant.Name);
  
            var verticalModel = (DummyViewModel) verticalVariant.Model;
            Assert.AreEqual("John", verticalModel.Name);
            Assert.AreEqual("CEO", verticalModel.Label);

            var horizontalVariant = viewModel.Variants.Last();
            Assert.AreEqual(typeof(DummyViewModel), horizontalVariant.Model.GetType());
            Assert.AreEqual("Horizontal layout", horizontalVariant.Name);
    
            var horizontalModel = (DummyViewModel) horizontalVariant.Model;
            Assert.AreEqual("John", horizontalModel.Name);
            Assert.AreEqual("CTO", horizontalModel.Label);

        }
      
        private class DummyViewModel
        {
          public string Name { get; set; }
          public string Label { get; set; }
        }
    }
}
