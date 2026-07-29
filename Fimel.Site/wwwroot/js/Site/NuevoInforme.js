var ModuloNuevoInforme = (function () {
    var pacienteActual = null;
    var plantillaActual = null;
    var camposActuales = [];
    var dictadoToken = null;
    var dictadoInterval = null;
    var dictadoContinuoRecognition = null;
    var dictadoContinuoActivo = false;

    // ── PACIENTE ────────────────────────────────────────────────────────────

    function mostrarPaciente(p) {
        pacienteActual = p;
        $('#hiddenIdPaciente').val(p.Id);
        $('#lblNombrePaciente').text((p.Nombres + ' ' + (p.ApellidoPaterno || '') + ' ' + (p.ApellidoMaterno || '')).trim());
        var info = [];
        if (p.FechaNacimiento) {
            var edad = Math.floor((new Date() - new Date(p.FechaNacimiento)) / (365.25 * 24 * 3600 * 1000));
            info.push(edad + ' años');
        }
        if (p.RUT) info.push('RUT ' + p.RUT);
        else if (p.NumeroDocumento) info.push(p.NumeroDocumento);
        $('#lblInfoPaciente').text(info.join(' · '));
        $('#divDatosPaciente').show();
    }

    // ── CAMPOS ──────────────────────────────────────────────────────────────

    function renderCampos() {
        var cont = $('#contenedorCampos');
        if (!camposActuales.length) { cont.html('<p class="text-muted">Sin campos configurados en esta plantilla.</p>'); return; }

        var html = camposActuales.map(function (c) {
            return '<div class="mb-3" id="grupo_' + c.NombreCampo + '">' +
                '<label class="form-label fw-semibold" style="font-size:.9rem">' + c.Etiqueta +
                    (c.Obligatorio ? ' <span class="text-danger">*</span>' : '') +
                '</label>' +
                '<div class="input-group">' +
                    '<textarea class="form-control form-control-sm" rows="2" ' +
                        'id="campo_' + c.NombreCampo + '" placeholder="' + c.Etiqueta + '..."></textarea>' +
                    '<button class="btn btn-outline-secondary" type="button" ' +
                        'onclick="ModuloNuevoInforme.DictarCampo(\'' + c.NombreCampo + '\', this)" ' +
                        'title="Dictar este campo">' +
                        '<i class="fas fa-microphone"></i>' +
                    '</button>' +
                '</div>' +
            '</div>';
        }).join('');

        cont.html(html);
        $('#divCamposInforme').show();
        $('#divAcciones').show();
    }

    // ── PARSER DICTADO CONTINUO ─────────────────────────────────────────────

    function parsearYDistribuir(texto) {
        if (!camposActuales.length || !texto.trim()) return;

        // Ordena campos por longitud de etiqueta descendente (evita coincidencias parciales)
        var sorted = camposActuales.slice().sort(function (a, b) { return b.Etiqueta.length - a.Etiqueta.length; });
        var keywords = sorted.map(function (c) {
            return c.Etiqueta.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
        });

        var pattern = new RegExp('(' + keywords.join('|') + ')\\s*:?\\s*', 'gi');
        var parts = texto.split(pattern).filter(function (s) { return s && s.trim(); });

        var campoActivo = null;
        parts.forEach(function (part) {
            var matchCampo = sorted.find(function (c) {
                return c.Etiqueta.toLowerCase() === part.trim().toLowerCase();
            });
            if (matchCampo) {
                campoActivo = matchCampo;
            } else if (campoActivo) {
                var el = document.getElementById('campo_' + campoActivo.NombreCampo);
                if (el) el.value = (el.value ? el.value + ' ' : '') + part.trim();
            }
        });
    }

    // ── QR ──────────────────────────────────────────────────────────────────

    function generarQR(url) {
        $('#divQR').empty();
        $('#divQRModal').empty();
        if (typeof QRCode !== 'undefined') {
            new QRCode(document.getElementById('divQR'), { text: url, width: 180, height: 180 });
            new QRCode(document.getElementById('divQRModal'), { text: url, width: 180, height: 180 });
        }
    }

    // ── POLLING ─────────────────────────────────────────────────────────────

    function iniciarPolling(token) {
        if (dictadoInterval) clearInterval(dictadoInterval);
        dictadoInterval = setInterval(function () {
            $.get($('#hdnURL_PollDictado').val(), { token: token }, function (data) {
                if (!data.activa) {
                    detenerPolling();
                    $('#divEstadoDictado').removeClass('bg-success').addClass('bg-secondary').text('Sesión cerrada');
                    return;
                }
                if (data.textos && data.textos.length) {
                    var textoCompleto = data.textos.join(' ');
                    $('#divUltimoTexto').text(textoCompleto);
                    parsearYDistribuir(textoCompleto);
                }
            });
        }, 2000);
    }

    function detenerPolling() {
        if (dictadoInterval) { clearInterval(dictadoInterval); dictadoInterval = null; }
    }

    return {
        // ── BUSCAR PACIENTE ────────────────────────────────────────────────
        BuscarPaciente: function () {
            var rut = $('#inputRutBusqueda').val().trim().replace(/\./g, '').replace(/-/g, '');
            var numDoc = $('#inputNumDocBusqueda').val().trim();

            if (!rut && !numDoc) { Swal.fire('Ingresa RUT o N° documento', '', 'warning'); return; }

            var url, data;
            if (rut) {
                url = $('#hdnURL_BuscarPaciente').val();
                data = { rut: parseInt(rut) };
            } else {
                url = $('#hdnURL_BuscarPorNumDoc').val();
                data = { numDoc: numDoc };
            }

            $.get(url, data, function (response) {
                if (response && response.Id) { mostrarPaciente(response); }
                else { Swal.fire('Paciente no encontrado', 'Verifica el RUT o documento.', 'warning'); }
            }).fail(function () { Swal.fire('Error', 'Error al buscar paciente.', 'error'); });
        },

        // ── CARGAR PLANTILLA ───────────────────────────────────────────────
        CargarPlantilla: function (id) {
            if (!id) { $('#divCamposInforme').hide(); $('#divAcciones').hide(); return; }
            $.get($('#hdnURL_ObtenerPlantilla').val(), { id: id }, function (response) {
                if (!response.success) { Swal.fire('Error', 'No se pudo cargar la plantilla.', 'error'); return; }
                plantillaActual = response.data;
                camposActuales = response.data.Campos || [];
                renderCampos();
            });
        },

        // ── DICTADO POR CAMPO ──────────────────────────────────────────────
        DictarCampo: function (nombreCampo, btn) {
            var SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
            if (!SpeechRecognition) { Swal.fire('No soportado', 'Usa Chrome o Edge para dictar.', 'warning'); return; }

            $(btn).prop('disabled', true).addClass('btn-danger').removeClass('btn-outline-secondary');

            var r = new SpeechRecognition();
            r.lang = 'es-CL'; r.continuous = false; r.interimResults = false;

            r.onresult = function (e) {
                var texto = Array.from(e.results).map(function (res) { return res[0].transcript; }).join(' ');
                var el = document.getElementById('campo_' + nombreCampo);
                if (el) el.value = (el.value ? el.value + ' ' : '') + texto;
            };
            r.onend = function () {
                $(btn).prop('disabled', false).removeClass('btn-danger').addClass('btn-outline-secondary');
            };
            r.start();
        },

        // ── DICTADO CONTINUO ───────────────────────────────────────────────
        ToggleDictadoContinuo: function (btn) {
            var SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
            if (!SpeechRecognition) { Swal.fire('No soportado', 'Usa Chrome o Edge.', 'warning'); return; }

            if (!dictadoContinuoActivo) {
                dictadoContinuoRecognition = new SpeechRecognition();
                dictadoContinuoRecognition.lang = 'es-CL';
                dictadoContinuoRecognition.continuous = true;
                dictadoContinuoRecognition.interimResults = false;

                dictadoContinuoRecognition.onresult = function (e) {
                    var texto = '';
                    for (var i = e.resultIndex; i < e.results.length; i++) {
                        if (e.results[i].isFinal) texto += e.results[i][0].transcript + ' ';
                    }
                    if (texto.trim()) parsearYDistribuir(texto.trim());
                };

                dictadoContinuoRecognition.start();
                dictadoContinuoActivo = true;
                $(btn).addClass('btn-danger').removeClass('btn-outline-secondary')
                      .html('<i class="fas fa-stop me-1"></i> Detener dictado');
            } else {
                if (dictadoContinuoRecognition) dictadoContinuoRecognition.stop();
                dictadoContinuoActivo = false;
                $(btn).removeClass('btn-danger').addClass('btn-outline-secondary')
                      .html('<i class="fas fa-microphone-lines me-1"></i> Dictado continuo');
            }
        },

        // ── DICTADO DESDE CELULAR ──────────────────────────────────────────
        IniciarDictadoCelular: function (btn) {
            $(btn).prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-1"></i> Iniciando...');

            $.post($('#hdnURL_IniciarDictadoCelular').val(), function (response) {
                if (!response.success) {
                    Swal.fire('Error', response.message || 'Error al iniciar sesión.', 'error');
                    $(btn).prop('disabled', false).html('<i class="fas fa-mobile-alt me-1"></i> Dictar desde celular');
                    return;
                }

                dictadoToken = response.token;
                generarQR(response.url);

                $('#cardDictadoCelular').show();
                $('#divEstadoDictado').removeClass('bg-secondary').addClass('bg-success').text('Sesión activa');
                $(btn).prop('disabled', false).html('<i class="fas fa-mobile-alt me-1"></i> Celular conectado');
                $(btn).addClass('btn-success').removeClass('btn-blue');

                iniciarPolling(response.token);
            }).fail(function () {
                Swal.fire('Error', 'Error al iniciar sesión de dictado.', 'error');
                $(btn).prop('disabled', false).html('<i class="fas fa-mobile-alt me-1"></i> Dictar desde celular');
            });
        },

        DetenerDictadoCelular: function () {
            detenerPolling();
            if (dictadoToken) {
                $.post($('#hdnURL_CerrarDictado').val(), { token: dictadoToken });
                dictadoToken = null;
            }
            $('#cardDictadoCelular').hide();
            var btn = $('#btnDictarCelular');
            btn.prop('disabled', false).html('<i class="fas fa-mobile-alt me-1"></i> Dictar desde celular')
               .removeClass('btn-success').addClass('btn-blue');
        },

        // ── GENERAR INFORME ────────────────────────────────────────────────
        GenerarInforme: function () {
            if (!plantillaActual) { Swal.fire('Sin plantilla', 'Selecciona una plantilla.', 'warning'); return; }

            var htmlFinal = plantillaActual.HtmlBase;
            camposActuales.forEach(function (c) {
                var valor = ($('#campo_' + c.NombreCampo).val() || '').trim();
                if (!valor && c.Obligatorio) {
                    Swal.fire('Campo requerido', 'El campo "' + c.Etiqueta + '" es obligatorio.', 'warning');
                    return;
                }
                htmlFinal = htmlFinal.split('{{' + c.NombreCampo + '}}').join(valor || '—');
            });

            var nombreDoctor = $('#hdnNombreDoctor').val();
            var titulo = $('#hdnTituloProfesional').val();
            var hoy = new Date().toLocaleDateString('es-CL');

            var ventana = window.open('', '_blank', 'width=860,height=700');
            ventana.document.write('<!DOCTYPE html><html lang="es"><head><meta charset="utf-8">' +
                '<title>Informe Ecográfico</title>' +
                '<style>' +
                'body{font-family:Arial,sans-serif;max-width:720px;margin:0 auto;padding:40px 50px;color:#1a2a3a;}' +
                'h2{color:#1a2a3a;letter-spacing:1px;text-transform:uppercase;font-size:18px;margin:0;}' +
                '.divider{height:3px;background:linear-gradient(to right,#F89B9B,#f5c6c6);margin:12px 0 20px;border-radius:2px;}' +
                '.prof-bar{background:#fff8f8;border-left:4px solid #F89B9B;padding:10px 14px;display:flex;justify-content:space-between;margin-bottom:24px;border-radius:0 6px 6px 0;}' +
                '.footer{margin-top:60px;text-align:right;border-top:1px solid #eee;padding-top:12px;}' +
                '.powered{text-align:center;margin-top:36px;font-size:10px;color:#ddd;letter-spacing:1px;}' +
                '@media print{body{-webkit-print-color-adjust:exact;print-color-adjust:exact;}}' +
                '</style></head><body>' +
                '<div style="display:flex;justify-content:space-between;align-items:center;">' +
                    '<h2>Informe Ecográfico</h2>' +
                    '<span style="font-size:11px;color:#888">' + hoy + '</span>' +
                '</div>' +
                '<div class="divider"></div>' +
                '<div class="prof-bar">' +
                    '<div><div style="font-weight:bold;font-size:15px">' + titulo + ' ' + nombreDoctor + '</div></div>' +
                    '<div style="font-size:12px;color:#666">' + hoy + '</div>' +
                '</div>' +
                htmlFinal +
                '<div class="footer">' +
                    '<div style="font-weight:bold;font-size:13px">' + titulo + ' ' + nombreDoctor + '</div>' +
                '</div>' +
                '<div class="powered">FIMEL · Sistema de Gestión Médica</div>' +
                '<script>window.onload=function(){window.print();};window.onafterprint=function(){window.close();};<\/script>' +
                '</body></html>');
            ventana.document.close();
        },

        LimpiarFormulario: function () {
            camposActuales.forEach(function (c) { $('#campo_' + c.NombreCampo).val(''); });
        }
    };
})();
