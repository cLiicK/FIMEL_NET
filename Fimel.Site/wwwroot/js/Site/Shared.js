function ObtenerConsultasAnteriores(rutPaciente, tipoDocumento) {
    let urlBusqueda;
    if (tipoDocumento == "RUT") {
        urlBusqueda = $('#hdnURL_ObtenerConsultasAnteriores').val();
    }
    else {
        urlBusqueda = $('#hdnURL_ObtenerConsultasAnterioresPorNumDocumento').val();
    }
    $.ajax({
        url: urlBusqueda,
        data: {
            rutPaciente: rutPaciente
        },
        method: 'POST',
        cache: false,
        async: false,
        success: function (response, jqXHR) {
            if (response.length > 0) {
                for (var i = 0; i < response.length; i++) {


                    let timestamp;
                    if (/\/Date\((\d+)\)\//.test(response[i].FechaCreacion)) {
                        timestamp = parseInt(response[i].FechaCreacion.replace(/\/Date\((\d+)\)\//, '$1'), 10);
                    } else {
                        timestamp = Date.parse(response[i].FechaCreacion);
                    }
                    const date = new Date(timestamp);
                    const day = ('0' + date.getDate()).slice(-2);
                    const month = ('0' + (date.getMonth() + 1)).slice(-2);
                    const year = date.getFullYear();

                    let fechaSTR = `${day}-${month}-${year}`;

                    const encryptedId = btoa(response[i].Id.toString());
                    const url = `/Consulta/DetalleConsulta?idEncrypted=${encryptedId}`;

                    $("#ListConsultasAnteriores")
                        .append('<div class="row d-flex justify-content-between align-items-end mb-2">'+
                            '<div class="col">' +
                            '<p class="mb-0">' + response[i].TipoConsulta + '</p>' +
                            '</div>' +
                            '<div class="col">' +
                        '<p class="mb-0 text-center">' + fechaSTR + '</p>' +
                            '</div>' +
                            '<div class="col-md-2 text-center">' +
                        '<a class="btn-ico" href="' + url +'" target="_blank"><i class="fas fa-clipboard-list"></i></a>' +
                            '</div>'
                            + '</div>'
                        );

                }
            }
            else {
                $("#ListConsultasAnteriores").html('<li class="list-group-item">Sin consultas anteriores</li>');
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            Swal.fire('Error', 'Ha ocurrido un error al buscar las consultas anteriores', 'error');
        }
    });
}

function MostrarDetalleConsulta(idConsulta, object) {
    showLoading(object);
    $.ajax({
        url: $('#hdnURL_ObtenerConsulta').val(),
        data: {
            idConsulta: idConsulta
        },
        method: 'POST',
        cache: false,
        async: true,
        success: function (response, jqXHR) {
            if (response.Id > 0) {
                $('#modalDetalleConsulta').modal({ backdrop: 'static', keyboard: false }, 'show');
                $("#modalDetalleConsulta").modal("show");

                let fecha = new Date(parseInt(response.FechaCreacion.replace("Date", "").replace("/", "").replace("(", "").replace(")", "").replace("/", "")));
                var day = ("0" + fecha.getDate()).slice(-2);
                var month = ("0" + (fecha.getMonth() + 1)).slice(-2);
                var fechaString = day + " de " + fecha.toLocaleString('default', { month: 'long' }) + " del " + fecha.getFullYear()

                $("#lblFechaConsulta").html(fechaString + " - " + response.TipoConsulta);

                $("#inputTipoConsulta").val(response.TipoConsulta);
                $("#inputPesoAnteriores").val(response.Peso);
                $("#inputTallaAnteriores").val(response.Talla);
                $("#inputIMCAnteriores").val(response.IMC);
                $("#inputEstadoNutricionalAnteriores").val(response.EstadoNutricional);

                if (response.EstadoNutricional = "Obesidad") {
                    classEstadoNutricional = "form-control alert-danger";
                } else if (response.EstadoNutricional = "Sobrepeso") {
                    classEstadoNutricional = "form-control alert-warning";
                } else if (response.EstadoNutricional = "Normal") {
                    classEstadoNutricional = "form-control alert-success";
                } else {
                    classEstadoNutricional = "form-control alert-primary";
                }
                $("#inputEstadoNutricionalAnteriores").removeClass();
                $("#inputEstadoNutricionalAnteriores").addClass(classEstadoNutricional);

                $("textarea#lblMotivoConsulta").val(response.MotivoConsulta);
                $("textarea#lblAnamnesis").val(response.Anamnesis);
                $("textarea#lblExamenFisico").val(response.ExamenFisico);
                $("textarea#lblDiagnostico").val(response.Diagnostico);
                $("textarea#lblIndicaciones").val(response.Indicaciones);
                $("textarea#lblReceta").val(response.Receta);
                $("textarea#lblOrdenExamenes").val(response.OrdenExamenes);

                $("#hfIdConsulta").val(idConsulta)
                closeLoading(object);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            Swal.fire('Error', 'Ha ocurrido un error al buscar las consultas anteriores', 'error');
        }
    });
}

function ModificarDatosConsulta() {
    let _formData = new FormData();
    _formData.append('Id', $("#hfIdConsulta").val());
    _formData.append('MotivoConsulta', $("#lblMotivoConsulta").val());
    _formData.append('Anamnesis', $("#lblAnamnesis").val());
    _formData.append('ExamenFisico', $("#lblExamenFisico").val());
    _formData.append('Diagnostico', $("#lblDiagnostico").val());
    _formData.append('Indicaciones', $("#lblIndicaciones").val());
    _formData.append('Receta', $("#lblReceta").val());
    _formData.append('OrdenExamenes', $("#lblOrdenExamenes").val());

    $.ajax({
        url: '/Consulta/ActualizarConsulta',
        data: _formData,
        method: 'POST',
        processData: false,
        contentType: false,
        success: function (response, jqXHR) {
            if (response.Codigo === 200) {
                Swal.fire({
                    title: 'Datos de Consulta Actualizada!',
                    icon: 'success',
                }).then((result) => {
                    if (result.isConfirmed) {
                        CancelarModificarDatos();
                    }
                })
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            Swal.fire('Error', 'Favor comuniquese con un administrador', 'error');
        }
    });
}

function HabilitaModificarDatos() {
    //$("#inputPesoAnteriores").prop('disabled', false);
    //$("#inputTallaAnteriores").prop('disabled', false);
    //$("#inputIMCAnteriores").prop('disabled', false);
    //$("#inputEstadoNutricionalAnteriores").prop('disabled', false);

    $("#lblMotivoConsulta").prop('disabled', false);
    $("#lblAnamnesis").prop('disabled', false);
    $("#lblExamenFisico").prop('disabled', false);
    $("#lblDiagnostico").prop('disabled', false);
    $("#lblIndicaciones").prop('disabled', false);
    $("#lblReceta").prop('disabled', false);
    $("#lblOrdenExamenes").prop('disabled', false);

    $("#btnModificarDatos").hide();
    $("#btnGuardarDetalleConsulta").show();
    $("#btnCancelarActualizacion").show();
}

function CancelarModificarDatos() {
    //$("#inputPesoAnteriores").prop('disabled', true);
    //$("#inputTallaAnteriores").prop('disabled', true);
    //$("#inputIMCAnteriores").prop('disabled', true);
    //$("#inputEstadoNutricionalAnteriores").prop('disabled', true);

    $("#lblMotivoConsulta").prop('disabled', true);
    $("#lblAnamnesis").prop('disabled', true);
    $("#lblExamenFisico").prop('disabled', true);
    $("#lblDiagnostico").prop('disabled', true);
    $("#lblIndicaciones").prop('disabled', true);
    $("#lblReceta").prop('disabled', true);
    $("#lblOrdenExamenes").prop('disabled', true);

    $("#btnModificarDatos").show();
    $("#btnGuardarDetalleConsulta").hide();
    $("#btnCancelarActualizacion").hide();
}

function LimpiarFormularioConsulta() {
    $("#inputNombres").val("");
    $("#inputPrimerApellido").val("");
    $("#inputSegundoApellido").val("");
    $('radioSexo1').prop('checked', false);
    $('radioSexo2').prop('checked', false);
    $("#inputFechaNacimiento").val("");
    $("#inputDireccion").val("");
    $("#inputCelular").val("");
    $("#inputEmail").val("");
    $("#comboNacionalidad").val("");
    $("#inputPrevision").val("");
    $("#inputAntFamiliares").val("");
    $("#inputAntPersonales").val("");
    $("#inputAntQuirurgicos").val("");
    let btnTabaco = document.getElementById('radioTabaco1');
    let btnAlcohol = document.getElementById('radioAlcohol1');
    let btnDrogas = document.getElementById('radioDrogas1');
    let btnAlergias = document.getElementById('radioAlergias1');
    if (btnTabaco && btnTabaco.classList.contains('active')) {
        btnTabaco.classList.remove('active');
    }
    if (btnAlcohol && btnAlcohol.classList.contains('active')) {
        btnAlcohol.classList.remove('active');
    }
    if (btnDrogas && btnDrogas.classList.contains('active')) {
        btnDrogas.classList.remove('active');
    }
    if (btnAlergias && btnAlergias.classList.contains('active')) {
        btnAlergias.classList.remove('active');
    }
    $("#inputTabaco").val("");
    $("#inputAlcohol").val("");
    $("#inputDrogas").val("");
    $("#inputAlergias").val("");
    $("#inputGesta").val("");
    $("#inputParto").val("");
    $("#inputAborto").val("");
    $("#inputMenarquia").val("");
    $("#inputMenopausia").val("");
    $("#inputMedicamentos").val("");

    //$("#btnDatosPaciente").html('<i class="fas fa-user"></i>&nbsp; Datos Paciente: ');
    //$('#btnDatosPaciente').prop("disabled", true);
    $("#ListConsultasAnteriores").html('');

}