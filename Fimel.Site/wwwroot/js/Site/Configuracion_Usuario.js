var ModuloConfiguracionUsuario = (function () {
    return {
        IniciarScripts: function () {
            document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (el) {
                new bootstrap.Tooltip(el);
            });
        },
        GuardarConfiguracion: function () {

            if (!$("#inputBloqueHorarioConfig").val()) {
                Swal.fire('Seleccione la duración del Bloque Horario', 'Nueva Configuracion', 'warning');
                return null;
            }

            let config = {
                DuracionBloqueHorario: $('#inputBloqueHorarioConfig').val(),
                DiasAvisoPrevioControl: parseInt($('#inputDiasAvisoControl').val()) || 0,
                TituloProfesional: $('#inputTituloProfesional').val().trim() || 'Matrón/a',
                MinAntHoras: $('#inputMinAntHoras').val() !== '' ? parseInt($('#inputMinAntHoras').val()) : null,
                MaxAntDias: $('#inputMaxAntDias').val() !== '' ? parseInt($('#inputMaxAntDias').val()) : null
            }

            var btnGuardar = $('#btnGuardarConfigUser');

            Swal.fire({
                title: 'Guardar',
                text: '¿Esta seguro de guardar esta configuración?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Guardar',
            }).then((result) => {
                if (result.isConfirmed) {
                    btnGuardar.prop('disabled', true);
                    btnGuardar.text('Guardando...');

                    $.ajax({
                        url: $('#hdnURL_GuardarConfiguracion').val(),
                        data: {
                            config: config
                        },
                        method: 'POST',
                        success: function (response, jqXHR) {
                            if (response.success === true) {
                                Swal.fire({
                                    title: response.message,
                                    icon: 'success',
                                }).then((result) => {
                                    if (result.isConfirmed) {
                                        location.reload();
                                    }
                                })
                            }
                            else {
                                btnGuardar.prop('disabled', false);
                                btnGuardar.text('Guardar');
                                Swal.fire('Error', response.message, 'error');
                                return;
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            Swal.fire('Error', 'Favor comuniquese con un administrador', 'error');
                            btnGuardar.prop('disabled', false);
                            btnGuardar.text('Guardar');
                            return;
                        }
                    });
                }
                else {
                    btnGuardar.prop('disabled', false);
                    btnGuardar.text('Guardar');
                }
            })
        },
        GenerarEnlace: function (btn) {
            Swal.fire({
                title: 'Generar enlace público',
                text: 'Si ya tienes un enlace activo, será reemplazado y el anterior dejará de funcionar. ¿Continuar?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Generar'
            }).then(result => {
                if (!result.isConfirmed) return;

                $(btn).prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-1"></i> Generando...');

                $.post($('#hdnURL_GenerarToken').val(), function (response) {
                    if (response.success) {
                        if (!$('#inputUrlPublica').length) {
                            location.reload();
                            return;
                        }
                        var urlAgendar = response.url;
                        var urlAgenda  = urlAgendar.replace('/Agendar/', '/AgendaPublica/');

                        $('#inputUrlPublica').val(urlAgendar);
                        $('#linkUrlPublica').attr('href', urlAgendar);

                        $('#inputUrlAgendaPublica').val(urlAgenda);
                        $('#linkUrlAgendaPublica').attr('href', urlAgenda);

                        Swal.fire('¡Listo!', 'Enlace generado correctamente.', 'success');
                    } else {
                        Swal.fire('Error', response.message, 'error');
                    }
                }).fail(() => {
                    Swal.fire('Error', 'Favor comuníquese con un administrador', 'error');
                }).always(() => {
                    $(btn).prop('disabled', false).html('<i class="fas fa-link me-1"></i> Regenerar enlace');
                });
            });
        },
        CopiarEnlace: function (inputId) {
            var input = document.getElementById(inputId || 'inputUrlPublica');
            if (!input) return;
            navigator.clipboard.writeText(input.value).then(() => {
                Swal.fire({ title: '¡Copiado!', icon: 'success', timer: 1200, showConfirmButton: false });
            });
        },
        SubirLogo: function (btn) {
            var fileInput = document.getElementById('inputLogoFile');
            if (!fileInput.files || fileInput.files.length === 0) {
                Swal.fire('Seleccione un archivo', 'Debe elegir una imagen antes de subir.', 'warning');
                return;
            }

            var formData = new FormData();
            formData.append('logo', fileInput.files[0]);

            $(btn).prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-1"></i> Subiendo...');

            $.ajax({
                url: $('#hdnURL_SubirLogo').val(),
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    if (response.success) {
                        // Actualizar imagen sin recargar la página
                        var src = 'data:image/png;base64,' + response.logoBase64;
                        if ($('#imgLogoActual').length) {
                            $('#imgLogoActual').attr('src', src);
                        } else {
                            var img = $('<img id="imgLogoActual" alt="Logo actual" style="max-height:80px;max-width:220px;border:1px solid #e0e8f0;border-radius:8px;padding:6px;background:#fff">').attr('src', src);
                            $('#divPreviewNuevoLogo').before($('<div class="mb-3"><label class="form-label">Logo actual</label><br></div>').append(img));
                        }
                        $('#divPreviewNuevoLogo').hide();
                        Swal.fire('Logo guardado', response.message, 'success');
                    } else {
                        Swal.fire('Error', response.message, 'error');
                    }
                    $(btn).prop('disabled', false).html('<i class="fas fa-upload me-1"></i> Subir logo');
                },
                error: function () {
                    Swal.fire('Error', 'Favor comuníquese con un administrador', 'error');
                    $(btn).prop('disabled', false).html('<i class="fas fa-upload me-1"></i> Subir logo');
                }
            });
        },
        GuardarCorreo: function (btn) {
            var email = $('#inputCorreoConfig').val().trim();
            if (!email) { Swal.fire('Campo requerido', 'Ingrese un correo electrónico.', 'warning'); return; }
            showLoading(btn);
            $.ajax({
                url: $('#hdnURL_GuardarCorreo').val(), method: 'POST',
                data: { email: email },
                success: function (r) {
                    closeLoading(btn);
                    if (r.success) Swal.fire('Listo', r.message, 'success');
                    else Swal.fire('Error', r.message, 'error');
                },
                error: function () { closeLoading(btn); Swal.fire('Error', 'No se pudo guardar el correo.', 'error'); }
            });
        },
        GuardarInstitucion: function (btn) {
            var direccion = $('#inputDireccionInst').val().trim();
            if (!direccion) {
                Swal.fire('Campo requerido', 'Ingrese la dirección de la institución.', 'warning');
                return;
            }

            var inst = {
                RazonSocial: $('#inputRazonSocial').val().trim(),
                Dirección: direccion,
                Telefono: parseInt($('#inputTelefonoInst').val()) || null,
                Email: $('#inputEmailInst').val().trim() || null
            };

            Swal.fire({
                title: 'Guardar',
                text: '¿Guardar los datos de la institución?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Guardar'
            }).then(function (result) {
                if (!result.isConfirmed) return;

                $(btn).prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-1"></i> Guardando...');

                $.ajax({
                    url: $('#hdnURL_ActualizarInstitucion').val(),
                    data: { id: $('#idInstitucion').val(), inst: inst },
                    method: 'POST',
                    success: function (response) {
                        if (response.success) {
                            Swal.fire('Guardado', response.message, 'success');
                        } else {
                            Swal.fire('Error', response.message, 'error');
                        }
                        $(btn).prop('disabled', false).html('<i class="fas fa-save me-1"></i> Guardar institución');
                    },
                    error: function () {
                        Swal.fire('Error', 'Favor comuníquese con un administrador', 'error');
                        $(btn).prop('disabled', false).html('<i class="fas fa-save me-1"></i> Guardar institución');
                    }
                });
            });
        },
        ActualizarConfiguracion: function () {

            let config = {
                DuracionBloqueHorario: $('#inputBloqueHorarioConfig').val(),
                DiasAvisoPrevioControl: parseInt($('#inputDiasAvisoControl').val()) || 0,
                TituloProfesional: $('#inputTituloProfesional').val().trim() || 'Matrón/a',
                MinAntHoras: $('#inputMinAntHoras').val() !== '' ? parseInt($('#inputMinAntHoras').val()) : null,
                MaxAntDias: $('#inputMaxAntDias').val() !== '' ? parseInt($('#inputMaxAntDias').val()) : null
            }

            var btnGuardar = $('#btnActualizarConfigUser');

            Swal.fire({
                title: 'Actualizar',
                text: '¿Esta seguro de actualizar esta configuración?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Actualizar',
            }).then((result) => {
                if (result.isConfirmed) {
                    btnGuardar.prop('disabled', true);
                    btnGuardar.text('Actualizando...');

                    $.ajax({
                        url: $('#hdnURL_ActualizarConfiguracion').val(),
                        data: {
                            Id: $('#idConfig').val(),
                            config: config
                        },
                        method: 'POST',
                        success: function (response, jqXHR) {
                            if (response.success === true) {
                                Swal.fire({
                                    title: response.message,
                                    icon: 'success',
                                }).then((result) => {
                                    if (result.isConfirmed) {
                                        location.reload();
                                    }
                                })
                            }
                            else {
                                btnGuardar.prop('disabled', false);
                                btnGuardar.text('Actualizar');
                                Swal.fire('Error', response.message, 'error');
                                return;
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            Swal.fire('Error', 'Favor comuniquese con un administrador', 'error');
                            btnGuardar.prop('disabled', false);
                            btnGuardar.text('Actualizar');
                            return;
                        }
                    });
                }
                else {
                    btnGuardar.prop('disabled', false);
                    btnGuardar.text('Actualizar');
                }
            })
        }
    }
})();

$(function () {
    ModuloConfiguracionUsuario.IniciarScripts();

    $('#inputLogoFile').on('change', function () {
        var file = this.files[0];
        if (!file) return;
        var reader = new FileReader();
        reader.onload = function (e) {
            $('#imgPreviewLogo').attr('src', e.target.result);
            $('#divPreviewNuevoLogo').show();
        };
        reader.readAsDataURL(file);
    });
});