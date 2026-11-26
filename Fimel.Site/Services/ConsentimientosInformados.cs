using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Fimel.Models;

namespace Fimel.Site.Services
{
    /// <summary>
    /// Textos y armado HTML de los consentimientos informados (DIU/SIU e implante subdérmico).
    /// Es la única fuente de los textos: se usa para imprimir, para la página de firma con QR
    /// y para ver un consentimiento ya firmado.
    /// </summary>
    public static class ConsentimientosInformados
    {
        public class Item
        {
            public string Texto { get; init; } = "";
            public string[] Riesgos { get; init; } = Array.Empty<string>();
            public static implicit operator Item(string texto) => new() { Texto = texto };
        }

        public class Tipo
        {
            public string Codigo { get; init; } = "";
            public string Nombre { get; init; } = "";
            public string Titulo { get; init; } = "";
            public string Rol { get; init; } = "";
            public string PlaceholderDispositivo { get; init; } = "";
            public bool Observaciones { get; init; }
            /// <summary>Orden de las firmas: profesional, usuaria, representante.</summary>
            public string[] Firmas { get; init; } = Array.Empty<string>();
            public Item[] Items { get; init; } = Array.Empty<Item>();
        }

        /// <summary>Datos con que se llena un consentimiento.</summary>
        public class Datos
        {
            public string? Paciente { get; set; }
            public string? Rut { get; set; }
            public string? Domicilio { get; set; }
            public string? Fecha { get; set; }
            public string? Profesional { get; set; }
            public string? Dispositivo { get; set; }
            public string? Observaciones { get; set; }
        }

        private const string Ley20584 = "Además, acorde a la Ley Nacional 20.584, autorizo la creación de una base de datos a partir " +
            "de información de mi historia clínica, consejería y el examen con fines académicos y de " +
            "investigación, resguardando en todo momento la confidencialidad de mi información " +
            "personal y sin menoscabo de mi atención ni menos sanción alguna.";

