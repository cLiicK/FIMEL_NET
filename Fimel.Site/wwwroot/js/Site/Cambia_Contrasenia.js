var ModuloCambioContrasena = (function () {
    return {
        IniciarScripts: function () {

        },
        CambiarContrasenia: function (object) {
            let password1 = $("#inputPassword1").val();
            let password2 = $("#inputPassword2").val();
            let usuarioEncryptado = $("#hfUsuarioEncryptado").val();
            if (password1 != password2) {
                Swal.fire('', "Las contraseñas no coinciden", 'warning');
                return;
            }
            if (password1.length < 8) {
                Swal.fire('', "La contraseña debe tener mínimo 8 caracteres", 'warning');
                return;
            }

            showLoading(object);

            $.ajax({
                url: $('#hdnURL_GuardarNuevaContrasenia').val(),
                data: {
                    password1: password1,
                    password2: password2,
                    usuarioEncryptado: usuarioEncryptado
                },
                method: 'POST',
                cache: false,
                async: true,
                success: function (response, jqXHR) {
                    if (response.Codigo === 200) {
                        Swal.fire({ text: 'Se ha actualizado la contraseña', icon: 'success', })
                            .then((result) => {
                                if (result.isConfirmed) {
                                    window.location.href = window.location.origin + "/Login/Login";
                                }
                            })
                    }
                    closeLoading(object);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    Swal.fire('Error', 'Ha ocurrido un problema. Favor contáctese con el administrador.', 'error');
                    closeLoading(object);
                }
            });

        }
    }
})();

$(function () {
    ModuloCambioContrasena.IniciarScripts();
});