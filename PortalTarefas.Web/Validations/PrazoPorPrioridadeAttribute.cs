using System;
using System.ComponentModel.DataAnnotations;
using PortalTarefas.Web.Models;
using PortalTarefas.Web.ViewModels;

namespace PortalTarefas.Web.Validations
{
    public class PrazoPorPrioridadeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var model = (TarefaInputModel)validationContext.ObjectInstance;

            // Regra 1: Não permite datas no passado
            if (model.Prazo < DateTime.Today)
            {
                return new ValidationResult("O prazo não pode ser uma data no passado.");
            }

            // Regra 2: Se a prioridade for ALTA, exige pelo menos 24h de prazo
            if (model.Prioridade == Prioridade.Alta && model.Prazo < DateTime.Today.AddDays(1))
            {
                return new ValidationResult("Tarefas de prioridade ALTA exigem um prazo de pelo menos 24 horas a contar de hoje.");
            }

            return ValidationResult.Success;
        }
    }
}