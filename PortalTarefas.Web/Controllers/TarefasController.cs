using Microsoft.AspNetCore.Mvc;
using PortalTarefas.Web.Filters;
using PortalTarefas.Web.Models;
using PortalTarefas.Web.Services;
using PortalTarefas.Web.ViewModels;

namespace PortalTarefas.Web.Controllers
{
    [ServiceFilter(typeof(LogAuditoriaActionFilter))]
    public class TarefasController : Controller
    {
        private readonly ITarefaService _tarefaService;

        public TarefasController(ITarefaService tarefaService)
        {
            _tarefaService = tarefaService;
        }

        // GET: Tarefas
        public IActionResult Index()
        {
            var tarefas = _tarefaService.ObterTodas();
            return View(tarefas);
        }

        // GET: Tarefas/Details/1
        public IActionResult Details(int id)
        {
            var tarefa = _tarefaService.ObterPorId(id);
            if (tarefa == null)
            {
                return NotFound();
            }
            return View(tarefa);
        }

        // GET: Tarefas/Create
        public IActionResult Create()
        {
            return View(new TarefaInputModel());
        }

        // POST: Tarefas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TarefaInputModel model)
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
                Prazo = model.Prazo
            };

            _tarefaService.Adicionar(tarefa);
            return RedirectToAction(nameof(Index));
        }

        // GET: Tarefas/Edit/1
        public IActionResult Edit(int id)
        {
            var tarefa = _tarefaService.ObterPorId(id);
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
                Prazo = tarefa.Prazo
            };

            return View(model);
        }

        // POST: Tarefas/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, TarefaInputModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

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
                Prazo = model.Prazo
            };

            if (!_tarefaService.Atualizar(tarefa))
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}