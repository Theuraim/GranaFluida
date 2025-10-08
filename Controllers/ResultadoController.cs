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

            // Se n�o filtrar, pega o m�s atual
            if (!dataInicio.HasValue || !dataFim.HasValue)
            {
                var hoje = DateTime.Now;
                hoje = DateTime.SpecifyKind(hoje, DateTimeKind.Utc);
                dataInicio = new DateTime(hoje.Year, hoje.Month, 1);
                dataFim = dataInicio.Value.AddMonths(1).AddDays(-1);
            }

            dataInicio = DateTime.SpecifyKind(dataInicio.Value.Date, DateTimeKind.Utc);
            dataFim = DateTime.SpecifyKind(dataFim.Value.Date, DateTimeKind.Utc);

            int? idUsuario = HttpContext.Session.GetInt32("IDUSUARIO");

            // --------- 1) MOVIMENTA��ES DO M�S (expandindo FIXAS para o intervalo dataInicio..dataFim) ----------
            var naoFixasMes = db.Movimentacao
                .Where(m => !m.FIXA && m.IDUSUARIO == idUsuario && m.DATAMOVIMENTACAO >= dataInicio && m.DATAMOVIMENTACAO <= dataFim)
                .ToList();

            var templatesFixas = db.Movimentacao
                .Where(m => m.FIXA && m.IDUSUARIO == idUsuario && m.DATAMOVIMENTACAO <= dataFim) // template que j� come�ou antes ou durante o per�odo
                .ToList();

            var movimentacoesMes = new List<Movimentacoes>();
            movimentacoesMes.AddRange(naoFixasMes);

            foreach (var t in templatesFixas)
            {
                // data da primeira ocorr�ncia do template
                var occ = DateTime.SpecifyKind(t.DATAMOVIMENTACAO.Date, DateTimeKind.Utc);

                // avan�a at� a primeira ocorr�ncia dentro do per�odo (>= dataInicio)
                while (occ < dataInicio)
                    occ = occ.AddMonths(1);

                // gera ocorr�ncias mensais at� dataFim
                while (occ <= dataFim)
                {
                    movimentacoesMes.Add(new Movimentacoes
                    {
                        IDMOVIMENTACAO = 0, // opcional: placeholder
                        DESCRICAOMOVIMENTACAO = t.DESCRICAOMOVIMENTACAO,
                        VALORMOVIMENTADO = t.VALORMOVIMENTADO,
                        DATAMOVIMENTACAO = occ,
                        TIPOMOVIMENTACAO = t.TIPOMOVIMENTACAO,
                        FIXA = true,
                        IDUSUARIO = t.IDUSUARIO
                    });
                    occ = occ.AddMonths(1);
                }
            }

            // Totais do m�s
            decimal entradas = movimentacoesMes
                .Where(m => m.TIPOMOVIMENTACAO == "Entrada")
                .Sum(m => Convert.ToDecimal(m.VALORMOVIMENTADO));
            decimal saidas = movimentacoesMes
                .Where(m => m.TIPOMOVIMENTACAO == "Sa�da")
                .Sum(m => Convert.ToDecimal(m.VALORMOVIMENTADO));
            decimal restante = entradas - saidas; // quanto foi economizado no m�s

            // --------- 2) C�LCULO DA META (expandindo FIXAS entre DATACADASTRO .. DATAFINALMETA) ----------
            var metaAtiva = db.Meta.FirstOrDefault(m => m.IDUSUARIO == idUsuario && m.ATIVA);

            decimal valorMeta = 0M;
            decimal faltaParaMeta = 0M;
            decimal restanteMeta = 0M;

            if (metaAtiva != null)
            {
                // seguran�a: datas em UTC e por dia (ignora hor�rio)
                var dataCadastro = DateTime.SpecifyKind(metaAtiva.DATACADASTRO.Date, DateTimeKind.Utc);
                var dataFinalMeta = DateTime.SpecifyKind(metaAtiva.DATAFINALMETA.Date, DateTimeKind.Utc);

                if (dataCadastro > dataFinalMeta)
                {
                    // sanity check: se cadastro for maior que final, inverte ou considera inv�lido
                    var tmp = dataCadastro;
                    dataCadastro = dataFinalMeta;
                    dataFinalMeta = tmp;
                }

                // ocorr�ncias n�o fixas no per�odo da meta (s�o lan�amentos pontuais)
                var naoFixasMeta = db.Movimentacao
                    .Where(m => !m.FIXA && m.IDUSUARIO == idUsuario && m.DATAMOVIMENTACAO >= dataCadastro && m.DATAMOVIMENTACAO <= dataFinalMeta)
                    .ToList();

                // templates fixos aplic�veis ao per�odo da meta (iniciaram antes do fim da meta)
                var templatesFixasMeta = db.Movimentacao
                    .Where(m => m.FIXA && m.IDUSUARIO == idUsuario && m.DATAMOVIMENTACAO <= dataFinalMeta)
                    .ToList();

                var movimentacoesMeta = new List<Movimentacoes>();
                movimentacoesMeta.AddRange(naoFixasMeta);

                foreach (var t in templatesFixasMeta)
                {
                    var occ = DateTime.SpecifyKind(t.DATAMOVIMENTACAO.Date, DateTimeKind.Utc);

                    // avan�a at� a primeira ocorr�ncia dentro do per�odo da meta
                    while (occ < dataCadastro)
                        occ = occ.AddMonths(1);

                    // gera ocorr�ncias mensais at� dataFinalMeta
                    while (occ <= dataFinalMeta)
                    {
                        movimentacoesMeta.Add(new Movimentacoes
                        {
                            IDMOVIMENTACAO = 0,
                            DESCRICAOMOVIMENTACAO = t.DESCRICAOMOVIMENTACAO,
                            VALORMOVIMENTADO = t.VALORMOVIMENTADO,
                            DATAMOVIMENTACAO = occ,
                            TIPOMOVIMENTACAO = t.TIPOMOVIMENTACAO,
                            FIXA = true,
                            IDUSUARIO = t.IDUSUARIO
                        });
                        occ = occ.AddMonths(1);
                    }
                }

                // Totais da meta (soma das ocorr�ncias reais + fixas expandidas)
                decimal entradasMeta = movimentacoesMeta
                    .Where(m => m.TIPOMOVIMENTACAO == "Entrada")
                    .Sum(m => Convert.ToDecimal(m.VALORMOVIMENTADO));
                decimal saidasMeta = movimentacoesMeta
                    .Where(m => m.TIPOMOVIMENTACAO == "Sa�da")
                    .Sum(m => Convert.ToDecimal(m.VALORMOVIMENTADO));

                restanteMeta = entradasMeta - saidasMeta;

                valorMeta = Convert.ToDecimal(metaAtiva.VALORMETA);
                faltaParaMeta = valorMeta > 0 ? valorMeta - restanteMeta : 0M;
                if (faltaParaMeta < 0) faltaParaMeta = 0M;
            }

            // --------- 3) Dados para gr�ficos, top3 e ViewBag ----------
            var dadosEntradasSaidas = new[]
            { new { label = "Entradas", value = (double)entradas },
              new { label = "Sa�das", value = (double)saidas } };

            var dadosPorDescricao = movimentacoesMes
                .GroupBy(m => m.DESCRICAOMOVIMENTACAO)
                .Select(g => new { label = g.Key, value = (double)g.Sum(m => Convert.ToDecimal(m.VALORMOVIMENTADO)) })
                .ToList();

            var top3 = movimentacoesMes
                .OrderByDescending(m => Convert.ToDecimal(m.VALORMOVIMENTADO))
                .Take(3)
                .ToList();

            ViewBag.DadosEntradasSaidas = JsonConvert.SerializeObject(dadosEntradasSaidas);
            ViewBag.DadosPorDescricao = JsonConvert.SerializeObject(dadosPorDescricao);
            ViewBag.Top3 = top3;
            ViewBag.Entradas = entradas;
            ViewBag.Saidas = saidas;
            ViewBag.Restante = restante;

            ViewBag.ValorMeta = valorMeta;
            ViewBag.FaltaParaMeta = faltaParaMeta;
            ViewBag.RestanteMeta = restanteMeta;

            ViewBag.DataInicio = dataInicio.Value.ToString("yyyy-MM-dd");
            ViewBag.DataFim = dataFim.Value.ToString("yyyy-MM-dd");

            return View();
        }

    }
}
