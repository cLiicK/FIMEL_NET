using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BitacoraMensajeriasController : ControllerBase
    {
        private FimelDbContext db;
        public BitacoraMensajeriasController(FimelDbContext context)
        {
            db = context;
        }

        [HttpPost]
        public IActionResult Post(BitacoraMensajerias bitacora)
        {
            try
            {

                bitacora.Vigente = "S";
                bitacora.FechaCreacion = DateTime.Now;

                db.BitacoraMensajerias.Add(bitacora);
                db.SaveChanges();
 
                return Ok(bitacora);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error BitacoraMensajerias: {ex}");
                return StatusCode(500, ex);
            }
        }
    }
}
