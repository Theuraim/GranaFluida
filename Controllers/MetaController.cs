using GranaFluida.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GranaFluida.Controllers
{
    public class MetaController : Controller
    {
        private readonly DbContexto db;

        public MetaController(DbContexto db)
        {
            this.db = db;
        }

        // GET: Meta
        public async Task<IActionResult> Index()
        {
            if ((HttpContext.Session.GetInt32("UsuarioLogado") != 1))
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var Meta = await db.Meta
                .Include(m => m.Usuario)
                .ToListAsync();
                return View(Meta);
            }
        }

        //// GET: Meta/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null) return NotFound();

        //    var meta = await db.Meta
        //        .Include(m => m.Usuario)
        //        .FirstOrDefaultAsync(m => m.IDMETA == id);

        //    if (meta == null) return NotFound();

        //    return View(meta);
        //}

        // GET: Meta/Create
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

        // POST: Metas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IDMETA,DESCRICAOMETA,VALORMETA,DATAFINALMETA,ATIVA,IDUSUARIO")] Metas meta)
        {
            meta.IDUSUARIO = HttpContext.Session.GetInt32("IDUSUARIO");
            ModelState.Remove("Usuario");
            ModelState.Remove("IDUSUARIO");

            meta.DATACADASTRO = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            ModelState.Remove("DATACADASTRO");

            if (ModelState.IsValid)
            {
                if (meta.ATIVA)
                {
                    // Verifica se o usuário já possui meta ativa
                    bool existeMetaAtiva = await db.Meta
                        .AnyAsync(m => m.IDUSUARIO == meta.IDUSUARIO && m.ATIVA);

                    if (existeMetaAtiva)
                    {
                        ModelState.AddModelError("", "O usuário já possui uma meta ativa. Desative a atual antes de criar outra.");
                        return View(meta);
                    }
                }


                meta.DATAFINALMETA = DateTime.SpecifyKind(meta.DATAFINALMETA, DateTimeKind.Utc);
               
                db.Add(meta);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(meta);
        }


        // GET: Meta/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if ((HttpContext.Session.GetInt32("UsuarioLogado") != 1))
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                if (id == null) return NotFound();

                var meta = await db.Meta.FindAsync(id);
                if (meta == null) return NotFound();

                return View(meta);
            }
        }

        // POST: Metas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IDMETA,DESCRICAOMETA,VALORMETA,DATAFINALMETA,ATIVA,IDUSUARIO")] Metas meta)
        {
            if (id != meta.IDMETA) return NotFound();


            meta.IDUSUARIO = HttpContext.Session.GetInt32("IDUSUARIO");
            ModelState.Remove("Usuario");
            ModelState.Remove("IDUSUARIO");

            meta.DATACADASTRO = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            ModelState.Remove("DATACADASTRO");

            if (ModelState.IsValid)
            {
                if (meta.ATIVA)
                {
                    // Verifica se já existe outra meta ativa para o mesmo usuário (exclui a própria meta)
                    bool existeOutraAtiva = await db.Meta
                        .AnyAsync(m => m.IDUSUARIO == meta.IDUSUARIO && m.ATIVA && m.IDMETA != meta.IDMETA);

                    if (existeOutraAtiva)
                    {
                        ModelState.AddModelError("", "O usuário já possui outra meta ativa. Desative-a antes de ativar esta.");
                        return View(meta);
                    }
                }

                try
                {
                    meta.DATAFINALMETA = DateTime.SpecifyKind(meta.DATAFINALMETA, DateTimeKind.Utc);
                    db.Update(meta);
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MetaExists(meta.IDMETA)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(meta);
        }

        // GET: Meta/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if ((HttpContext.Session.GetInt32("UsuarioLogado") != 1))
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                if (id == null) return NotFound();

                var meta = await db.Meta
                    .Include(m => m.Usuario)
                    .FirstOrDefaultAsync(m => m.IDMETA == id);

                if (meta == null) return NotFound();

                return View(meta);
            }
        }

        // POST: Meta/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var meta = await db.Meta.FindAsync(id);
            if (meta != null)
            {
                db.Meta.Remove(meta);
                await db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MetaExists(int id)
        {
            return db.Meta.Any(e => e.IDMETA == id);
        }
    }
}
