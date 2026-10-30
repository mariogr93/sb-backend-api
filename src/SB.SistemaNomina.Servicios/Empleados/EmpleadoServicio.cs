using FluentValidation;
using Microsoft.Extensions.Logging;
using SB.SistemaNomina.Aplicacion.Comun;
using SB.SistemaNomina.Aplicacion.Excepciones;
using SB.SistemaNomina.Aplicacion.Interfaces.Fabricas;
using SB.SistemaNomina.Aplicacion.Interfaces.Repositorios;
using SB.SistemaNomina.Aplicacion.Interfaces.Servicios;
using SB.SistemaNomina.Aplicacion.Mapeos;
using SB.SistemaNomina.Aplicacion.Modelos.Empleados;
using SB.SistemaNomina.Aplicacion.Validadores;
using SB.SistemaNomina.Dominio.Entidades;
using SB.SistemaNomina.Servicios.Comun;

namespace SB.SistemaNomina.Servicios.Empleados;

public sealed class EmpleadoServicio : IEmpleadoServicio
{
    private static readonly CerrojoEscritura _cerrojo = CerrojoEscritura.EmpleadosYDepartamentos;

    private readonly IEmpleadoRepositorio _empleadoRepositorio;
    private readonly IDepartamentoRepositorio _departamentoRepositorio;
    private readonly IFabricaEmpleado _fabricaEmpleado;
    private readonly IValidator<GuardarEmpleadoSolicitud> _validadorGuardar;
    private readonly IValidator<FiltroEmpleados> _validadorFiltro;
    private readonly TimeProvider _proveedorTiempo;
    private readonly ILogger<EmpleadoServicio> _registro;

    public EmpleadoServicio(
        IEmpleadoRepositorio empleadoRepositorio,
        IDepartamentoRepositorio departamentoRepositorio,
        IFabricaEmpleado fabricaEmpleado,
        IValidator<GuardarEmpleadoSolicitud> validadorGuardar,
        IValidator<FiltroEmpleados> validadorFiltro,
        TimeProvider proveedorTiempo,
        ILogger<EmpleadoServicio> registro)
    {
        _empleadoRepositorio = empleadoRepositorio;
        _departamentoRepositorio = departamentoRepositorio;
        _fabricaEmpleado = fabricaEmpleado;
        _validadorGuardar = validadorGuardar;
        _validadorFiltro = validadorFiltro;
        _proveedorTiempo = proveedorTiempo;
        _registro = registro;
    }

    public async Task<ResultadoPaginado<EmpleadoRespuesta>> ListarAsync(FiltroEmpleados filtro, CancellationToken tokenCancelacion)
    {
        await _validadorFiltro.ValidarSolicitudAsync(filtro, tokenCancelacion);

        var resultado = await _empleadoRepositorio.ListarAsync(filtro, tokenCancelacion);
        var nombresDepartamentos = await ObtenerNombresDepartamentosAsync(tokenCancelacion);

        return resultado.Mapear(empleado => ARespuesta(empleado, nombresDepartamentos));
    }

    public async Task<EmpleadoRespuesta> ObtenerPorIdAsync(Guid id, CancellationToken tokenCancelacion)
    {
        var empleado = await ObtenerExistenteAsync(id, tokenCancelacion);
        var departamento = await _departamentoRepositorio.ObtenerPorIdAsync(empleado.DepartamentoId, tokenCancelacion);

        return ARespuesta(empleado, departamento?.Nombre ?? string.Empty);
    }

    public async Task<EmpleadoRespuesta> CrearAsync(GuardarEmpleadoSolicitud solicitud, CancellationToken tokenCancelacion)
    {
        await _validadorGuardar.ValidarSolicitudAsync(solicitud, tokenCancelacion);

        return await _cerrojo.EjecutarAsync(
            async () =>
            {
                var departamento = await ObtenerDepartamentoActivoAsync(solicitud.DepartamentoId, tokenCancelacion);
                await ValidarNumeroSeguroSocialUnicoAsync(solicitud.NumeroSeguroSocial, null, tokenCancelacion);

                var empleado = _fabricaEmpleado.Crear(solicitud);
                empleado.FechaCreacion = _proveedorTiempo.GetUtcNow().UtcDateTime;
                await _empleadoRepositorio.AgregarAsync(empleado, tokenCancelacion);

                _registro.LogInformation("Empleado {EmpleadoId} creado ({Tipo}).", empleado.Id, empleado.Tipo);
                return ARespuesta(empleado, departamento.Nombre);
            },
            tokenCancelacion);
    }

