var ModuloConfiguracionUsuario = (function () {
    return {
        IniciarScripts: function () {

        },
        GuardarConfiguracion: function () {

            if (!$("#inputBloqueHorarioConfig").val()) {
                Swal.fire('Seleccione la duración del Bloque Horario', 'Nueva Configuracion', 'warning');
                return null;
            }

            let config = {
                DuracionBloqueHorario: $('#inputBloqueHorarioConfig').val()
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
        ActualizarConfiguracion: function () {

            let config = {
                DuracionBloqueHorario: $('#inputBloqueHorarioConfig').val()
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
});