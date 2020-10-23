$(document).ready(function () {
    checkStartTimeDelay(); checktimefirst(); checkVideotimefirst(); checkNeedleTime();
    if ($(".gotVal_rad").is(':checked')) {
        $('.childarivaldelay').show(200);
    } else {
        $('.childarivaldelay').hide(200);
    }
    if ($("#201").is(':checked')) {
        $('.childpoorems').show(200);
    } else {
        $('.childpoorems').hide(200);
    }
    if ($("#208").is(':checked')) {
        $('.childidntifcationaccurd').show(200);
    } else {
        $('.childidntifcationaccurd').hide(200);
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
    if ($(this).is(':checked')) {
        $('.childarivaldelay').show(200);
    } else {
        if ($("#201").is(':checked') || $("#208").is(':checked') || $("#cas_ems_arivaltostarttimedelay").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty()
            }, 2000);
            $(this).prop('checked', true);
            $('.childarivaldelay').show(200);
        } else {
            $('.childarivaldelay').hide(200);
        }
    }
});

$("#201").click(function () {
    if ($(this).is(':checked')) {
        $('.childpoorems').show(200);
    }
    else {
        if ($("#202").is(':checked') || $("#203").is(':checked') || $("#204").is(':checked') || $("#205").is(':checked') || $("#206").is(':checked') || $("#207").is(':checked') || $("#cas_ems_poor_identification").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty()
            }, 2000);
            $(this).prop('checked', true);
            $('.childpoorems').show(200);
        } else {
            $('.childpoorems').hide(200);
        }

    }
});

$("#208").click(function () {
    if ($(this).is(':checked')) {
        $('.childidntifcationaccurd').show(200);
    }
    else {
        if ($("#209").is(':checked') || $("#210").is(':checked') || $("#211").is(':checked') || $("#212").is(':checked') || $("#213").is(':checked') || $("#214").is(':checked') || $("#cas_ems_identification_occurred").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox");
            setTimeout(function () {
                $("#messagealert").empty()
            }, 2000);
            $(this).prop('checked', true);
            $('.childidntifcationaccurd').show(200);
        } else {
            $('.childidntifcationaccurd').hide(200);
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