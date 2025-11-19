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
            objDatosConsulta["TipoConsulta"] = $("#comboTipoConsulta").val() || null;
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
            objDatosConsulta["Receta"] = $("#inputReceta").val() || null;
            objDatosConsulta["OrdenExamenes"] = $("#inputOrdenExamenes").val();
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
        }
    }
})();

$(function () {
    ModuloConsulta.IniciarScripts();

    $("#inputPeso, #inputTalla").on("input", function () {
        ModuloConsulta.CalcularIMC();
    });
     ModuloConsulta.SetFechaHoy('#inputFechaConsulta');
});