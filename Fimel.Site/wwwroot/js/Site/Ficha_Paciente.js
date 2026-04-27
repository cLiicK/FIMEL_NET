
var idPerfil = 0;
var idPacienteActual = 0;

var ModuloFichaPaciente = (function () {
    return {
        IniciarScripts: function () {
            //$("#navLinkFichaPaciente").addClass("active");
            $("#inputRut").keypress(function (e) { onlyNumbersWithK(e); });
            $("#inputRutData").keypress(function (e) { onlyNumbersWithK(e); });
            $("#inputCelular").keypress(function (e) { onlyNumbers(e); });
            $("#inputNombres").keypress(function (e) { onlyLetters(e); });
            $("#inputPrimerApellido").keypress(function (e) { onlyLetters(e); });
            $("#inputSegundoApellido").keypress(function (e) { onlyLetters(e); });
            $("#inputGesta").keypress(function (e) { onlyNumbers(e); });
            $("#inputParto").keypress(function (e) { onlyNumbers(e); });
            $("#inputAborto").keypress(function (e) { onlyNumbers(e); });


            $("#inputRut").keyup(function () {

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

            //$("#inputRutData").keyup(function () {

            //    let cadena = $("#inputRutData").val();
            //    cadena = cadena.replace(/[.]/gi, "").replace("-", "");
            //    if (cadena.length > 9) {
            //        cadena = cadena.substr(0, 9);
            //    }
            //    let concatenar = "";
            //    let i = cadena.length - 1;
            //    for (; i >= 0;) {
            //        concatenar = cadena[i] + concatenar;
            //        if (i + 1 == (cadena.length) && i > 0) {
            //            concatenar = "-" + concatenar;
            //        }
            //        if (concatenar.length == 9 && cadena.length > 7) {
            //            concatenar = "." + concatenar;
            //        }
            //        if (concatenar.length == 5 && cadena.length > 4) {
            //            concatenar = "." + concatenar;
            //        }
            //        i--;
            //    }
            //    $("#inputRutData").val(concatenar);
            //});

            var paramValue = getUrlParameter("p");
            if (paramValue !== null) {
                $("#inputRut").val(paramValue);
            };

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

            $('#inputFechaNacimiento').change(function () {
                $('#inputEdad').val(calcularEdad($('#inputFechaNacimiento').val()));
            });


        },
        BuscarPaciente: function (object) {
            var rutCompleto = $("#inputRut").val();
            let validacionRut;
            let urlBusqueda;
            let usuarioConectado;

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
                $("#hiddenRutPaciente").val(validacionRut);
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
                        usuarioConectado = response.UsuarioConectado;
                        if (response.Id > 0) {
                            const nombreCompleto = response.Nombres + " " + response.PrimerApellido + " " + response.SegundoApellido;
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

                            if (tipoDocto == "RUT") {
                                $("#inputRutData").val(ObtenerRutSTR(response.Rut, response.Dv));
                            } else {
                                $("#inputRutData").val(response.NumeroDocumento)
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

                            idPacienteActual = response.Id;
                            ObtenerConsultasAnteriores(validacionRut, response.TipoDocumento);
                            ModuloFichaPaciente.CargarExamenes(response.Id);

                            Swal.fire('Paciente encontrado', 'Se cargaron los datos del paciente', 'success')
                        }
                        else {
                            Swal.fire('Nuevo paciente', 'Favor complete los datos del nuevo paciente', 'info')

                            idPacienteActual = 0;
                            $('#btnGuardarPaciente').prop('disabled', false);
                            $("#btnGuardarPaciente").show();
                            $("#btnActualizarPaciente").hide();

                            $("#ListConsultasAnteriores").html('<li class="list-group-item">Sin consultas anteriores</li>');
                            $("#tablaExamenesContainer").html('<p class="text-muted" style="font-size:0.85rem">Sin exámenes registrados.</p>');

                            $("#inputRutData").val($("#inputRut").val());
                        }
                    } else {
                        Swal.fire('Nuevo paciente', 'Favor complete los datos del nuevo paciente', 'info')

                        idPacienteActual = 0;
                        $('#btnGuardarPaciente').prop('disabled', false);
                        $("#btnGuardarPaciente").show();
                        $("#btnActualizarPaciente").hide();

                        $("#ListConsultasAnteriores").html('<li class="list-group-item">Sin consultas anteriores</li>');
                        $("#tablaExamenesContainer").html('<p class="text-muted" style="font-size:0.85rem">Sin exámenes registrados.</p>');

                        $("#inputRutData").val($("#inputRut").val());
                    }

                    //$(".accordion-collapse").collapse("show");

                    ModuloFichaPaciente.BloquearCamposSegunPerfil(usuarioConectado);

                    $("#divFichaPaciente").fadeIn(500);

                    closeLoading(object);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    Swal.fire('Error', 'Ha ocurrido un error al buscar los datos del paciente', 'error');
                    closeLoading(object);
                }
            });
        },
        GrabarNuevoPaciente: function (object) {
            Swal.fire({
                title: 'Grabar nuevo paciente',
                text: '¿Esta seguro de grabar el nuevo paciente?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Grabar',
            }).then((result) => {
                if (result.isConfirmed) {
                    var datosPaciente = ModuloFichaPaciente.CapturarDatosPaciente(object);
                    if (!datosPaciente) { return }
                    $.ajax({
                        url: $('#hdnURL_GrabarNuevoPaciente').val(),
                        data: { datosPaciente: JSON.stringify(datosPaciente) },
                        method: 'POST',
                        cache: false,
                        async: false,
                        success: function (response, jqXHR) {
                            if (response.Codigo === 200) {
                                Swal.fire({
                                    title: 'Paciente Guardado!',
                                    icon: 'success',
                                        showCancelButton: true,
                                    confirmButtonText: 'OK',
                                    cancelButtonText: 'Iniciar Consulta',
                                    reverseButtons: true
                                }).then((result) => {
                                    if (result.isConfirmed) {
                                        // Limpiar la URL antes de recargar
                                        window.history.replaceState({}, document.title, window.location.pathname);
                                        location.reload();
                                    } else if (result.dismiss === Swal.DismissReason.cancel) {
                                        // Obtener el RUT del paciente recién creado
                                        let rutPaciente = '';
                                        let tipoDocumento = $('#comboTipoDocumento option:selected').val();
                                        
                                        if (tipoDocumento == "RUT") {
                                            rutPaciente = $("#inputRutData").val();
                                        } else {
                                            rutPaciente = $("#inputNumDocumento").val();
                                        }
                                        
                                        // Redirigir a nueva consulta con el RUT precargado
                                        let url = '/Consulta/NuevaConsulta?rut=' + encodeURIComponent(rutPaciente) + '&tipo=' + encodeURIComponent(tipoDocumento);
                                        window.location.href = url;
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
        ActualizarPaciente: function (object) {
            Swal.fire({
                title: 'Actualizar datos paciente',
                text: '¿Esta seguro de actualizar los datos del paciente?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Actualizar',
            }).then((result) => {
                if (result.isConfirmed) {
                    var datosPaciente = ModuloFichaPaciente.CapturarDatosPaciente(object);
                    if (!datosPaciente) { return }
                    $.ajax({
                        url: $('#hdnURL_ActualizarPaciente').val(),
                        data: { datosPaciente: JSON.stringify(datosPaciente) },
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
        CapturarDatosPaciente: function (object) {
            let objDatosPaciente = {};

            objDatosPaciente["TipoDocumento"] = $('#comboTipoDocumento option:selected').val() || null;

            let tipoDocto = $('#comboTipoDocumento option:selected').val();

            // Solo validar RUT si el tipo de documento es RUT
            if (tipoDocto == "RUT") {
                let validacionRut = validarRut($("#inputRutData").val());
                if (validacionRut == "01" || validacionRut == "00") {
                    Swal.fire('Ingresa un Rut válido', '', 'warning');
                    return null;
                }
            }
            if (tipoDocto == "RUT") {
                const rutData = $("#inputRutData").val();
                objDatosPaciente["Rut"] = rutData ? rutData.split("-")[0].replace(/\./g, "") : null;
                objDatosPaciente["Dv"] = rutData ? rutData.split("-")[1] : null;
            } else {
                objDatosPaciente["NumeroDocumento"] = $("#inputNumDocumento").val() || null;
            }

            let nombres = $("#inputNombres").val();
            if (!nombres) {
                Swal.fire('Ingrese un Nombre', '', 'warning');
                return null;
            }
            objDatosPaciente["Nombres"] = nombres || null;

            let primerApellido = $("#inputPrimerApellido").val();
            if (!primerApellido) {
                Swal.fire('Ingrese un Apellido Paterno', '', 'warning');
                return null;
            }
            objDatosPaciente["PrimerApellido"] = primerApellido || null;

            objDatosPaciente["SegundoApellido"] = $("#inputSegundoApellido").val() || null;

            if (idPerfil != 3) { //Administrativo
                let sexoBiologico = $('input[name="sexobiologico"]:checked').val();
                if (!sexoBiologico) {
                    Swal.fire('Ingrese Sexo Biologico', '', 'warning');
                    return null;
                }
                objDatosPaciente["SexoBiologico"] = sexoBiologico || null;
            }

            let fechaNacimiento = $("#inputFechaNacimiento").val();
            if (!fechaNacimiento) {
                Swal.fire('Ingrese Fecha de Nacimiento', '', 'warning');
                return null;
            }
            objDatosPaciente["FechaNacimiento"] = fechaNacimiento ? new Date(fechaNacimiento).toISOString() : null;

            let direccion = $("#inputDireccion").val();
            if (!direccion) {
                Swal.fire('Ingrese Dirección', 'Información de Contacto', 'warning');
                return null;
            }
            objDatosPaciente["Direccion"] = direccion || null;

            objDatosPaciente["Celular"] = $("#inputCelular").val() || null;

            let email = $("#inputEmail").val();
            if (!email) {
                Swal.fire('Ingrese un Correo Electrónico', 'Información de Contacto', 'warning');
                return null;
            }
            objDatosPaciente["Email"] = email || null;

            let nacionalidad = $('#comboNacionalidad').val();
            if (!nacionalidad) {
                Swal.fire('Ingrese Nacionalidad', '', 'warning');
                return null;
            }
            objDatosPaciente["Nacionalidad"] = nacionalidad || null;

            objDatosPaciente["Prevision"] = $("#inputPrevision").val() || null;
            objDatosPaciente["AntFamiliares"] = $("#inputAntFamiliares").val() || null;
            objDatosPaciente["AntPersonales"] = $("#inputAntPersonales").val() || null;
            objDatosPaciente["AntQuirurgicos"] = $("#inputAntQuirurgicos").val() || null;

            objDatosPaciente["Tabaco"] = document.getElementById('radioTabaco1').classList.contains('active') ? 'SI' : 'NO';
            objDatosPaciente["Alcohol"] = document.getElementById('radioAlcohol1').classList.contains('active') ? 'SI' : 'NO';
            objDatosPaciente["Drogas"] = document.getElementById('radioDrogas1').classList.contains('active') ? 'SI' : 'NO';
            objDatosPaciente["Alergias"] = document.getElementById('radioAlergias1').classList.contains('active') ? 'SI' : 'NO';

            objDatosPaciente["DescTabaco"] = $("#inputTabaco").val() || null;
            objDatosPaciente["DescAlcohol"] = $("#inputAlcohol").val() || null;
            objDatosPaciente["DescDrogas"] = $("#inputDrogas").val() || null;
            objDatosPaciente["DescAlergias"] = $("#inputAlergias").val() || null;

            objDatosPaciente["Gesta"] = $("#inputGesta").val() || null;
            objDatosPaciente["Parto"] = $("#inputParto").val() || null;
            objDatosPaciente["Aborto"] = $("#inputAborto").val() || null;
            objDatosPaciente["Menarquia"] = $("#inputMenarquia").val() || null;
            objDatosPaciente["Menopausia"] = $("#inputMenopausia").val() || null;

            objDatosPaciente["GrupoRH"] = $("#inputGrupoRH").val() || null;
            objDatosPaciente["Inmunizaciones"] = $("#inputInmunizaciones").val() || null;

            objDatosPaciente["Medicamentos"] = $("#inputMedicamentos").val() || null;
            objDatosPaciente["Religion"] = $('#comboReligion').val() || null;
            objDatosPaciente["RegimenAlimenticio"] = $('#comboRegimenAlimenticio').val() || null;

            objDatosPaciente["OrientacionSexual"] = $('#inputOrientacion').val() || null;
            objDatosPaciente["IdentidadGenero"] = $('#inputIdentidad').val() || null;

            showLoading(object);

            return objDatosPaciente;
        },
        CargarExamenes: function (idPaciente) {
            $.get($('#hdnURL_ObtenerExamenes').val(), { idPaciente: idPaciente }, function (response) {
                var container = $('#tablaExamenesContainer');
                if (!response.success || !response.data || response.data.length === 0) {
                    container.html('<p class="text-muted small fst-italic mb-0">Sin exámenes registrados.</p>');
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
                    var btnEliminar = '<button class="btn btn-sm" onclick="ModuloFichaPaciente.EliminarExamen(' + e.Id + ',' + idPaciente + ')" title="Eliminar" style="color:#dc3545;background:#fff5f5;border:1px solid #f5c2c7;"><i class="fas fa-trash-can"></i></button>';
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
                var $collapse = $('#accordionFicha-examenes');
                if (!$collapse.hasClass('show')) {
                    new bootstrap.Collapse($collapse[0], { toggle: false }).show();
                }
            });
        },
        GuardarExamen: function (btn) {
            if (idPacienteActual <= 0) {
                Swal.fire('Primero busque un paciente', '', 'warning');
                return;
            }
            var descripcion = $('#inputExamenDescripcion').val().trim();
            var fecha = $('#inputExamenFecha').val();
            if (!descripcion) { Swal.fire('Ingrese una descripción', '', 'warning'); return; }
            if (!fecha) { Swal.fire('Ingrese una fecha', '', 'warning'); return; }

            var formData = new FormData();
            formData.append('idPaciente', idPacienteActual);
            formData.append('descripcion', descripcion);
            formData.append('fecha', fecha);
            var archivo = $('#inputExamenArchivo')[0].files[0];
            if (archivo) formData.append('archivo', archivo);

            $(btn).prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-1"></i> Guardando...');

            $.ajax({
                url: $('#hdnURL_GuardarExamen').val(),
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    if (response.success) {
                        $('#inputExamenDescripcion').val('');
                        $('#inputExamenFecha').val('');
                        $('#inputExamenArchivo').val('');
                        ModuloFichaPaciente.CargarExamenes(idPacienteActual);
                        Swal.fire('Exámen guardado', response.message, 'success');
                    } else {
                        Swal.fire('Error', response.message, 'error');
                    }
                },
                error: function () {
                    Swal.fire('Error', 'No se pudo guardar el exámen.', 'error');
                },
                complete: function () {
                    $(btn).prop('disabled', false).html('<i class="fas fa-plus me-1"></i> Guardar Exámen');
                }
            });
        },
        EliminarExamen: function (id, idPaciente) {
            Swal.fire({
                title: 'Eliminar exámen',
                text: '¿Está seguro de eliminar este exámen?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Eliminar',
                cancelButtonText: 'Cancelar'
            }).then(function (result) {
                if (!result.isConfirmed) return;
                $.post($('#hdnURL_EliminarExamen').val(), { id: id }, function (response) {
                    if (response.success) {
                        ModuloFichaPaciente.CargarExamenes(idPaciente);
                    } else {
                        Swal.fire('Error', 'No se pudo eliminar el exámen.', 'error');
                    }
                });
            });
        },
        BloquearCamposSegunPerfil: function (object) {
            idPerfil = object.IdPerfil;
            if (object.IdPerfil == 3) { //Administrativo
                $('#divIdentidadGenero').hide();
                $('#divSexoBiologico').hide();
                $('#divOrientacionSexual').hide();
                $('#divRegimenAlimenticio').hide();

                $('#accordionFicha-antecedentes').collapse('hide');
                $('#accordionFicha-antecedentes-head .accordion-button').prop('disabled', true).addClass('disabled-visual');;

                $('#accordionFicha-antecedentesGine').collapse('hide');
                $('#accordionFicha-antecedentesGine-head .accordion-button').prop('disabled', true).addClass('disabled-visual');;

                $('a[name="linkConsultasAnteriores"]').hide();
            }

        }
    }
})();

$(function () {
    ModuloFichaPaciente.IniciarScripts();
});