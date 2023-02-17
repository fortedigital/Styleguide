using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Forte.Styleguide.Views.Styleguide;
using Microsoft.AspNetCore.Authorization;

namespace Forte.Styleguide.Controllers
{
    [Authorize]
    [Route("styleguide")]
    public class StyleguideController : Controller
    {
        private readonly ComponentCatalogLoader _loader;

        public StyleguideController(ComponentCatalogLoader loader)
        {
            _loader = loader;
        }
        public ActionResult Index()
        {
            var catalog = _loader.Load(reload: true);
            var tags = catalog.Components.SelectMany(c => c.Tags).Distinct().OrderBy(t => t);
            var model = new StyleguideIndexViewModel(catalog.Components, tags);
            
            return View(model);
        }
        [HttpGet("Component/{name}")]
        public async Task<ActionResult> Component([FromRoute]string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest("Name of the component is not defined");

            var component = GetComponentByName(name);
            if (component == null)
                return NotFound();

            return await component.Execute(ControllerContext);
        }
        [HttpGet("ComponentContext/{name}")]
        public ActionResult ComponentContext([FromRoute]string name)
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
