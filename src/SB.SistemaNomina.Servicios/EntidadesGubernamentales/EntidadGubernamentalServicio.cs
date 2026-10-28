using FluentValidation;
using Microsoft.Extensions.Logging;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Excepciones;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Mapeos;
using SB.SistemaNomina.Aplicacion.Modelos.EntidadesGubernamentales;
using SB.SistemaNomina.Aplicacion.Validadores;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Servicios.Comun;

namespace SB.SistemaNomina.Servicios.EntidadesGubernamentales;

public sealed class EntidadGubernamentalServicio : IEntidadGubernamentalServicio
{
    private readonly IEntidadGubernamentalRepositorio _entidadRepositorio;
    private readonly IValidator<GuardarEntidadGubernamentalSolicitud> _validadorGuardar;
    private readonly IValidator<FiltroEntidadesGubernamentales> _validadorFiltro;
    private readonly ILogger<EntidadGubernamentalServicio> _registro;

    public EntidadGubernamentalServicio(
        IEntidadGubernamentalRepositorio entidadRepositorio,
        IValidator<GuardarEntidadGubernamentalSolicitud> validadorGuardar,
        IValidator<FiltroEntidadesGubernamentales> validadorFiltro,
        ILogger<EntidadGubernamentalServicio> registro)
    {
        _entidadRepositorio = entidadRepositorio;
        _validadorGuardar = validadorGuardar;
        _validadorFiltro = validadorFiltro;
        _registro = registro;
    }

    public async Task<ResultadoPaginado<EntidadGubernamentalRespuesta>> ListarAsync(
        FiltroEntidadesGubernamentales filtro,
        CancellationToken tokenCancelacion)
    {
        await _validadorFiltro.ValidarSolicitudAsync(filtro, tokenCancelacion);
        var resultado = await _entidadRepositorio.ListarAsync(filtro, tokenCancelacion);
        return resultado.Mapear(entidad => entidad.ARespuesta());
    }

    public async Task<EntidadGubernamentalRespuesta> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        (await ObtenerExistenteAsync(id, tokenCancelacion)).ARespuesta();

    public async Task<EntidadGubernamentalRespuesta> CrearAsync(
        GuardarEntidadGubernamentalSolicitud solicitud,
        CancellationToken tokenCancelacion)
    {
        await _validadorGuardar.ValidarSolicitudAsync(solicitud, tokenCancelacion);
        await ValidarNombreUnicoAsync(solicitud.Nombre.Trim(), null, tokenCancelacion);

        var entidad = new EntidadGubernamental();
        AplicarDatos(entidad, solicitud);
        await _entidadRepositorio.AgregarAsync(entidad, tokenCancelacion);

        _registro.LogInformation("Entidad gubernamental {EntidadId} creada.", entidad.Id);
        return entidad.ARespuesta();
    }

    public async Task<EntidadGubernamentalRespuesta> ActualizarAsync(
        Guid id,
        GuardarEntidadGubernamentalSolicitud solicitud,
        CancellationToken tokenCancelacion)
    {
        var entidad = await ObtenerExistenteAsync(id, tokenCancelacion);
        await _validadorGuardar.ValidarSolicitudAsync(solicitud, tokenCancelacion);
        await ValidarNombreUnicoAsync(solicitud.Nombre.Trim(), id, tokenCancelacion);

        AplicarDatos(entidad, solicitud);
        await _entidadRepositorio.ActualizarAsync(entidad, tokenCancelacion);

        _registro.LogInformation("Entidad gubernamental {EntidadId} actualizada.", entidad.Id);
        return entidad.ARespuesta();
    }

    public async Task DesactivarAsync(Guid id, CancellationToken tokenCancelacion)
    {
        var entidad = await ObtenerExistenteAsync(id, tokenCancelacion);
        entidad.EstaActiva = false;
        await _entidadRepositorio.ActualizarAsync(entidad, tokenCancelacion);

        _registro.LogInformation("Entidad gubernamental {EntidadId} desactivada.", entidad.Id);
    }

    private static void AplicarDatos(EntidadGubernamental entidad, GuardarEntidadGubernamentalSolicitud solicitud)
    {
        entidad.Nombre = solicitud.Nombre.Trim();
        entidad.Categoria = solicitud.Categoria.Trim();
        entidad.PoderDelEstado = solicitud.PoderDelEstado.Trim();
        entidad.Sector = solicitud.Sector.Trim();
    }

    private async Task<EntidadGubernamental> ObtenerExistenteAsync(Guid id, CancellationToken tokenCancelacion) =>
        await _entidadRepositorio.ObtenerPorIdAsync(id, tokenCancelacion)
        ?? throw new RecursoNoEncontradoException(NombresRecursos.ENTIDAD_GUBERNAMENTAL, id);

    private async Task ValidarNombreUnicoAsync(string nombre, Guid? idExcluido, CancellationToken tokenCancelacion)
    {
        if (await _entidadRepositorio.ExisteNombreAsync(nombre, idExcluido, tokenCancelacion))
        {
            throw new ConflictoException(MensajesServicio.ENTIDAD_DUPLICADA);
        }
    }
}
