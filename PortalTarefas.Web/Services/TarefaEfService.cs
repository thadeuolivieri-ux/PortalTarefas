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
                // 1. Captura o token original que veio do formulário
                string tokenOriginal = tarefa.ConcurrencyToken;

                // 2. Anexa a entidade e define o token original no rastreador do EF
                var entry = _context.Entry(tarefa);
                entry.State = EntityState.Modified;
                entry.Property(t => t.ConcurrencyToken).OriginalValue = tokenOriginal;

                // 3. Define um novo token para a próxima alteração
                tarefa.ConcurrencyToken = Guid.NewGuid().ToString();

                // 4. Registra o evento de auditoria
                var auditoria = new EventoAuditoria
                {
                    Descricao = $"Tarefa ID {tarefa.Id} atualizada com sucesso.",
                    DataOcorrencia = DateTime.UtcNow
                };
                _context.EventosAuditoria.Add(auditoria);

                // 5. Salva tudo na mesma transação
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                throw; // Repassa para o Controller tratar
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}