using System;
using System.Collections.Generic;
using System.Linq;
using PortalTarefas.Web.Models;

namespace PortalTarefas.Web.Services
{
    public class TarefaMemoryService : ITarefaService
    {
        private readonly List<Tarefa> _tarefas = new();
        private int _nextId = 1;

        public TarefaMemoryService()
        {
            Adicionar(new Tarefa 
            { 
                Titulo = "Estudar ASP.NET Core MVC", 
                Descricao = "Revisar Razor, Filters e Tag Helpers", 
                Prioridade = Prioridade.Alta, 
                Prazo = DateTime.Today.AddDays(2) 
            });
        }

        public IEnumerable<Tarefa> ObterTodas() => _tarefas;

        public Tarefa? ObterPorId(int id) => _tarefas.FirstOrDefault(t => t.Id == id);

        public void Adicionar(Tarefa tarefa)
        {
            tarefa.Id = _nextId++;
            _tarefas.Add(tarefa);
        }

        public bool Atualizar(Tarefa tarefa)
        {
            var index = _tarefas.FindIndex(t => t.Id == tarefa.Id);
            if (index == -1) return false;
            _tarefas[index] = tarefa;
            return true;
        }
    }
}