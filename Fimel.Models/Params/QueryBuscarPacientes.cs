using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models.Params
{
    public class QueryBuscarPacientes
    {
        public int? UsuarioId { get; set; }
        public string? NumDoc { get; set; }
        public int? Rut { get; set; }
        public DateTime? FechaConsultaDesde { get; set; }
        public DateTime? FechaConsultaHasta { get; set; }
    }
}
