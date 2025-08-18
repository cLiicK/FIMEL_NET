using System.Text.Json;
using Fimel.Models;
using Fimel.Site.ViewModels;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;
using static Fimel.Models.Enums;

namespace Fimel.Site.Controllers
{
    public class HorarioController : Controller
    {
        private static IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private readonly APIClient APIBase = new APIClient(config["API_URL"]);

        public IActionResult MiHorario()
        {
            Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

            MiHorarioVM vm = new MiHorarioVM();
            vm.HorariosAtencion = APIBase.Get<List<HorarioAtencion>>($"HorariosAtencion/GetByUser/{usuario.Id}");
            vm.ConfiguracionUsuario = APIBase.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByUser/{usuario.Id}");

            if (usuario.IdPerfil == (int)EnumPerfiles.Administrativo)
            {
                List<Usuarios> listaUsuarios = APIBase.Get<List<Usuarios>>($"Usuarios/GetByInstitucion/{usuario.IdInstitucion}");
                vm.ListaUsuarios = listaUsuarios.Where(t => t.IdPerfil == (int)EnumPerfiles.Especialista).Select(u => new UsuarioVM
                                                    {
                                                        Id = u.Id,
                                                        NombreCompleto = u.Nombres + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno
                                                    }).ToList();
            }

            HttpContext.Session.SetString("HorarioUser", JsonSerializer.Serialize(vm.HorariosAtencion));

            return View(vm);
        }

        public ActionResult ObtenerCitas(int? idUsuario = null)
        {
            Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
            
            // Determinar de qué usuario obtener las citas
            int idUsuarioFinal;
            if (idUsuario.HasValue && usuario.IdPerfil == (int)EnumPerfiles.Administrativo)
            {
                // Si es administrativo y se especifica un usuario, usar ese
                idUsuarioFinal = idUsuario.Value;
            }
            else
            {
                // Si es especialista o no se especifica, usar el usuario conectado
                idUsuarioFinal = usuario.Id;
            }

            List<Cita> citas = APIBase.Get<List<Cita>>($"Citas/GetByUser/{idUsuarioFinal}");

            var jsonEventos = new List<object>();

            foreach (var cita in citas)
            {
                jsonEventos.Add(new
                {
                    title = cita.NombrePaciente,
                    start = cita.FechaHoraInicio.ToString("yyyy-MM-dd HH:mm:ss"),
                    end = cita.FechaHoraFinal.ToString("yyyy-MM-dd HH:mm:ss"),
                    backgroundColor = "#75BDE0",
                    borderColor = "#75BDE0",
                    extendedProps = new
                    {
                        idCita = cita.Id,
                        nombre = cita.NombrePaciente,
                        correo = cita.CorreoPaciente,
                        telefono = cita.Telefono,
                        nota = cita.Nota,
                        documento = cita.TipoDocumento,
                        numDocumento = cita.TipoDocumento == "RUT" ? Utileria.FormatearRutSinDv(Convert.ToInt32(cita.NumeroDocumento)) : cita.NumeroDocumento,
                        horaInicio = cita.FechaHoraInicio.ToString("HH:mm"),
                        horaFinal = cita.FechaHoraFinal.ToString("HH:mm")
                    }
                });
            }

            // Obtener horarios del usuario correspondiente
            List<HorarioAtencion> horarios = APIBase.Get<List<HorarioAtencion>>($"HorariosAtencion/GetByUser/{idUsuarioFinal}");

            foreach (var horario in horarios)
            {
                jsonEventos.Add(new
                {
                    title = "",
                    daysOfWeek = new List<int> { new Utileria().ConvertirDiaSemanaANumero(horario.DiaSemana) },
                    startTime = horario.HoraInicio.ToString(@"hh\:mm"),
                    endTime = horario.HoraFin.ToString(@"hh\:mm"),
                    display = "background",
                    backgroundColor = "#F89B9B",
                });
            }

            return Json(jsonEventos);
        }

