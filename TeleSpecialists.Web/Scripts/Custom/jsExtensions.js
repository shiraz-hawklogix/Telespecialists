
(function ($) {

    $.fn.readonly = function(){
        this.attr("readonly", true);
        this.attr("disabled", false); 
    }

    $.fn.getAllDropDownAllOptions = function () {
        var optVals = [];
        $(this).find('option').each(function (index, item) {
            if ($.trim(item.value) != "" && $.trim(item.value) != "00000000-0000-0000-0000-000000000000")
                optVals.push(item.value);
        });
        return optVals;
    }

    $.fn.fillKendoDropDown = function (url, textField, valueField, label, callBack) {

        try {
            var ele = $(this);
            if (ele != undefined) {
                url = url.addRandomParamToUrl();
                var drpJson = {
                    dataTextField: textField,
                    dataValueField: valueField,
                    dataSource: new kendo.data.DataSource({
                        transport: {
                            read: {
                                type: "GET",
                                url: url,
                                contentType: "application/json; charset=utf-8",
                                dataType: 'json'
                            }
                        }
                    }),

                    // filter: "contains" ,
                    dataBound: function (e) {
                        try {

                            if (e.sender != undefined)
                                if (e.sender.filterInput != undefined) {
                                    if (e.sender.filterInput.val() != "")
                                        return;
                                }

                            if (ele != undefined) {
                                if (ele.attr("data-selectedValue") != "" && ele.attr("data-selectedValue") != 0 && ele.attr("data-selectedValue") != "00000000-0000-0000-0000-000000000000")
                                    ele.data('kendoDropDownList').value(ele.attr("data-selectedValue"));

                            }
                            if (callBack != undefined) {
                                callBack();
                            }
                        }
                        catch (e) {
                            console.log(e);
                        }
                    },
                    filtering: function (e) {
                        onDropdDownOpen(e);
                    },
                    open: function (e) {
                        onDropdDownOpen(e);
                    },
                    optionLabel: label
                };

                if (ele.attr("data-searchfilter") == "true") {
                    drpJson["filter"] = "contains";
                }

                return ele.kendoDropDownList(drpJson);


            }
        } catch (error) {

        }

    };


    $.fn.fillStaticKendoDropDown = function (jsonArray, textField, valueField, label, callBack,ondropdownchanged) {

        try {
            var ele = $(this);
            if (ele != undefined) {
                return ele.kendoDropDownList({
                    dataTextField: textField,
                    dataValueField: valueField,
                    dataSource: jsonArray,
                    filter: "contains",


                    dataBound: function (e) {
                        try {
                            if (e.sender.filterInput != 'undefined') {
                                if (e.sender.filterInput.val() != "")
                                    return;
                            }

                            if (ele != undefined) {
                                if (ele.attr("data-selectedValue") != "" && ele.attr("data-selectedValue") != 0 && ele.attr("data-selectedValue") != "00000000-0000-0000-0000-000000000000")
                                    ele.data('kendoDropDownList').value(ele.attr("data-selectedValue"));

                            }
                            if (callBack != undefined) {
                                callBack(); 
                            }
                        }
                        catch (e) {
                            console.log(e);
                        }
                    },
                    open: function (e) {
                        onDropdDownOpen(e); 
                    },
                    close: function (e) {
                        if (ondropdownchanged != undefined) {
                            ondropdownchanged();
                        }
                    },
                    optionLabel: label
                });
            }
        } catch (error) {

        }

    };

    $.fn.showConfirmPopUpWithYesNo = function (title, message, callback,  cancelCallback) {
        
        return $(this).addModelTemplate(title)
            .find(".modal-body").html(message).end()
            .find(".save").val("Yes").off("click").click(function () {
                callback();
            }).end()
            .find(".cancel").off("click").click(function () {
                if (typeof cancelCallback === 'function') {
                    cancelCallback();
                }
            })
            .end().modal("show")
            .find(".modal-footer").find(".cancel").removeClass("btn-link").addClass("btn-dark").end()            
            //.find(".modal-footer").find(".cancel").html("No").end()
            .find(".lnkCancel").html("No").end()
            .find(".cancel").show().end()
           
    };

    $.fn.showCancelConfirmPopUp = function (title, message, callback, cancelCallback) {

        return $(this).addModelTemplate(title)
            .find(".modal-body").html(message).end()
            .find(".save").val("Yes").off("click").click(function () {
                callback();
            }).end()
            .find(".cancel").off("click").click(function () {
                if (typeof cancelCallback === 'function') {
                    cancelCallback();
                }
            })
            .end().modal("show"); 
    };

    $.fn.showConfirmPopUp = function (title, message, callback, btnText, cancelCallback) {
        if (btnText == null || btnText == undefined || btnText == "")
            btnText = "OK";
        return $(this).addModelTemplate(title)
            .find(".modal-body").html(message).end()
            .find(".save").val(btnText).off("click").click(function () {
                callback();
            }).end().modal("show")
            .find(".cancel").show().end()
            .find(".cancel").off("click").click(function () {
                if (typeof cancelCallback === 'function') {
                    cancelCallback();
                }
            });
    };

    $.fn.showAlertPopUp = function (title, message, callback) {

        return $(this).addModelTemplate(title)
            .find(".modal-body").html(message).end()
            .find(".save").val("OK").end()
            .find(".save").val("OK").off("click").click(function () {
                if (typeof (callback) != undefined) {
                    callback();
                }
            }).end()
            .find(".save").attr("data-dismiss", "modal").end()
            .find(".cancel").hide().end()
            .modal("show");
    };

    $.fn.addModelTemplate = function (title) {
        var html = '';
        html += "  <div class='modal-dialog' >";
        html += "    <div class='modal-content'>";
        html += "      <div class='modal-header'>";

        html += "        <h4 class='modal-title'>" + title + "</h4>";
        html += "        <button type='button' class='close cancel' data-dismiss='modal' aria-label='Close'><span aria-hidden='true'>&times;</span></button>";

        html += "      </div>";
        html += "      <div class='modal-body'>";
        //html += "        
        html += "      </div>";
        html += "      <div class='modal-footer'>";
        html += "        <a href='javascript:void()' class='btn btn-link cancel lnkCancel' data-dismiss='modal'>Cancel</a>";
        html += "       <input type='button' class='btn btn-dark save' value='Save' />";
        html += "      </div>";
        html += "    </div>";
        html += "  </div>";

        return $(this).append(html);

    };

    $.fn.bindModelErrors = function (modelErrors) {
        var err = {};
        for (var i = 0; i < modelErrors.length; i++) {
            err[modelErrors[i].Key] = modelErrors[i].Value;
        }
        return $(this).validate().showErrors(err);
    }

    $.fn.serializeToJson = function () {
        var data = $(this).serializeArray();
        var jsonObj = {};
        $.map(data, function (n, i) {
            jsonObj[n.name] = n.value;
        });
        return jsonObj;
    };

    $.fn.showBSDangerAlert = function (title, message) {
        return $(this).append(getAlert(title, message, "danger"));
    };

    $.fn.showBSSuccessAlert = function (title, message) {
        return $(this).append(getAlert(title, message, "success"));
    };

    $.fn.showBSWarningAlert = function (title, message) {
        return $(this).append(getAlert(title, message, "warning"));
    };

    $.fn.showBSInfoAlert = function (title, message) {
        return $(this).append(getAlert(title, message, "info"));
    };

    // adding a helper function to disable html control
    $.fn.disable = function () {
        return $(this).each(function () {
            $(this).attr('disabled', 'disabled');
        });
    };

    // adding a helper function to enable html control
    $.fn.enable = function () {
        return $(this).each(function () {
            $(this).removeAttr('disabled');
        });
    };

    $.fn.KendoDropDownReadOnly = function () {
        var dropDown = $(this).data("kendoDropDownList");
        if (dropDown != undefined)
            dropDown.readonly();
    };
    $.fn.KendoDropDownDisable = function () {
        var dropDown = $(this).data("kendoDropDownList");
        if (dropDown != undefined)
            dropDown.enable(false);
    };

    function getAlert(title, message, type) {

        var cssclass = "";
        var autoCloseId = "";
        if (type == "danger") {
            cssclass = "alert-danger error-message";
        }
        else if (type == "success") {
            cssclass = "alert-success";
            autoCloseId = "autoclose";
            autoCloseAlert();
        }
        else if (type == "warning") {
            cssclass = "alert-warning";
        }
        else {
            cssclass = "alert-info";
        }
        var html = "<div class=\"alert " + cssclass + "\"  id='" + autoCloseId + "'>";
        html += "<button class=\"close\" data-dismiss=\"alert\">×</button>";
        if (title != '') {
            html += " <strong style='margin-right:5px'>" + title + "</strong>";
        }

        html += message;
        html + "</div>";
        return html;
    }

    $.fn.printElement = function () {
        var html = $(this).html();
        var title = "<br/><br/><h3>" + $(document).find("head").find("title").html() + "</h3><br/><br/>";
        var headHtml = $(document).find("head").html();
        var newWin = window.open("");
        newWin.document.head.innerHTML = headHtml;
        newWin.document.write(headHtml);
        window.setTimeout(function () {
            newWin.document.write(title + html + headHtml);
            newWin.print();
            newWin.close();
        }, 100);

    }

    $.fn.showPopover = function (container) {
        $(this).popover({
            trigger: 'manual',
            content: $("#tooltip").html(),
            html: true,
            container: container
        }).on("mouseenter", function () {
            var _this = this;
            $(this).popover("show");
            $(container).children(".popover").on("mouseleave", function () {
                $(_this).popover('hide');
            });
        }).on("mouseleave", function () {
            var _this = this;
            setTimeout(function () {
                if (!$(".popover:hover").length) {
                    $(_this).popover("hide");
                }
            }, 0);
        });
    };

    $.fn.watch = function (ignoreList) {
        if (ignoreList != undefined)
            ignoreList = ignoreList.toLocaleLowerCase();


        return $(this).each(function () {
            $(this).find('input, select, textarea').each(function (index, item) {
                var name = $.trim($(this).attr("name")).toLocaleLowerCase();
                var ignoreAttribute = $.trim($(this).attr("data-IgnoreAutoSave")).toLocaleLowerCase();
                var readOnly = $(this).attr("readonly") != undefined ? true : false;
                if (ignoreList.indexOf(name) == -1 && ignoreAttribute == "" && !readOnly) {

                    if (!$(this).is("[data-original-value]")) {

                        if ($(this).is("input:checkbox")) {
                            var checked = $(this).prop("checked");
                            $(this).attr('data-original-value', checked);
                        }
                        else {
                            var selector = $(this);
                            if ($(this).is('input:radio')) {
                                selector = $("input:radio[name='" + name + "']:checked");
                                $(this).attr('data-original-value', $.trim(selector.val()));
                            }
                            else {
                                $(this).attr('data-original-value', $.trim(selector.val()));
                            }

                        }
                    }
                }

            });
        });
    }

    $.fn.getWatchChanges = function (selectIdsToAdd) {
        var obj;
        if (typeof (selectIdsToAdd) != undefined)
            selectIdsToAdd = "," + selectIdsToAdd;
        var list = new Array();
        $(this).each(function () {
            $(this).find(':input:not([type=hidden]), select, textarea' + $.trim(selectIdsToAdd)).each(function (index, item) {
                $this = $(this);
                if ($this.is("[data-original-value]")) {
                    obj = new Object();                 
                    if ($.trim($this.attr("name")) != "") {
                        obj.key = $.trim($this.attr("name"));
                    }
                    var selector = $this;
                    if ($this.is('input:radio')) {
                        selector = $("input:radio[name='" + obj.key + "']:checked");
                        if (selector.attr('data-original-value') != selector.val()) {
                            obj.value =Base64.encode(selector.val());
                            obj.PreviousValue = Base64.encode(selector.attr('data-original-value'));
                            // adding condtion for masked data
                            if (obj.value.indexOf("_") == -1 && list.findObjectIndex(obj.key, "key") == undefined) {
                                list.push(obj);
                            }

                        }
                    }

                    else if ($this.is('input:checkbox')) {

                        if (selector.attr('data-original-value') != $this.prop("checked").toString()) {
                            obj.value = Base64.encode($this.prop("checked").toString());
                            obj.PreviousValue = Base64.encode(selector.attr('data-original-value'));
                            // adding condtion for masked data
                            if (list.findObjectIndex(obj.key, "key") == undefined) {
                                list.push(obj);
                            }

                        }
                    }

                    else if ($this.attr('data-original-value') != $this.val()) {
                        obj.text = $this.closest('.form-group').children('label.control-label').text().replace('*', '').trim();
                     
                        if ($this.is("[ChangeType]")) {
                            obj.ChangeType = $this.attr("ChangeType");
                        }
                        else {
                            obj.ChangeType = "Change";
                        }

                        if ($.trim($this.attr('data-SaveAsUTC')) != '') {
                            obj.SaveAsUTC = true;
                        }
                         
                        var status = false;
                        //adding condition for date only
                        if ($this.hasClass('datepicker') && list.findObjectIndex(obj.key, "key") == undefined) {
                            if (!isValidDate($this.val())) {
                                status = true;
                            }                           
                        }
                         if ($this.hasClass('timepicker') &&  list.findObjectIndex(obj.key, "key") == undefined) {
                             if (!isValidDate($this.val().split(" ")[0]) || !isValidTime($this.val().split(" ")[1])) {
                                 status = true;
                             }
                        }
                        if ($this.val().indexOf("_") == -1 && status==false && list.findObjectIndex(obj.key, "key") == undefined) {
                            obj.value = Base64.encode($this.val());
                            obj.PreviousValue = Base64.encode(selector.attr('data-original-value'));
                            list.push(obj);
                        }
                    }
                }
            });
        });
        return list;
    }

})(jQuery);

