var recetaMedicamentos = [];
var ordenExamenes = [];
var bateriaExamenesActual = [];
var idPacienteConsultaActual = 0;
var _catalogoExamenes = [];

var ModuloConsulta = (function () {
    return {
        IniciarScripts: function () {
            $("#inputRut").keypress(function (e) { onlyNumbersWithK(e); });

            $("#inputPeso").keypress(function (e) { onlyNumbersWithDot(e); });
            $("#inputTalla").keypress(function (e) { onlyNumbersWithDot(e); });


            $("#inputRut").keyup(function () {

                let tipoDocto = $('#comboTipoDocumento option:selected').val();
                if (tipoDocto != "RUT") {
                    return;
                }

                let cadena = $("#inputRut").val();
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
                $("#inputRut").val(concatenar);
            });

            $('#comboTipoDocumento').on('change', function (e) {
                let tipoDocto = $('#comboTipoDocumento option:selected').val();
                if (tipoDocto == "RUT") {
                    $("#inputRut").show();
                    $("#inputNumDocumento").hide();
                }
                else {
                    $("#inputRut").hide();
                    $("#inputNumDocumento").show();
                }
            });
        },
        BuscarPaciente: function (object) {
            var rutCompleto = $("#inputRut").val();
            let validacionRut;
            let urlBusqueda;

            let tipoDocto = $('#comboTipoDocumento option:selected').val();
            if (tipoDocto == "RUT") {

                if (rutCompleto.length == 0) {
                    Swal.fire('Ingresa un rut válido', '', '');
                    return;
                }

                validacionRut = validarRut(rutCompleto);

                if (validacionRut == "01" || validacionRut == "00") {
                    Swal.fire('Ingresa un rut válido', '', '');
                    return;
                }
                $("#hiddenRutPaciente").val(validacionRut);
                urlBusqueda = $('#hdnURL_BuscarPaciente').val();
            }
            else {
                validacionRut = $("#inputNumDocumento").val();
                $("#hiddenNumDocumento").val(validacionRut);
                $("#inputRut").val("");
                urlBusqueda = $('#hdnURL_BuscarPacientePorNumDocumento').val();
            }

            LimpiarFormularioConsulta();
            idPacienteConsultaActual = 0;
            recetaMedicamentos = [];
            ModuloConsulta.RenderizarMedicamentos();

            showLoading(object);

            $.ajax({
                url: urlBusqueda,
                data: tipoDocto == "RUT" ? {
                    rutPaciente: validacionRut
                } : {
                    numDoc: validacionRut
                },
                method: 'POST',
                cache: false,
                async: true,
                success: function (response, jqXHR) {
                    if (response != null) {
                        if (response.Id > 0) {
                            const nombreCompleto = response.Nombres + " " + response.PrimerApellido + " " + response.SegundoApellido;
                            $("#spanNombrePaciente").text(" - " + nombreCompleto);
                            $("#inputNombres").val(response.Nombres);
                            $("#inputPrimerApellido").val(response.PrimerApellido);
                            $("#inputSegundoApellido").val(response.SegundoApellido);
                            if (response.SexoBiologico === "M") {
                                $('#radioSexo1').prop('checked', true);
                            } else if (response.SexoBiologico === "F") {
                                $('#radioSexo2').prop('checked', true);
                            }

                            if (response.FechaNacimiento != null) {
                                let fechaNacimiento = new Date(response.FechaNacimiento);
                                let year = fechaNacimiento.getFullYear();
                                let month = ("0" + (fechaNacimiento.getMonth() + 1)).slice(-2);
                                let day = ("0" + fechaNacimiento.getDate()).slice(-2);
                                let fechaNacimientoString = `${year}-${month}-${day}`;
                                $("#inputFechaNacimiento").val(fechaNacimientoString);
                                $("#inputEdad").val(calcularEdad(response.FechaNacimiento));
                            }


                            $("#inputDireccion").val(response.Direccion);
                            $("#inputCelular").val(response.Celular);
                            $("#inputEmail").val(response.Email);
                            $("#comboNacionalidad").val(response.Nacionalidad);
                            $("#inputPrevision").val(response.Prevision);
                            $("#inputAntFamiliares").val(response.AntFamiliares);
                            $("#inputAntPersonales").val(response.AntPersonales);
                            $("#inputAntQuirurgicos").val(response.AntQuirurgicos);
                            $("#inputMedicamentos").val(response.Medicamentos);

                            $("#inputOrientacion").val(response.OrientacionSexual);
                            $("#inputIdentidad").val(response.IdentidadGenero);

                            let btnTabaco = document.getElementById('radioTabaco1');
                            let btnAlcohol = document.getElementById('radioAlcohol1');
                            let btnDrogas = document.getElementById('radioDrogas1');
                            let btnAlergias = document.getElementById('radioAlergias1');

                            if (response.Tabaco === "SI") {
                                btnTabaco.classList.add('active')
                            }
                            if (response.Alcohol === "SI") {
                                btnAlcohol.classList.add('active')
                            }
                            if (response.Drogas === "SI") {
                                btnDrogas.classList.add('active')
                            }
                            if (response.Alergias === "SI") {
                                btnAlergias.classList.add('active')
                            }

                            $("#inputTabaco").val(response.DescTabaco);
                            $("#inputAlcohol").val(response.DescAlcohol);
                            $("#inputDrogas").val(response.DescDrogas);
                            $("#inputAlergias").val(response.DescAlergias);

                            $("#inputGesta").val(response.Gesta);
                            $("#inputParto").val(response.Parto);
                            $("#inputAborto").val(response.Aborto);
                            $("#inputMenarquia").val(response.Menarquia);
                            $("#inputMenopausia").val(response.Menopausia);
                            $("#inputGrupoRH").val(response.GrupoRH);
                            $("#inputInmunizaciones").val(response.Inmunizaciones);

                            $("#comboReligion").val(response.Religion);
                            $("#comboRegimenAlimenticio").val(response.RegimenAlimenticio);

                            $("#btnGuardarPaciente").hide();
                            $("#btnActualizarPaciente").show();

                            ObtenerConsultasAnteriores(validacionRut, response.TipoDocumento);

                            idPacienteConsultaActual = response.Id;
                            ModuloConsulta.CargarExamenesConsulta(response.Id);
                            ModuloConsulta.CargarRecordatoriosConsulta(response.Id);

                            Swal.fire('Paciente encontrado', 'Se cargaron los datos del paciente', 'success')

                            $('#btnGuardarConsulta').prop('disabled', false);

                            $("#accordionFicha-historial").collapse("hide");
                            $("#accordionFicha-nuevaConsulta").collapse("show");
                            $("#divNuevaConsulta").fadeIn(500);

                        }
                        else {
                            Swal.fire({
                                title: 'Paciente nuevo',
                                text: 'Registra al paciente en "Ficha Paciente"',
                                icon: 'info',
                                confirmButtonText: 'Ir a Ficha Paciente'
                            }).then((result) => {
                                if (result.isConfirmed) {
                                    let urlFichaPaciente;
                                    if (tipoDocto == "RUT") {
                                        urlFichaPaciente = "/Pacientes/FichaPaciente?p=" + encodeURIComponent(rutCompleto);
                                    } else {
                                        urlFichaPaciente = "/Pacientes/FichaPaciente?numDoc=" + encodeURIComponent(validacionRut) + "&tipo=" + encodeURIComponent(tipoDocto);
                                    }
                                    window.location.replace(urlFichaPaciente);
                                }
                            });
                        }
                    } else {
                        Swal.fire({
                            title: 'Paciente nuevo',
                            text: 'Registra al paciente en "Ficha Paciente"',
                            icon: 'info',
                            confirmButtonText: 'Ir a Ficha Paciente'
                        }).then((result) => {
                            if (result.isConfirmed) {
                                let urlFichaPaciente;
                                if (tipoDocto == "RUT") {
                                    urlFichaPaciente = "/Pacientes/FichaPaciente?p=" + encodeURIComponent(rutCompleto);
                                } else {
                                    urlFichaPaciente = "/Pacientes/FichaPaciente?numDoc=" + encodeURIComponent(validacionRut) + "&tipo=" + encodeURIComponent(tipoDocto);
                                }
                                window.location.replace(urlFichaPaciente);
                            }
                        });
                    }
                    closeLoading(object);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    Swal.fire('Error', 'Ha ocurrido un error al buscar los datos del paciente', 'error');
                    closeLoading(object);
                }
            });
        },
        GrabarConsulta: function (object) {
            let rutPaciente = $("#hiddenRutPaciente").val();
            let numDocumento = $("#hiddenNumDocumento").val();
            Swal.fire({
                title: 'Grabar consulta',
                text: '¿Esta seguro de grabar los datos de la consulta?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Grabar',
            }).then((result) => {
                if (result.isConfirmed) {
                    var datosConsulta = ModuloConsulta.CapturarDatosConsulta(object);
                    if (!datosConsulta) { return }
                    $.ajax({
                        url: $('#hdnURL_GrabarConsulta').val(),
                        data: {
                            datosConsulta: JSON.stringify(datosConsulta),
                            rutPaciente: rutPaciente,
                            numDocumento: numDocumento
                        },
                        method: 'POST',
                        cache: false,
                        async: true,
                        success: function (response, jqXHR) {
                            if (response.Codigo === 200) {
                                closeLoading(object);
                                Swal.fire({
                                    title: 'Datos de Consulta Guardada!',
                                    icon: 'success',
                                }).then((result) => {
                                    if (result.isConfirmed) {
                                        // Limpiar la URL antes de recargar
                                        window.history.replaceState({}, document.title, window.location.pathname);
                                        location.reload();
                                    }
                                })
                            }
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

            if (!$("#comboTipoConsulta").val()) {
                Swal.fire('Ingrese el Tipo de Consulta', '', 'warning');
                return null;
            }
            objDatosConsulta["TipoConsultaId"] = $("#comboTipoConsulta").val() || null;
            objDatosConsulta["TipoConsulta"] = $("#comboTipoConsulta option:selected").text() || null;
            objDatosConsulta["Peso"] = $("#inputPeso").val() || null;
            objDatosConsulta["Talla"] = $("#inputTalla").val() || null;
            objDatosConsulta["IMC"] = $("#inputIMC").val() || null;
            objDatosConsulta["PresionArterial"] = $("#inputPresionArterial").val();
            objDatosConsulta["EstadoNutricional"] = $("#inputEstadoNutricional").val() || null;

            if (!$("#inputMotivoConsulta").val()) {
                Swal.fire('Ingrese el Motivo', '', 'warning');
                return null;
            }
            objDatosConsulta["MotivoConsulta"] = $("#inputMotivoConsulta").val() || null;
            objDatosConsulta["Anamnesis"] = $("#inputAnamnesis").val() || null;
            objDatosConsulta["ExamenFisico"] = $("#inputExamenFisico").val() || null;
            objDatosConsulta["Diagnostico"] = $("#inputDiagnostico").val() || null;
            objDatosConsulta["Indicaciones"] = $("#inputIndicaciones").val() || null;
            objDatosConsulta["Receta"] = recetaMedicamentos.length > 0 ? JSON.stringify(recetaMedicamentos) : null;
            objDatosConsulta["OrdenExamenes"] = ordenExamenes.length > 0 ? JSON.stringify(ordenExamenes) : null;
            objDatosConsulta["FechaConsulta"] = $("#inputFechaConsulta").val();

            if ($("#inputFechaProximoControl").val() != "") {
                objDatosConsulta["FechaProximoControl"] = $("#inputFechaProximoControl").val();
            }

            showLoading(object);
            return objDatosConsulta;
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

        SetFechaHoy: function (inputSelector) {
            const hoy = new Date();
            const yyyy = hoy.getFullYear();
            const mm = String(hoy.getMonth() + 1).padStart(2, '0');
            const dd = String(hoy.getDate()).padStart(2, '0');
            const fechaFormateada = `${yyyy}-${mm}-${dd}`;
            $(inputSelector).val(fechaFormateada);
        },

        AbrirModalPlantillas: function (tipo) {
            const modalElement = document.getElementById('modalPlantillas');
            let modal = bootstrap.Modal.getInstance(modalElement);
            
            // Si no existe instancia, crear una nueva
            if (!modal) {
                modal = new bootstrap.Modal(modalElement);
            }
            
            // Agregar listener para limpiar backdrop cuando se cierre el modal
            modalElement.addEventListener('hidden.bs.modal', function() {
                const backdrops = document.querySelectorAll('.modal-backdrop');
                backdrops.forEach(function(backdrop) {
                    backdrop.remove();
                });
                document.body.classList.remove('modal-open');
                document.body.style.overflow = '';
                document.body.style.paddingRight = '';
            }, { once: true });
            
            const modalTitle = document.getElementById('modalPlantillasLabel');
            const contenedor = $('#contenedorPlantillas');
            
            // Guardar el tipo actual para usarlo después
            $('#modalPlantillas').data('tipo-actual', tipo);
            
            // Ocultar formulario y botón de crear
            $('#formularioNuevaPlantilla').hide();
            $('#btnMostrarFormulario').hide();
            
            // Limpiar formulario y resetear modo edición
            $('#inputTituloPlantilla').val('');
            $('#inputContenidoPlantilla').val('');
            $('#inputIdPlantillaEditar').val('');
            $('#tituloFormularioPlantilla').text('Crear Nueva Plantilla');
            $('#btnGuardarPlantilla').text('Guardar Plantilla');
            
            // Actualizar título según el tipo
            let titulo = 'Plantillas';
            if (tipo === 'Anamnesis') {
                titulo = 'Plantillas de Anamnesis';
            } else if (tipo === 'ExamenFisico') {
                titulo = 'Plantillas de Examen Físico';
            } else if (tipo === 'Indicaciones') {
                titulo = 'Plantillas de Indicaciones';
            }
            modalTitle.textContent = titulo;
            
            // Mostrar loading
            contenedor.html('<div class="text-center p-4"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></div>');
            
            // Abrir modal
            modal.show();
            
            // Cargar plantillas
            $.ajax({
                url: '/Consulta/ObtenerPlantillasPorTipo',
                type: 'GET',
                data: { tipo: tipo },
                success: function (response) {
                    if (response.success && response.data && response.data.length > 0) {
                        let html = '';
                        response.data.forEach(function (plantilla, index) {
                            const contenidoEscapado = (plantilla.Contenido || '').replace(/"/g, '&quot;').replace(/'/g, '&#39;').replace(/\n/g, '\\n');
                            const tituloEscapado = (plantilla.Titulo || 'Sin título').replace(/"/g, '&quot;').replace(/'/g, '&#39;');
                            html += `
                                <div class="list-group-item plantilla-item-container">
                                    <div class="d-flex w-100 justify-content-between align-items-start">
                                        <div class="plantilla-item flex-grow-1" style="cursor: pointer;" 
                                             data-tipo="${tipo}" 
                                             data-id="${plantilla.Id}" 
                                             data-titulo="${tituloEscapado}" 
                                             data-contenido="${contenidoEscapado}">
                                            <h6 class="mb-1">${tituloEscapado}</h6>
                                            <p class="mb-1 text-muted small">${(plantilla.Contenido || '').substring(0, 100)}${(plantilla.Contenido || '').length > 100 ? '...' : ''}</p>
                                        </div>
                                        <div class="d-flex gap-1 ms-2">
                                            <button class="btn btn-sm btn-outline-primary btn-editar-plantilla" 
                                                    data-id="${plantilla.Id}" 
                                                    data-titulo="${tituloEscapado}" 
                                                    data-contenido="${contenidoEscapado}"
                                                    title="Editar plantilla">
                                                <i class="fas fa-edit"></i>
                                            </button>
                                            <button class="btn btn-sm btn-outline-danger btn-eliminar-plantilla" 
                                                    data-id="${plantilla.Id}" 
                                                    data-titulo="${tituloEscapado}"
                                                    title="Eliminar plantilla">
                                                <i class="fas fa-trash-can"></i>
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            `;
                        });
                        contenedor.html(html);
                        
                        // Agregar event listeners para seleccionar plantilla
                        contenedor.find('.plantilla-item').on('click', function() {
                            const $item = $(this);
                            ModuloConsulta.SeleccionarPlantilla(
                                $item.data('tipo'),
                                $item.data('id'),
                                $item.data('titulo'),
                                $item.data('contenido')
                            );
                        });
                        
                        // Agregar event listeners para editar plantilla
                        contenedor.find('.btn-editar-plantilla').on('click', function(e) {
                            e.stopPropagation();
                            const $btn = $(this);
                            ModuloConsulta.EditarPlantilla(
                                $btn.data('id'),
                                $btn.data('titulo'),
                                $btn.data('contenido')
                            );
                        });
                        
                        // Agregar event listeners para eliminar plantilla
                        contenedor.find('.btn-eliminar-plantilla').on('click', function(e) {
                            e.stopPropagation();
                            const $btn = $(this);
                            ModuloConsulta.EliminarPlantilla(
                                $btn.data('id'),
                                $btn.data('titulo')
                            );
                        });
                        
                        // Mostrar botón de crear siempre (también cuando hay plantillas)
                        $('#btnMostrarFormulario').show();
                    } else {
                        contenedor.html('<div class="alert alert-info"><i class="fas fa-info-circle"></i> No hay plantillas disponibles para este tipo.</div>');
                        // Mostrar botón para crear nueva plantilla
                        $('#btnMostrarFormulario').show();
                    }
                },
                error: function () {
                    contenedor.html('<div class="alert alert-danger"><i class="fas fa-triangle-exclamation"></i> Error al cargar las plantillas.</div>');
                }
            });
        },

        SeleccionarPlantilla: function (tipo, id, titulo, contenido) {
            // Determinar el textarea según el tipo
            let textareaId = '';
            if (tipo === 'Anamnesis') {
                textareaId = '#inputAnamnesis';
            } else if (tipo === 'ExamenFisico') {
                textareaId = '#inputExamenFisico';
            } else if (tipo === 'Indicaciones') {
                textareaId = '#inputIndicaciones';
            }
            
            if (textareaId) {
                const textarea = $(textareaId);
                const contenidoActual = textarea.val();
                
                // Decodificar el contenido (revertir el escape)
                const contenidoDecodificado = contenido
                    .replace(/&#39;/g, "'")
                    .replace(/&quot;/g, '"')
                    .replace(/\\n/g, '\n');
                
                // Si ya hay contenido, agregar con salto de línea
                if (contenidoActual.trim() !== '') {
                    textarea.val(contenidoActual + '\n\n' + contenidoDecodificado);
                } else {
                    textarea.val(contenidoDecodificado);
                }
                
                // Cerrar modal completamente
                const modalElement = document.getElementById('modalPlantillas');
                const modal = bootstrap.Modal.getInstance(modalElement);
                
                if (modal) {
                    modal.hide();
                }
                
                // Asegurarse de eliminar el backdrop si existe
                setTimeout(function() {
                    const backdrops = document.querySelectorAll('.modal-backdrop');
                    backdrops.forEach(function(backdrop) {
                        backdrop.remove();
                    });
                    // Remover la clase modal-open del body
                    document.body.classList.remove('modal-open');
                    document.body.style.overflow = '';
                    document.body.style.paddingRight = '';
                }, 100);
                
                // Mostrar mensaje de éxito
                Swal.fire({
                    icon: 'success',
                    title: 'Plantilla aplicada',
                    text: `La plantilla "${titulo}" ha sido aplicada correctamente.`,
                    timer: 2000,
                    showConfirmButton: false
                });
            }
        },

        MostrarFormularioCrear: function () {
            // Limpiar formulario y resetear modo edición
            $('#inputTituloPlantilla').val('');
            $('#inputContenidoPlantilla').val('');
            $('#inputIdPlantillaEditar').val('');
            $('#tituloFormularioPlantilla').text('Crear Nueva Plantilla');
            $('#btnGuardarPlantilla').text('Guardar Plantilla');
            
            // Mostrar formulario
            $('#formularioNuevaPlantilla').show();
            $('#btnMostrarFormulario').hide();
            
            // Scroll al formulario
            $('#formularioNuevaPlantilla')[0].scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        },

        CancelarCrearPlantilla: function () {
            $('#formularioNuevaPlantilla').hide();
            $('#btnMostrarFormulario').show();
            // Limpiar formulario y resetear modo edición
            $('#inputTituloPlantilla').val('');
            $('#inputContenidoPlantilla').val('');
            $('#inputIdPlantillaEditar').val('');
            $('#tituloFormularioPlantilla').text('Crear Nueva Plantilla');
            $('#btnGuardarPlantilla').text('Guardar Plantilla');
        },

        EditarPlantilla: function (id, titulo, contenido) {
            // Decodificar el contenido si viene escapado
            const contenidoDecodificado = typeof contenido === 'string' 
                ? contenido.replace(/&quot;/g, '"').replace(/&#39;/g, "'").replace(/\\n/g, '\n')
                : contenido;
            
            // Cargar datos en el formulario
            $('#inputIdPlantillaEditar').val(id);
            $('#inputTituloPlantilla').val(titulo);
            $('#inputContenidoPlantilla').val(contenidoDecodificado);
            
            // Cambiar título y botón
            $('#tituloFormularioPlantilla').text('Editar Plantilla');
            $('#btnGuardarPlantilla').text('Actualizar Plantilla');
            
            // Mostrar formulario
            $('#formularioNuevaPlantilla').show();
            $('#btnMostrarFormulario').hide();
            
            // Scroll al formulario
            $('#formularioNuevaPlantilla')[0].scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        },

        GuardarNuevaPlantilla: function () {
            const tipo = $('#modalPlantillas').data('tipo-actual');
            const idEditar = $('#inputIdPlantillaEditar').val();
            const titulo = $('#inputTituloPlantilla').val().trim();
            const contenido = $('#inputContenidoPlantilla').val().trim();
            const esEdicion = idEditar && idEditar !== '';

            // Validaciones
            if (!titulo) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Campo requerido',
                    text: 'Por favor ingrese un título para la plantilla.'
                });
                $('#inputTituloPlantilla').focus();
                return;
            }

            if (!contenido) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Campo requerido',
                    text: 'Por favor ingrese el contenido de la plantilla.'
                });
                $('#inputContenidoPlantilla').focus();
                return;
            }

            // Mostrar loading
            Swal.fire({
                title: esEdicion ? 'Actualizando...' : 'Guardando...',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            // Determinar URL y método según si es edición o creación
            const url = esEdicion ? '/Consulta/ActualizarPlantilla' : '/Consulta/GuardarPlantilla';
            const data = esEdicion 
                ? { id: idEditar, tipo: tipo, titulo: titulo, contenido: contenido }
                : { tipo: tipo, titulo: titulo, contenido: contenido };

            // Guardar o actualizar plantilla
            $.ajax({
                url: url,
                type: 'POST',
                data: data,
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: esEdicion ? 'Plantilla actualizada' : 'Plantilla guardada',
                            text: esEdicion ? 'La plantilla se ha actualizado correctamente.' : 'La plantilla se ha guardado correctamente.',
                            timer: 2000,
                            showConfirmButton: false
                        }).then(() => {
                            // Recargar plantillas y ocultar formulario
                            const tipoActual = $('#modalPlantillas').data('tipo-actual');
                            ModuloConsulta.AbrirModalPlantillas(tipoActual || tipo);
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: response.message || (esEdicion ? 'Error al actualizar la plantilla.' : 'Error al guardar la plantilla.')
                        });
                    }
                },
                error: function () {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: esEdicion ? 'Error al actualizar la plantilla. Por favor intente nuevamente.' : 'Error al guardar la plantilla. Por favor intente nuevamente.'
                    });
                }
            });
        },

        AgregarMedicamento: function () {
            var medicamento = $('#inputNuevoMedicamento').val().trim();
            var dosis = $('#inputNuevaDosis').val().trim();
            var posologia = $('#inputNuevaPosologia').val().trim();
            if (!medicamento) {
                Swal.fire('Campo requerido', 'Ingrese el nombre del medicamento.', 'warning');
                return;
            }
            recetaMedicamentos.push({ medicamento: medicamento, dosis: dosis, posologia: posologia });
            $('#inputNuevoMedicamento').val('');
            $('#inputNuevaDosis').val('');
            $('#inputNuevaPosologia').val('');
            ModuloConsulta.RenderizarMedicamentos();
        },

        EliminarMedicamento: function (index) {
            recetaMedicamentos.splice(index, 1);
            ModuloConsulta.RenderizarMedicamentos();
        },

        RenderizarMedicamentos: function () {
            var container = $('#listaMedicamentosConsulta');
            if (recetaMedicamentos.length === 0) {
                container.html('<p class="text-muted small fst-italic mb-0">Sin medicamentos agregados.</p>');
                return;
            }
            var items = recetaMedicamentos.map(function (m, i) {
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
                    '<button type="button" class="btn p-0 flex-shrink-0 text-danger" onclick="ModuloConsulta.EliminarMedicamento(' + i + ')" title="Eliminar" style="font-size:1rem;line-height:1;">' +
                    '<i class="fas fa-times-circle"></i></button>' +
                    '</div>';
            }).join('');
            container.html('<div class="overflow-hidden" style="background:#fafafa;">' + items + '</div>');
        },

        ImprimirReceta: function () {
            if (recetaMedicamentos.length === 0) {
                Swal.fire('Sin medicamentos', 'Agregue al menos un medicamento a la receta.', 'warning');
                return;
            }
            var nombreDoctor = $('#hdnNombreDoctor').val() || '';
            var tituloProfesional = $('#hdnTituloProfesional').val() || 'Matrón/a';
            var nombreInstitucion = $('#hdnNombreInstitucion').val() || 'FIMEL';
            var nombrePaciente = ($('#inputNombres').val() + ' ' + $('#inputPrimerApellido').val()).trim();
            var tipoDoc = $('#comboTipoDocumento').val();
            var rutPaciente;
            if (tipoDoc === 'RUT') {
                var rutNum = parseInt($('#hiddenRutPaciente').val());
                var dv = getDV(rutNum).toString();
                rutPaciente = ObtenerRutSTR(rutNum, dv);
            } else {
                rutPaciente = $('#hiddenNumDocumento').val() || '';
            }
            var edadPaciente = $('#inputEdad').val() || '';
            var fechaConsulta = $('#inputFechaConsulta').val() || new Date().toISOString().split('T')[0];
            var partesFecha = fechaConsulta.split('-');
            var fechaFormateada = partesFecha.length === 3 ? partesFecha[2] + '/' + partesFecha[1] + '/' + partesFecha[0] : fechaConsulta;

            var ahora = new Date();
            var horaFormateada = String(ahora.getHours()).padStart(2, '0') + ':' + String(ahora.getMinutes()).padStart(2, '0');

            var logoBase64 = $('#hdnLogoInstitucion').val();
            var logoUrl = logoBase64
                ? 'data:image/png;base64,' + logoBase64
                : (window.location.origin + '/img/logo_fimel_correo.png');

            var medicamentosHtml = recetaMedicamentos.map(function (m, i) {
                var detalle = [];
                if (m.dosis) detalle.push(m.dosis);
                if (m.posologia) detalle.push(m.posologia);
                return '<li class="med-item">' +
                    '<div class="med-bullet">' + (i + 1) + '</div>' +
                    '<div class="med-body">' +
                        '<div class="med-nombre">' + m.medicamento + '</div>' +
                        (detalle.length ? '<div class="med-detalle">' + detalle.join(' &nbsp;·&nbsp; ') + '</div>' : '') +
                    '</div>' +
                '</li>';
            }).join('');

            fetch('/mails/receta-medica.html?v=' + Date.now())
                .then(function (r) { return r.text(); })
                .then(function (template) {
                    var html = template
                        .replace(/\{\{logo_url\}\}/g, logoUrl)
                        .replace(/\{\{institucion\}\}/g, nombreInstitucion)
                        .replace(/\{\{titulo\}\}/g, tituloProfesional)
                        .replace(/\{\{doctor\}\}/g, nombreDoctor)
                        .replace(/\{\{fecha\}\}/g, fechaFormateada)
                        .replace(/\{\{hora\}\}/g, horaFormateada)
                        .replace(/\{\{paciente\}\}/g, nombrePaciente)
                        .replace(/\{\{rut_doc\}\}/g, rutPaciente)
                        .replace(/\{\{edad\}\}/g, edadPaciente)
                        .replace(/\{\{medicamentos\}\}/g, medicamentosHtml);

                    var w = window.open('', '_blank', 'width=800,height=600');
                    w.document.write(html);
                    w.document.close();
                });
        },

        EnviarRecetaCorreo: function (btn) {
            if (recetaMedicamentos.length === 0) {
                Swal.fire('Sin medicamentos', 'Agregue al menos un medicamento a la receta.', 'warning');
                return;
            }
            var email = $('#inputEmail').val();
            if (!email) {
                Swal.fire('Sin correo', 'El paciente no tiene un correo electrónico registrado.', 'warning');
                return;
            }
            showLoading(btn);
            $.ajax({
                url: $('#hdnURL_EnviarReceta').val(),
                method: 'POST',
                data: {
                    emailPaciente: email,
                    nombrePaciente: ($('#inputNombres').val() + ' ' + $('#inputPrimerApellido').val()).trim(),
                    rutPaciente: $('#hiddenRutPaciente').val() || $('#hiddenNumDocumento').val() || '',
                    edadPaciente: $('#inputEdad').val() || '',
                    fechaConsulta: $('#inputFechaConsulta').val(),
                    medicamentosJson: JSON.stringify(recetaMedicamentos)
                },
                success: function (response) {
                    closeLoading(btn);
                    if (response.success) {
                        Swal.fire('Correo enviado', response.message, 'success');
                    } else {
                        Swal.fire('Error', response.message || 'Error al enviar el correo.', 'error');
                    }
                },
                error: function () {
                    closeLoading(btn);
                    Swal.fire('Error', 'Error al enviar el correo.', 'error');
                }
            });
        },

        EnviarOrdenExamenesCorreo: function (btn) {
            if (ordenExamenes.length === 0) {
                Swal.fire('Sin exámenes', 'Agregue al menos un examen a la orden.', 'warning');
                return;
            }
            var email = $('#inputEmail').val();
            if (!email) {
                Swal.fire('Sin correo', 'El paciente no tiene un correo electrónico registrado.', 'warning');
                return;
            }
            showLoading(btn);
            $.ajax({
                url: $('#hdnURL_EnviarOrdenExamenes').val(),
                method: 'POST',
                data: {
                    emailPaciente: email,
                    nombrePaciente: ($('#inputNombres').val() + ' ' + $('#inputPrimerApellido').val()).trim(),
                    rutPaciente: $('#hiddenRutPaciente').val() || $('#hiddenNumDocumento').val() || '',
                    edadPaciente: $('#inputEdad').val() || '',
                    fechaConsulta: $('#inputFechaConsulta').val(),
                    examenesJson: JSON.stringify(ordenExamenes)
                },
                success: function (response) {
                    closeLoading(btn);
                    if (response.success) {
                        Swal.fire('Correo enviado', response.message, 'success');
                    } else {
                        Swal.fire('Error', response.message || 'Error al enviar el correo.', 'error');
                    }
                },
                error: function () {
                    closeLoading(btn);
                    Swal.fire('Error', 'Error al enviar el correo.', 'error');
                }
            });
        },

        // ── BATERÍAS DE EXÁMENES ───────────────────────────────────────────────

        AbrirModalBateriasExamenes: function () {
            const modalElement = document.getElementById('modalBateriasExamenes');
            let modal = bootstrap.Modal.getInstance(modalElement);

            if (!modal) {
                modal = new bootstrap.Modal(modalElement);
            }

            modalElement.addEventListener('hidden.bs.modal', function () {
                const backdrops = document.querySelectorAll('.modal-backdrop');
                backdrops.forEach(function (backdrop) { backdrop.remove(); });
                document.body.classList.remove('modal-open');
                document.body.style.overflow = '';
                document.body.style.paddingRight = '';
            }, { once: true });

            const contenedor = $('#contenedorBaterias');

            // Ocultar formulario y botón de crear
            $('#formularioNuevaBateria').hide();
            $('#btnMostrarFormularioBateria').hide();

            // Limpiar formulario y resetear modo edición
            bateriaExamenesActual = [];
            $('#inputTituloBateria').val('');
            $('#inputIdBateriaEditar').val('');
            $('#tituloFormularioBateria').text('Crear Nueva Batería');
            $('#btnGuardarBateria').text('Guardar Batería');
            ModuloConsulta.RenderizarExamenesBateria();

            contenedor.html('<div class="text-center p-4"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></div>');

            modal.show();

            $.ajax({
                url: '/Consulta/ObtenerPlantillasPorTipo',
                type: 'GET',
                data: { tipo: 'BateriaExamenes' },
                success: function (response) {
                    if (response.success && response.data && response.data.length > 0) {
                        let html = '';
                        response.data.forEach(function (bateria) {
                            const contenidoEscapado = (bateria.Contenido || '[]').replace(/"/g, '&quot;').replace(/'/g, '&#39;');
                            const tituloEscapado = (bateria.Titulo || 'Sin título').replace(/"/g, '&quot;').replace(/'/g, '&#39;');

                            let resumen = '';
                            try {
                                const examenes = JSON.parse(bateria.Contenido || '[]');
                                resumen = examenes.map(function (e) { return e.examen; }).join(', ');
                            } catch (e) { resumen = ''; }

                            html += `
                                <div class="list-group-item bateria-item-container">
                                    <div class="d-flex w-100 justify-content-between align-items-start">
                                        <div class="bateria-item flex-grow-1" style="cursor: pointer;"
                                             data-id="${bateria.Id}"
                                             data-titulo="${tituloEscapado}"
                                             data-contenido="${contenidoEscapado}">
                                            <h6 class="mb-1">${tituloEscapado}</h6>
                                            <p class="mb-1 text-muted small">${resumen.substring(0, 100)}${resumen.length > 100 ? '...' : ''}</p>
                                        </div>
                                        <div class="d-flex gap-1 ms-2">
                                            <button class="btn btn-sm btn-outline-primary btn-editar-bateria"
                                                    data-id="${bateria.Id}"
                                                    data-titulo="${tituloEscapado}"
                                                    data-contenido="${contenidoEscapado}"
                                                    title="Editar batería">
                                                <i class="fas fa-edit"></i>
                                            </button>
                                            <button class="btn btn-sm btn-outline-danger btn-eliminar-bateria"
                                                    data-id="${bateria.Id}"
                                                    data-titulo="${tituloEscapado}"
                                                    title="Eliminar batería">
                                                <i class="fas fa-trash-can"></i>
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            `;
                        });
                        contenedor.html(html);

                        contenedor.find('.bateria-item').on('click', function () {
                            const $item = $(this);
                            ModuloConsulta.SeleccionarBateria(
                                $item.data('id'),
                                $item.data('titulo'),
                                $item.attr('data-contenido')
                            );
                        });

                        contenedor.find('.btn-editar-bateria').on('click', function (e) {
                            e.stopPropagation();
                            const $btn = $(this);
                            ModuloConsulta.EditarBateria(
                                $btn.data('id'),
                                $btn.data('titulo'),
                                $btn.attr('data-contenido')
                            );
                        });

                        contenedor.find('.btn-eliminar-bateria').on('click', function (e) {
                            e.stopPropagation();
                            const $btn = $(this);
                            ModuloConsulta.EliminarBateria(
                                $btn.data('id'),
                                $btn.data('titulo')
                            );
                        });

                        $('#btnMostrarFormularioBateria').show();
                    } else {
                        contenedor.html('<div class="alert alert-info"><i class="fas fa-info-circle"></i> No hay baterías de exámenes guardadas.</div>');
                        $('#btnMostrarFormularioBateria').show();
                    }
                },
                error: function () {
                    contenedor.html('<div class="alert alert-danger"><i class="fas fa-triangle-exclamation"></i> Error al cargar las baterías.</div>');
                }
            });
        },

        SeleccionarBateria: function (id, titulo, contenido) {
            const contenidoDecodificado = contenido.replace(/&#39;/g, "'").replace(/&quot;/g, '"');
            let examenes = [];
            try {
                examenes = JSON.parse(contenidoDecodificado || '[]');
            } catch (e) {
                Swal.fire('Error', 'No se pudo leer la batería de exámenes.', 'error');
                return;
            }

            examenes.forEach(function (ex) {
                ordenExamenes.push({ examen: ex.examen || '', indicaciones: ex.indicaciones || '' });
            });
            ModuloConsulta.RenderizarOrdenExamenes();

            const modalElement = document.getElementById('modalBateriasExamenes');
            const modal = bootstrap.Modal.getInstance(modalElement);
            if (modal) modal.hide();

            setTimeout(function () {
                const backdrops = document.querySelectorAll('.modal-backdrop');
                backdrops.forEach(function (backdrop) { backdrop.remove(); });
                document.body.classList.remove('modal-open');
                document.body.style.overflow = '';
                document.body.style.paddingRight = '';
            }, 100);

            Swal.fire({
                icon: 'success',
                title: 'Batería aplicada',
                text: `Se agregaron ${examenes.length} examen(es) de "${titulo}" a la orden.`,
                timer: 2000,
                showConfirmButton: false
            });
        },

        MostrarFormularioCrearBateria: function () {
            bateriaExamenesActual = [];
            $('#inputTituloBateria').val('');
            $('#inputExamenBateria').val('');
            $('#inputIndicacionesBateria').val('');
            $('#inputIdBateriaEditar').val('');
            $('#tituloFormularioBateria').text('Crear Nueva Batería');
            $('#btnGuardarBateria').text('Guardar Batería');
            ModuloConsulta.RenderizarExamenesBateria();

            $('#formularioNuevaBateria').show();
            $('#btnMostrarFormularioBateria').hide();

            $('#formularioNuevaBateria')[0].scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        },

        CancelarCrearBateria: function () {
            $('#formularioNuevaBateria').hide();
            $('#btnMostrarFormularioBateria').show();
            bateriaExamenesActual = [];
            $('#inputTituloBateria').val('');
            $('#inputIdBateriaEditar').val('');
            $('#tituloFormularioBateria').text('Crear Nueva Batería');
            $('#btnGuardarBateria').text('Guardar Batería');
        },

        EditarBateria: function (id, titulo, contenido) {
            const contenidoDecodificado = contenido.replace(/&#39;/g, "'").replace(/&quot;/g, '"');
            try {
                bateriaExamenesActual = JSON.parse(contenidoDecodificado || '[]');
            } catch (e) {
                bateriaExamenesActual = [];
            }

            $('#inputIdBateriaEditar').val(id);
            $('#inputTituloBateria').val(titulo);
            ModuloConsulta.RenderizarExamenesBateria();

            $('#tituloFormularioBateria').text('Editar Batería');
            $('#btnGuardarBateria').text('Actualizar Batería');

            $('#formularioNuevaBateria').show();
            $('#btnMostrarFormularioBateria').hide();

            $('#formularioNuevaBateria')[0].scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        },

        AgregarExamenBateria: function () {
            var examen = $('#inputExamenBateria').val().trim();
            var indicaciones = $('#inputIndicacionesBateria').val().trim();
            if (!examen) {
                Swal.fire('Campo requerido', 'Ingrese el nombre del examen.', 'warning');
                return;
            }
            bateriaExamenesActual.push({ examen: examen, indicaciones: indicaciones });
            $('#inputExamenBateria').val('');
            $('#inputIndicacionesBateria').val('');
            ModuloConsulta.RenderizarExamenesBateria();
        },

        EliminarExamenBateria: function (index) {
            bateriaExamenesActual.splice(index, 1);
            ModuloConsulta.RenderizarExamenesBateria();
        },

        RenderizarExamenesBateria: function () {
            var container = $('#listaExamenesBateria');
            if (bateriaExamenesActual.length === 0) {
                container.html('<p class="text-muted small fst-italic mb-0">Sin exámenes agregados.</p>');
                return;
            }
            var items = bateriaExamenesActual.map(function (e, i) {
                var indTag = e.indicaciones
                    ? '<span class="badge" style="background:#e8f4fa;color:#0E96CC;border:1px solid #b8dff0;font-weight:500;">' + e.indicaciones + '</span>'
                    : '';
                return '<div class="d-flex align-items-center gap-2 px-3 py-2 mb-1 rounded border bg-white">' +
                    '<span class="d-flex align-items-center justify-content-center rounded-circle flex-shrink-0 text-white fw-bold" ' +
                    'style="width:26px;height:26px;font-size:0.7rem;background:#0E96CC;">' + (i + 1) + '</span>' +
                    '<div class="flex-grow-1">' +
                    '<div class="fw-semibold text-dark lh-sm" style="font-size:0.9rem;">' + e.examen + '</div>' +
                    (indTag ? '<div class="mt-1">' + indTag + '</div>' : '') +
                    '</div>' +
                    '<button type="button" class="btn p-0 flex-shrink-0 text-danger" onclick="ModuloConsulta.EliminarExamenBateria(' + i + ')" title="Eliminar" style="font-size:1rem;line-height:1;">' +
                    '<i class="fas fa-times-circle"></i></button>' +
                    '</div>';
            }).join('');
            container.html('<div class="overflow-hidden" style="background:#fafafa;">' + items + '</div>');
        },

        GuardarNuevaBateria: function () {
            const idEditar = $('#inputIdBateriaEditar').val();
            const titulo = $('#inputTituloBateria').val().trim();
            const esEdicion = idEditar && idEditar !== '';

            if (!titulo) {
                Swal.fire({ icon: 'warning', title: 'Campo requerido', text: 'Por favor ingrese un título para la batería.' });
                $('#inputTituloBateria').focus();
                return;
            }

            if (bateriaExamenesActual.length === 0) {
                Swal.fire({ icon: 'warning', title: 'Campo requerido', text: 'Agregue al menos un examen a la batería.' });
                return;
            }

            Swal.fire({
                title: esEdicion ? 'Actualizando...' : 'Guardando...',
                allowOutsideClick: false,
                didOpen: () => { Swal.showLoading(); }
            });

            const contenido = JSON.stringify(bateriaExamenesActual);
            const url = esEdicion ? '/Consulta/ActualizarPlantilla' : '/Consulta/GuardarPlantilla';
            const data = esEdicion
                ? { id: idEditar, tipo: 'BateriaExamenes', titulo: titulo, contenido: contenido }
                : { tipo: 'BateriaExamenes', titulo: titulo, contenido: contenido };

            $.ajax({
                url: url,
                type: 'POST',
                data: data,
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: esEdicion ? 'Batería actualizada' : 'Batería guardada',
                            text: esEdicion ? 'La batería se ha actualizado correctamente.' : 'La batería se ha guardado correctamente.',
                            timer: 2000,
                            showConfirmButton: false
                        }).then(() => {
                            ModuloConsulta.AbrirModalBateriasExamenes();
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: response.message || (esEdicion ? 'Error al actualizar la batería.' : 'Error al guardar la batería.')
                        });
                    }
                },
                error: function () {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: esEdicion ? 'Error al actualizar la batería. Por favor intente nuevamente.' : 'Error al guardar la batería. Por favor intente nuevamente.'
                    });
                }
            });
        },

        EliminarBateria: function (id, titulo) {
            Swal.fire({
                icon: 'warning',
                title: '¿Eliminar batería?',
                text: `¿Está seguro que desea eliminar la batería "${titulo}"?`,
                showCancelButton: true,
                confirmButtonText: 'Sí, eliminar',
                cancelButtonText: 'Cancelar',
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6'
            }).then((result) => {
                if (result.isConfirmed) {
                    Swal.fire({
                        title: 'Eliminando...',
                        allowOutsideClick: false,
                        didOpen: () => { Swal.showLoading(); }
                    });

                    $.ajax({
                        url: '/Consulta/EliminarPlantilla',
                        type: 'POST',
                        data: { id: id },
                        success: function (response) {
                            if (response.success) {
                                Swal.fire({
                                    icon: 'success',
                                    title: 'Batería eliminada',
                                    text: 'La batería se ha eliminado correctamente.',
                                    timer: 2000,
                                    showConfirmButton: false
                                }).then(() => {
                                    ModuloConsulta.AbrirModalBateriasExamenes();
                                });
                            } else {
                                Swal.fire({
                                    icon: 'error',
                                    title: 'Error',
                                    text: response.message || 'Error al eliminar la batería.'
                                });
                            }
                        },
                        error: function () {
                            Swal.fire({
                                icon: 'error',
                                title: 'Error',
                                text: 'Error al eliminar la batería. Por favor intente nuevamente.'
                            });
                        }
                    });
                }
            });
        },

        SeleccionarSugerenciaExamenBateria: function (nombre) {
            $('#inputExamenBateria').val(nombre);
            $('#autocompleteExamenBateria').remove();
        },

        SeleccionarSugerenciaExamen: function (nombre) {
            $('#inputNuevoExamen').val(nombre);
            $('#autocompleteNuevoExamen').remove();
        },

        AgregarExamenOrden: function () {
            var examen = $('#inputNuevoExamen').val().trim();
            var indicaciones = $('#inputIndicacionesExamen').val().trim();
            if (!examen) {
                Swal.fire('Campo requerido', 'Ingrese el nombre del examen.', 'warning');
                return;
            }
            ordenExamenes.push({ examen: examen, indicaciones: indicaciones });
            $('#inputNuevoExamen').val('');
            $('#inputIndicacionesExamen').val('');
            ModuloConsulta.RenderizarOrdenExamenes();
        },

        EliminarExamenOrden: function (index) {
            ordenExamenes.splice(index, 1);
            ModuloConsulta.RenderizarOrdenExamenes();
        },

        RenderizarOrdenExamenes: function () {
            var container = $('#listaExamenesConsulta');
            if (ordenExamenes.length === 0) {
                container.html('<p class="text-muted small fst-italic mb-0">Sin exámenes agregados.</p>');
                return;
            }
            var items = ordenExamenes.map(function (e, i) {
                var indTag = e.indicaciones
                    ? '<span class="badge" style="background:#e8f4fa;color:#0E96CC;border:1px solid #b8dff0;font-weight:500;">' + e.indicaciones + '</span>'
                    : '';
                return '<div class="d-flex align-items-center gap-2 px-3 py-2 mb-1 rounded border bg-white">' +
                    '<span class="d-flex align-items-center justify-content-center rounded-circle flex-shrink-0 text-white fw-bold" ' +
                    'style="width:26px;height:26px;font-size:0.7rem;background:#0E96CC;">' + (i + 1) + '</span>' +
                    '<div class="flex-grow-1">' +
                    '<div class="fw-semibold text-dark lh-sm" style="font-size:0.9rem;">' + e.examen + '</div>' +
                    (indTag ? '<div class="mt-1">' + indTag + '</div>' : '') +
                    '</div>' +
                    '<button type="button" class="btn p-0 flex-shrink-0 text-danger" onclick="ModuloConsulta.EliminarExamenOrden(' + i + ')" title="Eliminar" style="font-size:1rem;line-height:1;">' +
                    '<i class="fas fa-times-circle"></i></button>' +
                    '</div>';
            }).join('');
            container.html('<div class="overflow-hidden" style="background:#fafafa;">' + items + '</div>');
        },

        ImprimirOrdenExamenes: function () {
            if (ordenExamenes.length === 0) {
                Swal.fire('Sin exámenes', 'Agregue al menos un examen a la orden.', 'warning');
                return;
            }
            var nombreDoctor = $('#hdnNombreDoctor').val() || '';
            var tituloProfesional = $('#hdnTituloProfesional').val() || 'Matrón/a';
            var nombreInstitucion = $('#hdnNombreInstitucion').val() || 'FIMEL';
            var nombrePaciente = ($('#inputNombres').val() + ' ' + $('#inputPrimerApellido').val()).trim();
            var tipoDoc = $('#comboTipoDocumento').val();
            var rutPaciente;
            if (tipoDoc === 'RUT') {
                var rutNum = parseInt($('#hiddenRutPaciente').val());
                var dv = getDV(rutNum).toString();
                rutPaciente = ObtenerRutSTR(rutNum, dv);
            } else {
                rutPaciente = $('#hiddenNumDocumento').val() || '';
            }
            var edadPaciente = $('#inputEdad').val() || '';
            var fechaConsulta = $('#inputFechaConsulta').val() || new Date().toISOString().split('T')[0];
            var partesFecha = fechaConsulta.split('-');
            var fechaFormateada = partesFecha.length === 3 ? partesFecha[2] + '/' + partesFecha[1] + '/' + partesFecha[0] : fechaConsulta;
            var ahora = new Date();
            var horaFormateada = String(ahora.getHours()).padStart(2, '0') + ':' + String(ahora.getMinutes()).padStart(2, '0');
            var logoBase64 = $('#hdnLogoInstitucion').val();
            var logoUrl = logoBase64
                ? 'data:image/png;base64,' + logoBase64
                : (window.location.origin + '/img/logo_fimel_correo.png');

            var examenesHtml = ordenExamenes.map(function (e, i) {
                return '<li class="exam-item">' +
                    '<div class="exam-bullet">' + (i + 1) + '</div>' +
                    '<div class="exam-body">' +
                    '<div class="exam-nombre">' + e.examen + '</div>' +
                    (e.indicaciones ? '<div class="exam-detalle">' + e.indicaciones + '</div>' : '') +
                    '</div>' +
                    '</li>';
            }).join('');

            fetch('/mails/orden-examenes.html?v=' + Date.now())
                .then(function (r) { return r.text(); })
                .then(function (template) {
                    var html = template
                        .replace(/\{\{logo_url\}\}/g, logoUrl)
                        .replace(/\{\{institucion\}\}/g, nombreInstitucion)
                        .replace(/\{\{titulo\}\}/g, tituloProfesional)
                        .replace(/\{\{doctor\}\}/g, nombreDoctor)
                        .replace(/\{\{fecha\}\}/g, fechaFormateada)
                        .replace(/\{\{hora\}\}/g, horaFormateada)
                        .replace(/\{\{paciente\}\}/g, nombrePaciente)
                        .replace(/\{\{rut_doc\}\}/g, rutPaciente)
                        .replace(/\{\{edad\}\}/g, edadPaciente)
                        .replace(/\{\{examenes\}\}/g, examenesHtml);

                    var w = window.open('', '_blank', 'width=800,height=600');
                    w.document.write(html);
                    w.document.close();
                });
        },

        CargarExamenesConsulta: function (idPaciente) {
            $.ajax({
                url: $('#hdnURL_ObtenerExamenes').val(),
                method: 'GET',
                data: { idPaciente: idPaciente },
                success: function (response) {
                    var container = $('#tablaExamenesConsultaContainer');
                    if (!response.success || !response.data || response.data.length === 0) {
                        container.html('<p class="text-muted small fst-italic mb-0">No hay exámenes registrados.</p>');
                        return;
                    }
                    var urlDescargar = $('#hdnURL_DescargarExamen').val();
                    var items = response.data.map(function (e) {
                        var fecha = e.Fecha ? new Date(e.Fecha) : null;
                        var fechaStr = fecha
                            ? ('0' + fecha.getDate()).slice(-2) + '/' + ('0' + (fecha.getMonth() + 1)).slice(-2) + '/' + fecha.getFullYear()
                            : '';
                        var iconClass = e.NombreArchivo ? 'fa-file-medical' : 'fa-clipboard-list';
                        var iconColor = e.NombreArchivo ? '#0E96CC' : '#aaa';
                        var btnDescarga = e.NombreArchivo
                            ? '<a href="' + urlDescargar + '?id=' + e.Id + '" target="_blank" class="btn btn-sm me-1" style="color:#0E96CC;background:#e8f4fa;border:1px solid #b8dff0;" title="Descargar archivo"><i class="fas fa-download"></i></a>'
                            : '';
                        var btnEliminar = '<button class="btn btn-sm" onclick="ModuloConsulta.EliminarExamenConsulta(' + e.Id + ',' + idPaciente + ')" title="Eliminar" style="color:#dc3545;background:#fff5f5;border:1px solid #f5c2c7;"><i class="fas fa-trash-can"></i></button>';
                        return '<div class="d-flex align-items-center gap-2 px-3 py-2 mb-1 rounded border bg-white">' +
                            '<i class="fas ' + iconClass + ' flex-shrink-0" style="color:' + iconColor + ';font-size:1.2rem;width:20px;text-align:center;"></i>' +
                            '<div class="flex-grow-1 min-w-0">' +
                            '<div class="fw-semibold text-dark lh-sm text-truncate" style="font-size:0.9rem;">' + (e.Descripcion || '') + '</div>' +
                            (fechaStr ? '<div class="text-muted mt-1" style="font-size:0.78rem;"><i class="fas fa-calendar-alt me-1"></i>' + fechaStr + '</div>' : '') +
                            '</div>' +
                            '<div class="d-flex gap-1 flex-shrink-0">' + btnDescarga + btnEliminar + '</div>' +
                            '</div>';
                    }).join('');
                    container.html('<div class="overflow-hidden" style="background:#fafafa;">' + items + '</div>');
                }
            });
        },

        GuardarExamenConsulta: function (btn) {
            if (idPacienteConsultaActual <= 0) {
                Swal.fire('Sin paciente', 'Busca un paciente primero.', 'warning');
                return;
            }
            var descripcion = $('#inputDescripcionExamenConsulta').val().trim();
            var fecha = $('#inputFechaExamenConsulta').val();
            if (!descripcion || !fecha) {
                Swal.fire('Campos requeridos', 'Ingrese descripción y fecha del examen.', 'warning');
                return;
            }
            showLoading(btn);
            var formData = new FormData();
            formData.append('idPaciente', idPacienteConsultaActual);
            formData.append('descripcion', descripcion);
            formData.append('fecha', fecha);
            var archivo = $('#inputArchivoExamenConsulta')[0].files[0];
            if (archivo) { formData.append('archivo', archivo); }
            $.ajax({
                url: $('#hdnURL_GuardarExamen').val(),
                method: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    closeLoading(btn);
                    if (response.success) {
                        $('#inputDescripcionExamenConsulta').val('');
                        $('#inputFechaExamenConsulta').val('');
                        $('#inputArchivoExamenConsulta').val('');
                        ModuloConsulta.CargarExamenesConsulta(idPacienteConsultaActual);
                        Swal.fire({ icon: 'success', title: 'Examen guardado', timer: 1500, showConfirmButton: false });
                    } else {
                        Swal.fire('Error', response.message || 'Error al guardar el examen.', 'error');
                    }
                },
                error: function () {
                    closeLoading(btn);
                    Swal.fire('Error', 'Error al guardar el examen.', 'error');
                }
            });
        },

        EliminarExamenConsulta: function (id, idPaciente) {
            Swal.fire({
                title: '¿Eliminar examen?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Sí, eliminar',
                cancelButtonText: 'Cancelar'
            }).then(function (result) {
                if (result.isConfirmed) {
                    $.ajax({
                        url: $('#hdnURL_EliminarExamen').val(),
                        method: 'POST',
                        data: { id: id },
                        success: function (response) {
                            if (response.success) {
                                ModuloConsulta.CargarExamenesConsulta(idPaciente);
                            }
                        }
                    });
                }
            });
        },

        EliminarPlantilla: function (id, titulo) {
            // Confirmar eliminación
            Swal.fire({
                icon: 'warning',
                title: '¿Eliminar plantilla?',
                text: `¿Está seguro que desea eliminar la plantilla "${titulo}"?`,
                showCancelButton: true,
                confirmButtonText: 'Sí, eliminar',
                cancelButtonText: 'Cancelar',
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Mostrar loading
                    Swal.fire({
                        title: 'Eliminando...',
                        allowOutsideClick: false,
                        didOpen: () => {
                            Swal.showLoading();
                        }
                    });

                    // Eliminar plantilla
                    $.ajax({
                        url: '/Consulta/EliminarPlantilla',
                        type: 'POST',
                        data: { id: id },
                        success: function (response) {
                            if (response.success) {
                                Swal.fire({
                                    icon: 'success',
                                    title: 'Plantilla eliminada',
                                    text: 'La plantilla se ha eliminado correctamente.',
                                    timer: 2000,
                                    showConfirmButton: false
                                }).then(() => {
                                    // Recargar plantillas
                                    const tipo = $('#modalPlantillas').data('tipo-actual');
                                    ModuloConsulta.AbrirModalPlantillas(tipo);
                                });
                            } else {
                                Swal.fire({
                                    icon: 'error',
                                    title: 'Error',
                                    text: response.message || 'Error al eliminar la plantilla.'
                                });
                            }
                        },
                        error: function () {
                            Swal.fire({
                                icon: 'error',
                                title: 'Error',
                                text: 'Error al eliminar la plantilla. Por favor intente nuevamente.'
                            });
                        }
                    });
                }
            });
        },

        AbrirModalRecordatorio: function () {
            if (idPacienteConsultaActual <= 0) {
                Swal.fire('Primero busque un paciente', '', 'warning');
                return;
            }
            $('#inputTituloRecordatorio').val('');
            $('#inputCuerpoRecordatorio').val('');
            $('#inputFechaInicioRecordatorio').val('');
            $('#selectRepetirCadaRecordatorio').val('Mensual');

            const modalElement = document.getElementById('modalRecordatorioManual');
            let modal = bootstrap.Modal.getInstance(modalElement);
            if (!modal) {
                modal = new bootstrap.Modal(modalElement);
            }
            modalElement.addEventListener('hidden.bs.modal', function () {
                const backdrops = document.querySelectorAll('.modal-backdrop');
                backdrops.forEach(function (backdrop) { backdrop.remove(); });
                document.body.classList.remove('modal-open');
                document.body.style.overflow = '';
                document.body.style.paddingRight = '';
            }, { once: true });
            modal.show();
        },

        GuardarRecordatorioManual: function (btn) {
            var titulo = $('#inputTituloRecordatorio').val().trim();
            var cuerpo = $('#inputCuerpoRecordatorio').val().trim();
            var fechaInicio = $('#inputFechaInicioRecordatorio').val();
            var repetirCada = $('#selectRepetirCadaRecordatorio').val();

            if (!titulo) { Swal.fire('Ingrese un título', '', 'warning'); return; }
            if (!cuerpo) { Swal.fire('Ingrese el cuerpo del recordatorio', '', 'warning'); return; }
            if (!fechaInicio) { Swal.fire('Ingrese la fecha de inicio', '', 'warning'); return; }

            showLoading(btn);
            $.ajax({
                url: $('#hdnURL_GuardarRecordatorioManual').val(),
                method: 'POST',
                data: {
                    idPaciente: idPacienteConsultaActual,
                    titulo: titulo,
                    cuerpo: cuerpo,
                    repetirCada: repetirCada,
                    fechaInicio: fechaInicio
                },
                success: function (response) {
                    closeLoading(btn);
                    if (response.success) {
                        const modalElement = document.getElementById('modalRecordatorioManual');
                        const modal = bootstrap.Modal.getInstance(modalElement);
                        if (modal) modal.hide();
                        Swal.fire('Recordatorio creado', response.message, 'success');
                        ModuloConsulta.CargarRecordatoriosConsulta(idPacienteConsultaActual);
                    } else {
                        Swal.fire('Error', response.message || 'Error al guardar el recordatorio.', 'error');
                    }
                },
                error: function () {
                    closeLoading(btn);
                    Swal.fire('Error', 'Error al guardar el recordatorio.', 'error');
                }
            });
        },

        CargarRecordatoriosConsulta: function (idPaciente) {
            var etiquetasRepetirCada = {
                'Semana': 'Cada semana',
                'Mensual': 'Mensual',
                'Trimestral': 'Trimestral',
                'Anual': 'Anual',
                '3Anios': 'Cada 3 años',
                '5Anios': 'Cada 5 años'
            };
            $.get($('#hdnURL_ObtenerRecordatorios').val(), { idPaciente: idPaciente }, function (response) {
                var container = $('#tablaRecordatoriosContainerConsulta');
                if (!response.success || !response.data || response.data.length === 0) {
                    container.html('<p class="text-muted small fst-italic mb-0">Sin recordatorios registrados.</p>');
                    return;
                }
                var items = response.data.map(function (r) {
                    var fecha = r.FechaProximoEnvio ? new Date(r.FechaProximoEnvio) : null;
                    var fechaStr = fecha
                        ? ('0' + fecha.getDate()).slice(-2) + '/' + ('0' + (fecha.getMonth() + 1)).slice(-2) + '/' + fecha.getFullYear()
                        : '';
                    var etiqueta = etiquetasRepetirCada[r.RepetirCada] || r.RepetirCada || '';
                    var cuerpoEsc = (r.Cuerpo || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
                    var btnEliminar = '<button class="btn btn-sm" onclick="ModuloConsulta.EliminarRecordatorioConsulta(' + r.Id + ',' + idPaciente + ')" title="Eliminar" style="color:#dc3545;background:#fff5f5;border:1px solid #f5c2c7;"><i class="fas fa-trash-can"></i></button>';
                    return '<div class="px-3 py-2 mb-1 rounded border bg-white">' +
                        '<div class="d-flex align-items-start gap-2">' +
                        '<i class="fas fa-bell flex-shrink-0 mt-1" style="color:#0E96CC;font-size:1rem;width:18px;text-align:center;"></i>' +
                        '<div class="flex-grow-1 min-w-0">' +
                        '<div class="fw-semibold text-dark lh-sm" style="font-size:0.88rem;">' + (r.Titulo || '') + '</div>' +
                        (cuerpoEsc ? '<div class="text-secondary mt-1" style="font-size:0.82rem;white-space:pre-line;">' + cuerpoEsc + '</div>' : '') +
                        '<div class="text-muted mt-1" style="font-size:0.75rem;">' +
                        '<i class="fas fa-rotate me-1"></i>' + etiqueta +
                        (fechaStr ? ' &middot; pr&oacute;ximo env&iacute;o: <strong>' + fechaStr + '</strong>' : '') +
                        '</div>' +
                        '</div>' +
                        '<div class="flex-shrink-0">' + btnEliminar + '</div>' +
                        '</div>' +
                        '</div>';
                }).join('');
                container.html('<div style="background:#fafafa;">' + items + '</div>');
            });
        },

        EliminarRecordatorioConsulta: function (id, idPaciente) {
            Swal.fire({
                title: '¿Eliminar recordatorio?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Sí, eliminar',
                cancelButtonText: 'Cancelar'
            }).then(function (result) {
                if (result.isConfirmed) {
                    $.post($('#hdnURL_EliminarRecordatorio').val(), { id: id }, function (response) {
                        if (response.success) {
                            ModuloConsulta.CargarRecordatoriosConsulta(idPaciente);
                        } else {
                            Swal.fire('Error', 'No se pudo eliminar el recordatorio.', 'error');
                        }
                    });
                }
            });
        }
    }
})();

