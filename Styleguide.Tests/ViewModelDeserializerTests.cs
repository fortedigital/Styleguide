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
              ""layoutPath"" : ""~/Views/Shared/_Layout.cshtml"",
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
            Assert.AreEqual("~/Views/Shared/_Layout.cshtml", viewModel.LayoutPath);
            Assert.AreEqual("Test", viewModel.PartialName);

            var variant = viewModel.Variants.First();
            Assert.AreEqual(typeof(DummyViewModel), variant.Model.GetType());
            Assert.AreEqual("Vertical layout", variant.Name);

            var model = (DummyViewModel)variant.Model;
            Assert.AreEqual("John", model.Name);
            Assert.AreEqual("CEO", model.Label);

        }

        [Test]
        public void GivenJsonWithPartialName_Deserialize_ReturnsValidViewModel()
        {
            //given
            var content = @"{
                  ""model"" : {
                    ""name""  : ""John"",
                    ""label"" : ""CEO""
                  },
                  ""partialName"" : ""Avatar"",
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
            Assert.AreEqual("Avatar", viewModel.PartialName);
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

            //then
            Assert.AreEqual(2, viewModel.Variants.Count());

            var verticalVariant = viewModel.Variants.First();
            Assert.AreEqual(typeof(DummyViewModel), verticalVariant.Model.GetType());
            Assert.AreEqual("Vertical layout", verticalVariant.Name);

            var verticalModel = (DummyViewModel)verticalVariant.Model;
            Assert.AreEqual("John", verticalModel.Name);
            Assert.AreEqual("CEO", verticalModel.Label);

            var horizontalVariant = viewModel.Variants.Last();
            Assert.AreEqual(typeof(DummyViewModel), horizontalVariant.Model.GetType());
            Assert.AreEqual("Horizontal layout", horizontalVariant.Name);

            var horizontalModel = (DummyViewModel)horizontalVariant.Model;
            Assert.AreEqual("John", horizontalModel.Name);
            Assert.AreEqual("CTO", horizontalModel.Label);

        }

        [Test]
        public void GivenJsonWithStringModel_Deserialize_ReturnsValidViewModel()
        {
            //given
            var content = @"{
                  ""model"" : ""default string"",
                  ""variants"":[
                    {
                      ""name"" : ""Vertical layout""
                    },
                    {
                      ""name"" : ""Horizontal layout"",
                      ""model"" : ""overriden string""
                    }
                  ]
                }";

            //when
            var viewModel = ViewModelDeserializer.Deserialize(typeof(string), content, "Test");

            //then
            Assert.AreEqual(2, viewModel.Variants.Count());

            var verticalVariant = viewModel.Variants.First();
            Assert.AreEqual(typeof(string), verticalVariant.Model.GetType());
            Assert.AreEqual("Vertical layout", verticalVariant.Name);

            var verticalModel = (string)verticalVariant.Model;
            Assert.AreEqual("default string", verticalModel);

            var horizontalVariant = viewModel.Variants.Last();
            Assert.AreEqual(typeof(string), horizontalVariant.Model.GetType());
            Assert.AreEqual("Horizontal layout", horizontalVariant.Name);

            var horizontalModel = (string)horizontalVariant.Model;
            Assert.AreEqual("overriden string", horizontalModel);
        }

        [Test]
        public void GivenJsonWithIntModel_Deserialize_ReturnsValidViewModel()
        {
            //given
            var content = @"{
                      ""model"" : 1,
                      ""variants"":[
                        {
                          ""name"" : ""Vertical layout""
                        },
                        {
                          ""name"" : ""Horizontal layout"",
                          ""model"" : 2
                        }
                      ]
                    }";

            //when
            var viewModel = ViewModelDeserializer.Deserialize(typeof(int), content, "Test");

            //then
            Assert.AreEqual(2, viewModel.Variants.Count());

            var verticalVariant = viewModel.Variants.First();
            Assert.AreEqual(typeof(int), verticalVariant.Model.GetType());
            Assert.AreEqual("Vertical layout", verticalVariant.Name);

            var verticalModel = (int)verticalVariant.Model;
            Assert.AreEqual(1, verticalModel);

            var horizontalVariant = viewModel.Variants.Last();
            Assert.AreEqual(typeof(int), horizontalVariant.Model.GetType());
            Assert.AreEqual("Horizontal layout", horizontalVariant.Name);

            var horizontalModel = (int)horizontalVariant.Model;
            Assert.AreEqual(2, horizontalModel);
        }

        [Test]
        public void GivenJsonWithNestedObjectsViewModel_Deserialize_ReturnsValidViewModel()
        {
            //given
            var content = @"{
              ""model"" : {
                  ""count""  : 1,
                  ""DummyViewModel"" : {
                    ""name"": ""John"",
                    ""label"": ""CTO""
                  }
                },
              
                ""variants"":[
                {
                  ""name"" : ""Vertical layout"",
                },
                {
                  ""name"" : ""Horizontal layout"",
                  ""model"" : {
                    ""count"" : 7,
                    ""DummyViewModel"" : {
                      ""label"" : ""Dev""
                    }
                  }
                }
                ]
            }";

            //when
            var viewModel = ViewModelDeserializer.Deserialize(typeof(DummyNestedViewModel), content, "Test");

            //then
            Assert.AreEqual(2, viewModel.Variants.Count());

            var verticalVariant = viewModel.Variants.First();
            Assert.AreEqual(typeof(DummyNestedViewModel), verticalVariant.Model.GetType());
            Assert.AreEqual("Vertical layout", verticalVariant.Name);

            var verticalModel = (DummyNestedViewModel)verticalVariant.Model;
            Assert.AreEqual(1, verticalModel.Count);
            Assert.AreEqual("John", verticalModel.DummyViewModel.Name);

            var horizontalVariant = viewModel.Variants.Last();
            Assert.AreEqual(typeof(DummyNestedViewModel), horizontalVariant.Model.GetType());
            Assert.AreEqual("Horizontal layout", horizontalVariant.Name);

            var horizontalModel = (DummyNestedViewModel)horizontalVariant.Model;
            Assert.AreEqual(7, horizontalModel.Count);
            Assert.AreEqual("John", horizontalModel.DummyViewModel.Name);
            Assert.AreEqual("Dev", horizontalModel.DummyViewModel.Label);
        }

        private class DummyViewModel
        {
            public string Name { get; set; }
            public string Label { get; set; }
        }

        private class DummyNestedViewModel
        {
            public int Count { get; set; }
            public DummyViewModel DummyViewModel { get; set; }
        }

        public class EmptyViewModel
        {

        }


        [Test]
        public void GivenEmptyJson_Deserialize_ReturnsValidViewModel()
        {
            //given
            var content = @"{   
                variants:[{
                    name:""test""
                }]
            }";

            //when
            var viewModel = ViewModelDeserializer.Deserialize(typeof(EmptyViewModel), content, "Test");

            //then
            Assert.That(viewModel.Model, Is.Not.Null);
            Assert.That(viewModel.Model, Is.InstanceOf<EmptyViewModel>());
        }
      
      [Test]
      public void GivenJson_Deserialize_ReturnsVariantViewData()
      {
        //given
        var content = @"{
              ""model"" : {
                ""name""  : ""John"",
                ""label"" : ""CEO""
              },
              ""variants"":[
                {
                  ""viewData"" : {
                    ""variant"": ""test""
                  }
                }
              ]
            }";


        //when
        var viewModel = ViewModelDeserializer.Deserialize(typeof(DummyViewModel), content, "Test");

        //then
        var variant = viewModel.Variants.First();
        Assert.AreEqual("test", variant.ViewData["variant"]);
      }
     
      [Test]
      public void GivenUnnamedVariants_Deserialize_ReturnsVariantNames()
      {
        //given
        var content = @"{
              ""model"" : {
                ""name""  : ""John"",
                ""label"" : ""CEO""
              },
              ""variants"":[
                {}, {}
              ]
            }";


        //when
        var viewModel = ViewModelDeserializer.Deserialize(typeof(DummyViewModel), content, "Test");

        //then
        Assert.AreEqual("Variant 1", viewModel.Variants.First().Name);
        Assert.AreEqual("Variant 2", viewModel.Variants.Skip(1).First().Name);
      }
      
      [Test]
      public void GivenSingleUnnamedVariant_Deserialize_ReturnsVariantName()
      {
        //given
        var content = @"{
              ""model"" : {
                ""name""  : ""John"",
                ""label"" : ""CEO""
              },
              ""variants"":[
                {}
              ]
            }";


        //when
        var viewModel = ViewModelDeserializer.Deserialize(typeof(DummyViewModel), content, "Test");

        //then
        Assert.AreEqual("Normal", viewModel.Variants.First().Name);
      }                  
      
    }
}
