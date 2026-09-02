using System;
using System.ComponentModel.DataAnnotations;

namespace PortalTarefas.Web.Models
{
    public class Tarefa
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Descricao { get; set; } = string.Empty;

        public Prioridade Prioridade { get; set; }

        public DateTime Prazo { get; set; }

        // Token de concorrência otimista (Etapa 3)
        [ConcurrencyCheck]
        public string ConcurrencyToken { get; set; } = Guid.NewGuid().ToString();
    }
}