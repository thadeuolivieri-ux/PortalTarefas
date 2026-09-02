using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PortalTarefas.Web.Data;
using PortalTarefas.Web.Models;

namespace PortalTarefas.Web.Services
{
    public class TarefaEfService
    {
        private readonly PortalTarefasDbContext _context;

        public TarefaEfService(PortalTarefasDbContext context)
        {
            _context = context;
        }

        public async Task<List<Tarefa>> ObterTodasAsync()
        {
            return await _context.Tarefas.AsNoTracking().ToListAsync();
        }

        public async Task<Tarefa?> ObterPorIdAsync(int id)
        {
            return await _context.Tarefas.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AdicionarAsync(Tarefa tarefa)
        {
            _context.Tarefas.Add(tarefa);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarComTransacaoEConcorrenciaAsync(Tarefa tarefa)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Altera o token de concorrência para a nova versão
                tarefa.ConcurrencyToken = Guid.NewGuid().ToString();
                _context.Tarefas.Update(tarefa);

                // Registra o evento de auditoria
                var auditoria = new EventoAuditoria
                {
                    Descricao = $"Tarefa ID {tarefa.Id} atualizada com sucesso.",
                    DataOcorrencia = DateTime.UtcNow
                };
                _context.EventosAuditoria.Add(auditoria);

                // Salva tudo na mesma transação
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                throw; // Repassa para ser capturado no Controller
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}