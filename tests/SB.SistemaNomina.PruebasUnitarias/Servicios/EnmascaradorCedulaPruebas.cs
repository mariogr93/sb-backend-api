using SB.SistemaNomina.Servicios.Empleados;

namespace SB.SistemaNomina.PruebasUnitarias.Servicios;

public class EnmascaradorCedulaPruebas
{
    [Theory]
    [InlineData("123-1234567-1", "***-****567-1")]
    [InlineData("001-0000000-0", "***-****000-0")]
    public void CedulaValidaMuestraSoloUltimosDigitos(string cedula, string esperada)
    {
        Assert.Equal(esperada, EnmascaradorCedula.Enmascarar(cedula));
    }

    [Theory]
    [InlineData("100-00-0001", "***-**-****")]
    [InlineData("12345", "*****")]
    [InlineData("", "")]
    public void FormatoInesperadoOcultaTodosLosDigitos(string cedula, string esperada)
    {
        Assert.Equal(esperada, EnmascaradorCedula.Enmascarar(cedula));
    }
}
