using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class Config
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string? Descripcion { get; set; }
    }
}
