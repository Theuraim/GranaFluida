using GranaFluida.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
//using System.Data.Entity;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;

namespace GranaFluida.Controllers
{
    public class MovimentacaoController : Controller
    {
        private readonly DbContexto db;

        public MovimentacaoController(DbContexto db)
        {
            this.db = db;
        }

        // GET: Movimentacao
        public IActionResult Index()
        {
            if ((HttpContext.Session.GetInt32("UsuarioLogado") != 1))
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var movimentacoes = db.Movimentacao
                                           .Include(m => m.Usuario)
                                           .Where(m => m.IDUSUARIO == HttpContext.Session.GetInt32("IDUSUARIO"))
                                           .ToList();
                        return View(movimentacoes);
            }
        }

        // GET: Movimentacao/Details/5
        public IActionResult Details(int? id)
        {
            if ((HttpContext.Session.GetInt32("UsuarioLogado") != 1))
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                if (id == null) return NotFound();

                var movimentacao = db.Movimentacao
                                     .Include(m => m.Usuario)
                                     .FirstOrDefault(m => m.IDMOVIMENTACAO == id);

                if (movimentacao == null) return NotFound();

                return View(movimentacao);
            }
        }

        // GET: Movimentacao/Create
        public IActionResult Create()
        {
            if ((HttpContext.Session.GetInt32("UsuarioLogado") != 1))
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                return View();
            }
        }

        // POST: Movimentacao/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Movimentacoes movimentacao)
        {
            movimentacao.IDUSUARIO = HttpContext.Session.GetInt32("IDUSUARIO");
            ModelState.Remove("Usuario");
            ModelState.Remove("IDUSUARIO");

            if (ModelState.IsValid)
            {
                movimentacao.DATAMOVIMENTACAO = DateTime.SpecifyKind(movimentacao.DATAMOVIMENTACAO, DateTimeKind.Utc);

                db.Add(movimentacao);
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(movimentacao);
        }

        // GET: Movimentacao/Edit/5
        public IActionResult Edit(int? id)
        {
            if ((HttpContext.Session.GetInt32("UsuarioLogado") != 1))
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                if (id == null) return NotFound();

                var movimentacao = db.Movimentacao.Find(id);
                if (movimentacao == null) return NotFound();

                return View(movimentacao);
            }
        }

        // POST: Movimentacao/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Movimentacoes movimentacao)
        {
            if (id != movimentacao.IDMOVIMENTACAO) return NotFound();

            ModelState.Remove("Usuario");
            ModelState.Remove("IDUSUARIO");
            if (ModelState.IsValid)
            {
                try
                {
                    movimentacao.DATAMOVIMENTACAO = DateTime.SpecifyKind(movimentacao.DATAMOVIMENTACAO, DateTimeKind.Utc);
                    db.Update(movimentacao);
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovimentacaoExists(movimentacao.IDMOVIMENTACAO))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movimentacao);
        }

        // GET: Movimentacao/Delete/5
        public IActionResult Delete(int? id)
        {
            if ((HttpContext.Session.GetInt32("UsuarioLogado") != 1))
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                if (id == null) return NotFound();

                var movimentacao = db.Movimentacao
                                     .Include(m => m.Usuario)
                                     .FirstOrDefault(m => m.IDMOVIMENTACAO == id);
                if (movimentacao == null) return NotFound();

                return View(movimentacao);
            }
        }

        // POST: Movimentacao/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var movimentacao = db.Movimentacao.Find(id);
            if (movimentacao != null)
            {
                db.Movimentacao.Remove(movimentacao);
                db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MovimentacaoExists(int id)
        {
            return db.Movimentacao.Any(e => e.IDMOVIMENTACAO == id);
        }
    }
}