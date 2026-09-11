using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalTarefas.Web.Data;
using PortalTarefas.Web.Filters;
using PortalTarefas.Web.Middlewares;
using PortalTarefas.Web.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// 1. Serviços da Atividade 1
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuração do CORS para permitir requisições no GitHub Codespaces
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendLocal", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 2. Serviços da Atividade 2 e 3
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ITarefaService, TarefaMemoryService>();
builder.Services.AddScoped<LogAuditoriaActionFilter>();

// Configuração do banco SQLite e Serviços da Atividade 3
builder.Services.AddDbContext<PortalTarefasDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ==========================================
// SERVIÇOS DA ATIVIDADE 6 (ASP.NET Core Identity & Segurança)
// ==========================================
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Requisitos de senha didáticos
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    
    // Configurações de Bloqueio (Lockout)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<PortalTarefasDbContext>()
.AddDefaultTokenProviders();

// Ajuste nos Cookies para API REST (retorna 401/403 em vez de redirecionar para tela HTML)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

// Configuração de Autorização e Políticas da Atividade 6
builder.Services.AddAuthorization(options =>
{
    // Política baseada no Claim 'Permissao' com valor 'EditarOutraEquipe'
    options.AddPolicy("PodeEditarOutraEquipe", policy =>
        policy.RequireClaim("Permissao", "EditarOutraEquipe"));
});

builder.Services.AddScoped<TarefaEfService>();
builder.Services.AddScoped<TarefaDapperService>();

// ==========================================
// SERVIÇOS DA ATIVIDADE 4 (API REST, OpenAPI & Problem Details)
// ==========================================
builder.Services.AddScoped<ITarefaApiService, TarefaApiService>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "API de Gestão de Tarefas (PortalTarefas API)";
        document.Info.Version = "v1";
        document.Info.Description = "Contrato HTTP RESTful documentado para a Unidade 4 da disciplina de Desenvolvimento Web .NET.";
        document.Servers.Clear();
        return Task.CompletedTask;
    });
});
// ==========================================
// SERVIÇOS DA ATIVIDADE 7 (OpenTelemetry - Metrics & Tracing)
// ==========================================
builder.Services.AddSingleton<TarefasMetrics>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("PortalTarefas.API"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter(TarefasMetrics.MeterName)
            .AddConsoleExporter();
    });

// ==========================================
// SERVIÇOS DA ATIVIDADE 7 (Health Checks)
// ==========================================
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PortalTarefasDbContext>(
        name: "banco_sqlite",
        tags: new[] { "ready" });
        
// ==========================================
// VALIDAÇÃO DE CONFIGURAÇÃO OBRIGATÓRIA (Unidade 8 - Fail-Fast)
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("A string de conexão 'DefaultConnection' é obrigatória e não foi configurada.");
}


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<PortalTarefasDbContext>();
    var config = services.GetRequiredService<IConfiguration>();
    
    DbInitializer.Initialize(context);
    await DbInitializer.SeedSecurityAsync(services, config);
}

// Middlewares e Tratamento de Erros
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            Erro = "Ocorreu um erro interno no servidor.",
            Status = 500
        });
    });
});

// ==========================================
// MIDDLEWARES DA ATIVIDADE 7 (Observabilidade)
// ==========================================
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("FrontendLocal");

app.UseRequestLogging();

app.UseRouting();

// Middlewares da Atividade 6 (Autenticação e Autorização na ordem correta)
app.UseAuthentication();
app.UseAuthorization();

// ==========================================
// MIDDLEWARES DE AMBIENTE DA ATIVIDADE 4 (Swagger & OpenAPI)
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "PortalTarefas API v1");
        c.RoutePrefix = "swagger";
    });
}

// ==========================================
// ENDPOINTS DA ATIVIDADE 1 (Isolados e Mantidos)
// ==========================================

List<TarefaDto> bancoTarefas = new()
{
    new TarefaDto(1, "Configurar Projeto", "Criar base em .NET 10", "Concluída"),
    new TarefaDto(2, "Criar Endpoints", "Implementar Minimal APIs", "Pendente")
};

app.MapGet("/api/minimal/diagnostico", () => Results.Ok(new
{
    Origem = "Minimal API",
    HorarioUtc = DateTime.UtcNow,
    VersaoRuntime = Environment.Version.ToString(),
    Status = "Saudável"
}));

app.MapGet("/api/tarefas", ([FromQuery] string? situacao, [FromHeader(Name = "X-Client-Id")] string? clientId) =>
{
    var resultado = string.IsNullOrEmpty(situacao) 
        ? bancoTarefas 
        : bancoTarefas.Where(t => t.Situacao.Equals(situacao, StringComparison.OrdinalIgnoreCase)).ToList();

    return Results.Ok(new { Client = clientId ?? "Anônimo", Dados = resultado });
});

app.MapGet("/api/tarefas/{id:int}", (int id) =>
{
    var tarefa = bancoTarefas.FirstOrDefault(t => t.Id == id);
    return tarefa is not null ? Results.Ok(tarefa) : Results.NotFound(new { Mensagem = "Tarefa não encontrada." });
});

app.MapPost("/api/tarefas", ([FromBody] TarefaCreateDto novaTarefa) =>
{
    if (string.IsNullOrWhiteSpace(novaTarefa.Titulo))
    {
        return Results.BadRequest(new { Erro = "O título da tarefa é obrigatório." });
    }

    var id = bancoTarefas.Max(t => t.Id) + 1;
    var tarefaCriada = new TarefaDto(id, novaTarefa.Titulo, novaTarefa.Descricao, "Pendente");
    bancoTarefas.Add(tarefaCriada);

    return Results.Created($"/api/tarefas/{id}", tarefaCriada);
});

// ==========================================
// ROTAS NOVAS DA ATIVIDADE 2, 3 E 4 (MVC + API Controllers)
// ==========================================
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tarefas}/{action=Index}/{id?}");

app.Run();

// Records da Atividade 1
public record TarefaDto(int Id, string Titulo, string Descricao, string Situacao);
public record TarefaCreateDto(string Titulo, string Descricao);