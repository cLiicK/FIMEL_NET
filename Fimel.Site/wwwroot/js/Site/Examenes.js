var ModuloExamenes = (function () {

    var _categorias = [];
    var _examenes = [];

    function renderCategorias() {
        var html = '';
        _categorias.forEach(function (c) {
            html += '<tr>' +
                '<td>' + c.Nombre + '</td>' +
                '<td class="text-center">' + c.Orden + '</td>' +
                '<td class="text-end text-nowrap">' +
                '<button class="btn btn-ico btn-sm" title="Editar" onclick="ModuloExamenes.AbrirModalCategoria(' + c.Id + ')"><i class="fas fa-pencil-alt"></i></button>' +
                '<button class="btn btn-ico btn-sm text-danger ms-1" title="Eliminar" onclick="ModuloExamenes.EliminarCategoria(' + c.Id + ')"><i class="fas fa-trash-alt"></i></button>' +
                '</td></tr>';
        });
        $('#tbodyCategorias').html(html || '<tr><td colspan="3" class="text-center text-muted">Sin categorías</td></tr>');

        var $sel = $('#examenCategoriaId');
        var cur = $sel.val();
        $sel.html('<option value="">Seleccione...</option>');
        _categorias.forEach(function (c) {
            $sel.append('<option value="' + c.Id + '">' + c.Nombre + '</option>');
        });
        if (cur) $sel.val(cur);

        var $fil = $('#filtroCategoria');
        var curFil = $fil.val();
        $fil.html('<option value="">Todas las categorías</option>');
        _categorias.forEach(function (c) {
            $fil.append('<option value="' + c.Id + '">' + c.Nombre + '</option>');
        });
        if (curFil) $fil.val(curFil);
    }

    function renderExamenes(lista) {
        var html = '';
        lista.forEach(function (e) {
            html += '<tr>' +
                '<td><span class="badge" style="background:#e8f4fa;color:#0E96CC;font-weight:500;">' + (e.CategoriaNombre || '-') + '</span></td>' +
                '<td>' + e.NombreExamen + '</td>' +
                '<td class="text-muted">' + (e.CodigoFonasa || '-') + '</td>' +
                '<td class="text-center">' + e.Orden + '</td>' +
                '<td class="text-end text-nowrap">' +
                '<button class="btn btn-ico btn-sm" title="Editar" onclick="ModuloExamenes.AbrirModalExamen(' + e.Id + ')"><i class="fas fa-pencil-alt"></i></button>' +
                '<button class="btn btn-ico btn-sm text-danger ms-1" title="Eliminar" onclick="ModuloExamenes.EliminarExamen(' + e.Id + ')"><i class="fas fa-trash-alt"></i></button>' +
                '</td></tr>';
        });
        $('#tbodyExamenes').html(html || '<tr><td colspan="5" class="text-center text-muted">Sin exámenes</td></tr>');
    }

    return {

        IniciarScripts: function () {
            $.ajax({
                url: $('#hdnURL_GetCategorias').val(), method: 'GET',
                success: function (r) {
                    if (r.success) { _categorias = r.data; renderCategorias(); }
                }
            });
            $.ajax({
                url: $('#hdnURL_GetExamenes').val(), method: 'GET',
                success: function (r) {
                    if (r.success) { _examenes = r.data; renderExamenes(_examenes); }
                }
            });
        },

        FiltrarExamenes: function () {
            var q = $('#filtroExamen').val().toLowerCase();
            var cat = $('#filtroCategoria').val();
            var filtrado = _examenes.filter(function (e) {
                var matchNombre = !q || e.NombreExamen.toLowerCase().includes(q);
                var matchCat = !cat || e.CategoriaExamenId == cat;
                return matchNombre && matchCat;
            });
            renderExamenes(filtrado);
        },

        AbrirModalCategoria: function (id) {
            $('#categoriaId').val(id || 0);
            $('#categoriaNombre').val('');
            $('#categoriaOrden').val(1);
            if (id) {
                var cat = _categorias.find(function (c) { return c.Id == id; });
                if (cat) { $('#categoriaNombre').val(cat.Nombre); $('#categoriaOrden').val(cat.Orden); }
                $('#lblTituloModalCat').text('Editar Categoría');
            } else {
                $('#lblTituloModalCat').text('Nueva Categoría');
            }
            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalCategoria')).show();
        },

        GuardarCategoria: function (btn) {
            var nombre = $('#categoriaNombre').val().trim();
            if (!nombre) { Swal.fire('Campo requerido', 'Ingrese el nombre.', 'warning'); return; }
            showLoading(btn);
            $.ajax({
                url: $('#hdnURL_GuardarCategoria').val(), method: 'POST',
                data: { id: $('#categoriaId').val(), nombre: nombre, orden: $('#categoriaOrden').val() },
                success: function (r) {
                    closeLoading(btn);
                    if (r.success) {
                        bootstrap.Modal.getInstance(document.getElementById('modalCategoria')).hide();
                        ModuloExamenes.IniciarScripts();
                    }
                },
                error: function () { closeLoading(btn); Swal.fire('Error', 'No se pudo guardar.', 'error'); }
            });
        },

        EliminarCategoria: function (id) {
            Swal.fire({ title: 'Eliminar categoría', text: '¿Confirma eliminar esta categoría?', icon: 'warning', showCancelButton: true, confirmButtonText: 'Eliminar' })
                .then(function (r) {
                    if (!r.isConfirmed) return;
                    $.post($('#hdnURL_EliminarCategoria').val(), { id: id }, function (res) {
                        if (res.success) ModuloExamenes.IniciarScripts();
                    });
                });
        },

        AbrirModalExamen: function (id) {
            $('#examenId').val(id || 0);
            $('#examenNombre').val('');
            $('#examenCodigoFonasa').val('');
            $('#examenOrden').val(1);
            $('#examenCategoriaId').val('');
            if (id) {
                var ex = _examenes.find(function (e) { return e.Id == id; });
                if (ex) {
                    $('#examenNombre').val(ex.NombreExamen);
                    $('#examenCodigoFonasa').val(ex.CodigoFonasa || '');
                    $('#examenOrden').val(ex.Orden);
                    $('#examenCategoriaId').val(ex.CategoriaExamenId);
                }
                $('#lblTituloModalExamen').text('Editar Examen');
            } else {
                $('#lblTituloModalExamen').text('Nuevo Examen');
            }
            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalExamen')).show();
        },

        GuardarExamen: function (btn) {
            var nombre = $('#examenNombre').val().trim();
            var catId = $('#examenCategoriaId').val();
            if (!nombre) { Swal.fire('Campo requerido', 'Ingrese el nombre del examen.', 'warning'); return; }
            if (!catId) { Swal.fire('Campo requerido', 'Seleccione una categoría.', 'warning'); return; }
            showLoading(btn);
            $.ajax({
                url: $('#hdnURL_GuardarExamen').val(), method: 'POST',
                data: { id: $('#examenId').val(), nombreExamen: nombre, codigoFonasa: $('#examenCodigoFonasa').val(), categoriaExamenId: catId, orden: $('#examenOrden').val() },
                success: function (r) {
                    closeLoading(btn);
                    if (r.success) {
                        bootstrap.Modal.getInstance(document.getElementById('modalExamen')).hide();
                        ModuloExamenes.IniciarScripts();
                    }
                },
                error: function () { closeLoading(btn); Swal.fire('Error', 'No se pudo guardar.', 'error'); }
            });
        },

        EliminarExamen: function (id) {
            Swal.fire({ title: 'Eliminar examen', text: '¿Confirma eliminar este examen?', icon: 'warning', showCancelButton: true, confirmButtonText: 'Eliminar' })
                .then(function (r) {
                    if (!r.isConfirmed) return;
                    $.post($('#hdnURL_EliminarExamen').val(), { id: id }, function (res) {
                        if (res.success) ModuloExamenes.IniciarScripts();
                    });
                });
        },
    };
})();

$(function () { ModuloExamenes.IniciarScripts(); });
