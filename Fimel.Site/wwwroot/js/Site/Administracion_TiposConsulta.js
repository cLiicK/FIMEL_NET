var ModuloAdminTiposConsulta = (function () {

    var _tiposConsulta = [];

    function render() {
        var html = '';
        _tiposConsulta.forEach(function (t) {
            html += '<tr>' +
                '<td>' + t.Nombre + '</td>' +
                '<td class="text-center">' + t.Orden + '</td>' +
                '<td class="text-end text-nowrap">' +
                '<button class="btn btn-ico btn-sm" title="Editar" onclick="ModuloAdminTiposConsulta.AbrirModalTipoConsulta(' + t.Id + ')"><i class="fas fa-pencil-alt"></i></button>' +
                '<button class="btn btn-ico btn-sm text-danger ms-1" title="Eliminar" onclick="ModuloAdminTiposConsulta.EliminarTipoConsulta(' + t.Id + ')"><i class="fas fa-trash-alt"></i></button>' +
                '</td></tr>';
        });
        $('#tbodyTiposConsulta').html(html || '<tr><td colspan="3" class="text-center text-muted">Sin tipos de consulta</td></tr>');
    }

    return {

        IniciarScripts: function () {
            $.ajax({
                url: $('#hdnURL_ObtenerTiposConsulta').val(), method: 'GET',
                success: function (r) { if (r.success) { _tiposConsulta = r.data; render(); } }
            });
        },

        AbrirModalTipoConsulta: function (id) {
            $('#tipoConsultaId').val(id || 0);
            $('#tipoConsultaNombre').val('');
            $('#tipoConsultaOrden').val((_tiposConsulta.length || 0) + 1);
            if (id) {
                var t = _tiposConsulta.find(function (x) { return x.Id == id; });
                if (t) { $('#tipoConsultaNombre').val(t.Nombre); $('#tipoConsultaOrden').val(t.Orden); }
                $('#lblTituloModalTipoConsulta').text('Editar Tipo de Consulta');
            } else {
                $('#lblTituloModalTipoConsulta').text('Nuevo Tipo de Consulta');
            }
            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalTipoConsulta')).show();
        },

        GuardarTipoConsulta: function (btn) {
            var nombre = $('#tipoConsultaNombre').val().trim();
            if (!nombre) { Swal.fire('Campo requerido', 'Ingrese el nombre.', 'warning'); return; }
            showLoading(btn);
            $.ajax({
                url: $('#hdnURL_GuardarTipoConsulta').val(), method: 'POST',
                data: { id: $('#tipoConsultaId').val(), nombre: nombre, orden: $('#tipoConsultaOrden').val() },
                success: function (r) {
                    closeLoading(btn);
                    if (r.success) {
                        bootstrap.Modal.getInstance(document.getElementById('modalTipoConsulta')).hide();
                        ModuloAdminTiposConsulta.IniciarScripts();
                    } else {
                        Swal.fire('Error', r.message || 'No se pudo guardar.', 'error');
                    }
                },
                error: function () { closeLoading(btn); Swal.fire('Error', 'No se pudo guardar.', 'error'); }
            });
        },

        EliminarTipoConsulta: function (id) {
            Swal.fire({ title: 'Eliminar tipo de consulta', text: '¿Confirma eliminar este tipo de consulta?', icon: 'warning', showCancelButton: true, confirmButtonText: 'Eliminar' })
                .then(function (r) {
                    if (!r.isConfirmed) return;
                    $.post($('#hdnURL_EliminarTipoConsulta').val(), { id: id }, function (res) {
                        if (res.success) ModuloAdminTiposConsulta.IniciarScripts();
                    });
                });
        }
    };
})();

$(function () { ModuloAdminTiposConsulta.IniciarScripts(); });
