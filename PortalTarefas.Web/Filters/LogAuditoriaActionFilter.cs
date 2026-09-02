using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace PortalTarefas.Web.Filters
{
    public class LogAuditoriaActionFilter : IActionFilter
    {
        private readonly ILogger<LogAuditoriaActionFilter> _logger;
        private Stopwatch _timer = null!;

        public LogAuditoriaActionFilter(ILogger<LogAuditoriaActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _timer = Stopwatch.StartNew();
            var actionName = context.ActionDescriptor.DisplayName;
            _logger.LogInformation("[AUDITORIA INÍCIO] Executando Ação: {ActionName}", actionName);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _timer.Stop();
            var actionName = context.ActionDescriptor.DisplayName;
            var elapsedMs = _timer.ElapsedMilliseconds;
            var statusCode = context.HttpContext.Response.StatusCode;

            _logger.LogInformation("[AUDITORIA FIM] Ação: {ActionName} | Duração: {Elapsed} ms | Status: {StatusCode}", 
                actionName, elapsedMs, statusCode);
        }
    }
}