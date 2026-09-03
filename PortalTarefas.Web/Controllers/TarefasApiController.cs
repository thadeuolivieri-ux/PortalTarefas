using PortalTarefas.Web.DTOs;
using PortalTarefas.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PortalTarefas.Web.Controllers;

[ApiController]
[Route("api/v1/tarefas")]
[Produces("application/json")]
public class TarefasApiController : ControllerBase
{
    private readonly ITarefaApiService _service;

    public TarefasApiController(ITarefaApiService service)
    {
        _service = service;
    }

    /// <summary>
    /// Recupera a lista paginada de tarefas com filtros e ordenação.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<TaskSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "datacriacao",
        [FromQuery] string? direction = "asc",
        [FromQuery] string? search = null,
        [FromQuery] bool? isCompleted = null)
    {
        try
        {
            var result = await _service.GetPagedAsync(page, pageSize, sortBy, direction, search, isCompleted);
            Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Parâmetro inválido");
        }
    }

    /// <summary>
    /// Busca uma tarefa específica pelo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _service.GetByIdAsync(id);
        if (task == null)
        {
            return Problem(
                detail: $"A tarefa com ID {id} não foi encontrada.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Recurso Não Encontrado");
        }

        return Ok(task);
    }

    /// <summary>
    /// Cria uma nova tarefa.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Atualiza uma tarefa existente verificando controle de concorrência.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto, [FromHeader(Name = "If-Match")] string? rowVersionBase64)
    {
        if (string.IsNullOrEmpty(rowVersionBase64))
        {
            return Problem(
                detail: "O cabeçalho 'If-Match' é obrigatório para atualização.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Cabeçalho Ausente");
        }

        byte[] rowVersion = Convert.FromBase64String(rowVersionBase64);

        try
        {
            var updated = await _service.UpdateAsync(id, dto, rowVersion);
            if (updated == null)
            {
                return Problem(
                    detail: $"A tarefa com ID {id} não existe.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Recurso Não Encontrado");
            }

            return Ok(updated);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(
                detail: "O registro foi alterado por outro usuário.",
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflito de Concorrência");
        }
    }

    /// <summary>
    /// Remove uma tarefa do sistema.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
        {
            return Problem(
                detail: $"A tarefa com ID {id} não existe para exclusão.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Recurso Não Encontrado");
        }

        return NoContent();
    }
}