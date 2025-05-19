var ModuloHistorialConsultas = (function () {
    return {
        IniciarScripts: function () {
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

            $("#btnGuardarEdicion").hide();
            $("#btnCancelarEdicion").hide();

            $("#btnEditarConsulta").click(function () {
                $("#btnGuardarEdicion").show();
                $("#btnCancelarEdicion").show();
                $("#btnEditarConsulta").hide();
                ModuloHistorialConsultas.CambiarAccesoCampos(false);
            });

            $("#btnCancelarEdicion").click(function () {
                $("#btnEditarConsulta").show();
                $("#btnGuardarEdicion").hide();
                $("#btnCancelarEdicion").hide();
                ModuloHistorialConsultas.CambiarAccesoCampos(true);
            });

        },
        CargarHistorial: function (object) {

            if (!$('#inputBusqFechaDesde').val() && !$('#inputRutBusq').val() && !$('#inputNumDocumentoBusq').val()) {
                Swal.fire('Ingrese algún criterio válido', 'Busqueda', 'warning');
                return;
            }

            if ($('#comboTipoDocumentoHist').val() === 'RUT' && !$('#inputRutBusq').val()) { Swal.fire('Ingrese el Rut', 'Búsqueda', 'warning'); return; }
            if ($('#comboTipoDocumentoHist').val() != 'RUT' && !$('#inputNumDocumentoBusq').val()) { Swal.fire('Ingrese el N° de Documento', 'Búsqueda', 'warning'); return; }

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
                        FechaConsultaHasta: $('#inputBusqFechaHasta').val() ? new Date($('#inputBusqFechaHasta').val()).toISOString() : null
                    },
                    type: "GET",
                    dataSrc: function (json) {
                        if (json.success) {
                            $('#idPacienteConsultado').text(json.dataPaciente.Nombres + ' ' + json.dataPaciente.PrimerApellido);
                            return json.data;
                        } else {
                            alert(json.message);
                            return [];
                        }
                    }
                },
                columns: [
                    { data: "TipoConsulta" },
                    { data: "MotivoConsulta" },
                    { data: "Diagnostico" },
                    { data: "Indicaciones" },
                    {
                        data: null,
                        orderable: false,
                        render: function (data, type, row) {
                            const encryptedId = btoa(data.Id.toString());
                            const url = `/Consulta/DetalleConsulta?idEncrypted=${encryptedId}`;
                            return `<a href="${url}" class="btn btn-ico" target="_blank"><i class="fas fa-clipboard-list"></i></a>`;
                        }
                    },
                    {
                        data: null,
                        orderable: false,
                        render: function (data, type, row) {
                            return `<a class="btn btn-ico" onclick="ModuloHistorialConsultas.EliminarConsulta(${data.Id})"><i class="fas fa-trash-alt" title="Eliminar"></i></a>`;
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
                order: false,
                initComplete: function (settings, json) {
                    closeLoading(object);
                }
            });

        },
        CambiarAccesoCampos: function (bool) {
            $('#comboTipoConsulta').prop('disabled', bool);
            $('#inputPeso').prop('disabled', bool);
            $('#inputTalla').prop('disabled', bool);
            $('#inputIMC').prop('disabled', bool);
            $('#inputEstadoNutricional').prop('disabled', bool);
            $('#inputMotivoConsulta').prop('disabled', bool);
            $('#inputAnamnesis').prop('disabled', bool);
            $('#inputExamenFisico').prop('disabled', bool);
            $('#inputDiagnostico').prop('disabled', bool);
            $('#inputIndicaciones').prop('disabled', bool);
            //$('#inputReceta').prop('disabled', bool);
        },
        ActualizarConsulta: function (object) {
            Swal.fire({
                title: 'Actualizar datos Consulta',
                text: '¿Esta seguro de actualizar los datos de la Consulta?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Actualizar',
            }).then((result) => {
                if (result.isConfirmed) {
                    var datosConsulta = ModuloHistorialConsultas.CapturarDatosConsulta(object);
                    if (!datosConsulta) { return }
                    $.ajax({
                        url: $('#hdnURL_ActualizarConsulta').val(),
                        data: { _datosConsultaJson: JSON.stringify(datosConsulta) },
                        method: 'POST',
                        cache: false,
                        async: false,
                        success: function (response, jqXHR) {
                            if (response.Codigo === 200) {
                                Swal.fire({
                                    title: 'Datos Actualizados!',
                                    icon: 'success',
                                }).then((result) => {
                                    if (result.isConfirmed) {
                                        location.reload();
                                    }
                                })
                            }
                            else {
                                Swal.fire('Error', response.Mensaje, 'error');
                            }
                            closeLoading(object);
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            Swal.fire('Error', 'Favor comuniquese con un administrador', 'error');
                            closeLoading(object);
                        }
                    });
                }
                else {
                    closeLoading(object);
                }
            })
        },
        CapturarDatosConsulta: function (object) {
            let objDatosConsulta = {};

            objDatosConsulta["Id"] = $("#idConsulta").val()

            if (!$("#comboTipoConsulta").val()) {
                Swal.fire('Ingrese el Tipo de Consulta', 'Nueva Consulta', 'warning');
                return null;
            }
            objDatosConsulta["TipoConsulta"] = $("#comboTipoConsulta").val() || null;

            if (!$("#inputPeso").val()) {
                Swal.fire('Ingrese el Peso', 'Nueva Consulta', 'warning');
                return null;
            }
            objDatosConsulta["Peso"] = $("#inputPeso").val() || null;

            if (!$("#inputTalla").val()) {
                Swal.fire('Ingrese la Talla', 'Nueva Consulta', 'warning');
                return null;
            }
            objDatosConsulta["Talla"] = $("#inputTalla").val() || null;

            if (!$("#inputIMC").val()) {
                Swal.fire('Haga el cálculo del IMC', 'Nueva Consulta', 'warning');
                return null;
            }
            objDatosConsulta["IMC"] = $("#inputIMC").val() || null;

            if (!$("#inputEstadoNutricional").val()) {
                Swal.fire('Haga el calculo del IMC', 'Nueva Consultae', 'warning');
                return null;
            }
            objDatosConsulta["EstadoNutricional"] = $("#inputEstadoNutricional").val() || null;

            if (!$("#inputMotivoConsulta").val()) {
                Swal.fire('Ingrese el Motivo', 'Nueva Consulta', 'warning');
                return null;
            }
            objDatosConsulta["MotivoConsulta"] = $("#inputMotivoConsulta").val() || null;

            if (!$("#inputAnamnesis").val()) {
                Swal.fire('Ingrese Anamnesis', 'Nueva Consulta', 'warning');
                return null;
            }
            objDatosConsulta["Anamnesis"] = $("#inputAnamnesis").val() || null;

            if (!$("#inputExamenFisico").val()) {
                Swal.fire('Ingrese el Examen Físico', 'Nueva Consulta', 'warning');
                return null;
            }
            objDatosConsulta["ExamenFisico"] = $("#inputExamenFisico").val() || null;

            if (!$("#inputDiagnostico").val()) {
                Swal.fire('Ingrese un Diagnóstico', 'Nueva Consulta', 'warning');
                return null;
            }
            objDatosConsulta["Diagnostico"] = $("#inputDiagnostico").val() || null;

            if (!$("#inputIndicaciones").val()) {
                Swal.fire('Ingrese las Indicaciones', 'Nueva Consulta', 'warning');
                return null;
            }
            objDatosConsulta["Indicaciones"] = $("#inputIndicaciones").val() || null;

            //if (!$("#inputReceta").val()) {
            //    Swal.fire('Ingrese la Receta', 'Nueva Consulta', 'warning');
            //    return null;
            //}
            //objDatosConsulta["Receta"] = $("#inputReceta").val() || null;



            //objDatosConsulta["OrdenExamenes"] = $("#inputOrdenExamenes").val();
            showLoading(object);
            return objDatosConsulta;
        },
        EliminarConsulta: function (idConsulta) {
            Swal.fire({
                title: 'Eliminar Consulta',
                text: '¿Esta seguro de ELIMINAR la Consulta?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Eliminar',
            }).then((result) => {
                if (result.isConfirmed) {
                    $.ajax({
                        url: $('#hdnURL_EliminarConsulta').val(),
                        data: {
                            idConsulta: idConsulta
                        },
                        method: 'POST',
                        success: function (response) {
                            if (response.success === true) {
                                Swal.fire({
                                    title: 'Consulta Eliminada!',
                                    icon: 'success',
                                }).then((result) => {
                                    if (result.isConfirmed) {
                                        location.reload();
                                    }
                                })
                            }
                            else {
                                Swal.fire('Error', 'No se pudo eliminar la Consulta', 'error');
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
        }
    }
})();

$(function () {
    ModuloHistorialConsultas.IniciarScripts();
});