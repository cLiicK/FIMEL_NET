var ModuloHistorialPaciente = (function () {
    return {
        IniciarScripts: function () {
            ModuloHistorialPaciente.CargarHistorialFull();


            $("#inputRutBusq").keypress(function (e) { onlyNumbersWithK(e); });
            $("#inputRutBusq").keyup(function () {

                let cadena = $("#inputRutBusq").val();
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
                $("#inputRutBusq").val(concatenar);
            });

            var paramValue = getUrlParameter("p");
            if (paramValue !== null) {
                $("#inputRutBusq").val(paramValue);
            };

            $('#comboTipoDocumentoHist').on('change', function (e) {
                let tipoDocto = $('#comboTipoDocumentoHist option:selected').val();
                if (tipoDocto == "RUT") {
                    $("#inputRutBusq").show();
                    $("#inputNumDocumentoBusq").hide();
                }
                else {
                    $("#inputRutBusq").hide();
                    $("#inputNumDocumentoBusq").show();
                }
            });
        },
        CargarHistorialFull: function () {

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
                            if (row.Rut && row.Dv) { result = ObtenerRutSTR(row.Rut, row.Dv) }
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
                            const encryptedId = btoa(data.Id.toString());
                            const url = `/Pacientes/DetallePaciente?idEncrypted=${encryptedId}`;
                            return `<a href="${url}" class="btn btn-ico" target="_blank"><i class="fas fa-clipboard-list"></i></a>`;
                        }
                    },
                    {
                        data: null,
                        orderable: false,
                        render: function (data, type, row) {
                            return `<a class="btn btn-ico" onclick="ModuloHistorialPaciente.EliminarPaciente(${data.Id})"><i class="fas fa-trash-alt" title="Eliminar"></i></a>`;
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

            if (!$('#inputBusqFechaDesde').val() && !$('#inputRutBusq').val() && !$('#inputNumDocumentoBusq').val()) {
                Swal.fire('Ingrese algún criterio válido', 'Busqueda', 'warning');
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
                        Rut: $('#inputRutBusq').val().split("-")[0].replace(/\./g, "") || null,
                        NumDoc: $('#inputNumDocumentoBusq').val() || null,
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
                        } },
                    {
                        data: null,
                        render: function (data, type, row) {
                            let result = "S/I";
                            if (row.Rut && row.Dv) { result = ObtenerRutSTR(row.Rut, row.Dv) }
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
                            const encryptedId = btoa(data.Id.toString());
                            const url = `/Pacientes/DetallePaciente?idEncrypted=${encryptedId}`;
                            return `<a href="${url}" class="btn btn-ico" target="_blank"><i class="fas fa-clipboard-list"></i></a>`;
                        }
                    },
                    {
                        data: null,
                        orderable: false,
                        render: function (data, type, row) {
                            return `<a class="btn btn-ico" onclick="ModuloHistorialPaciente.EliminarPaciente(${data.Id})"><i class="fas fa-trash-alt" title="Eliminar"></i></a>`;
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
                else {
                }
            })
        },
    }
})();

$(function () {
    ModuloHistorialPaciente.IniciarScripts();
});