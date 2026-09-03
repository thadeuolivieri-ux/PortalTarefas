using PortalTarefas.Web.DTOs;

namespace PortalTarefas.Web.Services;

public interface ITarefaApiService
{
    Task<PagedResultDto<TaskSummaryDto>> GetPagedAsync(
        int page, int pageSize, string? sortBy, string? direction, string? search, bool? isCompleted);
    Task<TaskDetailDto?> GetByIdAsync(int id);
    Task<TaskDetailDto> CreateAsync(CreateTaskDto dto);
    Task<TaskDetailDto?> UpdateAsync(int id, UpdateTaskDto dto, byte[] rowVersion);
    Task<bool> DeleteAsync(int id);
}