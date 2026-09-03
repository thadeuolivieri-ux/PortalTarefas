using PortalTarefas.Web.Data;
using PortalTarefas.Web.DTOs;
using PortalTarefas.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace PortalTarefas.Web.Services;

public class TarefaApiService : ITarefaApiService
{
    private readonly PortalTarefasDbContext _db;

    // Whitelist para ordenação segura
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "id", "titulo", "prioridade", "prazo"
    };

    public TarefaApiService(PortalTarefasDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResultDto<TaskSummaryDto>> GetPagedAsync(
        int page, int pageSize, string? sortBy, string? direction, string? search, bool? isCompleted)
    {
        int actualPage = Math.Max(1, page);
        int actualPageSize = Math.Clamp(pageSize, 1, 50);

        IQueryable<Tarefa> query = _db.Tarefas.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => t.Titulo.Contains(search) || (t.Descricao != null && t.Descricao.Contains(search)));
        }

        string sortField = string.IsNullOrWhiteSpace(sortBy) ? "id" : sortBy.ToLower();
        if (!AllowedSortFields.Contains(sortField))
        {
            throw new ArgumentException($"O campo de ordenação '{sortBy}' não é permitido.");
        }

        bool isDesc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);

        query = sortField switch
        {
            "titulo" => isDesc ? query.OrderByDescending(t => t.Titulo) : query.OrderBy(t => t.Titulo),
            "prioridade" => isDesc ? query.OrderByDescending(t => t.Prioridade) : query.OrderBy(t => t.Prioridade),
            "prazo" => isDesc ? query.OrderByDescending(t => t.Prazo) : query.OrderBy(t => t.Prazo),
            _ => isDesc ? query.OrderByDescending(t => t.Id) : query.OrderBy(t => t.Id)
        };

        int totalCount = await query.CountAsync();

        var items = await query
            .Skip((actualPage - 1) * actualPageSize)
            .Take(actualPageSize)
            .Select(t => new TaskSummaryDto
            {
                Id = t.Id,
                Title = t.Titulo,
                Priority = t.Prioridade.ToString(),
                IsCompleted = false,
                DueDate = t.Prazo
            })
            .ToListAsync();

        return new PagedResultDto<TaskSummaryDto>
        {
            Items = items,
            PageIndex = actualPage,
            PageSize = actualPageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TaskDetailDto?> GetByIdAsync(int id)
    {
        var task = await _db.Tarefas.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (task == null) return null;

        return MapToDetailDto(task);
    }

    public async Task<TaskDetailDto> CreateAsync(CreateTaskDto dto)
    {
        Enum.TryParse<Prioridade>(dto.Priority, true, out var prioridadeEnum);

        var task = new Tarefa
        {
            Titulo = dto.Title,
            Descricao = dto.Description ?? string.Empty,
            Prioridade = prioridadeEnum,
            Prazo = dto.DueDate ?? DateTime.UtcNow.AddDays(7),
            ConcurrencyToken = Guid.NewGuid().ToString()
        };

        _db.Tarefas.Add(task);
        await _db.SaveChangesAsync();

        return MapToDetailDto(task);
    }

    public async Task<TaskDetailDto?> UpdateAsync(int id, UpdateTaskDto dto, byte[] rowVersion)
    {
        var task = await _db.Tarefas.FirstOrDefaultAsync(t => t.Id == id);
        if (task == null) return null;

        Enum.TryParse<Prioridade>(dto.Priority, true, out var prioridadeEnum);

        task.Titulo = dto.Title;
        task.Descricao = dto.Description ?? string.Empty;
        task.Prioridade = prioridadeEnum;
        task.Prazo = dto.DueDate ?? task.Prazo;
        
        // Atualiza o token de concorrência a cada modificação
        task.ConcurrencyToken = Guid.NewGuid().ToString();

        await _db.SaveChangesAsync();

        return MapToDetailDto(task);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _db.Tarefas.FindAsync(id);
        if (task == null) return false;

        _db.Tarefas.Remove(task);
        await _db.SaveChangesAsync();
        return true;
    }

    private static TaskDetailDto MapToDetailDto(Tarefa t) => new()
    {
        Id = t.Id,
        Title = t.Titulo,
        Description = t.Descricao,
        Priority = t.Prioridade.ToString(),
        IsCompleted = false,
        DueDate = t.Prazo,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        RowVersion = System.Text.Encoding.UTF8.GetBytes(t.ConcurrencyToken)
    };
}