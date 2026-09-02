using System;
using System.ComponentModel.DataAnnotations;
using PortalTarefas.Web.Models;
using PortalTarefas.Web.Validations;

namespace PortalTarefas.Web.ViewModels
{
    public class TarefaInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O título deve ter entre 3 e 100 caracteres.")]
        [Display(Name = "Título da Tarefa")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(500, ErrorMessage = "A descrição não pode ultrapassar 500 caracteres.")]
        [Display(Name = "Descrição Detalhada")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "A prioridade é obrigatória.")]
        [Display(Name = "Prioridade")]
        public Prioridade Prioridade { get; set; }

        [Required(ErrorMessage = "O prazo é obrigatório.")]
        [DataType(DataType.Date)]
        [Display(Name = "Prazo de Conclusão")]
        [PrazoPorPrioridade]
        public DateTime Prazo { get; set; } = DateTime.Today.AddDays(1);
    }
}