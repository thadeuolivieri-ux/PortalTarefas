using System;

namespace PortalTarefas.Web.Models
{
    public class EventoAuditoria
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public DateTime DataOcorrencia { get; set; } = DateTime.UtcNow;
    }
}