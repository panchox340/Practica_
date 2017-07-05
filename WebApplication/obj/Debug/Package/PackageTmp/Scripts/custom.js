$(document).on('ready', function () {
    $(document).on('click', 'a.main-content', function (e) {
        e.preventDefault();
        var url = $(this).attr("href");
        loadAjaxContent(url);
    });

    $("#myModal").on("show.bs.modal", function (e) {
        var link = $(e.relatedTarget);

        if (link.hasClass('modal-ajax-load')) {
            if (link.hasClass('pdf-size')) {
                $(this).find('.modal-dialog').css({
                    position: 'relative',
                    display: 'table',
                    'overflow-y': 'auto',
                    'overflow-x': 'auto',
                    width: 'auto',
                    'min-width': 'calc(100% - 100px)'
                });
            } else {
                $(this).find('.modal-dialog').removeAttr('style');
            }
            var modal_content = $(this).find(".modal-content");
            $.ajax({
                async: true,
                beforeSend: function () {
                    modal_content.html('<h2><i class="fa fa-refresh fa-spin"></i> Cargando Solicitud...</h2>');
                },
                success: function (result) {
                    modal_content.html(result);
                },
                error: function () {
                    modal_content.html('<h2>Se ha generado un error</h2>');
                },
                url: link.attr("href")
            });
            
        }
    });
});


function loadAjaxContent(url) {
    $.ajax({
        async: true,
        beforeSend: function () {
            $("#content").html('<h2><i class="fa fa-refresh fa-spin"></i> Cargando Solicitud...</h2>');
        },
        success: function (result) {
            $("#content").html(result);
            window.history.pushState("", "PayRoll-Portal", url);
        },
        error: function () {
            $("#content").html('<h2>Se ha generado un error</h2>');
        },
        url: url
    });
}






function setFormulario(form, rules, messages, contenedor) {
    if (contenedor === undefined || contenedor === null) {
        contenedor = "#remoteModal .modal-content";
    }

    var $orderForm = $("#" + form).validate({
        rules: rules,
        messages: messages,
        // Do not change code below
        errorPlacement: function (error, element) {
            error.insertAfter(element.parent());
        }
    });

    $("button[action=save]").click(function () {
        $("button[action=save]").prop('disabled', true);
        if ($('#' + form).valid()) {
            var formJS = document.getElementById(form);
            var formData = new FormData(formJS);
            $.ajax({
                url: document.getElementById(form).action,
                type: document.getElementById(form).method,
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    if (result.success) {
                        //console.log("form", "retorno exito");
                        mensajeExito("Formulario", "Registro procesado con exito");
                        loadAjaxContent(window.location.pathname);
                        //window.location.reload(true);
                    } else {
                        if (result.error) {
                            //console.log("form", "retorno error");
                            mensajeError("Formulario", "Registro con errores<br>" + result.mensaje);
                        } else {
                            //console.log("form", "retorno html", contenedor, result);
                            $(contenedor).html(result);
                        }
                    }
                },
                complete: function () {
                    $("button[action=save]").prop('disabled', false);
                }
            });
        } else {
            console.log("error formulario");
            $("button[action=save]").prop('disabled', false);
        }
    });
}



function mensajeExito(titulo, mensaje) {
    $('#myModal').modal('hide');
    iziToast.success({
        timeout: 3000,
        title: titulo,
        message: "<i class='fa fa-clock-o'></i> <i>" + mensaje + "...</i>",
        position: 'topRight',
        transitionIn: 'bounceInLeft'
    });
}

function mensajeError(titulo, mensaje) {
    iziToast.error({
        title: titulo,
        message: "<i class='fa fa-clock-o'></i> <i>" + mensaje + "...</i>",
        position: 'topRight',
        transitionIn: 'fadeInDown'
    });
}

//funcion para modal de confirmacion de eliminacion
function confirmDelete(mensaje, url) {
    var msg = mensaje;
    if (msg == null || msg == "") {
        msg = "¿Desea eliminar el registro?";
    }
    $('#Modal_Background').modal({ backdrop: 'static', keyboard: false }).modal('show');
    iziToast.show({
        color: 'dark',
        icon: 'icon-person',
        title: 'Eliminación',
        transitionIn: 'flipInX',
        message: msg,
        position: 'center', // bottomRight, bottomLeft, topRight, topLeft, topCenter, bottomCenter
        progressBarColor: 'rgb(0, 255, 184)',
        buttons: [
            ['<button>Ok</button>', function (instance, toast) {
                $.ajax({
                    url: url,
                    error: function () { },
                    success: function (result) {
                        if (result.success) {
                            $('#Modal_Background').modal('hide');
                            mensajeExito("Formulario", "Registro procesado con exito");
                            iziToast.hide({
                                transitionOut: 'flipOutX', 
                            }, toast);
                        } else {
                            if (result.error) {
                                $('#Modal_Background').modal('hide');
                                mensajeError("Formulario", "Registro con errores<br>" + result.mensaje);
                                iziToast.hide({
                                    transitionOut: 'flipOutX'
                                }, toast);    
                            } 
                        }
                    },
                    complete: function () {
                       loadAjaxContent(window.location.pathname);                     
                    }
                });
            }],
            ['<button>Close</button>', function (instance, toast) {
                iziToast.hide({
                    transitionOut: 'flipOutX'
                }, toast); 
                $('#Modal_Background').modal('hide');
            }]           
        ],
        onClose: function (instance, toast) { //esconde el modal si la barra de progreso del alert termina
            $('#Modal_Background').modal('hide');                  
        }
    });
}
