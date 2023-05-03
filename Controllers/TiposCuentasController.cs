using Dapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController: Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas,IServicioUsuarios servicioUsuarios)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
        }

        public async Task<IActionResult> Index()
        {
            var userId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(userId);
            return View(tiposCuentas);
        }

        public IActionResult Crear()
        {
                return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipocuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipocuenta);
            }
            tipocuenta.UsuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuentaExistente = await repositorioTiposCuentas.Existe(tipocuenta.Nombre, tipocuenta.UsuarioId);
            if (tipoCuentaExistente)
            {
                ModelState.AddModelError(nameof(tipocuenta.Nombre),$"El nombre {tipocuenta.Nombre} ya existe");
                return View(tipocuenta);
            }
            await repositorioTiposCuentas.Crear(tipocuenta);
            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
                return RedirectToAction("No encontrado", "Home");

            return View(tipoCuenta);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuentaExiste = await repositorioTiposCuentas.ObtenerPorId(tipoCuenta.Id, usuarioId);

            if (tipoCuenta is null)
                return RedirectToAction("No encontrado", "Home");

            await repositorioTiposCuentas.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre,int id)
        {
            var userId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuentaExistente = await repositorioTiposCuentas.Existe(nombre, userId,id);

            if (tipoCuentaExistente)
            {
                return Json($"El nombre {nombre} ya existe");
            }
            return Json(true);
        }

        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if (tipoCuenta is null)
                return RedirectToAction("NoEncontrado", "Home");

            return View(tipoCuenta);
        }
        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if (tipoCuenta is null)
                return RedirectToAction("NoEncontrado", "Home");

            await repositorioTiposCuentas.Borrar(id);
            return RedirectToAction("Index");

        }

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var userId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(userId);
            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);

            var idsTiposCuentasNoPertenecen = ids.Except(idsTiposCuentas).ToList();

            if (idsTiposCuentasNoPertenecen.Count > 0)
                return Forbid();

            var tiposCuentasOrdenados = ids.Select((valor, indice) => new TipoCuenta()
            {
                Id = valor,
                Orden = indice + 1
            }).AsEnumerable();

            await repositorioTiposCuentas.Ordenar(tiposCuentasOrdenados);
            return Ok();
        }


    }
}
