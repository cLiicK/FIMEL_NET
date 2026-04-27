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
                            $('#idPacienteConsultado').text(json.dataPaciente.Nombres + ' ' + json.dataPaciente.PrimerApellido + ' ' + json.dataPaciente.SegundoApellido );
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
                            return `<a class="btn btn-ico" onclick="ModuloHistorialConsultas.EliminarConsulta(${data.Id})"><i class="fas fa-trash-can" title="Eliminar"></i></a>`;
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
            $('#Consulta_TipoConsulta').prop('disabled', bool);
            $('#inputPeso').prop('disabled', bool);
            $('#inputTalla').prop('disabled', bool);
            //$('#inputIMC').prop('disabled', bool);
            //$('#inputEstadoNutricional').prop('disabled', bool);
            $('#inputPresionArterial').prop('disabled', bool);
            $('#inputMotivoConsulta').prop('disabled', bool);
            $('#inputAnamnesis').prop('disabled', bool);
            $('#inputExamenFisico').prop('disabled', bool);
            $('#inputDiagnostico').prop('disabled', bool);
            $('#inputIndicaciones').prop('disabled', bool);
            $('#inputReceta').prop('disabled', bool);
            $('#inputOrdenExamenes').prop('disabled', bool);
            //$('#inputFechaProximoControl').prop('disabled', bool);
            //$('#inputFechaConsulta').prop('disabled', bool);
            //$('#inputReceta').prop('disabled', bool);
            if (bool) {
                $('.btn-plantilla-detalle').addClass('d-none');
            } else {
                $('.btn-plantilla-detalle').removeClass('d-none');
            }
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

            if (!$("#Consulta_TipoConsulta").val()) {
                Swal.fire('Ingrese el Tipo de Consulta', '', 'warning');
                return null;
            }
            objDatosConsulta["TipoConsulta"] = $('#Consulta_TipoConsulta option:selected').val();
            objDatosConsulta["Peso"] = $("#inputPeso").val() || null;
            objDatosConsulta["Talla"] = $("#inputTalla").val() || null;
            objDatosConsulta["IMC"] = $("#inputIMC").val() || null;
            objDatosConsulta["EstadoNutricional"] = $("#inputEstadoNutricional").val() || null;
            objDatosConsulta["PresionArterial"] = $("#inputPresionArterial").val() || null;

            if (!$("#inputMotivoConsulta").val()) {
                Swal.fire('Ingrese el Motivo', '', 'warning');
                return null;
            }
            objDatosConsulta["MotivoConsulta"] = $("#inputMotivoConsulta").val() || null;
            objDatosConsulta["Anamnesis"] = $("#inputAnamnesis").val() || null;
            objDatosConsulta["ExamenFisico"] = $("#inputExamenFisico").val() || null;
            objDatosConsulta["Diagnostico"] = $("#inputDiagnostico").val() || null;
            objDatosConsulta["Indicaciones"] = $("#inputIndicaciones").val() || null;
            objDatosConsulta["Receta"] = $("#inputReceta").val() || null;
            objDatosConsulta["OrdenExamenes"] = $("#inputOrdenExamenes").val() || null;
            objDatosConsulta["FechaProximoControl"] = $("#inputFechaProximoControl").val() || null;
            objDatosConsulta["FechaConsulta"] = $("#inputFechaConsulta").val() || null;

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
        },
        CalcularIMC: function () {
            let estadoNutricional, classEstadoNutricional;
            let peso = $("#inputPeso").val();
            let talla = $("#inputTalla").val() / 100;
            let IMC = (peso / (talla * talla));
            IMC = Math.round(IMC * 100) / 100

            if (peso != "" && peso != 0 && talla != "" && talla != 0) {
                $("#inputIMC").val(IMC);

                if (IMC >= 30) {
                    estadoNutricional = "Obesidad";
                    classEstadoNutricional = "form-control alert-danger";
                } else if (IMC >= 25) {
                    estadoNutricional = "Sobrepeso";
                    classEstadoNutricional = "form-control alert-warning";
                } else if (IMC >= 18.5) {
                    estadoNutricional = "Normal";
                    classEstadoNutricional = "form-control alert-success";
                } else {
                    estadoNutricional = "Bajo Peso";
                    classEstadoNutricional = "form-control alert-primary";
                }
                $("#inputEstadoNutricional").val(estadoNutricional);
                $("#inputEstadoNutricional").removeClass();
                $("#inputEstadoNutricional").addClass(classEstadoNutricional);
            }
        },

        AbrirModalPlantillas: function (tipo) {
            const modalElement = document.getElementById('modalPlantillas');
            let modal = bootstrap.Modal.getInstance(modalElement);
            if (!modal) {
                modal = new bootstrap.Modal(modalElement);
            }
            modalElement.addEventListener('hidden.bs.modal', function () {
                document.querySelectorAll('.modal-backdrop').forEach(function (b) { b.remove(); });
                document.body.classList.remove('modal-open');
                document.body.style.overflow = '';
                document.body.style.paddingRight = '';
            }, { once: true });

            const modalTitle = document.getElementById('modalPlantillasLabel');
            const contenedor = $('#contenedorPlantillas');
            $('#modalPlantillas').data('tipo-actual', tipo);
            $('#formularioNuevaPlantilla').hide();
            $('#btnMostrarFormulario').hide();
            $('#inputTituloPlantilla').val('');
            $('#inputContenidoPlantilla').val('');
            $('#inputIdPlantillaEditar').val('');
            $('#tituloFormularioPlantilla').text('Crear Nueva Plantilla');
            $('#btnGuardarPlantilla').text('Guardar Plantilla');

            let titulo = 'Plantillas';
            if (tipo === 'Anamnesis') titulo = 'Plantillas de Anamnesis';
            else if (tipo === 'ExamenFisico') titulo = 'Plantillas de Examen Físico';
            else if (tipo === 'Indicaciones') titulo = 'Plantillas de Indicaciones';
            modalTitle.textContent = titulo;

            contenedor.html('<div class="text-center p-4"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></div>');
            modal.show();

            $.ajax({
                url: '/Consulta/ObtenerPlantillasPorTipo',
                type: 'GET',
                data: { tipo: tipo },
                success: function (response) {
                    if (response.success && response.data && response.data.length > 0) {
                        let html = '';
                        response.data.forEach(function (plantilla) {
                            const contenidoEscapado = (plantilla.Contenido || '').replace(/"/g, '&quot;').replace(/'/g, '&#39;').replace(/\n/g, '\\n');
                            const tituloEscapado = (plantilla.Titulo || 'Sin título').replace(/"/g, '&quot;').replace(/'/g, '&#39;');
                            html += `
                                <div class="list-group-item plantilla-item-container">
                                    <div class="d-flex w-100 justify-content-between align-items-start">
                                        <div class="plantilla-item flex-grow-1" style="cursor: pointer;"
                                             data-tipo="${tipo}" data-id="${plantilla.Id}"
                                             data-titulo="${tituloEscapado}" data-contenido="${contenidoEscapado}">
                                            <h6 class="mb-1">${tituloEscapado}</h6>
                                            <p class="mb-1 text-muted small">${(plantilla.Contenido || '').substring(0, 100)}${(plantilla.Contenido || '').length > 100 ? '...' : ''}</p>
                                        </div>
                                        <div class="d-flex gap-1 ms-2">
                                            <button class="btn btn-sm btn-outline-primary btn-editar-plantilla"
                                                    data-id="${plantilla.Id}" data-titulo="${tituloEscapado}"
                                                    data-contenido="${contenidoEscapado}" title="Editar plantilla">
                                                <i class="fas fa-edit"></i>
                                            </button>
                                            <button class="btn btn-sm btn-outline-danger btn-eliminar-plantilla"
                                                    data-id="${plantilla.Id}" data-titulo="${tituloEscapado}"
                                                    title="Eliminar plantilla">
                                                <i class="fas fa-trash-can"></i>
                                            </button>
                                        </div>
                                    </div>
                                </div>`;
                        });
                        contenedor.html(html);
                        contenedor.find('.plantilla-item').on('click', function () {
                            const $item = $(this);
                            ModuloHistorialConsultas.SeleccionarPlantilla($item.data('tipo'), $item.data('id'), $item.data('titulo'), $item.data('contenido'));
                        });
                        contenedor.find('.btn-editar-plantilla').on('click', function (e) {
                            e.stopPropagation();
                            const $btn = $(this);
                            ModuloHistorialConsultas.EditarPlantilla($btn.data('id'), $btn.data('titulo'), $btn.data('contenido'));
                        });
                        contenedor.find('.btn-eliminar-plantilla').on('click', function (e) {
                            e.stopPropagation();
                            const $btn = $(this);
                            ModuloHistorialConsultas.EliminarPlantilla($btn.data('id'), $btn.data('titulo'));
                        });
                        $('#btnMostrarFormulario').show();
                    } else {
                        contenedor.html('<div class="alert alert-info"><i class="fas fa-info-circle"></i> No hay plantillas disponibles para este tipo.</div>');
                        $('#btnMostrarFormulario').show();
                    }
                },
                error: function () {
                    contenedor.html('<div class="alert alert-danger"><i class="fas fa-triangle-exclamation"></i> Error al cargar las plantillas.</div>');
                }
            });
        },

        SeleccionarPlantilla: function (tipo, id, titulo, contenido) {
            let textareaId = '';
            if (tipo === 'Anamnesis') textareaId = '#inputAnamnesis';
            else if (tipo === 'ExamenFisico') textareaId = '#inputExamenFisico';
            else if (tipo === 'Indicaciones') textareaId = '#inputIndicaciones';

            if (textareaId) {
                const textarea = $(textareaId);
                const contenidoActual = textarea.val();
                const contenidoDecodificado = contenido
                    .replace(/&#39;/g, "'").replace(/&quot;/g, '"').replace(/\\n/g, '\n');

                if (contenidoActual.trim() !== '') {
                    textarea.val(contenidoActual + '\n\n' + contenidoDecodificado);
                } else {
                    textarea.val(contenidoDecodificado);
                }

                const modalElement = document.getElementById('modalPlantillas');
                const modal = bootstrap.Modal.getInstance(modalElement);
                if (modal) modal.hide();

                setTimeout(function () {
                    document.querySelectorAll('.modal-backdrop').forEach(function (b) { b.remove(); });
                    document.body.classList.remove('modal-open');
                    document.body.style.overflow = '';
                    document.body.style.paddingRight = '';
                }, 100);

                Swal.fire({ icon: 'success', title: 'Plantilla aplicada', text: `La plantilla "${titulo}" ha sido aplicada correctamente.`, timer: 2000, showConfirmButton: false });
            }
        },

        MostrarFormularioCrear: function () {
            $('#inputTituloPlantilla').val('');
            $('#inputContenidoPlantilla').val('');
            $('#inputIdPlantillaEditar').val('');
            $('#tituloFormularioPlantilla').text('Crear Nueva Plantilla');
            $('#btnGuardarPlantilla').text('Guardar Plantilla');
            $('#formularioNuevaPlantilla').show();
            $('#btnMostrarFormulario').hide();
            $('#formularioNuevaPlantilla')[0].scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        },

        CancelarCrearPlantilla: function () {
            $('#formularioNuevaPlantilla').hide();
            $('#btnMostrarFormulario').show();
            $('#inputTituloPlantilla').val('');
            $('#inputContenidoPlantilla').val('');
            $('#inputIdPlantillaEditar').val('');
            $('#tituloFormularioPlantilla').text('Crear Nueva Plantilla');
            $('#btnGuardarPlantilla').text('Guardar Plantilla');
        },

        EditarPlantilla: function (id, titulo, contenido) {
            const contenidoDecodificado = typeof contenido === 'string'
                ? contenido.replace(/&quot;/g, '"').replace(/&#39;/g, "'").replace(/\\n/g, '\n')
                : contenido;
            $('#inputIdPlantillaEditar').val(id);
            $('#inputTituloPlantilla').val(titulo);
            $('#inputContenidoPlantilla').val(contenidoDecodificado);
            $('#tituloFormularioPlantilla').text('Editar Plantilla');
            $('#btnGuardarPlantilla').text('Actualizar Plantilla');
            $('#formularioNuevaPlantilla').show();
            $('#btnMostrarFormulario').hide();
            $('#formularioNuevaPlantilla')[0].scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        },

        GuardarNuevaPlantilla: function () {
            const tipo = $('#modalPlantillas').data('tipo-actual');
            const idEditar = $('#inputIdPlantillaEditar').val();
            const titulo = $('#inputTituloPlantilla').val().trim();
            const contenido = $('#inputContenidoPlantilla').val().trim();
            const esEdicion = idEditar && idEditar !== '';

            if (!titulo) {
                Swal.fire({ icon: 'warning', title: 'Campo requerido', text: 'Por favor ingrese un título para la plantilla.' });
                $('#inputTituloPlantilla').focus();
                return;
            }
            if (!contenido) {
                Swal.fire({ icon: 'warning', title: 'Campo requerido', text: 'Por favor ingrese el contenido de la plantilla.' });
                $('#inputContenidoPlantilla').focus();
                return;
            }

            Swal.fire({ title: esEdicion ? 'Actualizando...' : 'Guardando...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

            const url = esEdicion ? '/Consulta/ActualizarPlantilla' : '/Consulta/GuardarPlantilla';
            const data = esEdicion
                ? { id: idEditar, tipo: tipo, titulo: titulo, contenido: contenido }
                : { tipo: tipo, titulo: titulo, contenido: contenido };

            $.ajax({
                url: url, type: 'POST', data: data,
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: esEdicion ? 'Plantilla actualizada' : 'Plantilla guardada',
                            text: esEdicion ? 'La plantilla se ha actualizado correctamente.' : 'La plantilla se ha guardado correctamente.',
                            timer: 2000, showConfirmButton: false
                        }).then(() => { ModuloHistorialConsultas.AbrirModalPlantillas($('#modalPlantillas').data('tipo-actual') || tipo); });
                    } else {
                        Swal.fire({ icon: 'error', title: 'Error', text: response.message || 'Error al guardar la plantilla.' });
                    }
                },
                error: function () {
                    Swal.fire({ icon: 'error', title: 'Error', text: 'Error al guardar la plantilla. Por favor intente nuevamente.' });
                }
            });
        },

        EliminarPlantilla: function (id, titulo) {
            Swal.fire({
                icon: 'warning', title: '¿Eliminar plantilla?',
                text: `¿Está seguro que desea eliminar la plantilla "${titulo}"?`,
                showCancelButton: true, confirmButtonText: 'Sí, eliminar', cancelButtonText: 'Cancelar',
                confirmButtonColor: '#d33', cancelButtonColor: '#3085d6'
            }).then((result) => {
                if (result.isConfirmed) {
                    Swal.fire({ title: 'Eliminando...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });
                    $.ajax({
                        url: '/Consulta/EliminarPlantilla', type: 'POST', data: { id: id },
                        success: function (response) {
                            if (response.success) {
                                Swal.fire({ icon: 'success', title: 'Plantilla eliminada', text: 'La plantilla se ha eliminado correctamente.', timer: 2000, showConfirmButton: false })
                                    .then(() => { ModuloHistorialConsultas.AbrirModalPlantillas($('#modalPlantillas').data('tipo-actual')); });
                            } else {
                                Swal.fire({ icon: 'error', title: 'Error', text: response.message || 'Error al eliminar la plantilla.' });
                            }
                        },
                        error: function () {
                            Swal.fire({ icon: 'error', title: 'Error', text: 'Error al eliminar la plantilla. Por favor intente nuevamente.' });
                        }
                    });
                }
            });
        },
    }
})();

$(function () {
    ModuloHistorialConsultas.IniciarScripts();

    $("#inputPeso, #inputTalla").on("input", function () {
        ModuloHistorialConsultas.CalcularIMC();
    });

    if ($('#listaMedicamentosDetalle').length) {
        RenderizarRecetaDetalle();
    }
});

function RenderizarRecetaDetalle() {
    var container = $('#listaMedicamentosDetalle');
    if (!container.length) return;
    var raw = $('#hdnRecetaJson').val();
    if (!raw) {
        container.html('<p class="text-muted small fst-italic mb-0">Sin receta registrada.</p>');
        return;
    }

    var medicamentos = null;
    try { medicamentos = JSON.parse(raw); } catch (e) { }

    if (!Array.isArray(medicamentos) || medicamentos.length === 0) {
        container.html(
            '<div class="p-3 rounded border bg-white" style="white-space:pre-wrap;font-size:0.9rem;">' +
            $('<div>').text(raw).html() + '</div>'
        );
        return;
    }

    var items = medicamentos.map(function (m, i) {
        var dosisTag = m.dosis
            ? '<span class="badge me-1" style="background:#e8f4fa;color:#0E96CC;border:1px solid #b8dff0;font-weight:500;">' + m.dosis + '</span>'
            : '';
        var posTag = m.posologia
            ? '<span class="badge" style="background:#f0f0f0;color:#555;border:1px solid #ddd;font-weight:500;">' + m.posologia + '</span>'
            : '';
        return '<div class="d-flex align-items-center gap-2 px-3 py-2 mb-1 rounded border bg-white">' +
            '<span class="d-flex align-items-center justify-content-center rounded-circle flex-shrink-0 text-white fw-bold" ' +
            'style="width:26px;height:26px;font-size:0.7rem;background:#0E96CC;">' + (i + 1) + '</span>' +
            '<div class="flex-grow-1">' +
            '<div class="fw-semibold text-dark lh-sm" style="font-size:0.9rem;">' + m.medicamento + '</div>' +
            (dosisTag || posTag ? '<div class="mt-1">' + dosisTag + posTag + '</div>' : '') +
            '</div>' +
            '</div>';
    }).join('');

    container.html('<div class="overflow-hidden" style="background:#fafafa;">' + items + '</div>');
    $('#divBotonesReceta').show();
}

function ImprimirRecetaDetalle() {
    var raw = $('#hdnRecetaJson').val();
    var medicamentos = [];
    try { medicamentos = JSON.parse(raw); } catch (e) { }
    if (!Array.isArray(medicamentos) || medicamentos.length === 0) return;

    var nombreDoctor      = $('#hdnNombreDoctor').val() || '';
    var nombreInstitucion = $('#hdnNombreInstitucion').val() || 'FIMEL';
    var nombrePaciente    = $('#hdnNombrePaciente').val() || '';
    var rutPaciente       = $('#hdnRutPaciente').val() || '';
    var edadPaciente      = $('#hdnEdadPaciente').val() || '';
    var fechaConsulta     = $('#hdnFechaConsulta').val() || '';

    var medicamentosHtml = medicamentos.map(function (m, i) {
        return '<tr>' +
            '<td style="padding:6px 10px;border-bottom:1px solid #eee;">' + (i + 1) + '</td>' +
            '<td style="padding:6px 10px;border-bottom:1px solid #eee;"><strong>' + (m.medicamento || '') + '</strong></td>' +
            '<td style="padding:6px 10px;border-bottom:1px solid #eee;">' + (m.dosis || '') + '</td>' +
            '<td style="padding:6px 10px;border-bottom:1px solid #eee;">' + (m.posologia || '') + '</td>' +
            '</tr>';
    }).join('');

    var html = '<!DOCTYPE html><html><head><meta charset="utf-8"><title>Receta Medica</title>' +
        '<style>' +
        'body{font-family:Arial,sans-serif;margin:40px;color:#333;}' +
        'h2{color:#0E96CC;border-bottom:3px solid #0E96CC;padding-bottom:10px;margin-bottom:20px;}' +
        '.meta{width:100%;border-collapse:collapse;margin-bottom:20px;}' +
        '.meta td{padding:4px 0;vertical-align:top;}' +
        'table.meds{width:100%;border-collapse:collapse;}' +
        'table.meds th{background:#0E96CC;color:#fff;padding:8px 10px;text-align:left;}' +
        '.footer{margin-top:80px;text-align:right;border-top:1px solid #ccc;padding-top:15px;}' +
        '</style></head><body>' +
        '<h2>' + nombreInstitucion + ' &mdash; Receta M&eacute;dica</h2>' +
        '<table class="meta"><tr>' +
        '<td><strong>M&eacute;dico:</strong> ' + nombreDoctor + '</td>' +
        '<td style="text-align:right;"><strong>Fecha:</strong> ' + fechaConsulta + '</td>' +
        '</tr><tr>' +
        '<td><strong>Paciente:</strong> ' + nombrePaciente + '</td>' +
        '<td style="text-align:right;"><strong>RUT/Doc:</strong> ' + rutPaciente + '</td>' +
        '</tr><tr>' +
        '<td><strong>Edad:</strong> ' + edadPaciente + ' a&ntilde;os</td>' +
        '<td></td></tr></table>' +
        '<table class="meds"><thead><tr>' +
        '<th style="width:30px;">#</th><th>Medicamento</th><th>Dosis</th><th>Posolog&iacute;a</th>' +
        '</tr></thead><tbody>' + medicamentosHtml + '</tbody></table>' +
        '<div class="footer">' +
        '<p style="font-weight:bold;margin:0;">' + nombreDoctor + '</p>' +
        '<p style="color:#888;font-size:0.85rem;margin:4px 0 0;">' + nombreInstitucion + '</p>' +
        '</div>' +
        '</body></html>';

    var w = window.open('', '_blank', 'width=800,height=600');
    w.document.write(html);
    w.document.close();
    w.onafterprint = function () { w.close(); };
    setTimeout(function () { w.focus(); w.print(); }, 300);
}