using System;

namespace PortalTarefas.Web.Models
{
    public class Tarefa
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public Prioridade Prioridade { get; set; }
        public DateTime Prazo { get; set; }
    }
}