        public static readonly IReadOnlyList<Tipo> Tipos = new List<Tipo>
        {
            new()
            {
                Codigo = "InsercionDIU",
                Nombre = "Inserción de DIU / SIU",
                Titulo = "Consentimiento Informado para Inserción de Dispositivo Intrauterino",
                Rol = "consultante o apoderada/o/e",
                PlaceholderDispositivo = "Ej: T de Cobre, Mirena, Kyleena",
                Observaciones = true,
                Firmas = new[] { "profesional", "usuaria", "representante" },
                Items = new Item[]
                {
                    "Que el profesional {profesional} me ha explicado de forma clara, en qué consiste el procedimiento de inserción del dispositivo o sistema intrauterino ({dispositivo}).",
                    new()
                    {
                        Texto = "Dentro de sus complicaciones y riesgos, se me ha informado que:",
                        Riesgos = new[]
                        {
                            "El procedimiento de inserción puede resultar doloroso para la mayoría de las mujeres.",
                            "Existe un riesgo de expulsión que es menor al 1%, principalmente los primeros 3 meses.",
                            "Existe un riesgo de infección pélvica del 1%.",
                            "Existe un riesgo de incrustación de 1 en 10.000 usuarias.",
                            "Existe un riesgo de perforación uterina alrededor del 1,5%.",
                            "Existe igualmente, riesgo de lipotimia, acompañada de mareos y/o náuseas.",
                            "Existe la posibilidad de que el DIU/SIU descienda y tenga que ser retirado, sin opción de reutilizarlo ni ser reembolsado."
                        }
                    },
                    "El examen pélvico y/o ecográfico, no garantiza el éxito de la inserción.",
                    "Que he recibido consejería y he elegido de forma consciente e informada este método anticonceptivo.",
                    "Declaro que toda la información entregada al profesional es completa y fidedigna.",
                    "Me han sido entregadas las indicaciones y cuidados que debo tener post inserción del dispositivo o sistema intrauterino.",
                    "Consiento de manera libre y voluntaria que se me realice el procedimiento de inserción del dispositivo o sistema intrauterino.",
                    Ley20584
                }
            },
            new()
            {
                Codigo = "ExtraccionDIU",
                Nombre = "Extracción de DIU / SIU",
                Titulo = "Consentimiento Informado para Extracción de Dispositivo Intrauterino",
                Rol = "consultante o apoderada/o/e",
                PlaceholderDispositivo = "Ej: T de Cobre, Mirena, Kyleena",
                Observaciones = true,
                Firmas = new[] { "profesional", "usuaria", "representante" },
                Items = new Item[]
                {
                    "Que el profesional {profesional} me ha explicado de forma clara, en qué consiste el procedimiento de extracción del dispositivo o sistema intrauterino ({dispositivo}).",
                    "Me han sido entregadas las indicaciones y cuidados que debo tener post extracción del dispositivo o sistema intrauterino.",
                    "Consiento de manera libre y voluntaria que se me realice el procedimiento de extracción del dispositivo o sistema intrauterino.",
                    Ley20584
                }
            },
            new()
            {
                Codigo = "InsercionImplante",
                Nombre = "Inserción de implante subdérmico",
                Titulo = "Consentimiento Informado para Inserción de Implante Subdérmico",
                Rol = "paciente o apoderado",
                PlaceholderDispositivo = "Ej: Implanon NXT, Jadelle",
                Observaciones = false,
                Firmas = new[] { "usuaria", "profesional", "representante" },
                Items = new Item[]
                {
                    "Que el profesional {profesional} me ha explicado de forma clara, en qué consiste el procedimiento de inserción del implante subdérmico ({dispositivo}), sus complicaciones y riesgos.",
                    "Que he recibido consejería, resuelto dudas y he elegido de forma consciente e informada este método anticonceptivo.",
                    "Me han sido entregadas las indicaciones y cuidados que debo tener post inserción del implante.",
                    "Consiento de manera libre y voluntaria que se me realice el procedimiento de inserción del implante subdérmico.",
                    "Dentro de sus complicaciones y riesgos, se me ha informado que: existe un riesgo de que, ante una cicatrización no adecuada, puede haber escape del implante. Existe riesgo de que el implante migre a la circulación venosa, por lo cual se me ha informado debo asistir a control en un mes o antes si noto que el implante se ha desplazado de la zona de inserción. Existe la posibilidad de presentar hematoma y/o moretones en la zona intervenida durante los días posteriores al procedimiento.",
                    Ley20584,
                    "Declaro que toda la información entregada por mí, al profesional tratante es completa y fidedigna, tengo la certeza de que no estoy embarazada y que no soy alérgica al anestésico local.",
                    "Me comprometo a asistir a los controles posteriores, de la semana, del mes y luego anual. Y a consultar en caso de necesidad fuera de estos tiempos establecidos."
                }
            },
            new()
            {
                Codigo = "RetiroImplante",
                Nombre = "Retiro de implante subdérmico",
                Titulo = "Consentimiento Informado para Retiro de Implante Subdérmico",
                Rol = "consultante o apoderado",
                PlaceholderDispositivo = "Ej: Implanon NXT, Jadelle",
                Observaciones = false,
                Firmas = new[] { "usuaria", "profesional", "representante" },
                Items = new Item[]
                {
                    new()
                    {
                        Texto = "Que el profesional {profesional} me ha explicado de forma clara, en qué consiste el procedimiento de retiro del implante subdérmico ({dispositivo}), sus complicaciones y riesgos, tales como:",
                        Riesgos = new[]
                        {
                            "Que la zona intervenida presente hematoma y/o “moretones” durante los días posteriores al procedimiento.",
                            "Existe un riesgo de que pueda haber infección en la zona intervenida.",
                            "En algunos casos, puede ser necesaria una segunda incisión para el éxito del procedimiento.",
                            "En algunos casos, es posible que no se pueda extraer el/los implante/es."
                        }
                    },
                    "Al término de la consulta, se me hará la entrega de las indicaciones y cuidados que debo tener posterior al procedimiento.",
                    "Consiento de manera libre y voluntaria que se me realice el procedimiento de retiro del implante subdérmico.",
                    "Declaro que toda la información entregada al profesional es completa y fidedigna.",
                    Ley20584
                }
            }
        };

        public static Tipo? ObtenerTipo(string? codigo) =>
            Tipos.FirstOrDefault(t => string.Equals(t.Codigo, codigo, StringComparison.OrdinalIgnoreCase));

        private static readonly HtmlEncoder Encoder = HtmlEncoder.Create(UnicodeRanges.All);
        private static string E(string? texto) => Encoder.Encode(texto ?? "");