        public ActionResult _CrearBloqueHorario(HorarioAtencion horario, int? idUsuarioDestino = null)
        {
            try
            {
                Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
                
                // Determinar para qué usuario se crea el bloque de horario
                int idUsuarioFinal;
                if (idUsuarioDestino.HasValue && usuario.IdPerfil == (int)EnumPerfiles.Administrativo)
                {
                    // Si es administrativo y se especifica un usuario destino, usar ese
                    idUsuarioFinal = idUsuarioDestino.Value;
                }
                else
                {
                    // Si es especialista o no se especifica destino, usar el usuario conectado
                    idUsuarioFinal = usuario.Id;
                }

                // Obtener el usuario destino completo para asignarlo al horario
                Usuarios usuarioDestino = APIBase.Get<Usuarios>($"Usuarios/{idUsuarioFinal}");
                horario.Usuario = usuarioDestino;

                HorarioAtencion? horarioPost = APIBase.Post<HorarioAtencion>($"HorariosAtencion", horario);

                if (horarioPost == null)
                    return Json(new { success = false, message = "Error interno al guardar Bloque..." });

                return Json(new { success = true, message = "Bloque Guardado!" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Horario _CrearBloqueHorario: {ex}");
                return null;
            }
        }

        public ActionResult _EliminarBloqueHorario(int id)
        {
            try
            {
                HorarioAtencion horarioAtencion = APIBase.Get<HorarioAtencion>($"HorariosAtencion/{id}");
                horarioAtencion.Vigente = "N";
                HorarioAtencion horarioAtencionPut = APIBase.Put<HorarioAtencion>($"HorariosAtencion/{id}", horarioAtencion);

                if (horarioAtencionPut == null)
                    return Json(new { success = false, message = "Error al eliminar el Horario de Atencion" });

                return Json(new { success = true, message = "Bloque de atención eliminado" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Horario _EliminarBloqueHorario: {ex}");
                return null;
            }
        }

        public ActionResult _EliminarCita(int id)
        {
            try
            {
                Cita cita = APIBase.Get<Cita>($"Citas/{id}");
                cita.Vigente = "N";
                Cita citaPut = APIBase.Put<Cita>($"Citas/{id}", cita);

                if (citaPut == null)
                    return Json(new { success = false, message = "Error al eliminar cita" });

                return Json(new { success = true, message = "Cita eliminada" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Horario _EliminarCita: {ex}");
                return null;
            }
        }

        public ActionResult _GuardarCita(Cita cita, int? idUsuarioDestino = null)
        {
            try
            {
                Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
                
                // Determinar para qué usuario se crea la cita
                int idUsuarioFinal;
                if (idUsuarioDestino.HasValue && usuario.IdPerfil == (int)EnumPerfiles.Administrativo)
                {
                    // Si es administrativo y se especifica un usuario destino, usar ese
                    idUsuarioFinal = idUsuarioDestino.Value;
                }
                else
                {
                    // Si es especialista o no se especifica destino, usar el usuario conectado
                    idUsuarioFinal = usuario.Id;
                }

                List<Cita> dbCita = APIBase.Get<List<Cita>>($"Citas/GetByCriteria", new
                {
                    FechaInicio = cita.FechaHoraInicio,
                    FechaTermino = cita.FechaHoraFinal,
                    UsuarioId = idUsuarioFinal
                });

                if (dbCita.Count > 0)
                    return Json(new { success = false, message = "Ya existe una cita agendada que topa con el Día y Hora" });

                // Obtener el usuario destino completo para asignarlo a la cita
                Usuarios usuarioDestino = APIBase.Get<Usuarios>($"Usuarios/{idUsuarioFinal}");
                cita.Usuario = usuarioDestino;
                
                Cita citaPost = APIBase.Post<Cita>($"Citas", cita);

                if (citaPost == null)
                    return Json(new { success = false, message = "Error al crear cita" });

                return Json(new { success = true, message = "Cita creada" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Horario _GuardarCita: {ex}");
                return null;
            }
        }

        [HttpGet]
        public IActionResult ObtenerVistaAgenda(int idUsuario)
        {
            try
            {
                var horarios = APIBase.Get<List<HorarioAtencion>>($"HorariosAtencion/GetByUser/{idUsuario}");
                var configUsuario = APIBase.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByUser/{idUsuario}");

                var vm = new MiHorarioVM
                {
                    HorariosAtencion = horarios,
                    ConfiguracionUsuario = configUsuario
                };

                // Guardar el ID del usuario seleccionado en la sesión para usar en las operaciones
                HttpContext.Session.SetString("UsuarioSeleccionado", idUsuario.ToString());

                return PartialView("_PartialContenedorAgenda", vm);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error ObtenerVistaAgenda: {ex}");
                return StatusCode(500);
            }
        }

    }
}
