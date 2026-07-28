var ModuloAdminModulos = (function () {

    var _modulos = [];
    var _perfiles = [];
    var _pares = [];

    function buscarPar(idModulo, idPerfil) {
        return _pares.find(function (p) { return p.ModuloId == idModulo && p.PerfilId == idPerfil; });
    }

    function render() {
        var htmlHead = '<th>Módulo</th>';
        _perfiles.forEach(function (p) {
            htmlHead += '<th class="text-center">' + p.Descripcion + '</th>';
        });
        $('#trEncabezadoPerfiles').html(htmlHead);

        var htmlBody = '';
        _modulos.forEach(function (m) {
            htmlBody += '<tr><td>' + m.Nombre + ' <span class="text-muted small">(' + m.Controller + '/' + m.Accion + ')</span></td>';
            _perfiles.forEach(function (p) {
                var par = buscarPar(m.Id, p.Id);
                var checked = par ? 'checked' : '';
                htmlBody += '<td class="text-center">' +
                    '<input type="checkbox" class="form-check-input" ' + checked +
                    ' onchange="ModuloAdminModulos.Toggle(' + m.Id + ',' + p.Id + ', this.checked)">' +
                    '</td>';
            });
            htmlBody += '</tr>';
        });
        $('#tbodyMatriz').html(htmlBody || '<tr><td class="text-center text-muted">Sin módulos</td></tr>');
    }

    return {

        IniciarScripts: function () {
            $.ajax({
                url: $('#hdnURL_ObtenerMatriz').val(), method: 'GET',
                success: function (r) {
                    if (r.success) {
                        _modulos = r.modulos;
                        _perfiles = r.perfiles;
                        _pares = r.pares;
                        render();
                    }
                }
            });
        },

        Toggle: function (idModulo, idPerfil, marcado) {
            if (marcado) {
                $.post($('#hdnURL_AsignarModuloPerfil').val(), { idModulo: idModulo, idPerfil: idPerfil }, function (r) {
                    if (r.success) ModuloAdminModulos.IniciarScripts();
                });
            } else {
                var par = buscarPar(idModulo, idPerfil);
                if (!par) return;
                $.post($('#hdnURL_QuitarModuloPerfil').val(), { id: par.Id }, function (r) {
                    if (r.success) ModuloAdminModulos.IniciarScripts();
                });
            }
        }
    };
})();

$(function () { ModuloAdminModulos.IniciarScripts(); });
