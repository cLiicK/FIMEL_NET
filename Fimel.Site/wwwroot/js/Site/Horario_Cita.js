var configuracionUsuario = $('#ObjectConfigUsuario').val() ? JSON.parse($('#ObjectConfigUsuario').val()) : null;

function getBlockMinutes() {
    if (!configuracionUsuario || !configuracionUsuario.DuracionBloqueHorario) return 30;
    var parts = configuracionUsuario.DuracionBloqueHorario.split(':');
    return parseInt(parts[0]) * 60 + parseInt(parts[1]);
}

function populateTimeSelect(selectId, blockMinutes) {
    var $select = $(selectId);
    var current = $select.val();
    $select.empty().append('<option value="">Seleccione...</option>');
    for (var h = 0; h < 24; h++) {
        for (var m = 0; m < 60; m += blockMinutes) {
            var val = String(h).padStart(2, '0') + ':' + String(m).padStart(2, '0');
            $select.append('<option value="' + val + '">' + val + '</option>');
        }
    }
    if (current) $select.val(current);
}

var ModuloHorarioCita = (function () {
    return {
        IniciarScripts: function () {
            ModuloHorarioCita.IniciarCalendario();

            var hoyLocal = new Date();
            var anio = hoyLocal.getFullYear();
            var mes = String(hoyLocal.getMonth() + 1).padStart(2, '0');
            var dia = String(hoyLocal.getDate()).padStart(2, '0');
            var fechaChile = `${anio}-${mes}-${dia}`;

            const inputFecha = $('#fechaCitaNueva');
            inputFecha.val(fechaChile);
            inputFecha.attr('min', fechaChile); // impide seleccionar fechas anteriores

            var blockMinutes = getBlockMinutes();
            populateTimeSelect('#horaInicioNuevoBloque', blockMinutes);
            populateTimeSelect('#horaFinNuevoBloque', blockMinutes);
            populateTimeSelect('#horaInicioEspecificoNuevoHorario', blockMinutes);
            populateTimeSelect('#horaFinEspecificoNuevoHorario', blockMinutes);

            // Establecer fecha mínima para horarios específicos
            hoyLocal = new Date();
            anio = hoyLocal.getFullYear();
            mes = String(hoyLocal.getMonth() + 1).padStart(2, '0');
            dia = String(hoyLocal.getDate()).padStart(2, '0');
            fechaChile = `${anio}-${mes}-${dia}`;
            $('#fechaEspecificaNuevoHorario').attr('min', fechaChile);

            // Validaciones para el input RUT del modal Iniciar Cita
            $("#inputRutIniciarCita").keypress(function (e) { 
                onlyNumbersWithK(e); 
            });
            
            $("#inputRutIniciarCita").keyup(function () {
                let tipoDocto = $('#comboTipoDocumentoIniciarCita option:selected').val();
                if (tipoDocto != "RUT") {
                    return;
                }

                let cadena = $("#inputRutIniciarCita").val();
                cadena = cadena.replace(/[.]/gi, "").replace("-", "");
                if (cadena.length > 9) {
                    cadena = cadena.substr(0, 9);
                }
                let concatenar = "";
                let i = cadena.length - 1;
                for (; i >= 0;) {
                    concatenar = cadena[i] + concatenar;
                    if (i + 1 == (cadena.length) && i > 0) {
                        concatenar = "-" + concatenar;
                    }
                    if (concatenar.length == 9 && cadena.length > 7) {
                        concatenar = "." + concatenar;
                    }
                    if (concatenar.length == 5 && cadena.length > 4) {
                        concatenar = "." + concatenar;
                    }
                    i--;
                }
                $("#inputRutIniciarCita").val(concatenar);
            });

            $('#comboTipoDocumentoIniciarCita').on('change', function (e) {
                let tipoDocto = $('#comboTipoDocumentoIniciarCita option:selected').val();
                if (tipoDocto == "RUT") {
                    $("#inputRutIniciarCita").show();
                    $("#inputNumDocumentoIniciarCita").hide();
                }
                else {
                    $("#inputRutIniciarCita").hide();
                    $("#inputNumDocumentoIniciarCita").show();
                }
            });

            // Handlers para el combo y RUT del modal de NUEVA cita
            $('#tipoDocNuevo').on('change', function () {
                if ($(this).val() === 'RUT') {
                    $('#numDocRutNuevo').show();
                    $('#numDocNumNuevo').hide().val('');
                } else {
                    $('#numDocRutNuevo').hide().val('');
                    $('#numDocNumNuevo').show();
                }
            });

            $('#numDocRutNuevo').keypress(function (e) { onlyNumbersWithK(e); });
            $('#numDocRutNuevo').keyup(function () {
                let cadena = $(this).val().replace(/[.]/gi, '').replace('-', '');
                if (cadena.length > 9) cadena = cadena.substr(0, 9);
                let concat = '', i = cadena.length - 1;
                for (; i >= 0;) {
                    concat = cadena[i] + concat;
                    if (i + 1 == cadena.length && i > 0) concat = '-' + concat;
                    if (concat.length == 9 && cadena.length > 7) concat = '.' + concat;
                    if (concat.length == 5 && cadena.length > 4) concat = '.' + concat;
                    i--;
                }
                $(this).val(concat);
            });

            $('#numDocRutNuevo').blur(function () {
                var rutValidado = validarRut($(this).val());
                if (rutValidado === '00' || rutValidado === '01' || !rutValidado) return;
                $.get('/Pacientes/BuscarPaciente', { rutPaciente: parseInt(rutValidado) }, function (p) {
                    if (p && p.Id && p.Id > 0) {
                        if (p.Nombres)         $('#nombreCitaNuevo').val(p.Nombres);
                        if (p.PrimerApellido)  $('#apellidoPaternoCitaNuevo').val(p.PrimerApellido);
                        if (p.SegundoApellido) $('#apellidoMaternoCitaNuevo').val(p.SegundoApellido);
                        if (p.Email)           $('#correoCitaNuevo').val(p.Email);
                        if (p.Celular)         $('#telefonoCitaNuevo').val(p.Celular);
                    }
                });
            });

            $('#numDocNumNuevo').blur(function () {
                var numDoc = $(this).val().trim();
                if (!numDoc) return;
                $.get('/Pacientes/BuscarPacientePorNumDocumento', { numDoc: numDoc }, function (p) {
                    if (p && p.Id && p.Id > 0) {
                        if (p.Nombres)         $('#nombreCitaNuevo').val(p.Nombres);
                        if (p.PrimerApellido)  $('#apellidoPaternoCitaNuevo').val(p.PrimerApellido);
                        if (p.SegundoApellido) $('#apellidoMaternoCitaNuevo').val(p.SegundoApellido);
                        if (p.Email)           $('#correoCitaNuevo').val(p.Email);
                        if (p.Celular)         $('#telefonoCitaNuevo').val(p.Celular);
                    }
                });
            });

            $('#modalEventoNuevo').on('hidden.bs.modal', function () {
                $('#tipoDocNuevo').val('RUT');
                $('#numDocRutNuevo').val('').show();
                $('#numDocNumNuevo').val('').hide();
                $('#nombreCitaNuevo').val('');
                $('#apellidoPaternoCitaNuevo').val('');
                $('#apellidoMaternoCitaNuevo').val('');
                $('#correoCitaNuevo').val('');
                $('#telefonoCitaNuevo').val('');
                $('#notaCitaNuevo').val('');
            });

            $('#selectProfesional').off('change').on('change', function () {
                var idUsuario = $(this).val();
                if (idUsuario) {
                    $('#hdnUsuarioSeleccionado').val(idUsuario);

                    $.get($('#hdnURL_ObtenerVistaAgenda').val(), { idUsuario: idUsuario }, function (html) {
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

            // Manejar el clic en el tab de horarios específicos
            $(document).on('click', '#horarios-especificos-tab', function() {
                var idUsuario = $('#hdnUsuarioSeleccionado').val();
                
                // Establecer fecha mínima para el input de fecha específica
                var hoyLocal = new Date();
                var anio = hoyLocal.getFullYear();
                var mes = String(hoyLocal.getMonth() + 1).padStart(2, '0');
                var dia = String(hoyLocal.getDate()).padStart(2, '0');
                var fechaChile = `${anio}-${mes}-${dia}`;
                $('#fechaEspecificaNuevoHorario').attr('min', fechaChile);
                
                if (idUsuario) {
                    console.log('Cargando horarios específicos para usuario:', idUsuario);
                    $.ajax({
                        url: $('#hdnURL_ObtenerHorariosEspecificos').val(),
                        data: { idUsuario: idUsuario },
                        method: 'GET',
                        dataType: 'json',
                        success: function (response) {
                            console.log('Respuesta JSON recibida:', response);
                            
                            if (response && response.success && response.horarios && response.horarios.length > 0) {
                                // Construir tabla HTML dinámicamente
                                var html = '<table class="table"><thead><tr><th>Fecha</th><th>Hora Inicio</th><th>Hora Fin</th><th>Comentario</th><th>&nbsp;</th></tr></thead><tbody>';
                                
                                response.horarios.forEach(function(horario) {
                                    try {
                                        var fecha = new Date(horario.FechaEspecifica).toLocaleDateString('es-ES');
                                        var horaInicio = horario.HoraInicio ? horario.HoraInicio.substring(0, 5) : 'N/A';
                                        var horaFin = horario.HoraFin ? horario.HoraFin.substring(0, 5) : 'N/A';
                                        var comentario = horario.Comentario || '';
                                        var id = horario.Id || horario.id || 0;
                                        
                                        html += '<tr>';
                                        html += '<td>' + fecha + '</td>';
                                        html += '<td>' + horaInicio + '</td>';
                                        html += '<td>' + horaFin + '</td>';
                                        html += '<td>' + comentario + '</td>';
                                        html += '<td><a class="btn btn-ico" onclick="ModuloHorarioCita.EliminarHorarioEspecifico(' + id + ')"><i class="fas fa-trash"></i></a></td>';
                                        html += '</tr>';
                                    } catch (e) {
                                        console.log('Error al procesar horario:', horario, e);
                                    }
                                });
                                
                                html += '</tbody></table>';
                                $('#contenedorHorariosEspecificos').html(html);
                            } else {
                                $('#contenedorHorariosEspecificos').html('<div class="alert alert-info"><i class="fas fa-info-circle"></i> No hay horarios específicos configurados.</div>');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log('Error al cargar horarios específicos:', xhr.status, status, error);
                            console.log('Response text:', xhr.responseText);
                            
                            // Intentar parsear la respuesta como JSON para ver si hay información útil
                            try {
                                var errorResponse = JSON.parse(xhr.responseText);
                                console.log('Error response:', errorResponse);
                            } catch (e) {
                                console.log('No se pudo parsear la respuesta de error como JSON');
                            }
                            
                            $('#contenedorHorariosEspecificos').html('<div class="alert alert-danger">Error al cargar los horarios específicos</div>');
                        }
                    });
                } else {
                    $('#contenedorHorariosEspecificos').html('<div class="alert alert-warning">Seleccione un profesional primero</div>');
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
                scrollTime:  document.getElementById('hdnScrollTime')?.value  || '08:00:00',
                slotMinTime: document.getElementById('hdnSlotMinTime')?.value || '07:30:00',
                slotMaxTime: document.getElementById('hdnSlotMaxTime')?.value || '21:00:00',
                nowIndicator: true,
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

                    let fechaISO = fechaSeleccionada.toISOString().split('T')[0];
                    $('#fechaCita').val(fechaISO);

                    $('#horaInicioCita').val(info.event.extendedProps.horaInicio);
                    $('#horaTerminoCita').val(info.event.extendedProps.horaFinal);
                    $('#tipoDocCita').val(info.event.extendedProps.documento || '');
                    $('#numDocCita').val(info.event.extendedProps.numDocumento || '');
                    $('#numDocCitaRaw').val(info.event.extendedProps.numDocumentoRaw || '');
                    $('#nombreCita').val(info.event.extendedProps.nombre);
                    $('#apellidoPaternoCita').val(info.event.extendedProps.apellidoPaterno || '');
                    $('#apellidoMaternoCita').val(info.event.extendedProps.apellidoMaterno || '');
                    $('#correoCita').val(info.event.extendedProps.correo);
                    $('#telefonoCita').val(info.event.extendedProps.telefono);
                    $('#notaCita').val(info.event.extendedProps.nota);
                    $('#idCitaModal').val(info.event.extendedProps.idCita);

                    // Campos siempre deshabilitados al abrir
                    $('#fechaCita, #horaInicioCita, #horaTerminoCita, #tipoDocCita, #numDocCita').prop('disabled', true);
                    $('#nombreCita, #apellidoPaternoCita, #apellidoMaternoCita, #correoCita, #telefonoCita, #notaCita').prop('disabled', true);

                    $('#btnModificarCitaModal').show();
                    $('#btnGuardarModificacionCita').hide();
                    $('#btnCancelarModificacion').hide();
                    $('#btnIniciarCitaModal').show();
                    $('#btnEliminarCitaModal').show();

                    $('#modalEvento').modal('show');
                },
                eventDidMount: function (info) {
                    // Cambiar el cursor cuando hay una cita agendada
                    info.el.style.cursor = 'pointer';
                },
                select: function (info) {
                    let fechaSeleccionada = new Date(info.startStr);
                    let formatoFecha = fechaSeleccionada.toLocaleDateString('es-ES', { day: 'numeric', month: 'long', year: 'numeric' });
                    let horaInicio = String(fechaSeleccionada.getHours()).padStart(2, '0') + ':' + String(fechaSeleccionada.getMinutes()).padStart(2, '0');
                    $('#modalEventoNuevoLabel').text(formatoFecha);
                    $('#fechaModal').val(info.startStr);
                    $('#horaInicioCitaNuevo').val(horaInicio);

                    let duracionStr = (configuracionUsuario && configuracionUsuario.DuracionBloqueHorario) ? configuracionUsuario.DuracionBloqueHorario : '00:30:00';
                    let partes = duracionStr.split(':');
                    let minutosDuracion = parseInt(partes[0]) * 60 + parseInt(partes[1]);
                    let fechaFin = new Date(fechaSeleccionada.getTime() + minutosDuracion * 60000);
                    let horaFin = String(fechaFin.getHours()).padStart(2, '0') + ':' + String(fechaFin.getMinutes()).padStart(2, '0');
                    $('#horaTerminoCitaNuevo').val(horaFin);

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

        GuardarNuevoHorarioEspecifico: function () {
            if (!$("#fechaEspecificaNuevoHorario").val()) {
                Swal.fire('Seleccione la Fecha', 'Nuevo Horario Específico', 'warning');
                return null;
            }
            if (!$("#horaInicioEspecificoNuevoHorario").val()) {
                Swal.fire('Seleccione la Hora de Inicio', 'Nuevo Horario Específico', 'warning');
                return null;
            }
            if (!$("#horaFinEspecificoNuevoHorario").val()) {
                Swal.fire('Seleccione la Hora Final', 'Nuevo Horario Específico', 'warning');
                return null;
            }

            var btnGuardar = $('#btnGuardarHorarioEspecifico');

            Swal.fire({
                title: 'Nuevo Horario Específico',
                text: '¿Esta seguro de crear este nuevo Horario Específico?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Guardar',
            }).then((result) => {
                if (result.isConfirmed) {

                    btnGuardar.prop('disabled', true);
                    btnGuardar.text('Guardando...');

                    $.ajax({
                        url: $('#hdnURL_CrearHorarioEspecifico').val(),
                        data: {
                            horario: {
                                Comentario: $('#comentarioEspecificoNuevoHorario').val(),
                                FechaEspecifica: $('#fechaEspecificaNuevoHorario').val(),
                                HoraFin: $('#horaFinEspecificoNuevoHorario').val(),
                                HoraInicio: $('#horaInicioEspecificoNuevoHorario').val(),
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
                                        // Limpiar el formulario
                                        $('#fechaEspecificaNuevoHorario').val('');
                                        $('#horaInicioEspecificoNuevoHorario').val('');
                                        $('#horaFinEspecificoNuevoHorario').val('');
                                        $('#comentarioEspecificoNuevoHorario').val('');
                                        
                                        //// Recargar la lista de horarios específicos
                                        //var idUsuario = $('#hdnUsuarioSeleccionado').val();
                                        //if (idUsuario) {
                                        //    $.get($('#hdnURL_ObtenerHorariosEspecificos').val(), { idUsuario: idUsuario }, function (html) {
                                        //        $('#contenedorHorariosEspecificos').html(html);
                                        //    });
                                        //}
                                        
                                        // Recargar la página para actualizar el calendario
                                        setTimeout(function() {
                                            location.reload();
                                        }, 1000);
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

        EliminarHorarioEspecifico: function (idHorario) {
            console.log('EliminarHorarioEspecifico - ID recibido:', idHorario);
            
            if (!idHorario || idHorario === 0) {
                Swal.fire('Error', 'ID de horario específico no válido', 'error');
                return;
            }
           
            Swal.fire({
                title: 'Eliminar Horario Específico',
                text: '¿Esta seguro de eliminar este Horario Específico?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Eliminar',
            }).then((result) => {
                if (result.isConfirmed) {

                    $.ajax({
                        url: $('#hdnURL_EliminarHorarioEspecifico').val(),
                        data: {
                            id: idHorario,
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
            var btnGuardar = $('#btnGuardarModificacionCita');

            // Validar documento
            var tipoDoc = $('#tipoDocNuevo').val();
            var numDoc;
            if (tipoDoc === 'RUT') {
                var rutVal = validarRut($('#numDocRutNuevo').val());
                if (rutVal === '00' || rutVal === '01') {
                    Swal.fire('Ingrese un RUT válido', '', 'warning');
                    return null;
                }
                numDoc = rutVal;
            } else {
                numDoc = $('#numDocNumNuevo').val().trim();
                if (!numDoc) {
                    Swal.fire('Ingrese el número de documento', '', 'warning');
                    return null;
                }
            }

            if (!$("#horaInicioCitaNuevo").val()) {
                Swal.fire('Ingrese la Hora de Inicio', '', 'warning');
                return null;
            }
            if (!$("#horaTerminoCitaNuevo").val()) {
                Swal.fire('Ingrese la Hora de Término', '', 'warning');
                return null;
            }
            if (!$("#nombreCitaNuevo").val()) {
                Swal.fire('Ingrese el Nombre', '', 'warning');
                return null;
            }
            if (!$("#correoCitaNuevo").val()) {
                Swal.fire('Ingrese un Correo Electrónico', '', 'warning');
                return null;
            }

            let fecha = $('#fechaModal').val().split('T')[0];

            let horaInicio = $('#horaInicioCitaNuevo').val();
            let horaFinal = $('#horaTerminoCitaNuevo').val();
            if (horaInicio.length === 5) horaInicio += ':00';
            if (horaFinal.length === 5) horaFinal += ':00';

            let cita = {
                FechaHoraInicio: `${fecha}T${horaInicio}`,
                FechaHoraFinal: `${fecha}T${horaFinal}`,
                TipoDocumento: tipoDoc,
                NumeroDocumento: numDoc,
                NombrePaciente: $('#nombreCitaNuevo').val(),
                ApellidoPaciente: $('#apellidoPaternoCitaNuevo').val(),
                SegundoApellidoPaciente: $('#apellidoMaternoCitaNuevo').val(),
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
        },

        IniciarCitaModal: function () {
            var numDocRaw = $('#numDocCitaRaw').val();
            var tipoDoc = $('#tipoDocCita').val();

            if (numDocRaw) {
                $('#modalEvento').modal('hide');

                var urlBusqueda, busquedaData, urlConsulta, urlFicha;

                if (tipoDoc === 'RUT') {
                    var dv = getDV(numDocRaw).toString();
                    var rutFormateado = ObtenerRutSTR(parseInt(numDocRaw), dv);
                    urlBusqueda = '/Pacientes/BuscarPaciente';
                    busquedaData = { rutPaciente: parseInt(numDocRaw) };
                    urlConsulta = '/Consulta/NuevaConsulta?rut=' + encodeURIComponent(rutFormateado);
                    urlFicha = '/Pacientes/FichaPaciente?p=' + encodeURIComponent(rutFormateado);
                } else {
                    urlBusqueda = '/Pacientes/BuscarPacientePorNumDocumento';
                    busquedaData = { numDoc: numDocRaw };
                    urlConsulta = '/Consulta/NuevaConsulta?numDoc=' + encodeURIComponent(numDocRaw) + '&tipo=' + encodeURIComponent(tipoDoc);
                    urlFicha = '/Pacientes/FichaPaciente?numDoc=' + encodeURIComponent(numDocRaw) + '&tipo=' + encodeURIComponent(tipoDoc);
                }

                $.ajax({
                    url: urlBusqueda,
                    data: busquedaData,
                    method: 'GET',
                    success: function (response) {
                        if (response && response.Id && response.Id > 0) {
                            Swal.fire({
                                title: 'Paciente encontrado',
                                text: '¿Desea iniciar una nueva consulta?',
                                icon: 'question',
                                showCancelButton: true,
                                confirmButtonText: 'Ir a Nueva Consulta',
                                cancelButtonText: 'Cancelar'
                            }).then((result) => {
                                if (result.isConfirmed) window.location.href = urlConsulta;
                            });
                        } else {
                            Swal.fire({
                                title: 'Ficha incompleta',
                                text: 'El paciente fue creado con datos básicos. ¿Desea completar la ficha clínica?',
                                icon: 'info',
                                showCancelButton: true,
                                confirmButtonText: 'Completar Ficha',
                                cancelButtonText: 'Cancelar'
                            }).then((result) => {
                                if (result.isConfirmed) window.location.href = urlFicha;
                            });
                        }
                    },
                    error: function () {
                        Swal.fire('Error', 'Ha ocurrido un error al buscar el paciente', 'error');
                    }
                });
            } else {
                // Cita sin documento (registrada antes de esta funcionalidad) — flujo anterior
                $('#modalEvento').modal('hide');
                $('#inputRutIniciarCita').val('');
                $('#modalIniciarCita').modal('show');
            }
        },

        ModificarCitaModal: function () {
            $('#fechaCita').prop('disabled', false);
            $('#horaInicioCita').prop('disabled', false);
            $('#horaTerminoCita').prop('disabled', false);
            // Tipo doc y num doc no se modifican (identifican al paciente)
            $('#nombreCita').prop('disabled', false);
            $('#apellidoPaternoCita').prop('disabled', false);
            $('#apellidoMaternoCita').prop('disabled', false);
            $('#correoCita').prop('disabled', false);
            $('#telefonoCita').prop('disabled', false);
            $('#notaCita').prop('disabled', false);

            $('#btnModificarCitaModal').hide();
            $('#btnGuardarModificacionCita').show();
            $('#btnCancelarModificacion').show();
            $('#btnIniciarCitaModal').hide();
            $('#btnEliminarCitaModal').hide();
        },

        CancelarModificacion: function () {
            $('#fechaCita, #horaInicioCita, #horaTerminoCita, #tipoDocCita, #numDocCita').prop('disabled', true);
            $('#nombreCita, #apellidoPaternoCita, #apellidoMaternoCita, #correoCita, #telefonoCita, #notaCita').prop('disabled', true);

            $('#btnModificarCitaModal').show();
            $('#btnGuardarModificacionCita').hide();
            $('#btnCancelarModificacion').hide();
            $('#btnIniciarCitaModal').show();
            $('#btnEliminarCitaModal').show();
        },


        GuardarModificacionCita: function () {
            var btnGuardar = $('#btnGuardarModificacionCita');

            // Validaciones
            if (!$("#fechaCita").val()) {
                Swal.fire('Seleccione la Fecha', '', 'warning');
                return null;
            }
            if (!$("#horaInicioCita").val()) {
                Swal.fire('Ingrese la Hora de Inicio', '', 'warning');
                return null;
            }
            if (!$("#horaTerminoCita").val()) {
                Swal.fire('Ingrese la Hora de Termino', '', 'warning');
                return null;
            }
            if (!$("#nombreCita").val()) {
                Swal.fire('Ingrese el Nombre y Apellido', '', 'warning');
                return null;
            }
            if (!$("#correoCita").val()) {
                Swal.fire('Ingrese un Correo Electrónico', '', 'warning');
                return null;
            }

            let fecha = $('#fechaCita').val();
            let horaInicio = $('#horaInicioCita').val();
            let horaFinal = $('#horaTerminoCita').val();

            if (horaInicio.length === 5) {
                horaInicio += ":00";
            }

            if (horaFinal.length === 5) {
                horaFinal += ":00";
            }

            let cita = {
                FechaHoraInicio: `${fecha}T${horaInicio}`,
                FechaHoraFinal: `${fecha}T${horaFinal}`,
                TipoDocumento: $('#tipoDocCita').val(),
                NumeroDocumento: $('#numDocCitaRaw').val(),
                NombrePaciente: $('#nombreCita').val(),
                ApellidoPaciente: $('#apellidoPaternoCita').val(),
                SegundoApellidoPaciente: $('#apellidoMaternoCita').val(),
                CorreoPaciente: $('#correoCita').val(),
                Telefono: $('#telefonoCita').val(),
                Nota: $('#notaCita').val(),
            }

            Swal.fire({
                title: 'Modificar Cita',
                text: '¿Está seguro de modificar esta cita? Se enviará un correo de confirmación al paciente.',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Guardar Cambios',
            }).then((result) => {
                if (result.isConfirmed) {

                    btnGuardar.prop('disabled', true);
                    btnGuardar.text('Guardando...');

                    $.ajax({
                        url: $('#hdnURL_ActualizarCitaConCorreo').val(),
                        data: {
                            id: $('#idCitaModal').val(),
                            cita: cita
                        },
                        method: 'POST',
                        success: function (response, jqXHR) {
                            if (response && response.success) {
                                Swal.fire({
                                    title: 'Cita modificada exitosamente',
                                    text: 'Se ha enviado un correo de confirmación al paciente.',
                                    icon: 'success',
                                }).then((result) => {
                                    if (result.isConfirmed) {
                                        $('#modalEvento').modal('hide');
                                        location.reload();
                                    }
                                })
                            }
                            else {
                                btnGuardar.prop('disabled', false);
                                btnGuardar.text('Guardar Cambios');
                                Swal.fire('Error', response.message || 'Error al modificar la cita', 'error');
                                return;
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            Swal.fire('Error', 'Favor comuníquese con un administrador', 'error');
                            btnGuardar.prop('disabled', false);
                            btnGuardar.text('Guardar Cambios');
                            return;
                        }
                    });
                }
                else {
                    btnGuardar.prop('disabled', false);
                    btnGuardar.text('Guardar Cambios');
                }
            })
        },

        ValidarRutIniciarCita: function () {
            var tipoDocumento = $('#comboTipoDocumentoIniciarCita option:selected').val();
            var numeroDocumento = '';
            var urlBusqueda = '';
            var documentoOriginal = '';
            
            if (tipoDocumento == "RUT") {
                var rutCompleto = $("#inputRutIniciarCita").val();
                
                if (rutCompleto.length == 0) {
                    Swal.fire('Ingresa un RUT válido', '', 'warning');
                    return;
                }

                let validacionRut = validarRut(rutCompleto);
                if (validacionRut == "01" || validacionRut == "00") {
                    Swal.fire('Ingresa un RUT válido', '', 'warning');
                    return;
                }
                
                numeroDocumento = validacionRut;
                documentoOriginal = rutCompleto;
                urlBusqueda = '/Pacientes/BuscarPaciente';
            } else {
                numeroDocumento = $('#inputNumDocumentoIniciarCita').val();
                
                if (numeroDocumento.length == 0) {
                    Swal.fire('Ingresa un número de documento válido', '', 'warning');
                    return;
                }
                
                documentoOriginal = numeroDocumento;
                urlBusqueda = '/Pacientes/BuscarPacientePorNumDocumento';
            }

            // Buscar el paciente
            $.ajax({
                url: urlBusqueda,
                data: tipoDocumento == "RUT" ? {
                    rutPaciente: numeroDocumento
                } : {
                    numDoc: numeroDocumento
                },
                method: 'GET',
                success: function (response) {
                    if (response && response.Id && response.Id > 0) {
                        // El paciente existe, ir a Nueva Consulta
                        Swal.fire({
                            title: 'Paciente encontrado',
                            text: '¿Desea iniciar una nueva consulta?',
                            icon: 'question',
                            showCancelButton: true,
                            confirmButtonText: 'Ir a Nueva Consulta',
                            cancelButtonText: 'Cancelar'
                        }).then((result) => {
                            if (result.isConfirmed) {
                                // Cerrar el modal
                                $('#modalIniciarCita').modal('hide');
                                // Redirigir a Nueva Consulta
                                if (tipoDocumento == "RUT") {
                                    window.location.href = '/Consulta/NuevaConsulta?rut=' + encodeURIComponent(documentoOriginal);
                                } else {
                                    window.location.href = '/Consulta/NuevaConsulta?numDoc=' + encodeURIComponent(numeroDocumento) + '&tipo=' + encodeURIComponent(tipoDocumento);
                                }
                            }
                        });
                    } else {
                        // El paciente no existe, ir a Ficha Paciente
                        Swal.fire({
                            title: 'Paciente no encontrado',
                            text: '¿Desea crear una nueva ficha clínica?',
                            icon: 'info',
                            showCancelButton: true,
                            confirmButtonText: 'Crear Ficha',
                            cancelButtonText: 'Cancelar'
                        }).then((result) => {
                            if (result.isConfirmed) {
                                // Cerrar el modal
                                $('#modalIniciarCita').modal('hide');
                                // Redirigir a Ficha Paciente
                                if (tipoDocumento == "RUT") {
                                    window.location.href = '/Pacientes/FichaPaciente?p=' + encodeURIComponent(documentoOriginal);
                                } else {
                                    window.location.href = '/Pacientes/FichaPaciente?numDoc=' + encodeURIComponent(numeroDocumento) + '&tipo=' + encodeURIComponent(tipoDocumento);
                                }
                            }
                        });
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Ha ocurrido un error al validar el documento', 'error');
                }
            });
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


