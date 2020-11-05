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


//$(".gotVal_rad").click(function () {
//    //$('.grandChild').hide();
//    if ($(this).is(':checked')) {
//        $('.hideShowUl').show(200);
//    }
//    else {
//        if ($(".checkRecognition").is(':checked') || $(".checkTrigger").is(':checked') || $(".checkRooming").is(':checked') || $("#cas_triage_arivalstarttodelay").val() != "") {
//            $("#messagealert").empty();
//            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
//            setTimeout(function () {
//                $("#messagealert").empty()
//            }, 2000);
//            $('.gotVal_rad').prop('checked', true);
//            $('.hideShowUl').show(200);
//        } else {
//            $('.hideShowUl').hide(200);
//        }
//    }
//    if ($(".checkRecognition").is(':checked')) {
//        $('.childRecognition').show(200);
//    } else {
//        $('.childRecognition').hide(200);
//    }
//    if ($(".checkTrigger").is(':checked')) {
//        $('.childTrigger').show(200);
//    } else {
//        $('.childTrigger').hide(200);
//    }
//    if ($(".checkRooming").is(':checked')) {
//        $('.childRooming').show(200);
//    } else {
//        $('.childRooming').hide(200);
//    }

//});

//$(".checkRecognition").click(function () {
//    if ($(this).is(':checked')) {
//        $('.childRecognition').show(200);
//    }
//    else {
//        if ($("#102").is(':checked') || $("#103").is(':checked') || $("#104").is(':checked') || $("#105").is(':checked') || $("#106").is(':checked') || $("#cas_triage_recognition").val() != "") {
//            $("#messagealert").empty();
//            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
//            setTimeout(function () {
//                $("#messagealert").empty()
//            }, 2000);
//            $('.checkRecognition').prop('checked', true);
//            $('.childRecognition').show(200);
//        } else {
//            $('.childRecognition').hide(200);
//        }
//    }
//});
//$(".checkTrigger").click(function () {
//    if ($(this).is(':checked')) {
//        $('.childTrigger').show(200);
//    }
//    else {
//        if ($("#108").is(':checked') || $("#109").is(':checked') || $("#110").is(':checked') || $("#111").is(':checked') || $("#cas_triage_strokealertrigger").val() != "") {
//            $("#messagealert").empty();
//            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
//            setTimeout(function () {
//                $("#messagealert").empty()
//            }, 2000);
//            $('.checkTrigger').prop('checked', true);
//            $('.childTrigger').show(200);
//        } else {
//            $('.childTrigger').hide(200);
//        }
//    }
//});
//$(".checkRooming").click(function () {
//    if ($(this).is(':checked')) {
//        $('.childRooming').show(200);
//    }
//    else {
//        if ($("#113").is(':checked') || $("#114").is(':checked') || $("#115").is(':checked') || $("#cas_triage_transportandrooming").val() != "") {
//            $("#messagealert").empty();
//            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
//            setTimeout(function () {
//                $("#messagealert").empty()
//            }, 2000);
//            $('.checkRooming').prop('checked', true);
//            $('.childRooming').show(200);
//        } else {
//            $('.childRooming').hide(200);
//        }
//    }
//});
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
//$(".gotVal").click(function () {
//    debugger;
//    var _val = $(this).val();
//    if ($(this).is(':checked')) {
//        //alert('add this to array :' + _val);  // checked
//        arr.push(_val);
//    }
//    else {
//        //alert('remove this to array :' + _val);
//        var index = arr.indexOf(_val);
//        if (index > -1) {
//            arr.splice(index, 1);
//        }
//    }
//    //alert('open arr :' + arr.toString());
//    $("#work-flow-ids").val(arr.toString());
//});

