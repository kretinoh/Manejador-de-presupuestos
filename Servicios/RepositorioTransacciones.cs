using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int annyo);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
    }
    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string connectionString;
        private readonly IConfiguration configuration;

        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(
            ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@"
            SELECT DATEDIFF(d,@fechaInicio,FechaTransaccion) / 7 + 1 as Semana,
            SUM(Monto) AS Monto, C.TipoOperacionId
            FROM Transacciones
            INNER JOIN Categorias C
            ON C.Id = Transacciones.CategoriaId
            WHERE Transacciones.UsuarioId = @usuarioId AND 
            FechaTransaccion BETWEEN @fechaInicio AND @fechaFin
            GROUP BY DATEDIFF(d,@fechaInicio,FechaTransaccion) / 7, C.TipoOperacionId", modelo);
        }

        public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int annyo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorMes>(@"
            SELECT MONTH(FechaTransaccion) as Mes,
            SUM(Monto) as Monto,C.TipoOperacionId
            FROM Transacciones
            INNER JOIN Categorias C
            ON C.Id = Transacciones.CategoriaId
            WHERE Transacciones.UsuarioId = @usuarioId AND YEAR(FechaTransaccion) = @annyo
            GROUP BY Month(FechaTransaccion), C.TipoOperacionId
            ", new {usuarioId,annyo});
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"Transacciones_Insertar",
                        new
                        {
                            transaccion.UsuarioId,
                            transaccion.FechaTransaccion,
                            transaccion.Monto,
                            transaccion.CategoriaId,
                            transaccion.CuentaId,
                            transaccion.Nota
                        }, commandType: CommandType.StoredProcedure);

            transaccion.Id = id;
        }
        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(
                @"SELECT T.Id,T.Monto,T.FechaTransaccion,C.Nombre as Categoria,A.Nombre as Cuenta,C.TipoOperacionId,Nota
                    FROM Transacciones T INNER JOIN Categorias C ON C.Id = T.CategoriaId INNER JOIN Cuentas A ON
                    A.Id = T.CuentaId WHERE T.UsuarioId = @UsuarioId AND FechaTransaccion 
                    BETWEEN @FechaInicio AND @FechaFin ORDER BY T.FechaTransaccion DESC", modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(
                @"SELECT T.Id,T.Monto,T.FechaTransaccion,C.Nombre as Categoria,A.Nombre as Cuenta,C.TipoOperacionId
                    FROM Transacciones T INNER JOIN Categorias C ON C.Id = T.CategoriaId INNER JOIN Cuentas A ON
                    A.Id = T.CuentaId WHERE T.CuentaId = @CuentaId AND T.UsuarioId = @UsuarioId AND FechaTransaccion 
                    BETWEEN @FechaInicio AND @FechaFin", modelo);
        }

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"Transacciones_Actualizar",
                new
                {
                    transaccion.Id,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota,
                    montoAnterior,
                    cuentaAnteriorId
                }, commandType: CommandType.StoredProcedure);
        }
        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(
                @"      SELECT T.*,C.TipoOperacionId 
                        FROM Transacciones T
                        INNER JOIN Categorias C 
                        ON C.Id = T.CategoriaId 
                        WHERE T.Id = @Id 
                        AND T.UsuarioId = @UsuarioId",
                        new { id, usuarioId });
        }

       
        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"Transacciones_Borrar",
                new { id }, commandType: CommandType.StoredProcedure);
        }
    }
}
