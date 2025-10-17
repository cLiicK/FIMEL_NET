$(document).ready(function () {
    var rutPaciente = $('#hiddenRutPaciente').val();
    var numDocumento = $('#hiddenNumDocumento').val();
    var tipoDocumento = $('#hiddenTipoDocumento').val();

    if (rutPaciente && tipoDocumento === "RUT") {
        ObtenerConsultasAnteriores(rutPaciente, "RUT");
    } else if (numDocumento && tipoDocumento !== "RUT") {
        ObtenerConsultasAnteriores(numDocumento, tipoDocumento);
    }
});