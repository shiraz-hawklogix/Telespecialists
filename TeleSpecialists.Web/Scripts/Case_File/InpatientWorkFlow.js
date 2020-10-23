$(document).ready(function () {
    
    checktimefirst(); checkVideotimefirst(); checkNeedleTime();
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
    if ($("#3").is(':checked')) {
        $('.childarrivalneedle').show(200);
    } else {
        $('.childarrivalneedle').hide(200);
    }
    if ($("#4").is(':checked')) {
        $('.childdelaysimaging').show(200);
    } else {
        $('.childdelaysimaging').hide(200);
    }
    if ($("#8").is(':checked')) {
        $('.childmedicaldecisionmaking').show(200);
    } else {
        $('.childmedicaldecisionmaking').hide(200);
    }
    if ($("#10").is(':checked')) {
        $('.childrelatedlab').show(200);
        if ($("#1001").is(':checked')) {
            $('.childlabdrawanddelivery').show(200);

        } else {
            $('.childlabdrawanddelivery').hide(200);
        }
        $('.childrelatedlab').show(200);
        if ($("#1002").is(':checked')) {
            $('.childlabprocessing').show(200);

        } else {
            $('.childlabprocessing').hide(200);
        }
    } else {
        $('.childrelatedlab').hide(200);
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