var ModuloFinanciero = (function () {

    var _movimientos = [];
    var _categorias = [];
    var _esSuperAdmin = false;
    var _esEspecialista = false;

    function formatearMonto(n) {
        return '$' + Number(n).toLocaleString('es-CL');
    }

    function formatearFecha(s) {
        if (!s) return '-';
        var d = new Date(s);
        if (isNaN(d.getTime())) return '-';
        return ('0' + d.getDate()).slice(-2) + '/' + ('0' + (d.getMonth() + 1)).slice(-2) + '/' + d.getFullYear();
    }

    function badgeTipo(tipo) {
        if (tipo === 'Ingreso') return '<span class="badge bg-success">Ingreso</span>';
        if (tipo === 'Egreso') return '<span class="badge bg-danger">Egreso</span>';
        return tipo || '-';
    }

    function cargarCategoriasPorTipo(tipo, selectedId) {
        var $sel = $('#modalCategoria');
        if (!tipo) {
            $sel.html('<option value="">Seleccione tipo primero...</option>');
            return;
        }
        $sel.html('<option value="">Seleccione...</option>');
        _categorias.filter(function (c) { return c.Tipo === tipo; }).forEach(function (c) {
            var sel = (selectedId && c.Id === selectedId) ? ' selected' : '';
            $sel.append('<option value="' + c.Id + '"' + sel + '>' + c.Nombre + '</option>');
        });
    }

    function limpiarPaciente() {
        $('#modalPacienteId').val('');
        $('#modalBuscarPaciente').val('');
        $('#divPacienteSeleccionado').hide();
        $('#divResultadosPaciente').hide().empty();
        $('#divConsultas').hide();
        $('#modalConsulta').html('<option value="">Sin consulta</option>');
    }

    function limpiarModal() {
        $('#modalIdMovimiento').val('0');
        $('#modalTipo').val('');
        $('#modalCategoria').html('<option value="">Seleccione tipo primero...</option>');
        $('#modalFecha').val(new Date().toISOString().split('T')[0]);
        $('#modalMonto').val('');
        $('#modalDescripcion').val('');
        if (_esSuperAdmin) $('#modalInstitucion').val('');
        limpiarPaciente();
        $('#lblTituloModal').text('Nuevo movimiento');
    }

    function renderTabla() {
        var html = '';
        _movimientos.forEach(function (m) {
            var catNombre = (m.Categoria && m.Categoria.Nombre) ? m.Categoria.Nombre : '-';
            var prof = m.Usuario ? (m.Usuario.Nombres + ' ' + (m.Usuario.ApellidoPaterno || '')).trim() : '-';
            html += '<tr>';
            html += '<td>' + formatearFecha(m.Fecha) + '</td>';
            html += '<td>' + badgeTipo(m.Tipo) + '</td>';
            html += '<td>' + catNombre + '</td>';
            html += '<td>' + (m.Descripcion || '-') + '</td>';
            html += '<td>' + (m.NombrePaciente || '-') + '</td>';
            html += '<td class="text-end fw-semibold">' + formatearMonto(m.Monto) + '</td>';
            if (!_esEspecialista) html += '<td>' + prof + '</td>';
            html += '<td class="text-end text-nowrap">';
            html += '<button class="btn btn-ico btn-sm" title="Editar" onclick="ModuloFinanciero.Editar(' + m.Id + ')"><i class="fas fa-pencil-alt"></i></button>';
            html += '<button class="btn btn-ico btn-sm text-danger ms-1" title="Eliminar" onclick="ModuloFinanciero.Eliminar(' + m.Id + ')"><i class="fas fa-trash-alt"></i></button>';
            html += '</td></tr>';
        });
        $('#tbodyMovimientos').html(html);
    }

    function actualizarResumen() {
        var ingresos = 0, egresos = 0;
        _movimientos.forEach(function (m) {
            if (m.Tipo === 'Ingreso') ingresos += m.Monto;
            else if (m.Tipo === 'Egreso') egresos += m.Monto;
        });
        var balance = ingresos - egresos;
        $('#lblTotalIngresos').text(formatearMonto(ingresos));
        $('#lblTotalEgresos').text(formatearMonto(egresos));
        $('#lblBalance').text(formatearMonto(balance))
            .removeClass('text-success text-danger')
            .addClass(balance >= 0 ? 'text-success' : 'text-danger');
        $('#divResumen').show();
    }

    return {

        IniciarScripts: function () {
            _esSuperAdmin = $('#hdnEsSuperAdmin').val() === 'true';
            _esEspecialista = $('#hdnEsEspecialista').val() === 'true';

            var hoy = new Date();
            var primerDia = new Date(hoy.getFullYear(), hoy.getMonth(), 1);
            $('#filtroFechaDesde').val(primerDia.toISOString().split('T')[0]);
            $('#filtroFechaHasta').val(hoy.toISOString().split('T')[0]);

            ModuloFinanciero.Buscar();
        },

        Buscar: function () {
            var data = {};
            var desde = $('#filtroFechaDesde').val();
            var hasta = $('#filtroFechaHasta').val();
            var tipo = $('#filtroTipo').val();
            if (desde) data.fechaDesde = desde;
            if (hasta) data.fechaHasta = hasta;
            if (tipo) data.tipo = tipo;
            if (_esSuperAdmin) {
                var filtroInst = $('#filtroInstitucion').val();
                if (filtroInst) data.idInstitucion = filtroInst;
            }

            $.ajax({
                url: $('#hdnURL_GetMovimientos').val(),
                method: 'POST',
                data: data,
                success: function (resp) {
                    if (!resp.success) { Swal.fire('Error', 'No se pudieron cargar los movimientos.', 'error'); return; }
                    _movimientos = resp.data || [];
                    _categorias = resp.categorias || [];

                    if (_movimientos.length === 0) {
                        $('#divTabla, #divResumen').hide();
                        $('#divSinResultados').show();
                    } else {
                        $('#divSinResultados').hide();
                        renderTabla();
                        actualizarResumen();
                        $('#divTabla').show();
                    }
                },
                error: function () { Swal.fire('Error', 'No se pudo conectar con el servidor.', 'error'); }
            });
        },

        AbrirModalNuevo: function () {
            limpiarModal();
            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalMovimiento')).show();
        },

        OnCambioTipo: function () {
            cargarCategoriasPorTipo($('#modalTipo').val(), null);
        },

        OnCambioInstitucionModal: function () {
            var idInst = $('#modalInstitucion').val();
            if (!idInst) return;
            $.ajax({
                url: $('#hdnURL_GetCategorias').val(),
                method: 'GET',
                data: { idInstitucion: idInst },
                success: function (resp) {
                    if (resp.success) {
                        _categorias = resp.data || [];
                        cargarCategoriasPorTipo($('#modalTipo').val(), null);
                    }
                }
            });
        },

        BuscarPaciente: function () {
            var criterio = $('#modalBuscarPaciente').val().trim();
            if (!criterio) return;

            var data = {};
            var limpio = criterio.replace(/\./g, '').replace(/-/g, '').replace(/[kK]$/, '');
            if (/^\d+$/.test(limpio)) {
                data.rut = parseInt(limpio);
            } else {
                data.NumDoc = criterio;
            }

            $.ajax({
                url: $('#hdnURL_BuscarPaciente').val(),
                method: 'GET',
                data: data,
                success: function (resp) {
                    var lista = (resp && resp.success) ? (resp.data || []) : [];
                    var $div = $('#divResultadosPaciente');
                    $div.empty();
                    if (!lista.length) {
                        $div.append('<div class="list-group-item text-muted py-1">Sin resultados</div>');
                    } else {
                        lista.slice(0, 8).forEach(function (p) {
                            var nombre = (p.Nombres + ' ' + (p.ApellidoPaterno || '')).trim();
                            var safe = nombre.replace(/'/g, '&apos;');
                            $div.append(
                                '<a href="#" class="list-group-item list-group-item-action py-1" ' +
                                'onclick="ModuloFinanciero.SeleccionarPaciente(' + p.Id + ', \'' + safe + '\'); return false;">' +
                                nombre + '</a>'
                            );
                        });
                    }
                    $div.show();
                }
            });
        },

        SeleccionarPaciente: function (id, nombre) {
            $('#modalPacienteId').val(id);
            $('#lblPacienteSeleccionado').text(nombre);
            $('#divPacienteSeleccionado').show();
            $('#divResultadosPaciente').hide().empty();
            $('#modalBuscarPaciente').val('');

            $.ajax({
                url: $('#hdnURL_GetConsultas').val(),
                method: 'GET',
                data: { idPaciente: id },
                success: function (resp) {
                    var $sel = $('#modalConsulta');
                    $sel.html('<option value="">Sin consulta</option>');
                    if (resp.success && resp.data && resp.data.length) {
                        resp.data.forEach(function (c) {
                            $sel.append('<option value="' + c.Id + '">' + c.Fecha + ' – ' + (c.TipoConsulta || c.MotivoConsulta || '') + '</option>');
                        });
                        $('#divConsultas').show();
                    }
                }
            });
        },

        LimpiarPaciente: function () { limpiarPaciente(); },

        GuardarMovimiento: function (btn) {
            var tipo = $('#modalTipo').val();
            var catId = $('#modalCategoria').val();
            var fecha = $('#modalFecha').val();
            var monto = parseFloat($('#modalMonto').val());

            if (!tipo || !catId || !fecha || isNaN(monto) || monto <= 0) {
                Swal.fire('Validación', 'Complete todos los campos obligatorios (tipo, categoría, fecha y monto).', 'warning');
                return;
            }

            var id = parseInt($('#modalIdMovimiento').val()) || 0;
            var payload = {
                Tipo: tipo,
                CategoriaFinancieraId: parseInt(catId),
                Fecha: fecha,
                Monto: monto,
                Descripcion: $('#modalDescripcion').val() || null,
                ConsultaId: $('#modalConsulta').val() ? parseInt($('#modalConsulta').val()) : null
            };
            if (_esSuperAdmin) payload.InstitucionId = parseInt($('#modalInstitucion').val()) || 0;

            showLoading(btn);

            $.ajax({
                url: id > 0 ? $('#hdnURL_ActualizarMovimiento').val() : $('#hdnURL_GuardarMovimiento').val(),
                method: 'POST',
                data: id > 0 ? $.extend({ id: id }, payload) : payload,
                success: function (resp) {
                    closeLoading(btn);
                    if (resp.success) {
                        bootstrap.Modal.getInstance(document.getElementById('modalMovimiento')).hide();
                        ModuloFinanciero.Buscar();
                    } else {
                        Swal.fire('Error', resp.message || 'No se pudo guardar el movimiento.', 'error');
                    }
                },
                error: function () {
                    closeLoading(btn);
                    Swal.fire('Error', 'No se pudo conectar con el servidor.', 'error');
                }
            });
        },

        Editar: function (id) {
            var m = _movimientos.find(function (x) { return x.Id === id; });
            if (!m) return;

            limpiarModal();
            $('#modalIdMovimiento').val(m.Id);
            $('#lblTituloModal').text('Editar movimiento');
            $('#modalTipo').val(m.Tipo);
            cargarCategoriasPorTipo(m.Tipo, m.CategoriaFinancieraId);
            $('#modalFecha').val(m.Fecha ? m.Fecha.split('T')[0] : '');
            $('#modalMonto').val(m.Monto);
            $('#modalDescripcion').val(m.Descripcion || '');
            if (_esSuperAdmin) $('#modalInstitucion').val(m.InstitucionId);

            if (m.NombrePaciente) {
                $('#lblPacienteSeleccionado').text(m.NombrePaciente);
                $('#divPacienteSeleccionado').show();
            }
            if (m.ConsultaId) {
                $('#modalConsulta').html('<option value="' + m.ConsultaId + '" selected>Consulta #' + m.ConsultaId + '</option>');
                $('#divConsultas').show();
            }

            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalMovimiento')).show();
        },

        AbrirModalNuevaCategoria: function () {
            $('#ncNombre').val('');
            $('#ncTipo').val($('#modalTipo').val());
            if ($('#ncInstitucion').length && $('#modalInstitucion').length)
                $('#ncInstitucion').val($('#modalInstitucion').val());
            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalNuevaCategoria')).show();
        },

        GuardarNuevaCategoria: function (btn) {
            var nombre = $('#ncNombre').val().trim();
            var tipo = $('#ncTipo').val();
            if (!nombre || !tipo) {
                Swal.fire('Validación', 'Complete nombre y tipo.', 'warning');
                return;
            }

            var payload = { Nombre: nombre, Tipo: tipo };
            if ($('#ncInstitucion').length) {
                payload.InstitucionId = parseInt($('#ncInstitucion').val()) || 0;
                if (!payload.InstitucionId) { Swal.fire('Validación', 'Seleccione una institución.', 'warning'); return; }
            }

            showLoading(btn);

            $.ajax({
                url: $('#hdnURL_GuardarCategoria').val(),
                method: 'POST',
                data: payload,
                success: function (resp) {
                    closeLoading(btn);
                    if (resp.success && resp.data) {
                        var nueva = resp.data;
                        _categorias.push(nueva);
                        cargarCategoriasPorTipo($('#modalTipo').val(), nueva.Id);
                        bootstrap.Modal.getInstance(document.getElementById('modalNuevaCategoria')).hide();
                    } else {
                        Swal.fire('Error', resp.message || 'No se pudo crear la categoría.', 'error');
                    }
                },
                error: function () {
                    closeLoading(btn);
                    Swal.fire('Error', 'No se pudo conectar con el servidor.', 'error');
                }
            });
        },

        Eliminar: function (id) {
            Swal.fire({
                title: 'Eliminar movimiento',
                text: '¿Está seguro de eliminar este movimiento?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Eliminar',
                cancelButtonText: 'Cancelar',
                confirmButtonColor: '#d33'
            }).then(function (result) {
                if (!result.isConfirmed) return;
                $.ajax({
                    url: $('#hdnURL_EliminarMovimiento').val(),
                    method: 'POST',
                    data: { id: id },
                    success: function (resp) {
                        if (resp.success) ModuloFinanciero.Buscar();
                        else Swal.fire('Error', resp.message || 'No se pudo eliminar.', 'error');
                    },
                    error: function () { Swal.fire('Error', 'No se pudo conectar con el servidor.', 'error'); }
                });
            });
        }
    };
})();

$(function () {
    ModuloFinanciero.IniciarScripts();
});
