using FluentValidation;
using Microsoft.Extensions.Logging;
using SB.SistemaNomina.Aplicacion.Excepciones;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Mapeos;
using SB.SistemaNomina.Aplicacion.Modelos.Departamentos;
using SB.SistemaNomina.Aplicacion.Validadores;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Servicios.Comun;

namespace SB.SistemaNomina.Servicios.Departamentos;

public sealed class DepartamentoServicio : IDepartamentoServicio
{
    private static readonly CerrojoEscritura _cerrojo = CerrojoEscritura.EmpleadosYDepartamentos;

    private readonly IDepartamentoRepositorio _departamentoRepositorio;
    private readonly IEmpleadoRepositorio _empleadoRepositorio;
    private readonly IValidator<GuardarDepartamentoSolicitud> _validadorGuardar;
    private readonly ILogger<DepartamentoServicio> _registro;

    public DepartamentoServicio(
        IDepartamentoRepositorio departamentoRepositorio,
        IEmpleadoRepositorio empleadoRepositorio,
        IValidator<GuardarDepartamentoSolicitud> validadorGuardar,
        ILogger<DepartamentoServicio> registro)
    {
        _departamentoRepositorio = departamentoRepositorio;
        _empleadoRepositorio = empleadoRepositorio;
        _validadorGuardar = validadorGuardar;
        _registro = registro;
    }

    public async Task<IReadOnlyList<DepartamentoRespuesta>> ListarAsync(bool incluirInactivos, CancellationToken tokenCancelacion)
    {
        var departamentos = await _departamentoRepositorio.ListarAsync(incluirInactivos, tokenCancelacion);
        return departamentos.Select(departamento => departamento.ARespuesta()).ToList();
    }

    public async Task<DepartamentoRespuesta> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion) =>
        (await ObtenerExistenteAsync(id, tokenCancelacion)).ARespuesta();

    public async Task<DepartamentoRespuesta> CrearAsync(GuardarDepartamentoSolicitud solicitud, CancellationToken tokenCancelacion)
    {
        await _validadorGuardar.ValidarSolicitudAsync(solicitud, tokenCancelacion);
        var nombre = solicitud.Nombre.Trim();

        return await _cerrojo.EjecutarAsync(
            async () =>
            {
                await ValidarNombreUnicoAsync(nombre, null, tokenCancelacion);

                var departamento = new Departamento { Nombre = nombre };
                await _departamentoRepositorio.AgregarAsync(departamento, tokenCancelacion);

                _registro.LogInformation("Departamento {DepartamentoId} creado.", departamento.Id);
                return departamento.ARespuesta();
            },
            tokenCancelacion);
    }

    public async Task<DepartamentoRespuesta> ActualizarAsync(Guid id, GuardarDepartamentoSolicitud solicitud, CancellationToken tokenCancelacion)
    {
        await _validadorGuardar.ValidarSolicitudAsync(solicitud, tokenCancelacion);
        var nombre = solicitud.Nombre.Trim();

        return await _cerrojo.EjecutarAsync(
            async () =>
            {
                var departamento = await ObtenerExistenteAsync(id, tokenCancelacion);
                await ValidarNombreUnicoAsync(nombre, id, tokenCancelacion);

                departamento.Nombre = nombre;
                await _departamentoRepositorio.ActualizarAsync(departamento, tokenCancelacion);

                _registro.LogInformation("Departamento {DepartamentoId} actualizado.", departamento.Id);
                return departamento.ARespuesta();
            },
            tokenCancelacion);
    }

    public Task DesactivarAsync(Guid id, CancellationToken tokenCancelacion) =>
        _cerrojo.EjecutarAsync(
            async () =>
            {
                var departamento = await ObtenerExistenteAsync(id, tokenCancelacion);
                var empleadosActivos = await _empleadoRepositorio.ListarActivosAsync(id, tokenCancelacion);
                if (empleadosActivos.Count > 0)
                {
                    throw new ReglaNegocioException(MensajesServicio.DEPARTAMENTO_CON_EMPLEADOS_ACTIVOS);
                }

                departamento.EstaActivo = false;
                await _departamentoRepositorio.ActualizarAsync(departamento, tokenCancelacion);

                _registro.LogInformation("Departamento {DepartamentoId} desactivado.", departamento.Id);
            },
            tokenCancelacion);

    private async Task<Departamento> ObtenerExistenteAsync(Guid id, CancellationToken tokenCancelacion) =>
        await _departamentoRepositorio.ObtenerPorIdAsync(id, tokenCancelacion)
        ?? throw new RecursoNoEncontradoException(NombresRecursos.DEPARTAMENTO, id);

    private async Task ValidarNombreUnicoAsync(string nombre, Guid? idExcluido, CancellationToken tokenCancelacion)
    {
        if (await _departamentoRepositorio.ExisteNombreAsync(nombre, idExcluido, tokenCancelacion))
        {
            throw new ConflictoException(MensajesServicio.DEPARTAMENTO_DUPLICADO);
        }
    }
}
