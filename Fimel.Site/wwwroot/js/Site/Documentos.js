var arrayFiles = []
var contDocumentos = 1;

$(document).ready(function () {

});

function ValidarDocumento(inputFile) {
    let file = inputFile.files[0];
    if (contDocumentos <= 4) {
        if (file.type.includes("gif") || file.type.includes("jpg") || file.type.includes("jpeg") || file.type.includes("png")
            || file.type.includes("tiff") || file.type.includes("bmp") || file.type.includes("pdf")) {
            let PesoArchivoEnMegas = file.size / Math.pow(1024, 2) //Peso en megas

            if (PesoArchivoEnMegas < 10) {
                let readFiles = new FileReader()

                readFiles.onload = function () {
                    let base64 = readFiles.result;
                    let docInfo = {
                        "Nombre": file.name,
                        "Formato": file.name.split('.').pop(),
                        "DocumentoBase64": base64
                    };
                    arrayFiles.push(docInfo);

                    if (arrayFiles.length > 0) {

                        let fileName = "";
                        if (file.name.length > 20) {
                            fileName = file.name.substr(0, 20) + "..." + file.name.split('.').pop();
                        } else {
                            fileName = file.name;
                        }

                        $("#divDocumentos").append(
                            '<div class="col-3" id="file' + contDocumentos + '"> <div class="card"> <div class="card-header"> <i class="far fa-file"></i> ' + fileName + ' </div> <div class="card-body"> <p class="card-text">Peso: ' + Math.round((PesoArchivoEnMegas + Number.EPSILON) * 100) / 100 + 'MB <br />Extension:' + file.name.split('.').pop() + ' </p> </div> </div> </div>'
                        )
                        contDocumentos++;

                        $('#inputFile').val("");
                    }
                }
                readFiles.readAsDataURL(file);
            }
            else {
                $('#inputFile').val("");
                alert("El archivo debe pesar menos de 10m");
            }
        }
        else {
            $('#inputFile').val("");
            alert("Tipo de archivo no válido. Debe incluir archivos de tipo imagen o pdf (gif, jpg, jpeg, png, tiff o pdf)");
        }
    }
    else {
        $('#inputFile').val("");
        alert("Cantidad máxima de documentos es cuatro");
    }
}

function GrabarDocumentos(object) {
    showLoading(object);
    Swal.fire({
        title: 'Grabar Documentos',
        text: '¿Esta seguro de grabar los documentos para el paciente?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Grabar',
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Paciente/GrabarDocumentos',
                data: {
                    archivosAdjuntos: JSON.stringify(arrayFiles)
                },
                method: 'POST',
                cache: false,
                async: true,
                success: function (response, jqXHR) {
                    if (response.Codigo === 200) {
                        closeLoading(object);
                        Swal.fire({
                            title: 'Documentos guardados!',
                            icon: 'success',
                        }).then((result) => {
                            if (result.isConfirmed) {
                                location.reload();
                            }
                        })
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    Swal.fire('Error', 'Favor comuniquese con un administrador', 'error');
                    closeLoading(object);
                }
            });
        }
        else {
            closeLoading(object);
        }
    })
}

