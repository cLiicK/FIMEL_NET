var configuracionUsuario = $('#ObjectConfigUsuario').val() ? JSON.parse($('#ObjectConfigUsuario').val()) : null;

var ModuloHorarioCita = (function () {
    return {
        IniciarScripts: function () {
            ModuloHorarioCita.IniciarCalendario();

            const hoyLocal = new Date();
            const anio = hoyLocal.getFullYear();
            const mes = String(hoyLocal.getMonth() + 1).padStart(2, '0');
            const dia = String(hoyLocal.getDate()).padStart(2, '0');
            const fechaChile = `${anio}-${mes}-${dia}`;

            const inputFecha = $('#fechaCitaNueva');
            inputFecha.val(fechaChile);
            inputFecha.attr('min', fechaChile); // impide seleccionar fechas anteriores

            $('#horaInicioNuevoBloque').change(function () {
                let value = $('#horaInicioNuevoBloque').val();
                let minutes = parseInt(value.split(':')[1]);
                if (minutes !== 0 && minutes !== 30) {
                    let newMinutes = (minutes < 30) ? "00" : "30";
                    let newValue = value.split(':')[0] + ":" + newMinutes;
                    $('#horaInicioNuevoBloque').val(newValue);
                }
            });

            $('#horaFinNuevoBloque').change(function () {
                let value = $('#horaFinNuevoBloque').val();
                let minutes = parseInt(value.split(':')[1]);
                if (minutes !== 0 && minutes !== 30) {
                    let newMinutes = (minutes < 30) ? "00" : "30";
                    let newValue = value.split(':')[0] + ":" + newMinutes;
                    $('#horaFinNuevoBloque').val(newValue);
                }
            });

            $('#selectProfesional').off('change').on('change', function () {
                var idUsuario = $(this).val();
                if (idUsuario) {
                    // Guardar el ID del usuario seleccionado
                    $('#hdnUsuarioSeleccionado').val(idUsuario);
                    
                    $.get('/Horario/ObtenerVistaAgenda', { idUsuario: idUsuario }, function (html) {
                        $('#contenedorAgenda').html(html).show();
                        ModuloHorarioCita.IniciarScripts();
                    }).fail(function () {
                        Swal.fire("Error", "No se pudo cargar la agenda del profesional", "error");
                    });
                } else {
                    $('#contenedorAgenda').hide().html('');
                    $('#hdnUsuarioSeleccionado').val('');
                }
            });

        },
        IniciarCalendario: function () {
            var calendarEl = document.getElementById('calendar');
            var calendar = new FullCalendar.Calendar(calendarEl, {
                initialView: 'timeGridWeek',
                locale: 'es',
                timeZone: 'local',
                slotDuration: configuracionUsuario ? configuracionUsuario.DuracionBloqueHorario : '00:30:00',
                firstDay: 1,
                headerToolbar: {
                    left: 'prev,next today',
                    center: 'title',
                    right: 'timeGridWeek,timeGridDay'
                },
                buttonText: {
                    today: 'Hoy',
                    month: 'Mes',
                    week: 'Semana',
                    day: 'Día',
                    list: 'Lista'
                },
                allDaySlot: false,
                events: function(info, successCallback, failureCallback) {
                    var url = $('#hdnURL_ObtenerCitas').val();
                    var idUsuario = $('#hdnUsuarioSeleccionado').val();
                    
                    if (idUsuario) {
                        url += '?idUsuario=' + idUsuario;
                    }
                    
                    $.ajax({
                        url: url,
                        type: 'GET',
                        success: function(data) {
                            successCallback(data);
                        },
                        error: function() {
                            failureCallback('Error al cargar eventos');
                        }
                    });
                },
                eventTimeFormat: {
                    hour: '2-digit',
                    minute: '2-digit',
                    hour12: false
                },
                titleFormat: {
                    month: 'long',
                    year: 'numeric'
                },
                views: {
                    timeGridWeek: {
                        titleFormat: { year: 'numeric', month: 'long' }
                    },
                    timeGridDay: {
                        titleFormat: { day: 'numeric', month: 'long', year: 'numeric' },
                        dayHeaderFormat: { weekday: 'long', day: 'numeric' }
                    }
                },
                dayHeaderFormat: { weekday: 'long', day: 'numeric' },
                selectable: true,
                eventClick: function (info) {
                    let fechaSeleccionada = new Date(info.event.startStr);
                    let formatoFecha = fechaSeleccionada.toLocaleDateString('es-ES', { day: 'numeric', month: 'long', year: 'numeric' });

                    $('#modalEventoLabel').text(formatoFecha);
                    $('#horaInicioCita').val(info.event.extendedProps.horaInicio);
                    $('#horaTerminoCita').val(info.event.extendedProps.horaFinal);
                    $('#nombreCita').val(info.event.extendedProps.nombre);
                    $('#correoCita').val(info.event.extendedProps.correo);
                    $('#telefonoCita').val(info.event.extendedProps.telefono);
                    $('#notaCita').val(info.event.extendedProps.nota);
                    $('#idCitaModal').val(info.event.extendedProps.idCita);

                    $('#modalEvento').modal('show');
                },
                select: function (info) {
                    let fechaSeleccionada = new Date(info.startStr);
                    let formatoFecha = fechaSeleccionada.toLocaleDateString('es-ES', { day: 'numeric', month: 'long', year: 'numeric' });
                    let horaSeleccionada = fechaSeleccionada.toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
                    $('#modalEventoNuevoLabel').text(formatoFecha);
                    $('#fechaModal').val(info.startStr);
                    $('#horaInicioCitaNuevo').val(horaSeleccionada);

                    $('#modalEventoNuevo').modal('show');
                },
                datesSet: function (info) {
                    const toolbarTitle = document.querySelector('.fc-toolbar-title');
                    if (info.view.type === 'timeGridWeek') {
                        let startDate = new Date(info.start);
                        let endDate = new Date(info.end);
                        let options = { year: 'numeric', month: 'long' };
                        let formattedStart = startDate.toLocaleDateString('es-ES', options);
                        let startDay = startDate.getDate();
                        let endDay = endDate.getDate() - 1;
                        let title = `${formattedStart} (${startDay} hasta ${endDay})`;

                        if (toolbarTitle) {
                            toolbarTitle.innerText = title;
                        }
                    } else if (info.view.type === 'timeGridDay') {
                        if (toolbarTitle) {
                            let dayTitle = info.start.toLocaleDateString('es-ES', { weekday: 'long', day: 'numeric', month: 'long' });
                            toolbarTitle.innerText = dayTitle;
                        }
                    } else if (info.view.type === 'dayGridMonth') {
                        const currentStartDate = info.view.currentStart;
                        if (toolbarTitle) {
                            let monthTitle = currentStartDate.toLocaleDateString('es-ES', { year: 'numeric', month: 'long' });
                            toolbarTitle.innerText = monthTitle;
                        }
                    }
                }
            });
            calendar.render();
        },
        GuardarNuevoBloque: function () {
            if (!$("#diaSemanaNuevoBloque").val()) {
                Swal.fire('Seleccione el Día', 'Nuevo Bloque', 'warning');
                return null;
            }
            if (!$("#horaInicioNuevoBloque").val()) {
                Swal.fire('Seleccione la Hora de Inicio', 'Nuevo Bloque', 'warning');
                return null;
            }
            if (!$("#horaFinNuevoBloque").val()) {
                Swal.fire('Seleccione la Hora Final', 'Nuevo Bloque', 'warning');
                return null;
            }

            var btnGuardar = $('#btnGuardarBloque');

            Swal.fire({
                title: 'Nuevo Bloque',
                text: '¿Esta seguro de crear este nuevo Bloque?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Guardar',
            }).then((result) => {
                if (result.isConfirmed) {

                    btnGuardar.prop('disabled', true);
                    btnGuardar.text('Guardando...');

                    $.ajax({
                        url: $('#hdnURL_CrearBloqueHorario').val(),
                        data: {
                            horario: {
                                Comentario: $('#comentarioNuevoBloque').val(),
                                DiaSemana: $('#diaSemanaNuevoBloque').val(),
                                HoraFin: $('#horaFinNuevoBloque').val(),
                                HoraInicio: $('#horaInicioNuevoBloque').val(),
                            },
                            idUsuarioDestino: $('#hdnUsuarioSeleccionado').val() || null
                        },
                        method: 'POST',
                        success: function (response, jqXHR) {
                            if (response.success === true) {
                                Swal.fire({
                                    title: response.message,
                                    icon: 'success',
                                }).then((result) => {
                                    if (result.isConfirmed) {
                                        location.reload();
                                    }
                                })
                            }
                            else {
                                btnGuardar.prop('disabled', false);
                                btnGuardar.text('Guardar');
                                Swal.fire('Error', response.message, 'error');
                                return;
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            Swal.fire('Error', 'Favor comuniquese con un administrador', 'error');
                            btnGuardar.prop('disabled', false);
                            btnGuardar.text('Guardar');
                            return;
                        }
                    });
                }
                else {
                    btnGuardar.prop('disabled', false);
                    btnGuardar.text('Guardar');
                }
            })
        },

        EliminarBloque: function (idBloque) {
           
            Swal.fire({
                title: 'Eliminar Bloque',
                text: '¿Esta seguro de eliminar este Bloque horario?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Eliminar',
            }).then((result) => {
                if (result.isConfirmed) {

                    $.ajax({
                        url: $('#hdnURL_EliminarBloqueHorario').val(),
                        data: {
                            id: idBloque,
                        },
                        method: 'POST',
                        success: function (response, jqXHR) {
                            if (response.success === true) {
                                Swal.fire({
                                    title: response.message,
                                    icon: 'success',
                                }).then((result) => {
                                    if (result.isConfirmed) {
                                        location.reload();
                                    }
                                })
                            }
                            else {
                                Swal.fire('Error', response.message, 'error');
                                return;
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            Swal.fire('Error', 'Favor comuniquese con un administrador', 'error');
                            return;
                        }
                    });
                }
                else {
                }
            })
        },

        EliminarCitaModal: function () {
            var btnEliminar = $('#btnEliminarCitaModal');

            Swal.fire({
                title: 'Eliminar Cita',
                text: '¿Esta seguro/a de eliminar la cita?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Eliminar',
            }).then((result) => {
                if (result.isConfirmed) {

                    btnEliminar.prop('disabled', true);
                    btnEliminar.text('Eliminando...');

                    $.ajax({
                        url: $('#hdnURL_EliminarCitaModal').val(),
                        data: {
                            id: $('#idCitaModal').val(),
                        },
                        method: 'POST',
                        success: function (response, jqXHR) {
                            if (response.success === true) {
                                Swal.fire({
                                    title: response.message,
                                    icon: 'success',
                                }).then((result) => {
                                    if (result.isConfirmed) {
                                        location.reload();
                                    }
                                })
                            }
                            else {
                                btnEliminar.prop('disabled', false);
                                btnEliminar.text('Eliminar');
                                Swal.fire('Error', response.message, 'error');
                                return;
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            Swal.fire('Error', 'Favor comuniquese con un administrador', 'error');
                            btnEliminar.prop('disabled', false);
                            btnEliminar.text('Eliminar');
                            return;
                        }
                    });
                }
                else {
                    btnEliminar.prop('disabled', false);
                    btnEliminar.text('Eliminar');
                }
            })
        },
        GuardarCitaModal: function () {
            var btnGuardar = $('#btnGuardarCitaModal');

            if (!$("#horaInicioCitaNuevo").val()) {
                Swal.fire('Ingrese la Hora de Inicio', '', 'warning');
                return null;
            }
            if (!$("#horaTerminoCitaNuevo").val()) {
                Swal.fire('Ingrese la Hora de Termino', '', 'warning');
                return null;
            }
            if (!$("#nombreCitaNuevo").val()) {
                Swal.fire('Ingrese el Nombre y Apellido', '', 'warning');
                return null;
            }
            if (!$("#correoCitaNuevo").val()) {
                Swal.fire('Ingrese un Correo Electrónico', '', 'warning');
                return null;
            }
            let fecha = $('#fechaModal').val();

            fecha = fecha.split("T")[0]

            let horaInicio = $('#horaInicioCitaNuevo').val();
            let horaFinal = $('#horaTerminoCitaNuevo').val();

            if (horaInicio.length === 5) {
                horaInicio += ":00";
            }

            if (horaFinal.length === 5) {
                horaFinal += ":00";
            }

            let cita = {
                FechaHoraInicio: `${fecha}T${horaInicio}`,
                FechaHoraFinal: `${fecha}T${horaFinal}`,
                NombrePaciente: $('#nombreCitaNuevo').val(),
                CorreoPaciente: $('#correoCitaNuevo').val(),
                Telefono: $('#telefonoCitaNuevo').val(),
                Nota: $('#notaCitaNuevo').val(),
            }


            Swal.fire({
                title: '',
                text: '¿Esta seguro de crear la Cita?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Guardar',
            }).then((result) => {
                if (result.isConfirmed) {

                    btnGuardar.prop('disabled', true);
                    btnGuardar.text('Guardando...');

                    $.ajax({
                        url: $('#hdnURL_CrearCita').val(),
                        data: {
                            cita: cita,
                            idUsuarioDestino: $('#hdnUsuarioSeleccionado').val() || null
                        },
                        method: 'POST',
                        success: function (response, jqXHR) {
                            if (response.success === true) {
                                Swal.fire({
                                    title: response.message,
                                    icon: 'success',
                                }).then((result) => {
                                    if (result.isConfirmed) {
                                        location.reload();
                                    }
                                })
                            }
                            else {
                                btnGuardar.prop('disabled', false);
                                btnGuardar.text('Guardar');
                                Swal.fire('Error', response.message, 'error');
                                return;
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            Swal.fire('Error', 'Favor comuniquese con un administrador', 'error');
                            btnGuardar.prop('disabled', false);
                            btnGuardar.text('Guardar');
                            return;
                        }
                    });
                }
                else {
                    btnGuardar.prop('disabled', false);
                    btnGuardar.text('Guardar');
                }
            })
        },
        GuardarCita: function () {
            var btnGuardar = $('#btnGuardarCita');

            if (!$("#horaInicioCitaNueva").val()) {
                Swal.fire('Ingrese la Hora de Inicio', '', 'warning');
                return null;
            }
            if (!$("#horaTerminoCitaNueva").val()) {
                Swal.fire('Ingrese la Hora de Termino', '', 'warning');
                return null;
            }
            if (!$("#nombreCitaNueva").val()) {
                Swal.fire('Ingrese el Nombre y Apellido', '', 'warning');
                return null;
            }
            if (!$("#correoCitaNueva").val()) {
                Swal.fire('Ingrese un Correo Electrónico', '', 'warning');
                return null;
            }

            let fecha = $('#fechaCitaNueva').val();

            let horaInicio = $('#horaInicioCitaNueva').val();
            let horaFinal = $('#horaTerminoCitaNueva').val();

            if (horaInicio.length === 5) {
                horaInicio += ":00";
            }

            if (horaFinal.length === 5) {
                horaFinal += ":00";
            }

            let cita = {
                FechaHoraInicio: `${fecha}T${horaInicio}`,
                FechaHoraFinal: `${fecha}T${horaFinal}`,
                NombrePaciente: $('#nombreCitaNueva').val(),
                CorreoPaciente: $('#correoCitaNueva').val(),
                Telefono: $('#telefonoCitaNueva').val(),
                Nota: $('#notaCitaNueva').val(),
            }


            Swal.fire({
                title: '',
                text: '¿Esta seguro de crear la Cita?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Guardar',
            }).then((result) => {
                if (result.isConfirmed) {

                    btnGuardar.prop('disabled', true);
                    btnGuardar.text('Guardando...');

                    $.ajax({
                        url: $('#hdnURL_CrearCita').val(),
                        data: {
                            cita: cita,
                            idUsuarioDestino: $('#hdnUsuarioSeleccionado').val() || null
                        },
                        method: 'POST',
                        success: function (response, jqXHR) {
                            if (response.success === true) {
                                Swal.fire({
                                    title: response.message,
                                    icon: 'success',
                                }).then((result) => {
                                    if (result.isConfirmed) {
                                        location.reload();
                                    }
                                })
                            }
                            else {
                                btnGuardar.prop('disabled', false);
                                btnGuardar.text('Guardar');
                                Swal.fire('Error', response.message, 'error');
                                return;
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            Swal.fire('Error', 'Favor comuniquese con un administrador', 'error');
                            btnGuardar.prop('disabled', false);
                            btnGuardar.text('Guardar');
                            return;
                        }
                    });
                }
                else {
                    btnGuardar.prop('disabled', false);
                    btnGuardar.text('Guardar');
                }
            })
        }
    }
})();

$(function () {
    ModuloHorarioCita.IniciarScripts();
});


//$(function () {
//    const esAdministrativo = $('#esAdministrativo').val() === 'true';
//    if (!esAdministrativo) {
//        $('#contenedorAgenda').show();
//        ModuloHorarioCita.IniciarScripts();
//    } else {
//        $('#selectProfesional').change(function () {
//            const usuarioId = $(this).val();

//            if (usuarioId) {
//                $('#contenedorAgenda').show();
//                ModuloHorarioCita.IniciarCalendario();
//            } else {
//                $('#contenedorAgenda').hide();
//                $('#calendar').html('');
//            }
//        });
//    }
//});


