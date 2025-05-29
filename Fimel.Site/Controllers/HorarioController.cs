using System.Text.Json;
using Fimel.Models;
using Fimel.Site.ViewModels;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

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

            HttpContext.Session.SetString("HorarioUser", JsonSerializer.Serialize(vm.HorariosAtencion));

            return View(vm);
        }

        public ActionResult ObtenerCitas()
        {
            Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

            List<Cita> citas = APIBase.Get<List<Cita>>($"Citas/GetByUser/{usuario.Id}");

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

            List<HorarioAtencion> horarios = new Utileria().ObtenerData<List<HorarioAtencion>>(HttpContext.Session.GetString("HorarioUser"));

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

        public ActionResult _CrearBloqueHorario(HorarioAtencion horario)
        {
            try
            {
                Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
                horario.Usuario = usuario;

                HorarioAtencion? horarioPost = APIBase.Post<HorarioAtencion>($"HorariosAtencion", horario);

                if (horarioPost == null)
                    return Json(new { success = true, message = "Error interno al guardar Bloque..." });

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

                if(citaPut == null)
                    return Json(new { success = false, message = "Error al eliminar cita" });

                return Json(new { success = true, message = "Cita eliminada" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Horario _EliminarCita: {ex}");
                return null;
            }
        }

        public ActionResult _GuardarCita(Cita cita)
        {
            try
            {
                Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

                List<Cita> dbCita = APIBase.Get<List<Cita>>($"Citas/GetByCriteria", new
                {
                    FechaInicio = cita.FechaHoraInicio,
                    FechaTermino = cita.FechaHoraFinal,
                    UsuarioId = usuario.Id
                });

                if (dbCita.Count > 0)
                    return Json(new { success = false, message = "Ya existe una cita agendada que topa con el Día y Hora" });

                cita.Usuario = usuario;
                Cita citaPost = APIBase.Post<Cita>($"Citas", cita);

                if (citaPost == null)
                    return Json(new { success = false, message = "Error al crear cita" });

                return Json(new { success = true, message = "Cita creada" });
            }
            catch(Exception ex)
            {
                Logger.Log($"Error Horario _GuardarCita: {ex}");
                return null;
            }
        }

    }
}
