var ModuloAdminUsuarios = (function () {

    var _usuarios = [];
    var _instituciones = [];
    var _perfiles = [];

    function nombreInstitucion(idInstitucion) {
        if (!idInstitucion) return '<span class="text-muted fst-italic">SuperAdmin</span>';
        var inst = _instituciones.find(function (i) { return i.Id == idInstitucion; });
        return inst ? inst.RazonSocial : '-';
    }

    function badgesPerfiles(usuario) {
        var perfiles = usuario.PerfilesAsignados || [];
        if (!perfiles.length) return '<span class="text-muted fst-italic small">Sin perfiles</span>';
        return perfiles.map(function (p) {
            return '<span class="badge me-1" style="background:#e8f4fa;color:#0E96CC;border:1px solid #b8dff0;font-weight:500;">' + p.Descripcion + '</span>';
        }).join('');
    }

    function renderUsuarios() {
        var html = '';
        _usuarios.forEach(function (u) {
            html += '<tr>' +
                '<td>' + (u.Nombres || '') + ' ' + (u.ApellidoPaterno || '') + '</td>' +
                '<td>' + (u.Usuario || '') + '</td>' +
                '<td>' + (u.Email || '') + '</td>' +
                '<td>' + nombreInstitucion(u.IdInstitucion) + '</td>' +
                '<td>' + badgesPerfiles(u) + '</td>' +
                '<td class="text-end">' +
                '<button class="btn btn-ico btn-sm" title="Editar perfiles" onclick="ModuloAdminUsuarios.AbrirModalPerfiles(' + u.Id + ')"><i class="fas fa-user-shield"></i></button>' +
                '</td></tr>';
        });
        $('#tbodyUsuarios').html(html || '<tr><td colspan="6" class="text-center text-muted">Sin usuarios</td></tr>');
    }

    function poblarInstitucionesSelect() {
        var $sel = $('#usrInstitucion');
        $sel.html('<option value="">Sin institución (SuperAdmin)</option>');
        _instituciones.forEach(function (i) {
            $sel.append('<option value="' + i.Id + '">' + i.RazonSocial + '</option>');
        });
    }

    function poblarPerfilesCheckboxes() {
        var html = '';
        _perfiles.forEach(function (p) {
            html += '<div class="form-check">' +
                '<input class="form-check-input" type="checkbox" value="' + p.Id + '" id="chkPerfilNuevo' + p.Id + '">' +
                '<label class="form-check-label" for="chkPerfilNuevo' + p.Id + '">' + p.Descripcion + '</label>' +
                '</div>';
        });
        $('#usrPerfiles').html(html);
    }

    return {

        IniciarScripts: function () {
            document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (el) {
                new bootstrap.Tooltip(el);
            });

            $.ajax({
                url: $('#hdnURL_ObtenerUsuarios').val(), method: 'GET',
                success: function (r) {
                    if (r.success) {
                        _usuarios = r.usuarios;
                        _instituciones = r.instituciones;
                        _perfiles = r.perfiles;
                        renderUsuarios();
                        poblarInstitucionesSelect();
                        poblarPerfilesCheckboxes();
                    }
                }
            });
        },

        AbrirModalUsuario: function () {
            $('#usrNombres').val('');
            $('#usrApellidoPaterno').val('');
            $('#usrApellidoMaterno').val('');
            $('#usrUsuario').val('');
            $('#usrEmail').val('');
            $('#usrInstitucion').val('');
            $('#usrRut').val('');
            $('#usrPerfiles input[type=checkbox]').prop('checked', false);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalUsuario')).show();
        },

        GuardarUsuario: function (btn) {
            var nombres = $('#usrNombres').val().trim();
            var usuarioLogin = $('#usrUsuario').val().trim();
            var email = $('#usrEmail').val().trim();
            var rut = $('#usrRut').val().trim();
            var perfilIds = $('#usrPerfiles input[type=checkbox]:checked').map(function () { return parseInt($(this).val()); }).get();

            if (!nombres) { Swal.fire('Campo requerido', 'Ingrese los nombres.', 'warning'); return; }
            if (!usuarioLogin) { Swal.fire('Campo requerido', 'Ingrese el nombre de usuario.', 'warning'); return; }
            if (!email) { Swal.fire('Campo requerido', 'Ingrese el email.', 'warning'); return; }
            if (!perfilIds.length) { Swal.fire('Campo requerido', 'Seleccione al menos un perfil.', 'warning'); return; }

            var rutNum = '', rutDv = '';
            if (rut) {
                var rutValidado = validarRut(rut);
                if (rutValidado === '00' || rutValidado === '01') {
                    Swal.fire('RUT inválido', 'Revisa el RUT ingresado.', 'warning');
                    return;
                }
                rutNum = parseInt(rutValidado);
                rutDv = getDV(rutNum).toString().toUpperCase();
            }

            showLoading(btn);
            $.ajax({
                url: $('#hdnURL_GuardarUsuario').val(),
                method: 'POST',
                traditional: true,
                data: {
                    Nombres: nombres,
                    ApellidoPaterno: $('#usrApellidoPaterno').val().trim(),
                    ApellidoMaterno: $('#usrApellidoMaterno').val().trim(),
                    Usuario: usuarioLogin,
                    Email: email,
                    IdInstitucion: $('#usrInstitucion').val() || '',
                    Rut: rutNum,
                    Dv: rutDv,
                    perfilIds: perfilIds
                },
                success: function (r) {
                    closeLoading(btn);
                    if (r.success) {
                        bootstrap.Modal.getInstance(document.getElementById('modalUsuario')).hide();
                        Swal.fire('Usuario creado', r.message, 'success');
                        ModuloAdminUsuarios.IniciarScripts();
                    } else {
                        Swal.fire('Error', r.message || 'No se pudo crear el usuario.', 'error');
                    }
                },
                error: function () { closeLoading(btn); Swal.fire('Error', 'No se pudo crear el usuario.', 'error'); }
            });
        },

        AbrirModalPerfiles: function (idUsuario) {
            var usuario = _usuarios.find(function (u) { return u.Id == idUsuario; });
            if (!usuario) return;

            $('#lblNombreUsuarioPerfiles').text(usuario.Nombres + ' ' + (usuario.ApellidoPaterno || ''));

            var asignados = (usuario.PerfilesAsignados || []).map(function (p) { return p.Id; });
            var html = '';
            _perfiles.forEach(function (p) {
                var checked = asignados.indexOf(p.Id) !== -1 ? 'checked' : '';
                html += '<div class="form-check form-switch">' +
                    '<input class="form-check-input" type="checkbox" role="switch" ' + checked +
                    ' onchange="ModuloAdminUsuarios.TogglePerfil(' + idUsuario + ',' + p.Id + ', this.checked)">' +
                    '<label class="form-check-label">' + p.Descripcion + '</label>' +
                    '</div>';
            });
            $('#perfilesUsuarioCheckboxes').html(html);

            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalPerfilesUsuario')).show();
        },

        TogglePerfil: function (idUsuario, idPerfil, marcado) {
            var url = marcado ? $('#hdnURL_AsignarPerfil').val() : $('#hdnURL_QuitarPerfil').val();
            $.post(url, { idUsuario: idUsuario, idPerfil: idPerfil }, function (r) {
                if (r.success) ModuloAdminUsuarios.IniciarScripts();
            });
        }
    };
})();

$(function () {
    ModuloAdminUsuarios.IniciarScripts();

    $('#usrRut').keypress(function (e) { onlyNumbersWithK(e); });
    $('#usrRut').keyup(function () {
        let cadena = $(this).val().replace(/[.]/gi, '').replace('-', '');
        if (cadena.length > 9) cadena = cadena.substr(0, 9);
        let concat = '', i = cadena.length - 1;
        for (; i >= 0;) {
            concat = cadena[i] + concat;
            if (i + 1 == cadena.length && i > 0) concat = '-' + concat;
            if (concat.length == 9 && cadena.length > 7) concat = '.' + concat;
            if (concat.length == 5 && cadena.length > 4) concat = '.' + concat;
            i--;
        }
        $(this).val(concat);
    });
});
