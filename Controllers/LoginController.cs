using GranaFluida.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GranaFluida.Controllers
{
    public class LoginController : Controller
    {
        private readonly DbContexto db;

        public LoginController(DbContexto db)
        {
            this.db = db;
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Usuarios usuario)
        {
            if (string.IsNullOrEmpty(usuario.NOMEUSUARIO) || string.IsNullOrEmpty(usuario.SENHA))
            {
                ModelState.AddModelError("", "Preencha todos os campos.");
                return View(usuario);
            }

            // Busca no banco se existe o usuário
            var user = db.Usuario
                         .FirstOrDefault(u => u.NOMEUSUARIO == usuario.NOMEUSUARIO && u.SENHA == usuario.SENHA);

            if (user != null)
            {
                // Login válido -> poderia guardar em sessão
                HttpContext.Session.SetInt32("IDUSUARIO", user.IDUSUARIO);
                HttpContext.Session.SetString("UsuarioNome", user.NOMEUSUARIO);
                HttpContext.Session.SetInt32("UsuarioLogado", 1);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Usuário ou senha inválidos.");
                return View(usuario);
            }
        }


        [HttpGet]
        public IActionResult CriarConta()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CriarConta(Usuarios usuario)
        {
            if (ModelState.IsValid)
            {
                usuario.DATACADASTRO = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                HttpContext.Session.SetInt32("IDUSUARIO", usuario.IDUSUARIO);
                HttpContext.Session.SetString("UsuarioNome", usuario.NOMEUSUARIO);
                HttpContext.Session.SetInt32("UsuarioLogado", 1);
                db.Usuario.Add(usuario);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.SetInt32("IDUSUARIO", -1);
            HttpContext.Session.SetString("UsuarioNome", String.Empty);
            HttpContext.Session.SetInt32("UsuarioLogado", 0);
            return RedirectToAction("Index", "Login");
        }

    }
}
