var ModuloHistorialPaciente = (function () {
    return {
        IniciarScripts: function () {
            ModuloHistorialPaciente.CargarHistorialFull();
        },
        
        CargarHistorialFull: function () {
            // Limpiar filtros de fecha
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
                        if (json.success) {
                            return json.data;
                        } else {
                            alert(json.message);
                            return [];
                        }
                    }
                },
                columns: [
                    {
                        data: null,
                        render: function (data, type, row) {
                            return `${row.Nombres} ${row.PrimerApellido}`;
                        }
                    },
                    {
                        data: null,
                        render: function (data, type, row) {
                            let result = "S/I";
                            if (row.Rut && row.Dv) { 
                                result = ObtenerRutSTR(row.Rut, row.Dv); 
                            }
                            return `${result}`;
                        }
                    },
                    { data: "Celular" },
                    { data: "Email" },
                    { data: "Nacionalidad" },
                    {
                        data: "FechaCreacion",
                        render: function (data, type, row) {
                            let timestamp;
                            if (/\/Date\((\d+)\)\//.test(data)) {
                                timestamp = parseInt(data.replace(/\/Date\((\d+)\)\//, '$1'), 10);
                            } else {
                                timestamp = Date.parse(data);
                            }
                            const date = new Date(timestamp);
                            const day = ('0' + date.getDate()).slice(-2);
                            const month = ('0' + (date.getMonth() + 1)).slice(-2);
                            const year = date.getFullYear();
                            const hours = ('0' + date.getHours()).slice(-2);
                            const minutes = ('0' + date.getMinutes()).slice(-2);

                            return `${day}-${month}-${year} ${hours}:${minutes}`;
                        }
                    },
                    { data: "NumeroDocumento" },
                    {
                        data: null,
                        orderable: false,
                        render: function (data, type, row) {
                            const encryptedId = btoa(row.Id.toString());
                            const url = `/Pacientes/DetallePaciente?idEncrypted=${encryptedId}`;
                            return `<a href="${url}" class="btn btn-ico" target="_blank"><i class="fas fa-clipboard-list"></i></a>`;
                        }
                    }
                ],
                language: {
                    "decimal": "",
                    "emptyTable": "No hay información",
                    "info": "Mostrando _START_ a _END_ de _TOTAL_ Entradas",
                    "infoEmpty": "Mostrando 0 to 0 of 0 Entradas",
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
                        "last": "Ultimo",
                        "next": "Siguiente",
                        "previous": "Anterior"
                    }
                },
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

            showLoading(object);

            $('#HistorialTable').DataTable({
                ajax: {
                    url: $('#hdnURL_ObtenerHistorial').val(),
                    data: {
                        FechaConsultaDesde: $('#inputBusqFechaDesde').val() ? new Date($('#inputBusqFechaDesde').val()).toISOString() : null,
                        FechaConsultaHasta: $('#inputBusqFechaHasta').val() ? new Date($('#inputBusqFechaHasta').val()).toISOString() : null,
                    },
                    type: "GET",
                    dataSrc: function (json) {
                        if (json.success) {
                            return json.data;
                        } else {
                            alert(json.message);
                            return [];
                        }
                    }
                },
                columns: [
                    {
                        data: null,
                        render: function (data, type, row) {
                            return `${row.Nombres} ${row.PrimerApellido}`;
                        }
                    },
                    {
                        data: null,
                        render: function (data, type, row) {
                            let result = "S/I";
                            if (row.Rut && row.Dv) { 
                                result = ObtenerRutSTR(row.Rut, row.Dv); 
                            }
                            return `${result}`;
                        }
                    },
                    { data: "Celular" },
                    { data: "Email" },
                    { data: "Nacionalidad" },
                    {
                        data: "FechaCreacion",
                        render: function (data, type, row) {
                            let timestamp;
                            if (/\/Date\((\d+)\)\//.test(data)) {
                                timestamp = parseInt(data.replace(/\/Date\((\d+)\)\//, '$1'), 10);
                            } else {
                                timestamp = Date.parse(data);
                            }
                            const date = new Date(timestamp);
                            const day = ('0' + date.getDate()).slice(-2);
                            const month = ('0' + (date.getMonth() + 1)).slice(-2);
                            const year = date.getFullYear();
                            const hours = ('0' + date.getHours()).slice(-2);
                            const minutes = ('0' + date.getMinutes()).slice(-2);

                            return `${day}-${month}-${year} ${hours}:${minutes}`;
                        }
                    },
                    { data: "NumeroDocumento" },
                    {
                        data: null,
                        orderable: false,
                        render: function (data, type, row) {
                            const encryptedId = btoa(row.Id.toString());
                            const url = `/Pacientes/DetallePaciente?idEncrypted=${encryptedId}`;
                            return `<a href="${url}" class="btn btn-ico" target="_blank"><i class="fas fa-clipboard-list"></i></a>`;
                        }
                    }
                ],
                language: {
                    "decimal": "",
                    "emptyTable": "No hay información",
                    "info": "Mostrando _START_ a _END_ de _TOTAL_ Entradas",
                    "infoEmpty": "Mostrando 0 to 0 of 0 Entradas",
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
                        "last": "Ultimo",
                        "next": "Siguiente",
                        "previous": "Anterior"
                    }
                },
                order: [[5, 'desc']],
                initComplete: function (settings, json) {
                    closeLoading(object);
                }
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
                        data: {
                            idPaciente: idPaciente
                        },
                        method: 'POST',
                        success: function (response) {
                            if (response.success === true) {
                                Swal.fire({
                                    title: 'Paciente Eliminado!',
                                    icon: 'success',
                                }).then((result) => {
                                    if (result.isConfirmed) {
                                        location.reload();
                                    }
                                })
                            }
                            else {
                                Swal.fire('Error', 'No se pudo eliminar el Paciente', 'error');
                            }
                        },
                        error: function () {
                            Swal.fire('Error', 'Favor comuniquese con un administrador', 'error');
                        }
                    });
                }
            })
        },
    }
})();

$(function () {
    ModuloHistorialPaciente.IniciarScripts();
});