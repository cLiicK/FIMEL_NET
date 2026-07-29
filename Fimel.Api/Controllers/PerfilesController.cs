using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PerfilesController : ControllerBase
    {
        private readonly FimelDbContext db;
        public PerfilesController(FimelDbContext context) => db = context;

        [HttpGet]
        [Route("GetAll")]
        public IActionResult GetAll()
        {
            try
            {
                var lista = db.Perfiles
                    .Where(x => x.Vigente == "S")
                    .OrderBy(x => x.Id)
                    .ToList();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GetAll Perfiles: {ex}");
                return StatusCode(500, ex);
            }
        }
    }
}
