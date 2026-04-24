var ModuloHistorialPaciente = (function () {

    var dtLang = {
        "decimal": "",
        "emptyTable": "No hay información",
        "info": "Mostrando _START_ a _END_ de _TOTAL_ Entradas",
        "infoEmpty": "Mostrando 0 a 0 de 0 Entradas",
        "infoFiltered": "(Filtrado de _MAX_ total entradas)",
        "infoPostFix": "",
        "thousands": ",",
        "lengthMenu": "Mostrar _MENU_ Entradas",
        "search": "Buscar:",
        "loadingRecords": "Cargando...",
        "processing": "Procesando...",
        "zeroRecords": "Sin resultados encontrados",
        "paginate": {
            "first": "Primero",
            "last": "Último",
            "next": "Siguiente",
            "previous": "Anterior"
        }
    };

    function formatearFecha(data) {
        let timestamp;
        if (/\/Date\((\d+)\)\//.test(data)) {
            timestamp = parseInt(data.replace(/\/Date\((\d+)\)\//, '$1'), 10);
        } else {
            timestamp = Date.parse(data);
        }
        if (isNaN(timestamp)) return '-';
        const d = new Date(timestamp);
        return ('0' + d.getDate()).slice(-2) + '-' + ('0' + (d.getMonth() + 1)).slice(-2) + '-' + d.getFullYear();
    }

    function columnasPacientes() {
        return [
            {
                data: null,
                render: function (data, type, row) {
                    return `${row.Nombres || ''} ${row.PrimerApellido || ''}`.trim();
                }
            },
            {
                data: null,
                render: function (data, type, row) {
                    if (row.Rut && row.Dv) return ObtenerRutSTR(row.Rut, row.Dv);
                    return 'S/I';
                }
            },
            { data: "Celular" },
            { data: "Email" },
            { data: "Nacionalidad" },
            {
                data: "FechaCreacion",
                render: function (data) { return formatearFecha(data); }
            },
            { data: "NumeroDocumento" },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    const encryptedId = btoa(row.Id.toString());
                    return `<a href="/Pacientes/DetallePaciente?idEncrypted=${encryptedId}" class="btn btn-ico" target="_blank"><i class="fas fa-clipboard-list"></i></a>`;
                }
            }
        ];
    }

    return {
        IniciarScripts: function () {
            // Fechas por defecto para la bandeja
            var hoy = new Date();
            var en30 = new Date();
            en30.setDate(hoy.getDate() + 30);
            $('#inputControlFechaDesde').val(hoy.toISOString().split('T')[0]);
            $('#inputControlFechaHasta').val(en30.toISOString().split('T')[0]);

            // Cargar bandeja cuando se activa el tab
            $('#tab-controles-btn').on('shown.bs.tab', function () {
                if (!$.fn.DataTable.isDataTable('#BandejaTable') || $('#BandejaTable').DataTable().data().count() === 0) {
                    ModuloHistorialPaciente.CargarBandeja(null);
                }
            });

            ModuloHistorialPaciente.CargarHistorialFull();
        },

        CargarHistorialFull: function () {
            $('#inputBusqFechaDesde').val('');
            $('#inputBusqFechaHasta').val('');

            if ($.fn.DataTable.isDataTable('#HistorialTable')) {
                $('#HistorialTable').DataTable().destroy();
            }

            $('#HistorialTable').DataTable({
                ajax: {
                    url: $('#hdnURL_ObtenerHistorial').val(),
                    type: "GET",
                    dataSrc: function (json) {
                        return json.success ? json.data : [];
                    }
                },
                columns: columnasPacientes(),
                language: dtLang,
                order: [[5, 'desc']]
            });
        },

        CargarHistorial: function (object) {
            if (!$('#inputBusqFechaDesde').val() && !$('#inputBusqFechaHasta').val()) {
                Swal.fire('Ingrese al menos una fecha para filtrar', 'Búsqueda', 'warning');
                return;
            }

            if ($.fn.DataTable.isDataTable('#HistorialTable')) {
                $('#HistorialTable').DataTable().destroy();
            }

            if (object) showLoading(object);

            $('#HistorialTable').DataTable({
                ajax: {
                    url: $('#hdnURL_ObtenerHistorial').val(),
                    data: {
                        FechaConsultaDesde: $('#inputBusqFechaDesde').val() ? new Date($('#inputBusqFechaDesde').val()).toISOString() : null,
                        FechaConsultaHasta: $('#inputBusqFechaHasta').val() ? new Date($('#inputBusqFechaHasta').val()).toISOString() : null,
                    },
                    type: "GET",
                    dataSrc: function (json) {
                        return json.success ? json.data : [];
                    }
                },
                columns: columnasPacientes(),
                language: dtLang,
                order: [[5, 'desc']],
                initComplete: function () {
                    if (object) closeLoading(object);
                }
            });
        },

        CargarBandeja: function (object) {
            if ($.fn.DataTable.isDataTable('#BandejaTable')) {
                $('#BandejaTable').DataTable().destroy();
            }

            if (object) showLoading(object);

            $('#BandejaTable').DataTable({
                ajax: {
                    url: $('#hdnURL_ObtenerProximosControles').val(),
                    data: {
                        fechaDesde: $('#inputControlFechaDesde').val() || null,
                        fechaHasta: $('#inputControlFechaHasta').val() || null
                    },
                    type: "GET",
                    dataSrc: function (json) {
                        return json.success ? json.data : [];
                    }
                },
                columns: [
                    {
                        data: null,
                        render: function (data, type, row) {
                            var p = row.Paciente || {};
                            return `${p.Nombres || ''} ${p.PrimerApellido || ''}`.trim() || '-';
                        }
                    },
                    {
                        data: null,
                        render: function (data, type, row) {
                            var p = row.Paciente || {};
                            return p.Email || '-';
                        }
                    },
                    {
                        data: "FechaProximoControl",
                        render: function (data) { return formatearFecha(data); }
                    },
                    {
                        data: "FechaProximoControl",
                        render: function (data, type, row) {
                            if (!data) return '-';
                            var fecha = new Date(data);
                            var hoy = new Date();
                            hoy.setHours(0, 0, 0, 0);
                            var diff = Math.round((fecha - hoy) / (1000 * 60 * 60 * 24));
                            if (diff < 0) return `<span class="badge bg-danger">Vencido hace ${Math.abs(diff)} días</span>`;
                            if (diff === 0) return `<span class="badge bg-warning text-dark">Hoy</span>`;
                            return `<span class="badge bg-info text-dark">${diff} días</span>`;
                        }
                    },
                    { data: "MotivoConsulta", defaultContent: '-' },
                    {
                        data: null,
                        orderable: false,
                        render: function (data, type, row) {
                            var p = row.Paciente || {};
                            var encryptedId = p.Id ? btoa(p.Id.toString()) : '';
                            var email = p.Email || '';
                            return `
                                <a href="/Pacientes/DetallePaciente?idEncrypted=${encryptedId}" class="btn btn-ico" target="_blank" title="Ver ficha"><i class="fas fa-clipboard-list"></i></a>
                                <button class="btn btn-ico" onclick="ModuloHistorialPaciente.EnviarCorreo(${row.Id}, '${email}')" title="Enviar correo recordatorio" ${!email ? 'disabled' : ''}>
                                    <i class="fas fa-envelope"></i>
                                </button>`;
                        }
                    }
                ],
                language: dtLang,
                order: [[2, 'asc']],
                initComplete: function () {
                    if (object) closeLoading(object);
                }
            });
        },

        EnviarCorreo: function (idConsulta, email) {
            Swal.fire({
                title: 'Enviar recordatorio',
                text: `¿Enviar correo de recordatorio a ${email}?`,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Enviar',
                cancelButtonText: 'Cancelar'
            }).then(function (result) {
                if (!result.isConfirmed) return;

                Swal.fire({ title: 'Enviando...', allowOutsideClick: false, didOpen: function () { Swal.showLoading(); } });

                $.ajax({
                    url: $('#hdnURL_EnviarCorreoProximoControl').val(),
                    method: 'POST',
                    data: { idConsulta: idConsulta },
                    success: function (response) {
                        if (response.success) {
                            Swal.fire('Correo enviado', response.message, 'success');
                        } else {
                            Swal.fire('Error', response.message, 'error');
                        }
                    },
                    error: function () {
                        Swal.fire('Error', 'No se pudo enviar el correo.', 'error');
                    }
                });
            });
        },

        EliminarPaciente: function (idPaciente) {
            Swal.fire({
                title: 'Eliminar Paciente',
                text: '¿Esta seguro de ELIMINAR el Paciente?, esto eliminará sus consultas igualmente',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Eliminar',
            }).then((result) => {
                if (result.isConfirmed) {
                    $.ajax({
                        url: $('#hdnURL_EliminarPaciente').val(),
                        data: { idPaciente: idPaciente },
                        method: 'POST',
                        success: function (response) {
                            if (response.success === true) {
                                Swal.fire({ title: 'Paciente Eliminado!', icon: 'success' })
                                    .then(() => { location.reload(); });
                            } else {
                                Swal.fire('Error', 'No se pudo eliminar el Paciente', 'error');
                            }
                        },
                        error: function () {
                            Swal.fire('Error', 'Favor comuniquese con un administrador', 'error');
                        }
                    });
                }
            });
        },
    };
})();

$(function () {
    ModuloHistorialPaciente.IniciarScripts();
});
