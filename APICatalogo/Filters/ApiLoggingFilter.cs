using Microsoft.AspNetCore.Mvc.Filters;

namespace APICatalogo.Filters
{
    public class ApiLoggingFilter : IActionFilter
    {
        private readonly ILogger<ApiLoggingFilter> _logger;
        public ApiLoggingFilter(ILogger<ApiLoggingFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation("Iniciando execução da ação: {ActionName}", context.ActionDescriptor.DisplayName);
            _logger.LogInformation($"{DateTime.Now.ToShortTimeString()}");
            _logger.LogInformation($"ModelState : {context.ModelState.IsValid}");

        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                _logger.LogError(context.Exception, "Erro ao executar a ação: {ActionName}", context.ActionDescriptor.DisplayName);
            }
            else
            {
                _logger.LogInformation("Ação executada com sucesso: {ActionName}", context.ActionDescriptor.DisplayName);
            }
        }
    }
}
