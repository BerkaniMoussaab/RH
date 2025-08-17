using System.Net;
using System.Text.Json;

namespace RH.Middleware
{
    /// <summary>
    /// Middleware pour la gestion globale des exceptions
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionMiddleware(
            RequestDelegate next, 
            ILogger<GlobalExceptionMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur non gérée s'est produite: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new ErrorResponse();

            switch (exception)
            {
                case ArgumentException argEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Paramètres invalides";
                    response.Details = argEx.Message;
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = "Accès non autorisé";
                    break;

                case FileNotFoundException fileEx:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = "Fichier non trouvé";
                    response.Details = fileEx.Message;
                    break;

                case InvalidOperationException opEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Opération invalide";
                    response.Details = opEx.Message;
                    break;

                case TimeoutException:
                    response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    response.Message = "Délai d'attente dépassé";
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "Une erreur interne s'est produite";
                    
                    // En développement, inclure les détails de l'exception
                    if (_environment.IsDevelopment())
                    {
                        response.Details = exception.Message;
                        response.StackTrace = exception.StackTrace;
                    }
                    break;
            }

            context.Response.StatusCode = response.StatusCode;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Modèle de réponse d'erreur
    /// </summary>
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? StackTrace { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Extensions pour enregistrer le middleware
    /// </summary>
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}

