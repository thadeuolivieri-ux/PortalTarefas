using System.Collections.Generic;
using PortalTarefas.Web.Models;

namespace PortalTarefas.Web.Services
{
    public interface ITarefaService
    {
        IEnumerable<Tarefa> ObterTodas();
        Tarefa? ObterPorId(int id);
        void Adicionar(Tarefa tarefa);
        bool Atualizar(Tarefa tarefa);
    }
}