$(function () {
    ModuloConsulta.IniciarScripts();

    $.ajax({
        url: $('#hdnURL_GetTiposConsulta').val(), method: 'GET',
        success: function (r) {
            if (r.success && r.data) {
                var $combo = $('#comboTipoConsulta');
                r.data.forEach(function (t) {
                    $combo.append('<option value="' + t.Id + '">' + t.Nombre + '</option>');
                });
            }
        }
    });

    $("#inputPeso, #inputTalla").on("input", function () {
        ModuloConsulta.CalcularIMC();
    });
    ModuloConsulta.SetFechaHoy('#inputFechaConsulta');

    $('#inputNuevoMedicamento, #inputNuevaDosis, #inputNuevaPosologia').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            ModuloConsulta.AgregarMedicamento();
        }
    });

    $('#inputNuevoExamen, #inputIndicacionesExamen').on('keydown', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); ModuloConsulta.AgregarExamenOrden(); }
    });

    $('#inputExamenBateria, #inputIndicacionesBateria').on('keydown', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); ModuloConsulta.AgregarExamenBateria(); }
    });

    var urlSug = $('#hdnURL_GetSugerenciasExamen').val();
    if (urlSug) {
        $.ajax({ url: urlSug, method: 'GET', success: function (resp) {
            if (resp.success && resp.data) _catalogoExamenes = resp.data;
        }});
    }

    $(document).on('input', '#inputNuevoExamen', function () {
        var q = $(this).val().trim().toLowerCase();
        $('#autocompleteNuevoExamen').remove();
        if (q.length < 2) return;
        var filtrado = _catalogoExamenes.filter(function (e) {
            return (e.NombreExamen || '').toLowerCase().indexOf(q) !== -1;
        }).slice(0, 20);
        if (!filtrado.length) return;
        var $dd = $('<div id="autocompleteNuevoExamen" class="list-group" style="position:absolute;z-index:9999;max-height:200px;overflow-y:auto;box-shadow:0 4px 12px rgba(0,0,0,.15);min-width:300px;"></div>');
        filtrado.forEach(function (e) {
            var nombreSafe = (e.NombreExamen || '').replace(/\\/g, '\\\\').replace(/'/g, "\\'");
            $dd.append(
                '<a href="#" class="list-group-item list-group-item-action py-1 px-2" ' +
                'onclick="ModuloConsulta.SeleccionarSugerenciaExamen(\'' + nombreSafe + '\'); return false;">' +
                '<span class="fw-semibold">' + (e.NombreExamen || '') + '</span>' +
                '<small class="text-muted ms-2">' + (e.CategoriaNombre || '') + '</small>' +
                '</a>'
            );
        });
        $('#inputNuevoExamen').after($dd);
    });

    $(document).on('input', '#inputExamenBateria', function () {
        var q = $(this).val().trim().toLowerCase();
        $('#autocompleteExamenBateria').remove();
        if (q.length < 2) return;
        var filtrado = _catalogoExamenes.filter(function (e) {
            return (e.NombreExamen || '').toLowerCase().indexOf(q) !== -1;
        }).slice(0, 20);
        if (!filtrado.length) return;
        var $dd = $('<div id="autocompleteExamenBateria" class="list-group" style="position:absolute;z-index:9999;max-height:200px;overflow-y:auto;box-shadow:0 4px 12px rgba(0,0,0,.15);min-width:300px;"></div>');
        filtrado.forEach(function (e) {
            var nombreSafe = (e.NombreExamen || '').replace(/\\/g, '\\\\').replace(/'/g, "\\'");
            $dd.append(
                '<a href="#" class="list-group-item list-group-item-action py-1 px-2" ' +
                'onclick="ModuloConsulta.SeleccionarSugerenciaExamenBateria(\'' + nombreSafe + '\'); return false;">' +
                '<span class="fw-semibold">' + (e.NombreExamen || '') + '</span>' +
                '<small class="text-muted ms-2">' + (e.CategoriaNombre || '') + '</small>' +
                '</a>'
            );
        });
        $('#inputExamenBateria').after($dd);
    });

    $(document).on('click', function (ev) {
        if (!$(ev.target).closest('#inputNuevoExamen, #autocompleteNuevoExamen').length)
            $('#autocompleteNuevoExamen').remove();
        if (!$(ev.target).closest('#inputExamenBateria, #autocompleteExamenBateria').length)
            $('#autocompleteExamenBateria').remove();
    });
});