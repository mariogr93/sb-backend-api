using Microsoft.AspNetCore.Mvc;
using SB.SistemaNomina.API.Extensiones;
using SB.SistemaNomina.Aplicacion.Excepciones;
using SB.SistemaNomina.Dominio.Excepciones;

namespace SB.SistemaNomina.API.Middlewares;

/// <summary>Convierte excepciones en ProblemDetails; nunca expone detalles internos fuera de Development.</summary>
public sealed class ManejoExcepcionesMiddleware
{
    private const string TITULO_ERROR_INTERNO = "Error interno del servidor.";
    private const string DETALLE_ERROR_INTERNO = "Ocurrió un error inesperado. Use el traceId para reportarlo.";
    private const string TIPO_CONTENIDO = "application/problem+json";

    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ManejoExcepcionesMiddleware> _registro;
    private readonly IHostEnvironment _entorno;

    public ManejoExcepcionesMiddleware(
        RequestDelegate siguiente,
        ILogger<ManejoExcepcionesMiddleware> registro,
        IHostEnvironment entorno)
    {
        _siguiente = siguiente;
        _registro = registro;
        _entorno = entorno;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (OperationCanceledException) when (contexto.RequestAborted.IsCancellationRequested)
        {
            // El cliente cerró la conexión; no hay a quién responder.
            _registro.LogInformation("Solicitud cancelada por el cliente: {Ruta}.", contexto.Request.Path);
        }
        catch (Exception excepcion)
        {
            if (contexto.Response.HasStarted)
            {
                _registro.LogError(excepcion, "Error después de iniciar la respuesta: {Ruta}.", contexto.Request.Path);
                throw;
            }

            await EscribirProblemaAsync(contexto, excepcion);
        }
    }

    private async Task EscribirProblemaAsync(HttpContext contexto, Exception excepcion)
    {
        var problema = CrearProblema(excepcion);
        problema.Instance = contexto.Request.Path;
        problema.Extensions[ExtensionesErrores.CLAVE_TRAZA] = contexto.TraceIdentifier;

        if (problema.Status >= StatusCodes.Status500InternalServerError)
        {
            _registro.LogError(excepcion, "Error no controlado en {Metodo} {Ruta}.", contexto.Request.Method, contexto.Request.Path);
        }
        else
        {
            _registro.LogWarning("Solicitud rechazada ({Estado}) en {Metodo} {Ruta}: {Mensaje}",
                problema.Status, contexto.Request.Method, contexto.Request.Path, problema.Detail ?? problema.Title);
        }

        contexto.Response.Clear();
        contexto.Response.StatusCode = problema.Status ?? StatusCodes.Status500InternalServerError;
        await contexto.Response.WriteAsJsonAsync(problema, problema.GetType(), options: null, TIPO_CONTENIDO, contexto.RequestAborted);
    }

    private ProblemDetails CrearProblema(Exception excepcion) => excepcion switch
    {
        ValidacionSolicitudException validacion => new ValidationProblemDetails(
            validacion.Errores.ToDictionary(error => error.Key, error => error.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = validacion.Message
        },
        CredencialesInvalidasException => Problema(StatusCodes.Status401Unauthorized, "No autorizado.", excepcion.Message),
        RecursoNoEncontradoException => Problema(StatusCodes.Status404NotFound, "Recurso no encontrado.", excepcion.Message),
        ConflictoException => Problema(StatusCodes.Status409Conflict, "Conflicto.", excepcion.Message),
        ReglaNegocioException or DominioException =>
            Problema(StatusCodes.Status422UnprocessableEntity, "Operación no permitida.", excepcion.Message),
        _ => Problema(
            StatusCodes.Status500InternalServerError,
            TITULO_ERROR_INTERNO,
            _entorno.IsDevelopment() ? excepcion.ToString() : DETALLE_ERROR_INTERNO)
    };

    private static ProblemDetails Problema(int estado, string titulo, string detalle) =>
        new() { Status = estado, Title = titulo, Detail = detalle };
}
