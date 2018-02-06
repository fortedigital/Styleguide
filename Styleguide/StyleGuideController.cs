using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Styleguide
{
    public class StyleGuideController : Controller
    {
        public ActionResult Index()
        {
            var partials = Directory.GetFiles(Server.MapPath("~"), "*.styleguide.json", SearchOption.AllDirectories).Select(Path.GetFileName);
            var knockout = Directory.GetFiles(Server.MapPath("~"), "*.styleguide.ts", SearchOption.AllDirectories).Select(Path.GetFileName);

            var model = new StyleGuideViewModel
            {
                PartialComponents = partials.Select(f => f.Substring(0, f.Length - ".styleguide.json".Length)),
                KnockoutComponents = knockout.Select(f => f.Substring(0, f.Length - ".styleguide.ts".Length))
            };

            return View("~/Views/StyleGuide/StyleGuide.cshtml", model);
        }

        public ActionResult Partial(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Name of the component is not defined");
            }

            var componentViewModelLoader = new ComponentViewModelLoader(
                new ComponentJsonDataLoader(ControllerContext),
                new RazorBasedComponentViewModelTypeResolver(ControllerContext));                
            
            var model = new PartialComponentViewModel
            {
                Name = name,
                Items = componentViewModelLoader.LoadManyFromJson(name)   
            };
            
            return View("~/Views/StyleGuide/PartialComponent.cshtml", model);
        }

        public ActionResult Knockout(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Name of the component is not defined");
            }

            var model = new KnockoutComponentViewModel
            {
                Name = name
            };

            return View("~/Views/StyleGuide/KnockoutComponent.cshtml", model);
        }
    }
}