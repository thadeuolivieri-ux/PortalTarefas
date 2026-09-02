using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalTarefas.Web.Models;
using PortalTarefas.Web.Services;
using PortalTarefas.Web.ViewModels;

namespace PortalTarefas.Web.Controllers
{
    public class TarefasController : Controller
    {
        private readonly TarefaEfService _service;

        public TarefasController(TarefaEfService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var tarefas = await _service.ObterTodasAsync();
            return View(tarefas);
        }

        public async Task<IActionResult> Detalhes(int id)
        {
            var tarefa = await _service.ObterPorIdAsync(id);
            if (tarefa == null)
            {
                return NotFound();
            }

            return View(tarefa);
        }

        public IActionResult Criar()
        {
            return View(new TarefaInputModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(TarefaInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tarefa = new Tarefa
            {
                Titulo = model.Titulo,
                Descricao = model.Descricao,
                Prioridade = model.Prioridade,
                Prazo = model.Prazo,
                ConcurrencyToken = Guid.NewGuid().ToString()
            };

            await _service.AdicionarAsync(tarefa);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int id)
        {
            var tarefa = await _service.ObterPorIdAsync(id);
            if (tarefa == null)
            {
                return NotFound();
            }

            var model = new TarefaInputModel
            {
                Id = tarefa.Id,
                Titulo = tarefa.Titulo,
                Descricao = tarefa.Descricao,
                Prioridade = tarefa.Prioridade,
                Prazo = tarefa.Prazo,
                ConcurrencyToken = tarefa.ConcurrencyToken
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(TarefaInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tarefa = new Tarefa
            {
                Id = model.Id,
                Titulo = model.Titulo,
                Descricao = model.Descricao,
                Prioridade = model.Prioridade,
                Prazo = model.Prazo,
                ConcurrencyToken = model.ConcurrencyToken
            };

            try
            {
                await _service.AtualizarComTransacaoEConcorrenciaAsync(tarefa);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty, "O registro foi alterado por outro usuário. Por favor, recarregue a página e tente novamente.");
                return View(model);
            }
        }
    }
}