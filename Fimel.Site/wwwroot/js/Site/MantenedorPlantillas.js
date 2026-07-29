var ModuloMantenedorPlantillas = (function () {
    var idPlantillaActual = 0;
    var camposActuales = [];

    function renderCampos() {
        var cont = $('#tablaCampos');
        if (!camposActuales.length) {
            cont.html('<p class="text-muted small fst-italic">Sin campos. Agrega al menos uno.</p>');
            return;
        }
        var rows = camposActuales.map(function (c) {
            return '<tr>' +
                '<td>' + c.Orden + '</td>' +
                '<td><code>{{' + c.NombreCampo + '}}</code></td>' +
                '<td>' + c.Etiqueta + '</td>' +
                '<td class="text-center">' + (c.Obligatorio ? '<i class="fas fa-check text-success"></i>' : '') + '</td>' +
                '<td class="text-end">' +
                    '<button class="btn btn-sm btn-outline-danger" onclick="ModuloMantenedorPlantillas.EliminarCampo(' + c.Id + ')"><i class="fas fa-trash"></i></button>' +
                '</td>' +
            '</tr>';
        }).join('');
        cont.html('<table class="table table-sm mb-0"><thead><tr>' +
            '<th>#</th><th>Variable</th><th>Etiqueta</th><th class="text-center">Obligatorio</th><th></th>' +
            '</tr></thead><tbody>' + rows + '</tbody></table>');
    }

    return {
        NuevaPlantilla: function () {
            idPlantillaActual = 0;
            camposActuales = [];
            $('#inputIdPlantilla').val(0);
            $('#inputNombrePlantilla').val('');
            $('#inputTipoExamen').val('');
            $('#inputHtmlBase').val('');
            $('#checkActiva').prop('checked', true);
            $('#lblTituloEditor').text('Nueva plantilla');
            $('#divCampos').hide();
            $('.plantilla-card').removeClass('border-primary');
        },

        SeleccionarPlantilla: function (id) {
            $.get($('#hdnURL_ObtenerPlantilla').val(), { id: id }, function (response) {
                if (!response.success) return;
                var p = response.data;
                idPlantillaActual = p.Id;
                camposActuales = p.Campos || [];

                $('#inputIdPlantilla').val(p.Id);
                $('#inputNombrePlantilla').val(p.Nombre);
                $('#inputTipoExamen').val(p.TipoExamen || '');
                $('#inputHtmlBase').val(p.HtmlBase);
                $('#checkActiva').prop('checked', p.Activa);
                $('#lblTituloEditor').text('Editar: ' + p.Nombre);
                $('#divCampos').show();
                renderCampos();

                $('.plantilla-card').removeClass('border-primary');
                $('.plantilla-card[data-id="' + id + '"]').addClass('border-primary');
            });
        },

        GuardarPlantilla: function (btn) {
            var nombre = $('#inputNombrePlantilla').val().trim();
            if (!nombre) { Swal.fire('Campo requerido', 'Ingresa el nombre de la plantilla.', 'warning'); return; }
            var htmlBase = $('#inputHtmlBase').val().trim();
            if (!htmlBase) { Swal.fire('Campo requerido', 'Ingresa el HTML base de la plantilla.', 'warning'); return; }

            var data = {
                Nombre: nombre,
                TipoExamen: $('#inputTipoExamen').val().trim(),
                HtmlBase: htmlBase,
                Activa: $('#checkActiva').is(':checked')
            };

            $(btn).prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-1"></i> Guardando...');

            var id = parseInt($('#inputIdPlantilla').val());
            var url = id > 0 ? $('#hdnURL_ActualizarPlantilla').val() : $('#hdnURL_GuardarPlantilla').val();
            var postData = id > 0 ? { id: id, plantilla: data } : { plantilla: data };

            $.ajax({
                url: url, method: 'POST', data: postData,
                success: function (r) {
                    if (r.success) {
                        if (id === 0 && r.id) {
                            idPlantillaActual = r.id;
                            $('#inputIdPlantilla').val(r.id);
                            $('#divCampos').show();
                        }
                        Swal.fire({ title: 'Guardado', icon: 'success', timer: 1200, showConfirmButton: false })
                            .then(function () { location.reload(); });
                    } else {
                        Swal.fire('Error', r.message || 'Error al guardar.', 'error');
                        $(btn).prop('disabled', false).html('<i class="fas fa-save me-1"></i> Guardar');
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Favor comuníquese con un administrador.', 'error');
                    $(btn).prop('disabled', false).html('<i class="fas fa-save me-1"></i> Guardar');
                }
            });
        },

        EliminarPlantilla: function (id) {
            Swal.fire({ title: '¿Eliminar plantilla?', text: 'Esta acción no se puede deshacer.', icon: 'warning',
                showCancelButton: true, confirmButtonText: 'Eliminar', confirmButtonColor: '#e53935' })
            .then(function (result) {
                if (!result.isConfirmed) return;
                $.ajax({ url: $('#hdnURL_EliminarPlantilla').val(), method: 'POST', data: { id: id },
                    success: function (r) {
                        if (r.success) {
                            Swal.fire({ title: 'Eliminada', icon: 'success', timer: 1000, showConfirmButton: false })
                                .then(function () { location.reload(); });
                        } else { Swal.fire('Error', r.message || 'Error al eliminar.', 'error'); }
                    }
                });
            });
        },

        MostrarFormCampo: function () {
            $('#inputIdCampo').val(0);
            $('#inputNombreCampo').val('');
            $('#inputEtiquetaCampo').val('');
            $('#inputOrdenCampo').val(camposActuales.length + 1);
            $('#checkObligatorio').prop('checked', false);
            $('#divFormCampo').show();
            $('#inputNombreCampo').focus();
        },

        GuardarCampo: function (btn) {
            var nombre = $('#inputNombreCampo').val().trim();
            var etiqueta = $('#inputEtiquetaCampo').val().trim();
            if (!nombre || !etiqueta) { Swal.fire('Campos requeridos', 'Ingresa la variable y la etiqueta.', 'warning'); return; }

            var idPlantilla = parseInt($('#inputIdPlantilla').val());
            if (!idPlantilla) { Swal.fire('Guarda la plantilla primero', '', 'info'); return; }

            var campo = {
                PlantillaInformeId: idPlantilla,
                NombreCampo: nombre,
                Etiqueta: etiqueta,
                Orden: parseInt($('#inputOrdenCampo').val()) || 1,
                Obligatorio: $('#checkObligatorio').is(':checked')
            };

            $(btn).prop('disabled', true);
            $.ajax({
                url: $('#hdnURL_GuardarCampo').val(), method: 'POST', data: { campo: campo },
                success: function (r) {
                    if (r.success && r.data) {
                        camposActuales.push(r.data);
                        camposActuales.sort(function (a, b) { return a.Orden - b.Orden; });
                        renderCampos();
                        $('#divFormCampo').hide();
                    } else { Swal.fire('Error', r.message || 'Error al guardar campo.', 'error'); }
                    $(btn).prop('disabled', false);
                },
                error: function () { Swal.fire('Error', 'Error al guardar campo.', 'error'); $(btn).prop('disabled', false); }
            });
        },

        EliminarCampo: function (id) {
            $.ajax({
                url: $('#hdnURL_EliminarCampo').val(), method: 'POST', data: { id: id },
                success: function (r) {
                    if (r.success) {
                        camposActuales = camposActuales.filter(function (c) { return c.Id !== id; });
                        renderCampos();
                    } else { Swal.fire('Error', r.message || 'Error al eliminar.', 'error'); }
                }
            });
        }
    };
})();

$(function () {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (el) {
        new bootstrap.Tooltip(el);
    });
});
