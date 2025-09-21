using GranaFluida.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GranaFluida.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if ((HttpContext.Session.GetInt32("UsuarioLogado") != 1))
            {
                return RedirectToAction("Index", "Login");
            }
            else { return RedirectToAction("Index", "Movimentacao"); }
        }
    }
}
