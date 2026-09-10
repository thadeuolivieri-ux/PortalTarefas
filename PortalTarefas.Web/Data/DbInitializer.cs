using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        public static async Task SeedSecurityAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // 1. Criar Roles
            string[] roles = { "Administrador", "Usuario" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Senha padronizada de desenvolvimento
            string defaultPassword = configuration["SeedPassword"] ?? "Senha@123";

            // 2. Criar Usuário Administrador
            var adminEmail = "admin@portal.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrador");
                    // Claim especial para política da Etapa 3 (Editar tarefas de outra equipe)
                    await userManager.AddClaimAsync(adminUser, new Claim("Permissao", "EditarOutraEquipe"));
                }
            }

            // 3. Criar Usuário Comum
            var userEmail = "usuario@portal.com";
            var comumUser = await userManager.FindByEmailAsync(userEmail);
            if (comumUser == null)
            {
                comumUser = new IdentityUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(comumUser, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(comumUser, "Usuario");
                }
            }
        }
    }
}