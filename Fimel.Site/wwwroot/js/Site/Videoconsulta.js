// Sala de videoconsulta (Daily Prebuilt embebido). La usan la página pública del paciente (Videoconsulta/Sala)
// y la del profesional (Horario/Videoconsulta). Datos desde hidden inputs:
//   hdnVideoEstado (Pendiente | Abierta | Finalizada | SinSala), hdnVideoUrlSala,
//   hdnVideoSegundosParaAbrir, hdnVideoSegundosParaCerrar.
// Contenedores: #contenedorVideo (donde va el iframe), #panelVideoSalida (aviso al salir), #panelVideoFinalizada.
var ModuloVideoconsulta = (function () {
    var frame = null;
    var MAX_TIMEOUT = 2147483647; // setTimeout no acepta más (~24,8 días)

    function mostrar(id) {
        $('#panelVideoSalida, #panelVideoFinalizada').addClass('d-none');
        if (id) $(id).removeClass('d-none');
    }

    function finalizar() {
        if (frame) {
            try { frame.destroy(); } catch (e) { }
            frame = null;
        }
        $('#contenedorVideo').addClass('d-none');
        mostrar('#panelVideoFinalizada');
    }

    function unirse() {
        var url = $('#hdnVideoUrlSala').val();
        if (!url || typeof window.DailyIframe === 'undefined') {
            Swal.fire('No se pudo cargar la videollamada', 'Revisa tu conexión a internet y recarga la página.', 'error');
            return;
        }

        mostrar(null);
        $('#contenedorVideo').removeClass('d-none');

        if (!frame) {
            frame = window.DailyIframe.createFrame(document.getElementById('contenedorVideo'), {
                iframeStyle: { position: 'absolute', inset: '0', width: '100%', height: '100%', border: '0' },
                showLeaveButton: true,
                showFullscreenButton: true
            });

            frame.on('left-meeting', function () {
                $('#contenedorVideo').addClass('d-none');
                mostrar('#panelVideoSalida');
            });

            frame.on('error', function (e) {
                // Sala expirada o eliminada (cita anulada o reagendada)
                var msg = (e && e.errorMsg) || '';
                if (/exp|not.?found|no longer|eject/i.test(msg)) {
                    finalizar();
                } else {
                    Swal.fire('Error en la videollamada', 'Recarga la página para volver a intentarlo.', 'error');
                }
            });
        }

        frame.join({ url: url, lang: 'es' });
    }

    return {
        Iniciar: function () {
            var estado = $('#hdnVideoEstado').val();
            var segAbrir = parseInt($('#hdnVideoSegundosParaAbrir').val() || '0', 10);
            var segCerrar = parseInt($('#hdnVideoSegundosParaCerrar').val() || '0', 10);

            if (estado === 'Pendiente') {
                // Recarga sola cuando abre la sala (más unos segundos de holgura)
                var ms = (segAbrir + 3) * 1000;
                if (ms < MAX_TIMEOUT) setTimeout(function () { location.reload(); }, ms);
                return;
            }

            if (estado === 'SinSala') {
                // Se reintenta crear la sala cada minuto mientras dure la ventana
                setTimeout(function () { location.reload(); }, 60 * 1000);
                return;
            }

            if (estado === 'Abierta') {
                var msCierre = segCerrar * 1000;
                if (msCierre < MAX_TIMEOUT) setTimeout(finalizar, msCierre);
                unirse();
            }
        },

        VolverAEntrar: function () {
            unirse();
        }
    };
})();

$(function () {
    ModuloVideoconsulta.Iniciar();
});
