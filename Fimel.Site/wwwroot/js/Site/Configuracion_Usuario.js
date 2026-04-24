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
                DiasAvisoPrevioControl: parseInt($('#inputDiasAvisoControl').val()) || 0
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
                        $('#inputUrlPublica').val(response.url);
                        if (!$('#inputUrlPublica').length) location.reload();
                        else Swal.fire('¡Listo!', 'Enlace generado correctamente.', 'success');
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
        CopiarEnlace: function () {
            var input = document.getElementById('inputUrlPublica');
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
        ActualizarConfiguracion: function () {

            let config = {
                DuracionBloqueHorario: $('#inputBloqueHorarioConfig').val(),
                DiasAvisoPrevioControl: parseInt($('#inputDiasAvisoControl').val()) || 0
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