        // Valor subrayado; si viene vacío queda la línea en blanco para completarla a mano.
        private static string Campo(string? valor, string clase = "") =>
            $"<span class=\"campo {clase}\">{(string.IsNullOrWhiteSpace(valor) ? "&nbsp;" : E(valor))}</span>";

        private static string RenderTexto(string texto, Datos d) =>
            E(texto)
                .Replace(E("{profesional}"), Campo(d.Profesional, "largo"))
                .Replace(E("{dispositivo}"), Campo(d.Dispositivo));

        /// <summary>
        /// Cuerpo del documento (título, declaración, puntos y observaciones), sin encabezado ni firmas.
        /// Es lo que se guarda al crear un consentimiento para firma digital.
        /// </summary>
        public static string RenderCuerpo(Tipo tipo, Datos d)
        {
            var sb = new StringBuilder();
            sb.Append($"<h1>{E(tipo.Titulo)}</h1>");
            sb.Append("<p class=\"declaracion\">");
            sb.Append($"Yo {Campo(d.Paciente, "largo")} RUT {Campo(d.Rut)} con domicilio en {Campo(d.Domicilio, "largo")} ");
            sb.Append($"como {E(tipo.Rol)}, con fecha {Campo(d.Fecha, "corto")}, declaro:");
            sb.Append("</p><ol class=\"items\">");
            foreach (Item item in tipo.Items)
            {
                sb.Append("<li>").Append(RenderTexto(item.Texto, d));
                if (item.Riesgos.Length > 0)
                {
                    sb.Append("<ul class=\"riesgos\">");
                    foreach (string r in item.Riesgos) sb.Append("<li>").Append(E(r)).Append("</li>");
                    sb.Append("</ul>");
                }
                sb.Append("</li>");
            }
            sb.Append("</ol>");

            if (tipo.Observaciones || !string.IsNullOrWhiteSpace(d.Observaciones))
            {
                sb.Append("<div class=\"observaciones\"><span class=\"lbl\">Observaciones:</span>");
                sb.Append(string.IsNullOrWhiteSpace(d.Observaciones)
                    ? "<div class=\"linea\"></div><div class=\"linea\"></div>"
                    : $"<div class=\"texto\">{E(d.Observaciones)}</div>");
                sb.Append("</div>");
            }
            return sb.ToString();
        }

        public static string RenderEncabezado(string? logoBase64, string? institucion, string urlSitio)
        {
            string logoUrl = string.IsNullOrEmpty(logoBase64)
                ? $"{urlSitio}img/logo_fimel_correo.png"
                : $"data:image/png;base64,{logoBase64}";
            return "<div class=\"header\">" +
                   $"<div class=\"header-logo\"><img src=\"{E(logoUrl)}\" alt=\"Logo\" onerror=\"this.style.display='none'\"></div>" +
                   $"<div class=\"header-right\">{E(institucion)}</div>" +
                   "</div><div class=\"divider-salmon\"></div>";
        }

        /// <summary>Fila de firmas. Si el consentimiento está firmado digitalmente, la firma va sobre la línea que corresponde.</summary>
        public static string RenderFirmas(Tipo tipo, ConsentimientoFirma c)
        {
            bool firmado = c.Estado == "Firmado" && !string.IsNullOrEmpty(c.FirmaPaciente);
            bool firmaRepresentante = firmado && c.TipoFirmante == "Representante";
            string Img(string? b64) => string.IsNullOrEmpty(b64) ? "" : $"<img class=\"firma-img\" src=\"data:image/png;base64,{E(b64)}\" alt=\"Firma\">";

            string titulo = c.TituloProfesional ?? "";
            string etiquetaProfesional = string.IsNullOrWhiteSpace(titulo) || titulo.Contains("matr", StringComparison.OrdinalIgnoreCase)
                ? "Matrona/ón" : titulo;

            var sb = new StringBuilder("<table class=\"firmas\"><tr>");
            foreach (string f in tipo.Firmas)
            {
                switch (f)
                {
                    case "profesional":
                        sb.Append($"<td><div class=\"firma-espacio\">{Img(c.FirmaProfesional)}</div>" +
                                  $"<div class=\"firma-linea\">Firma {E(etiquetaProfesional)}</div>" +
                                  $"<div class=\"firma-detalle\">{E(c.NombreProfesional)}</div>" +
                                  $"<div class=\"firma-detalle\">{E(c.RutProfesional)}</div></td>");
                        break;
                    case "usuaria":
                        sb.Append($"<td><div class=\"firma-espacio\">{(firmado && !firmaRepresentante ? Img(c.FirmaPaciente) : "")}</div>" +
                                  "<div class=\"firma-linea\">Firma Usuaria/o/e</div>" +
                                  $"<div class=\"firma-detalle\">{E(c.NombrePaciente)}</div>" +
                                  $"<div class=\"firma-detalle\">{E(c.RutPaciente)}</div></td>");
                        break;
                    case "representante":
                        sb.Append($"<td><div class=\"firma-espacio\">{(firmaRepresentante ? Img(c.FirmaPaciente) : "")}</div>" +
                                  "<div class=\"firma-linea\">Firma Representante Legal</div>" +
                                  (firmaRepresentante
                                      ? $"<div class=\"firma-detalle\">{E(c.NombreFirmante)}</div><div class=\"firma-detalle\">{E(c.RutFirmante)}</div>"
                                      : "<div class=\"firma-rut-blanco\">Nombre:</div><div class=\"firma-rut-blanco\">RUT:</div>") +
                                  "</td>");
                        break;
                }
            }
            sb.Append("</tr></table>");
            return sb.ToString();
        }