    public async Task<EmpleadoRespuesta> ActualizarAsync(Guid id, GuardarEmpleadoSolicitud solicitud, CancellationToken tokenCancelacion)
    {
        await _validadorGuardar.ValidarSolicitudAsync(solicitud, tokenCancelacion);

        return await _cerrojo.EjecutarAsync(
            async () =>
            {
                var existente = await ObtenerExistenteAsync(id, tokenCancelacion);
                var departamento = await ObtenerDepartamentoActivoAsync(solicitud.DepartamentoId, tokenCancelacion);
                await ValidarNumeroSeguroSocialUnicoAsync(solicitud.NumeroSeguroSocial, id, tokenCancelacion);

                // Se recrea la subclase para admitir cambio de tipo; se conservan Id, estado y fecha de creación.
                var empleado = _fabricaEmpleado.Crear(solicitud);
                empleado.Id = existente.Id;
                empleado.Estado = existente.Estado;
                empleado.FechaCreacion = existente.FechaCreacion;
                empleado.FechaActualizacion = _proveedorTiempo.GetUtcNow().UtcDateTime;
                await _empleadoRepositorio.ActualizarAsync(empleado, tokenCancelacion);

                _registro.LogInformation("Empleado {EmpleadoId} actualizado ({Tipo}).", empleado.Id, empleado.Tipo);
                return ARespuesta(empleado, departamento.Nombre);
            },
            tokenCancelacion);
    }

    public Task DesactivarAsync(Guid id, CancellationToken tokenCancelacion) =>
        _cerrojo.EjecutarAsync(
            async () =>
            {
                var empleado = await ObtenerExistenteAsync(id, tokenCancelacion);
                empleado.Desactivar(_proveedorTiempo.GetUtcNow().UtcDateTime);
                await _empleadoRepositorio.ActualizarAsync(empleado, tokenCancelacion);

                _registro.LogInformation("Empleado {EmpleadoId} desactivado.", empleado.Id);
            },
            tokenCancelacion);

    private async Task<Empleado> ObtenerExistenteAsync(Guid id, CancellationToken tokenCancelacion) =>
        await _empleadoRepositorio.ObtenerPorIdAsync(id, tokenCancelacion)
        ?? throw new RecursoNoEncontradoException(NombresRecursos.EMPLEADO, id);

    private async Task<Departamento> ObtenerDepartamentoActivoAsync(Guid departamentoId, CancellationToken tokenCancelacion)
    {
        var departamento = await _departamentoRepositorio.ObtenerPorIdAsync(departamentoId, tokenCancelacion);
        if (departamento is { EstaActivo: true })
        {
            return departamento;
        }

        throw new ValidacionSolicitudException(new Dictionary<string, string[]>
        {
            [nameof(GuardarEmpleadoSolicitud.DepartamentoId)] = [MensajesServicio.DEPARTAMENTO_NO_DISPONIBLE]
        });
    }

    private async Task ValidarNumeroSeguroSocialUnicoAsync(string numeroSeguroSocial, Guid? idExcluido, CancellationToken tokenCancelacion)
    {
        if (await _empleadoRepositorio.ExisteNumeroSeguroSocialAsync(numeroSeguroSocial.Trim(), idExcluido, tokenCancelacion))
        {
            throw new ConflictoException(MensajesServicio.NUMERO_SEGURO_SOCIAL_DUPLICADO);
        }
    }

    private async Task<IReadOnlyDictionary<Guid, string>> ObtenerNombresDepartamentosAsync(CancellationToken tokenCancelacion)
    {
        var departamentos = await _departamentoRepositorio.ListarAsync(incluirInactivos: true, tokenCancelacion);
        return departamentos.ToDictionary(departamento => departamento.Id, departamento => departamento.Nombre);
    }

    private EmpleadoRespuesta ARespuesta(Empleado empleado, IReadOnlyDictionary<Guid, string> nombresDepartamentos) =>
        ARespuesta(empleado, nombresDepartamentos.GetValueOrDefault(empleado.DepartamentoId, string.Empty));

    private EmpleadoRespuesta ARespuesta(Empleado empleado, string nombreDepartamento) =>
        new(
            empleado.Id,
            empleado.PrimerNombre,
            empleado.ApellidoPaterno,
            empleado.NombreCompleto,
            empleado.NumeroSeguroSocial,
            empleado.DepartamentoId,
            nombreDepartamento,
            empleado.Tipo,
            empleado.Estado,
            _fabricaEmpleado.ObtenerCompensacion(empleado),
            empleado.CalcularPagoSemanal().ARespuesta(),
            empleado.FechaCreacion,
            empleado.FechaActualizacion);
}
