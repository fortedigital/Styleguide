using System.IO;
using System.Linq;
using System.Net;
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
            var model = new StyleguideIndexViewModel(catalog.Components);

            return View(model);
        }

        public ActionResult Component(string name)
        {
            if (string.IsNullOrEmpty(name))            
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Name of the component is not defined");            

            var catalog = this.loader.Load();
            var component = catalog.GetComponentByName(name);
            if (component == null)
                return HttpNotFound();

            return component.Execute(ControllerContext);
        }
    }
}