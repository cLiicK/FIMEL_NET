﻿var ModuloConsulta = (function () {
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
                                    window.location.replace("/Pacientes/FichaPaciente?p=" + rutCompleto);
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
                                window.location.replace("/Pacientes/FichaPaciente?p=" + rutCompleto);
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

            if (!$("#inputPeso").val()) {
                Swal.fire('Ingrese el Peso', '', 'warning');
                return null;
            }
            objDatosConsulta["Peso"] = $("#inputPeso").val() || null;

            if (!$("#inputTalla").val()) {
                Swal.fire('Ingrese la Talla', '', 'warning');
                return null;
            }
            objDatosConsulta["Talla"] = $("#inputTalla").val() || null;
            objDatosConsulta["IMC"] = $("#inputIMC").val() || null;

            objDatosConsulta["PresionArterial"] = $("#inputPresionArterial").val();

            if (!$("#inputEstadoNutricional").val()) {
                Swal.fire('Haga el calculo del IMC', '', 'warning');
                return null;
            }
            objDatosConsulta["EstadoNutricional"] = $("#inputEstadoNutricional").val() || null;

            if (!$("#inputMotivoConsulta").val()) {
                Swal.fire('Ingrese el Motivo', '', 'warning');
                return null;
            }
            objDatosConsulta["MotivoConsulta"] = $("#inputMotivoConsulta").val() || null;

            if (!$("#inputAnamnesis").val()) {
                Swal.fire('Ingrese Anamnesis', '', 'warning');
                return null;
            }
            objDatosConsulta["Anamnesis"] = $("#inputAnamnesis").val() || null;

            if (!$("#inputExamenFisico").val()) {
                Swal.fire('Ingrese el Examen Físico', '', 'warning');
                return null;
            }
            objDatosConsulta["ExamenFisico"] = $("#inputExamenFisico").val() || null;

            if (!$("#inputDiagnostico").val()) {
                Swal.fire('Ingrese un Diagnóstico', '', 'warning');
                return null;
            }
            objDatosConsulta["Diagnostico"] = $("#inputDiagnostico").val() || null;

            if (!$("#inputIndicaciones").val()) {
                Swal.fire('Ingrese las Indicaciones', '', 'warning');
                return null;
            }
            objDatosConsulta["Indicaciones"] = $("#inputIndicaciones").val() || null;

            objDatosConsulta["Receta"] = $("#inputReceta").val() || null;
            objDatosConsulta["OrdenExamenes"] = $("#inputOrdenExamenes").val();

            if (!$("#inputFechaConsulta").val()) {
                Swal.fire('Ingrese la Fecha de consulta', '', 'warning');
                return null;
            }
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