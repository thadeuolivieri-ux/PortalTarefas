using System;
using System.Linq;
using PortalTarefas.Web.Models;

namespace PortalTarefas.Web.Data
{
    public static class DbInitializer
    {
        public static void Initialize(PortalTarefasDbContext context)
        {
            // Garante que o banco seja criado
            context.Database.EnsureCreated();

            // Verifica se já existem tarefas no banco (Seed idempotente)
            if (context.Tarefas.Any())
            {
                return; // O banco já foi populado
            }

            var tarefas = new Tarefa[]
            {
                new Tarefa
                {
                    Titulo = "Estudar EF Core 10",
                    Descricao = "Aprender sobre DbContext, Migrations e Concorrência Otimista.",
                    Prioridade = Prioridade.Alta,
                    Prazo = DateTime.Today.AddDays(2),
                    ConcurrencyToken = Guid.NewGuid().ToString()
                },
                new Tarefa
                {
                    Titulo = "Implementar consultas com Dapper",
                    Descricao = "Comparar o desempenho de leitura do EF Core com o Dapper.",
                    Prioridade = Prioridade.Media,
                    Prazo = DateTime.Today.AddDays(5),
                    ConcurrencyToken = Guid.NewGuid().ToString()
                }
            };

            context.Tarefas.AddRange(tarefas);
            context.SaveChanges();
        }
    }
}