        /// <summary>Cajita con la evidencia de la firma electrónica.</summary>
        public static string RenderEvidencia(ConsentimientoFirma c)
        {
            if (c.Estado != "Firmado") return "";
            string fecha = c.FechaFirma?.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture) ?? "";
            string firmante = c.TipoFirmante == "Representante" ? "Representante legal" : "Paciente";
            string hash = c.HashDocumento ?? "";
            string hashCorto = hash.Length >= 16
                ? string.Join("-", Enumerable.Range(0, 4).Select(i => hash.Substring(i * 4, 4)))
                : hash;

            return "<div class=\"evidencia\">" +
                   "<div class=\"evidencia-titulo\">Firmado electrónicamente</div>" +
                   "<table>" +
                   $"<tr><td class=\"lbl\">Firmado por</td><td>{E(c.NombreFirmante)}{(string.IsNullOrWhiteSpace(c.RutFirmante) ? "" : " · RUT " + E(c.RutFirmante))} ({firmante})</td></tr>" +
                   $"<tr><td class=\"lbl\">Fecha y hora</td><td>{E(fecha)} (registrada por el servidor) · IP {E(c.IpFirma)}</td></tr>" +
                   $"<tr><td class=\"lbl\">Dispositivo</td><td class=\"ua\">{E(c.UserAgentFirma)}</td></tr>" +
                   $"<tr><td class=\"lbl\">Código de verificación</td><td><b>{E(hashCorto)}</b><div class=\"hash\">SHA-256: {E(hash)}</div></td></tr>" +
                   "</table></div>";
        }

        /// <summary>Página HTML completa del consentimiento (para imprimir o ver).</summary>
        public static string RenderPagina(Tipo tipo, ConsentimientoFirma c, string? logoBase64, string urlSitio, bool imprimirAlCargar)
        {
            string script = imprimirAlCargar
                ? "<script>window.onafterprint=function(){window.close();};window.onload=function(){window.print();};</script>"
                : "";
            string barra = imprimirAlCargar
                ? ""
                : "<div class=\"barra no-print\"><button onclick=\"window.print()\">Imprimir</button></div>";

            return "<!DOCTYPE html><html lang=\"es\"><head><meta charset=\"utf-8\">" +
                   "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">" +
                   $"<title>{E(tipo.Titulo)}</title>" +
                   $"<link rel=\"stylesheet\" href=\"{E(urlSitio)}css/consentimiento.css?v=3\">" +
                   "</head><body" + (c.Estado == "Firmado" ? " class=\"firmado\"" : "") + ">" + barra + "<div class=\"page\">" +
                   RenderEncabezado(logoBase64, c.NombreInstitucion, urlSitio) +
                   (c.HtmlCuerpo ?? "") +
                   RenderFirmas(tipo, c) +
                   RenderEvidencia(c) +
                   "<div class=\"powered\">FIMEL · Sistema de Gestión Médica</div>" +
                   "</div>" + script + "</body></html>";
        }
    }
}
