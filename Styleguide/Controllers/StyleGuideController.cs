using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Styleguide.Views.Styleguide;

namespace Styleguide.Controllers
{
    public class StyleguideController : Controller
    {
        private readonly ComponentCatalogLoader _loader;

        public StyleguideController(ComponentCatalogLoader loader)
        {
            _loader = loader;
        }
        [HttpGet("styleguide")]
        public ActionResult Index()
        {
            
            var catalog = _loader.Load(reload: true);
            var model = new StyleguideIndexViewModel(catalog.Components);

            
            return View(model);
        }

        public async Task<ActionResult> Component(string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest("Name of the component is not defined");

            var component = GetComponentByName(name);
            if (component == null)
                return NotFound();

            return await component.Execute(ControllerContext);
        }

        [HttpGet]
        public ActionResult ComponentContext(string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest("Name of the component is not defined");

            var component = GetComponentByName(name);
            if (component == null)
                return NotFound();

            using var reader = component.File.OpenText();
            var model = new MvcPartialComponentContextViewModel
            {
                Name = component.Name,
                Context = reader.ReadToEnd()
            };
                
            return new PartialViewResult()
            {
                ViewName = "MvcPartialComponentContext",
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { { "model", model } }
            };
        }

        private IStyleguideComponentDescriptor GetComponentByName(string componentName)
        {
            var catalog = _loader.Load();
            return catalog.GetComponentByName(componentName);
        }
    }
}
