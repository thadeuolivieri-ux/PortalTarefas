using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalTarefas.Web.Models;

namespace PortalTarefas.Web.Data
{
    public class PortalTarefasDbContext : IdentityDbContext<IdentityUser>
    {
        public PortalTarefasDbContext(DbContextOptions<PortalTarefasDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tarefa> Tarefas => Set<Tarefa>();
        public DbSet<EventoAuditoria> EventosAuditoria => Set<EventoAuditoria>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tarefa>(builder =>
            {
                builder.HasKey(t => t.Id);
                builder.Property(t => t.Titulo).IsRequired().HasMaxLength(100);
                builder.Property(t => t.Descricao).HasMaxLength(500);
                
                // Configuração da concorrência
                builder.Property(t => t.ConcurrencyToken)
                       .IsConcurrencyToken();
            });

            modelBuilder.Entity<EventoAuditoria>(builder =>
            {
                builder.HasKey(e => e.Id);
                builder.Property(e => e.Descricao).IsRequired().HasMaxLength(255);
            });
        }
    }
}