$(".gotVal").click(function () {
    if ($(this).val() == '100') {

        if ($(".checkRecognition").is(':checked') || $(".checkTrigger").is(':checked') || $(".checkRooming").is(':checked') || $("#cas_triage_arivalstarttodelay").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty()
            }, 2000);
            $('#100').prop('checked', true);
            $('.hideShowUl').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.hideShowUl').show(200);
            }
            else {
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
        }

    }

    if ($(this).val() == '201') {
        if ($("#202").is(':checked') || $("#203").is(':checked') || $("#204").is(':checked') || $("#205").is(':checked') || $("#206").is(':checked') || $("#207").is(':checked') || $("#cas_ems_poor_identification").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty()
            }, 2000);
            $(this).prop('checked', true);
            $('.childpoorems').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childpoorems').show(200);
            } else {
                $('.childpoorems').hide(200);
            }

        }
    }

    if ($(this).val() == '208') {
        if ($("#209").is(':checked') || $("#210").is(':checked') || $("#211").is(':checked') || $("#212").is(':checked') || $("#213").is(':checked') || $("#214").is(':checked') || $("#cas_ems_identification_occurred").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox");
            setTimeout(function () {
                $("#messagealert").empty()
            }, 2000);
            $(this).prop('checked', true);
            $('.childidntifcationaccurd').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childidntifcationaccurd').show(200);
            } else {
                $('.childidntifcationaccurd').hide(200);
            }

        }
    }

    if ($(this).val() == '1') {
        if ($("#11").is(':checked') || $("#12").is(':checked') || $("#13").is(':checked') || $("#14").is(':checked') || $("#cas_inpatient_timefirstlogintonhssstartitme").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childtimefirst').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childtimefirst').show(200);
            } else {
                $('.childtimefirst').hide(200);
            }

        }
    }

    if ($(this).val() == '2') {
        if ($("#21").is(':checked') || $("#22").is(':checked') || $("#23").is(':checked') || $("#24").is(':checked') || $("#25").is(':checked') || $("#26").is(':checked') || $("#cas_inpatient_timefirstlogintovideostart").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childVideoStrt').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childVideoStrt').show(200);
            }
            else {
                $('.childVideoStrt').hide(200);
            }
        }
    }

    if ($(this).val() == '3') {
        if ($("#31").is(':checked') || $("#10").is(':checked') || $("#4").is(':checked') || $("#6").is(':checked') || $("#7").is(':checked') || $("#8").is(':checked') || $("#cas_inpatient_arivaltoneedletime").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $('.needleTime').prop('checked', true);
            $('.childarrivalneedle').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childarrivalneedle').show(200);
            }
            else {
                $('.childarrivalneedle').hide(200);
            }

            if ($("#10").is(':checked')) {
                $('.childrelatedlab').show(200);
            } else {
                $('.childrelatedlab').hide(200);
            }
            if ($("#8").is(':checked')) {
                $('.childmedicaldecisionmaking').show(200);
            } else {
                $('.childmedicaldecisionmaking').hide(200);
            }
        }
    }

    if ($(this).val() == '10') {
        if ($("#1000").is(':checked') || $("#1001").is(':checked') || $("#1002").is(':checked') || $("#1003").is(':checked') != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childrelatedlab').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childrelatedlab').show(200);
            }
            else {
                $('.childrelatedlab').hide(200);
            }
            if ($("#1001").is(':checked')) {
                $('.childlabdrawanddelivery').show(200);
            } else {
                $('.childlabdrawanddelivery').hide(200);
            }
            if ($("#1002").is(':checked')) {
                $('.childlabprocessing').show(200);
            } else {
                $('.childlabprocessing').hide(200);
            }
        }
    }

    if ($(this).val() == '4') {
        if ($("#40").is(':checked') || $("#5").is(':checked') || $("#60").is(':checked') || $("#cas_inpatient_delaysrelated_imaging").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childdelaysimaging').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childdelaysimaging').show(200);
            }
            else {
                $('.childdelaysimaging').hide(200);
            }
            if ($("#40").is(':checked')) {
                $('.childwastect').show(200);
            } else {
                $('.childwastect').hide(200);
            }
            if ($("#5").is(':checked')) {
                $('.childadvancedimg').show(200);
            } else {
                $('.childadvancedimg').hide(200);
            }
        }
    }

    if ($(this).val() == '6') {
        if ($("#61").is(':checked') || $("#67").is(':checked') || $("#cas_inpatient_bpmanagemntrelated").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childbpmanamement').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childbpmanamement').show(200);
            }
            else {
                $('.childbpmanamement').hide(200);
            }
            if ($("#61").is(':checked')) {
                $('.childdetection').show(200);
            } else {
                $('.childdetection').hide(200);
            }
            if ($("#67").is(':checked')) {
                $('.childpoormngmnt').show(200);
            } else {
                $('.childpoormngmnt').hide(200);
            }
        }
    }

    if ($(this).val() == '7') {
        if ($("#70").is(':checked') || $("#85").is(':checked') || $("#9").is(':checked') || $("#900").is(':checked') || $("#cas_inpatient_tpaadministration_delays").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childtpaadministration').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childtpaadministration').show(200);
            }
            else {
                $('.childtpaadministration').hide(200);
            }
            if ($("#70").is(':checked')) {
                $('.childworkflowmixing').show(200);
            } else {
                $('.childworkflowmixing').hide(200);
            }
            if ($("#85").is(':checked')) {
                $('.childdelaysinmixing').show(200);
            } else {
                $('.childdelaysinmixing').hide(200);
            }
            if ($("#9").is(':checked')) {
                $('.childaftermixing').show(200);
            } else {
                $('.childaftermixing').hide(200);
            }
            if ($("#900").is(':checked')) {
                $('.childobtainingweight').show(200);
            } else {
                $('.childobtainingweight').hide(200);
            }
        }
    }

    if ($(this).val() == '8') {
        if ($("#800").is(':checked') || $("#801").is(':checked') != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childmedicaldecisionmaking').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childmedicaldecisionmaking').show(200);
            }
            else {
                $('.childmedicaldecisionmaking').hide(200);
            }
        }
    }

    if ($(this).val() == '1001') {
        if ($("#10011").is(':checked') || $("#10012").is(':checked') || $("#10013").is(':checked') || $("#10014").is(':checked') || $("#10015").is(':checked') || $("#10016").is(':checked') != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childlabdrawanddelivery').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childlabdrawanddelivery').show(200);
            }
            else {
                $('.childlabdrawanddelivery').hide(200);
            }
        }
    }

    if ($(this).val() == '1002') {
        if ($("#10020").is(':checked') || $("#10021").is(':checked') || $("#10022").is(':checked') || $("#10023").is(':checked') != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childlabprocessing').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childlabprocessing').show(200);
            }
            else {
                $('.childlabprocessing').hide(200);
            }
        }
    }

    if ($(this).val() == '40') {
        if ($("#41").is(':checked') || $("#42").is(':checked') || $("#43").is(':checked') || $("#44").is(':checked') || $("#45").is(':checked') || $("#46").is(':checked') || $("#47").is(':checked') || $("#cas_inpatient_unenhancedct").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childwastect').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childwastect').show(200);
            }
            else {
                $('.childwastect').hide(200);
            }
        }
    }

    if ($(this).val() == '5') {
        if ($("#50").is(':checked') || $("#51").is(':checked') || $("#52").is(':checked') || $("#57").is(':checked') || $("#cas_inpatient_related_imaging").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childadvancedimg').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childadvancedimg').show(200);
            }
            else {
                $('.childadvancedimg').hide(200);
            }
            if ($("#52").is(':checked')) {
                $('.childassmntinroom').show(200);
            } else {
                $('.childassmntinroom').hide(200);
            }
            if ($("#57").is(':checked')) {
                $('.childassmentinct').show(200);
            } else {
                $('.childassmentinct').hide(200);
            }
        }
    }

    if ($(this).val() == '61') {
        if ($("#62").is(':checked') || $("#63").is(':checked') || $("#65").is(':checked') || $("#66").is(':checked') || $("#cas_inpatient_detection_hypertension").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childdetection').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childdetection').show(200);
            }
            else {
                $('.childdetection').hide(200);
            }
        }
    }

    if ($(this).val() == '67') {
        if ($("#68").is(':checked') || $("#69").is(':checked') || $("#600").is(':checked') || $("#64").is(':checked') || $("#cas_inpatient_poormanagement_hypertension").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childpoormngmnt').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childpoormngmnt').show(200);
            }
            else {
                $('.childpoormngmnt').hide(200);
            }
        }
    }

    if ($(this).val() == '70') {
        if ($("#71").is(':checked') || $("#74").is(':checked') || $("#77").is(':checked') || $("#82").is(':checked') || $("#83").is(':checked') || $("#84").is(':checked') || $("#cas_inpatient_workflowbeforemixing").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childworkflowmixing').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childworkflowmixing').show(200);
            }
            else {
                $('.childworkflowmixing').hide(200);
            }
            if ($("#71").is(':checked')) {
                $('.childsystem').show(200);
            } else {
                $('.childsystem').hide(200);
            }
            if ($("#74").is(':checked')) {
                $('.childphysicianrelated').show(200);
            } else {
                $('.childphysicianrelated').hide(200);
            }
            if ($("#77").is(':checked')) {
                $('.childcentralized').show(200);
            } else {
                $('.childcentralized').hide(200);
            }
        }
    }

    if ($(this).val() == '71') {
        if ($("#72").is(':checked') || $("#73").is(':checked') || $("#cas_inpatient_system").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childsystem').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childsystem').show(200);
            }
            else {
                $('.childsystem').hide(200);
            }
        }
    }

    if ($(this).val() == '900') {

        if ($("#901").is(':checked') != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childobtainingweight').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childobtainingweight').show(200);
            }
            else {
                $('.childobtainingweight').hide(200);
            }
        }
    }

    if ($(this).val() == '74') {
        if ($("#75").is(':checked') || $("#76").is(':checked') || $("#cas_inpatient_physician_related").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childphysicianrelated').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childphysicianrelated').show(200);
            }
            else {
                $('.childphysicianrelated').hide(200);
            }
        }
    }

    if ($(this).val() == '77') {
        if ($("#78").is(':checked') || $("#79").is(':checked') || $("#80").is(':checked') || $("#81").is(':checked') || $("#cas_inpatient_centralizedpharmacy_delivery").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childcentralized').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childcentralized').show(200);
            }
            else {
                $('.childcentralized').hide(200);
            }
        }
    }

    if ($(this).val() == '85') {
        if ($("#86").is(':checked') || $("#87").is(':checked') || $("#88").is(':checked') || $("#89").is(':checked') || $("#cas_inpatientdelays_mixing").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childdelaysinmixing').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childdelaysinmixing').show(200);
            }
            else {
                $('.childdelaysinmixing').hide(200);
            }
        }
    }

    if ($(this).val() == '9') {
        if ($("#91").is(':checked') || $("#92").is(':checked') || $("#93").is(':checked') || $("#94").is(':checked') || $("#95").is(':checked') || $("#96").is(':checked') || $("#cas_inpatient_workflowaftermixing").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childaftermixing').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childaftermixing').show(200);
            }
            else {
                $('.childaftermixing').hide(200);
            }
        }
    }

    if ($(this).val() == '101') {
        if ($("#102").is(':checked') || $("#103").is(':checked') || $("#104").is(':checked') || $("#105").is(':checked') || $("#106").is(':checked') || $("#cas_triage_recognition").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty()
            }, 2000);
            $('.checkRecognition').prop('checked', true);
            $('.childRecognition').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childRecognition').show(200);
            }
            else {
                $('.childRecognition').hide(200);
            }
        }
    }

    if ($(this).val() == '107') {

        if ($("#108").is(':checked') || $("#109").is(':checked') || $("#110").is(':checked') || $("#111").is(':checked') || $("#cas_triage_strokealertrigger").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty()
            }, 2000);
            $('.checkTrigger').prop('checked', true);
            $('.childTrigger').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childTrigger').show(200);
            }
            else {
                $('.childTrigger').hide(200);
            }
        }
    }

    if ($(this).val() == '112') {

        if ($("#113").is(':checked') || $("#114").is(':checked') || $("#115").is(':checked') || $("#cas_triage_transportandrooming").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty()
            }, 2000);
            $('.checkRooming').prop('checked', true);
            $('.childRooming').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childRooming').show(200);
            }
            else {
                $('.childRooming').hide(200);
            }
        }
    }

    if ($(this).val() == '52') {
        if ($("#53").is(':checked') || $("#54").is(':checked') || $("#55").is(':checked') || $("#56").is(':checked') || $("#cas_inpatient_telemedicineassessmentroom").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childassmntinroom').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childassmntinroom').show(200);
            }
            else {
                $('.childassmntinroom').hide(200);
            }
        }
    }

    if ($(this).val() == '57') {
        if ($("#58").is(':checked') || $("#59").is(':checked') || $("#500").is(':checked') || $("#501").is(':checked') || $("#502").is(':checked') || $("#503").is(':checked') || $("#504").is(':checked') || $("#cas_inpatient_telemedicineasesmentinct").val() != "") {
            $("#messagealert").empty();
            $("#messagealert").append("Uncheck the secondary checkbox, or clear textbox");
            setTimeout(function () {
                $("#messagealert").empty();
            }, 2000);
            $(this).prop('checked', true);
            $('.childassmentinct').show(200);
            return false;
        } else {
            if ($(this).is(':checked')) {
                $('.childassmentinct').show(200);
            }
            else {
                $('.childassmentinct').hide(200);
            }
        }
    }

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