Number.prototype.formatNumber = function (totalDigits) {
    try {

        var formattedNumber = ("0" + this).slice(-totalDigits);
        return formattedNumber;
    }
    catch (e) {
        return this;
    }
}

String.prototype.toFloat = function () {
    try {
        var n = parseFloat(this.toString());
        return isNaN(n) ? 0 : n;
    }
    catch (e) {
        return 0;
    }
}

String.prototype.replaceAll = function (textToReplace, replacer) {
    try {
        if ($.trim(this.toString()) != "") {
            return this.toString().replace(new RegExp(textToReplace, 'g'), replacer);
        }
        else return "";

    }
    catch (e) {
        return this.toString();
    }
}

String.prototype.toInt = function () {
    try {
        var n = parseInt(this.toString());
        return isNaN(n) ? 0 : n;
    }
    catch (e) {
        return 0;
    }
}

String.prototype.toFacilityTimeZone = function (currentOffset, targetOffset) {
    try {
        var value = this.toString() + " ";        
        if (currentOffset >= 0)
            value = value + "+";
        value = value + currentOffset.toString();        
        var dateTime = new Date(value).toUTCString();
        return moment.utc(dateTime.replace("GMT", "")).utcOffset(targetOffset).format("MM/DD/YYYY HH:mm:ss")        
    }
    catch (e) {
        console.log(e);
        return this;
    }
}

String.prototype.addParamToUrl = function (param, value) {
    try {
        var url = this.toString();
        if (url.indexOf((param + "=")) == -1) {
            if (url.indexOf("?") == -1) {
                url = url + "?" + param + "=" + value;
            }
            else {
                url = url + "&" + param + "=" + value;
            }
        }
        return url;
    }
    catch (e) {
        console.log(e);
        return this;
    }
}

String.prototype.replaceEncodingChars = function () {
    if (this == undefined)
        return "";

    return this.replaceAll('—', '-').replaceAll('–', '-').replaceAll("˜","~");
}

String.prototype.addRandomParamToUrl = function () {   
   return this.toString().addParamToUrl("rnd", getRandomInt(100000000000).toString());          
}



 

Array.prototype.findObjectIndex = function (val, prop) {
    var i;
    var list = this;
    for (var i = 0; i < list.length; i++) {
        if (list[i][prop] != null) {
            if (list[i][prop].toString() == val) {
                return i;
            }
        }
    }
    return undefined;
}