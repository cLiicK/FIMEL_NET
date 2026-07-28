using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModulosPerfilesController : ControllerBase
    {
        private readonly FimelDbContext db;
        public ModulosPerfilesController(FimelDbContext context) => db = context;

        [HttpGet]
        [Route("GetAll")]
        public IActionResult GetAll()
        {
            try
            {
                var pares = db.ModuloPerfil
                    .Where(x => x.Vigente == "S")
                    .ToList();

                var modulos = db.Modulos.Where(x => x.Vigente == "S").ToList();
                var perfiles = db.Perfiles.Where(x => x.Vigente == "S").ToList();

                foreach (var par in pares)
                {
                    par.Modulo = modulos.FirstOrDefault(m => m.Id == par.ModuloId);
                    par.Perfil = perfiles.FirstOrDefault(p => p.Id == par.PerfilId);
                }

                return Ok(pares);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GetAll ModulosPerfiles: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(ModuloPerfil item)
        {
            try
            {
                var existente = db.ModuloPerfil
                    .FirstOrDefault(x => x.ModuloId == item.ModuloId && x.PerfilId == item.PerfilId);

                if (existente != null)
                {
                    if (existente.Vigente != "S")
                    {
                        existente.Vigente = "S";
                        db.SaveChanges();
                    }
                    return Ok(existente);
                }

                item.Id = 0;
                item.Vigente = "S";
                item.FechaCreacion = DateTime.Now;
                db.ModuloPerfil.Add(item);
                db.SaveChanges();
                return Ok(item);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error POST ModuloPerfil: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var db_ = db.ModuloPerfil.Find(id);
                if (db_ == null) return NotFound();
                db_.Vigente = "N";
                db.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error DELETE ModuloPerfil: {ex}");
                return StatusCode(500, ex);
            }
        }
    }
}
