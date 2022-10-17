using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Forte.Styleguide
{
    public class StyleguideController : Controller
    {
        private readonly ComponentCatalogLoader loader;

        public StyleguideController(ComponentCatalogLoader loader)
        {
            this.loader = loader;
        }

        public ActionResult Index()
        {
            var catalog = this.loader.Load(reload: true);
            var tags = catalog.Components.SelectMany(c => c.Tags).Distinct().OrderBy(t => t);
            var model = new StyleguideIndexViewModel(catalog.Components, tags);

            return View(model);
        }

        public async Task<ActionResult> Component(string name)
        {
            if (string.IsNullOrEmpty(name))            
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Name of the component is not defined");            

            var component = GetComponentByName(name);
            if (component == null)
                return HttpNotFound();

            return await component.Execute(ControllerContext);
        }

        [HttpGet]
        public ActionResult ComponentContext(string name)
        {
            if (string.IsNullOrEmpty(name))            
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Name of the component is not defined");
            
            var component = GetComponentByName(name);
            if (component == null)
                return HttpNotFound();

            using (var reader = component.File.OpenText())
            {
                var model = new MvcPartialComponentContextViewModel
                {
                    Name = component.Name,
                    Context = reader.ReadToEnd()
                };
                
                return new PartialViewResult()
                {
                    // ReSharper disable once Mvc.ViewNotResolved
                    View = ViewEngines.Engines.FindView(this.ControllerContext, "MvcPartialComponentContext", null).View,
                    ViewName = "MvcPartialComponentContext",
                    ViewData = new ViewDataDictionary(model),
                    ViewEngineCollection = ViewEngines.Engines
                };    
            }
        }

        private IStyleguideComponentDescriptor GetComponentByName(string componentName)
        {
            var catalog = this.loader.Load();
            return catalog.GetComponentByName(componentName);
        }
    }
}
