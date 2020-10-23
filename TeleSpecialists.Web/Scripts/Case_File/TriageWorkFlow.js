$(document).ready(function () {
    checkStartTimeDelay(); checktimefirst(); checkVideotimefirst(); checkNeedleTime();

    if ($(".gotVal_rad").is(':checked')) {
        $('.hideShowUl').show(200);
    } else {
        $('.hideShowUl').hide(200);
    }
    if ($(".checkRecognition").is(':checked')) {
        $('.childRecognition').show(200);
    } else {
        $('.childRecognition').hide(200);
    }
    if ($(".checkTrigger").is(':checked')) {
        $('.childTrigger').show(200);
    } else {
        $('.childTrigger').hide(200);
    }
    if ($(".checkRooming").is(':checked')) {
        $('.childRooming').show(200);
    } else {
        $('.childRooming').hide(200);
    }
    if ($("#1").is(':checked')) {
        $('.childtimefirst').show(200);
    } else {
        $('.childtimefirst').hide(200);
    }
    if ($("#2").is(':checked')) {
        $('.childVideoStrt').show(200);
    } else {
        $('.childVideoStrt').hide(200);
    }
    if ($(".needleTime").is(':checked')) {
        $('.childarrivalneedle').show(200);
    } else {
        $('.childarrivalneedle').hide(200);
    }
    if ($("#4").is(':checked')) {
        $('.childdelaysimaging').show(200);
    } else {
        $('.childdelaysimaging').hide(200);
    }
    if ($("#6").is(':checked')) {
        $('.childbpmanamement').show(200);
    } else {
        $('.childbpmanamement').hide(200);
    }
    if ($("#7").is(':checked')) {
        $('.childtpaadministration').show(200);
    } else {
        $('.childtpaadministration').hide(200);
    }
});


$(".gotVal_rad").click(function () {
    //$('.grandChild').hide();
    if ($(this).is(':checked')) {
        $('.hideShowUl').show(200);
    }
    else {
        if ($(".checkRecognition").is(':checked') || $(".checkTrigger").is(':checked') || $(".checkRooming").is(':checked') || $("#cas_triage_arivalstarttodelay").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty()
            }, 2000);
            $('.gotVal_rad').prop('checked', true);
            $('.hideShowUl').show(200);
        } else {
            $('.hideShowUl').hide(200);
        }
    }
    if ($(".checkRecognition").is(':checked')) {
        $('.childRecognition').show(200);
    } else {
        $('.childRecognition').hide(200);
    }
    if ($(".checkTrigger").is(':checked')) {
        $('.childTrigger').show(200);
    } else {
        $('.childTrigger').hide(200);
    }
    if ($(".checkRooming").is(':checked')) {
        $('.childRooming').show(200);
    } else {
        $('.childRooming').hide(200);
    }

});

$(".checkRecognition").click(function () {
    if ($(this).is(':checked')) {
        $('.childRecognition').show(200);
    }
    else {
        if ($("#102").is(':checked') || $("#103").is(':checked') || $("#104").is(':checked') || $("#105").is(':checked') || $("#106").is(':checked') || $("#cas_triage_recognition").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty()
            }, 2000);
            $('.checkRecognition').prop('checked', true);
            $('.childRecognition').show(200);
        } else {
            $('.childRecognition').hide(200);
        }
    }
});
$(".checkTrigger").click(function () {
    if ($(this).is(':checked')) {
        $('.childTrigger').show(200);
    }
    else {
        if ($("#108").is(':checked') || $("#109").is(':checked') || $("#110").is(':checked') || $("#111").is(':checked') || $("#cas_triage_strokealertrigger").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty()
            }, 2000);
            $('.checkTrigger').prop('checked', true);
            $('.childTrigger').show(200);
        } else {
            $('.childTrigger').hide(200);
        }
    }
});
$(".checkRooming").click(function () {
    if ($(this).is(':checked')) {
        $('.childRooming').show(200);
    }
    else {
        if ($("#113").is(':checked') || $("#114").is(':checked') || $("#115").is(':checked') || $("#cas_triage_transportandrooming").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty()
            }, 2000);
            $('.checkRooming').prop('checked', true);
            $('.childRooming').show(200);
        } else {
            $('.childRooming').hide(200);
        }
    }
});
function checkStartTimeDelay() {
    if ($(".gotVal_rad").is(':checked')) {
        $('.hideShowUl').show();
    }
    else {
        $('.hideShowUl').hide();
    }
}

var ids = $("#work-flow-ids").val();
var arrayOfIds = [];
if (ids) {
    arrayOfIds = ids.split(',');
    $.each(arrayOfIds, function (key, val) {
        $("#" + val).prop("checked", true);
    });
}
else {
    $('#div').hide();
}
var arr = arrayOfIds;
$(".gotVal").click(function () {
    var _val = $(this).val();
    if ($(this).is(':checked')) {
        //alert('add this to array :' + _val);  // checked
        arr.push(_val);
    }
    else {
        //alert('remove this to array :' + _val);
        var index = arr.indexOf(_val);
        if (index > -1) {
            arr.splice(index, 1);
        }
    }
    //alert('open arr :' + arr.toString());
    $("#work-flow-ids").val(arr.toString());
});


$("#btnShow").click(function () {
    $('#div').show();
});