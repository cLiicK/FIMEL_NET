var ModuloAdminInstituciones = (function () {

    function renderInstituciones(lista) {
        var html = '';
        lista.forEach(function (i) {
            html += '<tr>' +
                '<td>' + (i.RazonSocial || '') + '</td>' +
                '<td>' + (i.Rut ? (i.Rut + '-' + (i.Dv || '')) : '-') + '</td>' +
                '<td>' + (i.Dirección || '-') + '</td>' +
                '<td>' + (i.Telefono || '-') + '</td>' +
                '<td>' + (i.Email || '-') + '</td>' +
                '</tr>';
        });
        $('#tbodyInstituciones').html(html || '<tr><td colspan="5" class="text-center text-muted">Sin instituciones</td></tr>');
    }

    return {

        IniciarScripts: function () {
            $.ajax({
                url: $('#hdnURL_ObtenerInstituciones').val(), method: 'GET',
                success: function (r) { if (r.success) renderInstituciones(r.data); }
            });
        },

        AbrirModalInstitucion: function () {
            $('#instRazonSocial').val('');
            $('#instRut').val('');
            $('#instDv').val('');
            $('#instDireccion').val('');
            $('#instTelefono').val('');
            $('#instEmail').val('');
            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalInstitucion')).show();
        },

        GuardarInstitucion: function (btn) {
            var razonSocial = $('#instRazonSocial').val().trim();
            if (!razonSocial) { Swal.fire('Campo requerido', 'Ingrese la razón social.', 'warning'); return; }

            showLoading(btn);
            $.ajax({
                url: $('#hdnURL_GuardarInstitucion').val(),
                method: 'POST',
                data: {
                    RazonSocial: razonSocial,
                    Rut: $('#instRut').val() || '',
                    Dv: $('#instDv').val(),
                    Dirección: $('#instDireccion').val(),
                    Telefono: $('#instTelefono').val() || '',
                    Email: $('#instEmail').val()
                },
                success: function (r) {
                    closeLoading(btn);
                    if (r.success) {
                        bootstrap.Modal.getInstance(document.getElementById('modalInstitucion')).hide();
                        ModuloAdminInstituciones.IniciarScripts();
                    } else {
                        Swal.fire('Error', r.message || 'No se pudo crear la institución.', 'error');
                    }
                },
                error: function () { closeLoading(btn); Swal.fire('Error', 'No se pudo crear la institución.', 'error'); }
            });
        }
    };
})();

$(function () { ModuloAdminInstituciones.IniciarScripts(); });
