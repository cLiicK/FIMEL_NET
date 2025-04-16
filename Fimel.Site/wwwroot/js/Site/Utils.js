function onlyNumbersWithK(e) {
    if (e.which == 75 || e.which == 107 || e.which == 48 || e.which == 49 || e.which == 50 || e.which == 51 || e.which == 52 || e.which == 53
        || e.which == 54 || e.which == 55 || e.which == 56 || e.which == 57) {
        return;
    } else {
        e.preventDefault();
    }
}

function onlyNumbersWithDot(e) {
    if (e.which == 48 || e.which == 49 || e.which == 50 || e.which == 51 || e.which == 52 || e.which == 53
        || e.which == 54 || e.which == 55 || e.which == 56 || e.which == 57 || e.which == 46) {
        return;
    } else {
        e.preventDefault();
    }
}

function onlyNumbers(e) {
    var key = window.event ? e.which : e.keyCode;
    if (key < 48 || key > 57) {
        e.preventDefault();
    }
}

function onlyLetters(e) {
    if ((e.which >= 65 && e.which <= 90) || (e.which >= 97 && e.which <= 122) || e.which == 32 || e.which == 241) {
        return;
    } else {
        event.preventDefault();
    }
}

function validarRut(rut) {
    let limpiarRut = rut.replace(/\.?-?/g, '');
    let _rut = limpiarRut.substr(0, limpiarRut.length - 1);

    if (!ValidaNumero(_rut)) {
        return "00";
    }
    let dv = limpiarRut.substr(limpiarRut.length - 1, 1).toUpperCase();
    if (dv != getDV(_rut).toString()) {
        return "01";
    }
    return _rut;
}

function ValidaNumero(numero) {
    let esNumero = false;
    for (i = 0; i < numero.length; i++) {
        for (var f = 0; f < 10; f++) {
            if (numero[i] == f) {
                esNumero = true;
                break;
            }
        }
        if (esNumero) {
            esNumero = false;
        } else {
            return false;
        }
    }
    return true;
}

function getDV(numero) {
    nuevo_numero = numero.toString().split("").reverse().join("");
    for (i = 0, j = 2, suma = 0; i < nuevo_numero.length; i++, ((j == 7) ? j = 2 : j++)) {
        suma += (parseInt(nuevo_numero.charAt(i)) * j);
    }
    n_dv = 11 - (suma % 11);
    return ((n_dv == 11) ? 0 : ((n_dv == 10) ? "K" : n_dv));
}

function getUrlParameter(name) {
    name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
        results = regex.exec(location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}

function calcularEdad(fecha) {
    var hoy = new Date();
    var cumpleanos = new Date(fecha);
    var edad = hoy.getFullYear() - cumpleanos.getFullYear();
    var m = hoy.getMonth() - cumpleanos.getMonth();

    if (m < 0 || (m === 0 && hoy.getDate() < cumpleanos.getDate())) {
        edad--;
    }

    return edad;
}

function _calculateAge(birthday) { // birthday is a date
    var ageDifMs = Date.now() - birthday.getTime();
    var ageDate = new Date(ageDifMs); // miliseconds from epoch
    return Math.abs(ageDate.getUTCFullYear() - 1970);
}

function ObtenerRutSTR(rut, dv) {
    let rutFormateado = rut.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    rutFormateado += '-' + dv.toUpperCase();
    return rutFormateado;
}



//LOADING
showLoading = function (button) {
    button.disabled = true;
    button.innerHTML = button.innerHTML + '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
}
closeLoading = function (button) {
    button.disabled = false;
    button.innerHTML = button.innerHTML.replace('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>', '');
}