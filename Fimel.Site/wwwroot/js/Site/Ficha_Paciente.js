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

            $("#inputRutData").keyup(function () {

                let cadena = $("#inputRutData").val();
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
                $("#inputRutData").val(concatenar);
            });

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
                data: {
                    rutPaciente: validacionRut
                },
                method: 'POST',
                cache: false,
                async: true,
                success: function (response, jqXHR) {
                    if (response != null) {
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

                            //$("#inputIdentidadGenero").val(response.IdentidadGenero);
                            //$("#inputOrientacionSexual").val(response.OrientacionSexual);

                            $("#inputRutData").val(ObtenerRutSTR(response.Rut, response.Dv));
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
                        }
                        else {
                            Swal.fire('Nuevo paciente', 'Favor complete los datos del nuevo paciente', 'info')

                            $("#btnGuardarPaciente").show();
                            $('#btnGuardarPaciente').prop('disabled', false);
                            $("#btnActualizarPaciente").hide();

                            $("#ListConsultasAnteriores").html('<li class="list-group-item">Sin consultas anteriores</li>');
                        }
                    } else {
                        Swal.fire('Nuevo Paciente', 'Completar los datos del nuevo paciente', 'info')
                    }
                    
                    $(".accordion-collapse").collapse("show");
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

            let validacionRut = validarRut($("#inputRutData").val());
            if (validacionRut == "01" || validacionRut == "00") {
                Swal.fire('Ingresa un Rut válido', 'Datos Paciente', 'warning');
                return null;
            }

            let tipoDocto = $('#comboTipoDocumento option:selected').val();
            if (tipoDocto == "RUT") {
                const rutData = $("#inputRutData").val();
                objDatosPaciente["Rut"] = rutData ? rutData.split("-")[0].replace(/\./g, "") : null;
                objDatosPaciente["Dv"] = rutData ? rutData.split("-")[1] : null;
            } else {
                objDatosPaciente["NumeroDocumento"] = $("#inputNumDocumento").val() || null;
            }

            let nombres = $("#inputNombres").val();
            if (!nombres) {
                Swal.fire('Ingrese un Nombre', 'Datos Paciente', 'warning');
                return null;
            }
            objDatosPaciente["Nombres"] = nombres || null;

            let primerApellido = $("#inputPrimerApellido").val();
            if (!primerApellido) {
                Swal.fire('Ingrese un Apellido Paterno', 'Datos Paciente', 'warning');
                return null;
            }
            objDatosPaciente["PrimerApellido"] = primerApellido || null;

            objDatosPaciente["SegundoApellido"] = $("#inputSegundoApellido").val() || null;

            let sexoBiologico = $('input[name="generos"]:checked').val();
            if (!sexoBiologico) {
                Swal.fire('Ingrese Género', 'Datos Paciente', 'warning');
                return null;
            }
            objDatosPaciente["SexoBiologico"] = sexoBiologico || null;

            let fechaNacimiento = $("#inputFechaNacimiento").val();
            if (!fechaNacimiento) {
                Swal.fire('Ingrese Fecha de Nacimiento', 'Datos Paciente', 'warning');
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
                Swal.fire('Ingrese Nacionalidad', 'Datos Paciente', 'warning');
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
        }
    }
})();

$(function () {
    ModuloFichaPaciente.IniciarScripts();
});