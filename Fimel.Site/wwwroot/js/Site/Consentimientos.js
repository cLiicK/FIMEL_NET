// Consentimientos informados imprimibles (DIU/SIU e implante subdérmico).
// Los textos replican el documento "Consentimientos DIU-SIU e implantes actualizados".
// Dentro de cada ítem, {{profesional}} y {{dispositivo}} se reemplazan por los datos
// ingresados; el resto del texto se imprime tal cual.
var ModuloConsentimientos = (function () {

    var LEY_20584 = 'Además, acorde a la Ley Nacional 20.584, autorizo la creación de una base de datos a partir ' +
        'de información de mi historia clínica, consejería y el examen con fines académicos y de ' +
        'investigación, resguardando en todo momento la confidencialidad de mi información ' +
        'personal y sin menoscabo de mi atención ni menos sanción alguna.';

    var TIPOS = {
        InsercionDIU: {
            nombre: 'Inserción de DIU / SIU',
            titulo: 'Consentimiento Informado para Inserción de Dispositivo Intrauterino',
            rol: 'consultante o apoderada/o/e',
            placeholderDispositivo: 'Ej: T de Cobre, Mirena, Kyleena',
            observaciones: true,
            firmas: ['profesional', 'usuaria', 'representante'],
            items: [
                'Que el profesional {{profesional}} me ha explicado de forma clara, en qué consiste el procedimiento de inserción del dispositivo o sistema intrauterino ({{dispositivo}}).',
                {
                    texto: 'Dentro de sus complicaciones y riesgos, se me ha informado que:',
                    riesgos: [
                        'El procedimiento de inserción puede resultar doloroso para la mayoría de las mujeres.',
                        'Existe un riesgo de expulsión que es menor al 1%, principalmente los primeros 3 meses.',
                        'Existe un riesgo de infección pélvica del 1%.',
                        'Existe un riesgo de incrustación de 1 en 10.000 usuarias.',
                        'Existe un riesgo de perforación uterina alrededor del 1,5%.',
                        'Existe igualmente, riesgo de lipotimia, acompañada de mareos y/o náuseas.',
                        'Existe la posibilidad de que el DIU/SIU descienda y tenga que ser retirado, sin opción de reutilizarlo ni ser reembolsado.'
                    ]
                },
                'El examen pélvico y/o ecográfico, no garantiza el éxito de la inserción.',
                'Que he recibido consejería y he elegido de forma consciente e informada este método anticonceptivo.',
                'Declaro que toda la información entregada al profesional es completa y fidedigna.',
                'Me han sido entregadas las indicaciones y cuidados que debo tener post inserción del dispositivo o sistema intrauterino.',
                'Consiento de manera libre y voluntaria que se me realice el procedimiento de inserción del dispositivo o sistema intrauterino.',
                LEY_20584
            ]
        },
        ExtraccionDIU: {
            nombre: 'Extracción de DIU / SIU',
            titulo: 'Consentimiento Informado para Extracción de Dispositivo Intrauterino',
            rol: 'consultante o apoderada/o/e',
            placeholderDispositivo: 'Ej: T de Cobre, Mirena, Kyleena',
            observaciones: true,
            firmas: ['profesional', 'usuaria', 'representante'],
            items: [
                'Que el profesional {{profesional}} me ha explicado de forma clara, en qué consiste el procedimiento de extracción del dispositivo o sistema intrauterino ({{dispositivo}}).',
                'Me han sido entregadas las indicaciones y cuidados que debo tener post extracción del dispositivo o sistema intrauterino.',
                'Consiento de manera libre y voluntaria que se me realice el procedimiento de extracción del dispositivo o sistema intrauterino.',
                LEY_20584
            ]
        },
        InsercionImplante: {
            nombre: 'Inserción de implante subdérmico',
            titulo: 'Consentimiento Informado para Inserción de Implante Subdérmico',
            rol: 'paciente o apoderado',
            placeholderDispositivo: 'Ej: Implanon NXT, Jadelle',
            observaciones: false,
            firmas: ['usuaria', 'profesional', 'representante'],
            items: [
                'Que el profesional {{profesional}} me ha explicado de forma clara, en qué consiste el procedimiento de inserción del implante subdérmico ({{dispositivo}}), sus complicaciones y riesgos.',
                'Que he recibido consejería, resuelto dudas y he elegido de forma consciente e informada este método anticonceptivo.',
                'Me han sido entregadas las indicaciones y cuidados que debo tener post inserción del implante.',
                'Consiento de manera libre y voluntaria que se me realice el procedimiento de inserción del implante subdérmico.',
                'Dentro de sus complicaciones y riesgos, se me ha informado que: existe un riesgo de que, ante una cicatrización no adecuada, puede haber escape del implante. Existe riesgo de que el implante migre a la circulación venosa, por lo cual se me ha informado debo asistir a control en un mes o antes si noto que el implante se ha desplazado de la zona de inserción. Existe la posibilidad de presentar hematoma y/o moretones en la zona intervenida durante los días posteriores al procedimiento.',
                LEY_20584,
                'Declaro que toda la información entregada por mí, al profesional tratante es completa y fidedigna, tengo la certeza de que no estoy embarazada y que no soy alérgica al anestésico local.',
                'Me comprometo a asistir a los controles posteriores, de la semana, del mes y luego anual. Y a consultar en caso de necesidad fuera de estos tiempos establecidos.'
            ]
        },
        RetiroImplante: {
            nombre: 'Retiro de implante subdérmico',
            titulo: 'Consentimiento Informado para Retiro de Implante Subdérmico',
            rol: 'consultante o apoderado',
            placeholderDispositivo: 'Ej: Implanon NXT, Jadelle',
            observaciones: false,
            firmas: ['usuaria', 'profesional', 'representante'],
            items: [
                {
                    texto: 'Que el profesional {{profesional}} me ha explicado de forma clara, en qué consiste el procedimiento de retiro del implante subdérmico ({{dispositivo}}), sus complicaciones y riesgos, tales como:',
                    riesgos: [
                        'Que la zona intervenida presente hematoma y/o “moretones” durante los días posteriores al procedimiento.',
                        'Existe un riesgo de que pueda haber infección en la zona intervenida.',
                        'En algunos casos, puede ser necesaria una segunda incisión para el éxito del procedimiento.',
                        'En algunos casos, es posible que no se pueda extraer el/los implante/es.'
                    ]
                },
                'Al término de la consulta, se me hará la entrega de las indicaciones y cuidados que debo tener posterior al procedimiento.',
                'Consiento de manera libre y voluntaria que se me realice el procedimiento de retiro del implante subdérmico.',
                'Declaro que toda la información entregada al profesional es completa y fidedigna.',
                LEY_20584
            ]
        }
    };

    function esc(texto) {
        return $('<div>').text(texto || '').html();
    }

    // Valor subrayado; si viene vacío queda la línea en blanco para completarla a mano.
    function campo(valor, clase) {
        return '<span class="campo' + (clase ? ' ' + clase : '') + '">' + (valor ? esc(valor) : '&nbsp;') + '</span>';
    }

    function renderTexto(texto, datos) {
        // Se escapa primero el texto fijo y luego se insertan los campos ya escapados.
        return esc(texto)
            .replace('{{profesional}}', campo(datos.profesional, 'largo'))
            .replace('{{dispositivo}}', campo(datos.dispositivo));
    }

    function renderItems(tipo, datos) {
        return tipo.items.map(function (item) {
            if (typeof item === 'string') {
                return '<li>' + renderTexto(item, datos) + '</li>';
            }
            var riesgos = item.riesgos.map(function (r) { return '<li>' + esc(r) + '</li>'; }).join('');
            return '<li>' + renderTexto(item.texto, datos) + '<ul class="riesgos">' + riesgos + '</ul></li>';
        }).join('');
    }

    function renderObservaciones(tipo, datos) {
        if (!tipo.observaciones && !datos.observaciones) return '';
        var contenido = datos.observaciones
            ? '<div class="texto">' + esc(datos.observaciones) + '</div>'
            : '<div class="linea"></div><div class="linea"></div>';
        return '<div class="observaciones"><span class="lbl">Observaciones:</span>' + contenido + '</div>';
    }

    function renderFirmas(tipo, datos) {
        var esMatrona = /matr/i.test(datos.tituloProfesional || '') || !datos.tituloProfesional;
        var bloques = {
            profesional: function () {
                var img = datos.firmaBase64
                    ? '<img class="firma-img" src="data:image/png;base64,' + datos.firmaBase64 + '" alt="Firma">'
                    : '';
                return '<td>' +
                    '<div class="firma-espacio">' + img + '</div>' +
                    '<div class="firma-linea">Firma ' + (esMatrona ? 'Matrona/ón' : esc(datos.tituloProfesional)) + '</div>' +
                    '<div class="firma-detalle">' + esc(datos.profesional) + '</div>' +
                    '<div class="firma-detalle">' + esc(datos.rutProfesional) + '</div>' +
                    '</td>';
            },
            usuaria: function () {
                return '<td>' +
                    '<div class="firma-espacio"></div>' +
                    '<div class="firma-linea">Firma Usuaria/o/e</div>' +
                    '<div class="firma-detalle">' + esc(datos.paciente) + '</div>' +
                    '<div class="firma-detalle">' + esc(datos.rut) + '</div>' +
                    '</td>';
            },
            representante: function () {
                return '<td>' +
                    '<div class="firma-espacio"></div>' +
                    '<div class="firma-linea">Firma Representante Legal</div>' +
                    '<div class="firma-rut-blanco">Nombre:</div>' +
                    '<div class="firma-rut-blanco">RUT:</div>' +
                    '</td>';
            }
        };
        return tipo.firmas.map(function (f) { return bloques[f](); }).join('');
    }

    return {
        Tipos: TIPOS,

        // datos: { paciente, rut, domicilio, fecha, profesional, tituloProfesional, rutProfesional,
        //          firmaBase64, dispositivo, observaciones, institucion, logoUrl }
        Imprimir: function (codigoTipo, datos) {
            var tipo = TIPOS[codigoTipo];
            if (!tipo) return;

            // Se abre la ventana antes del fetch para que el navegador no la bloquee como popup.
            var w = window.open('', '_blank', 'width=850,height=700');

            fetch('/mails/consentimiento-informado.html?v=' + Date.now())
                .then(function (r) { return r.text(); })
                .then(function (template) {
                    var html = template
                        .replace(/\{\{titulo_documento\}\}/g, esc(tipo.titulo))
                        .replace(/\{\{logo_url\}\}/g, datos.logoUrl || '')
                        .replace(/\{\{institucion\}\}/g, esc(datos.institucion))
                        .replace(/\{\{paciente\}\}/g, esc(datos.paciente) || '&nbsp;')
                        .replace(/\{\{rut_doc\}\}/g, esc(datos.rut) || '&nbsp;')
                        .replace(/\{\{domicilio\}\}/g, esc(datos.domicilio) || '&nbsp;')
                        .replace(/\{\{rol\}\}/g, esc(tipo.rol))
                        .replace(/\{\{fecha\}\}/g, esc(datos.fecha) || '&nbsp;')
                        .replace(/\{\{items\}\}/g, renderItems(tipo, datos))
                        .replace(/\{\{observaciones_section\}\}/g, renderObservaciones(tipo, datos))
                        .replace(/\{\{firmas\}\}/g, renderFirmas(tipo, datos));

                    w.document.open();
                    w.document.write(html);
                    w.document.close();
                })
                .catch(function () {
                    if (w) w.close();
                    Swal.fire('Error', 'No se pudo cargar la plantilla del consentimiento.', 'error');
                });
        }
    };
})();
