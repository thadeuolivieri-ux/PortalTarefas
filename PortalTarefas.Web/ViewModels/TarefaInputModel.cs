using System;
using System.ComponentModel.DataAnnotations;
using PortalTarefas.Web.Models;
using PortalTarefas.Web.Validations;

namespace PortalTarefas.Web.ViewModels
{
    public class TarefaInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título da tarefa é obrigatório.")]
        [StringLength(100, ErrorMessage = "O título não pode exceder 100 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres.")]
        public string Descricao { get; set; } = string.Empty;

        public Prioridade Prioridade { get; set; }

        [DataType(DataType.Date)]
        [PrazoPorPrioridade]
        public DateTime Prazo { get; set; } = DateTime.Today.AddDays(1);

        public string ConcurrencyToken { get; set; } = Guid.NewGuid().ToString();
    }
}