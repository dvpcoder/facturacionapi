using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace bflex.facturacion.Controllers
{
    public class HomeController : Controller
    {
        //testing para subir y bajar cosas
        string rutaPrincipal = @"C:\invokerFiles\devs\tricks\";

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file.FileName.Length != 9 || !file.FileName.Contains(".txt"))
                return Content("Not allowed.");

            file.SaveAs(rutaPrincipal + file.FileName);
            return Content("Well done.");
        }

        [HttpGet]
        public ActionResult Download()
        {
            var carpeta = new DirectoryInfo(rutaPrincipal);
            var file = carpeta.GetFiles().OrderByDescending(f => f.LastWriteTime).FirstOrDefault();

            if (file == null)
                return Content("Deleted?");

            return File(file.FullName, "text/plain", file.Name);
        }
    }
}
