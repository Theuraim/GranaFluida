using GranaFluida.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace GranaFluida.Controllers
{
    public class ResultadoController : Controller
    {
        private readonly DbContexto db;

        public ResultadoController(DbContexto context)
        {
            db = context;
        }

        public IActionResult Index(DateTime? dataInicio, DateTime? dataFim)
        {
            if ((HttpContext.Session.GetInt32("UsuarioLogado") != 1))
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                // Se n�o filtrar, pega o m�s atual
                if (!dataInicio.HasValue || !dataFim.HasValue)
                {
                    var hoje = DateTime.Now;
                    hoje = DateTime.SpecifyKind(hoje, DateTimeKind.Utc);
                    dataInicio = new DateTime(hoje.Year, hoje.Month, 1);
                    dataFim = dataInicio.Value.AddMonths(1).AddDays(-1);
                }

                dataInicio = DateTime.SpecifyKind((DateTime)dataInicio, DateTimeKind.Utc);
                dataFim = DateTime.SpecifyKind((DateTime)dataFim, DateTimeKind.Utc);

                int? idUsuario = HttpContext.Session.GetInt32("IDUSUARIO");

                var movimentacoes = db.Movimentacao
                    .Where(m => ((m.DATAMOVIMENTACAO >= dataInicio && m.DATAMOVIMENTACAO <= dataFim) || (m.FIXA)) && m.IDUSUARIO == idUsuario)
                    .ToList();

                // Totais
                var entradas = movimentacoes.Where(m => m.TIPOMOVIMENTACAO == "Entrada").Sum(m => m.VALORMOVIMENTADO);
                var sa�das = movimentacoes.Where(m => m.TIPOMOVIMENTACAO == "Sa�da").Sum(m => m.VALORMOVIMENTADO);
                var restante = entradas - sa�das; // quanto foi economizado no m�s

                // Busca meta ativa do usu�rio
                var metaAtiva = db.Meta.FirstOrDefault(m => m.IDUSUARIO == idUsuario && m.ATIVA);

                float? valorMeta = metaAtiva?.VALORMETA ?? 0F;
                decimal faltaParaMeta = valorMeta > 0 ? (decimal)valorMeta - (decimal)restante : 0;

                // Dados para gr�fico Entradas x Sa�das
                var dadosEntradasSaidas = new[]
                {
                    new { label = "Entradas", value = entradas },
                    new { label = "Sa�das", value = sa�das }
                };

                // Dados para gr�fico por descri��o
                var dadosPorDescricao = movimentacoes
                    .GroupBy(m => m.DESCRICAOMOVIMENTACAO)
                    .Select(g => new { label = g.Key, value = g.Sum(m => m.VALORMOVIMENTADO) })
                    .ToList();

                // Top 3 movimenta��es
                var top3 = movimentacoes
                    .OrderByDescending(m => m.VALORMOVIMENTADO)
                    .Take(3)
                    .ToList();

                // Passando dados para a View
                ViewBag.DadosEntradasSaidas = JsonConvert.SerializeObject(dadosEntradasSaidas);
                ViewBag.DadosPorDescricao = JsonConvert.SerializeObject(dadosPorDescricao);
                ViewBag.Top3 = top3;
                ViewBag.Entradas = entradas;
                ViewBag.Saidas = sa�das;
                ViewBag.Restante = restante;

                ViewBag.ValorMeta = valorMeta;
                ViewBag.FaltaParaMeta = faltaParaMeta < 0 ? 0 : faltaParaMeta;

                ViewBag.DataInicio = dataInicio.Value.ToString("yyyy-MM-dd");
                ViewBag.DataFim = dataFim.Value.ToString("yyyy-MM-dd");

                return View();
            }
        }
    }
}
