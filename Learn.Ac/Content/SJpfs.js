var postIng = false;
var dlgrettype = "";
var dlgform = "";
var dlgparm = "";
var dlgparm1 = "";

document.onkeydown = function (event) {
    var eve = event ? event : (window.event ? window.event : null);
    if (eve.keyCode == 13) { return false; }
}

$(document).on("click", "a.btevent", function () { FormPostEle(this) });
$(document).on("click", "div.btevent", function () { FormPostEle(this) });
$(document).on("click", "a.btdlg", function () { opendlgele(this) });
$(document).on("click", "div.input-icon", function () { opendlgele(this) });
$(document).on("click", "input.cthree", function () { CheckBoxthree(this) });
$(document).on("click", "input.idate", function () { dateinit(this,0) });
$(document).on("click", "input.itime", function () { dateinit(this,1) });


function getQueryParm() { var p = window.location.toString().split('?')[1]; if (p) return '?' + p; else return '?'; }
function getFormID(ele) { return $(ele).parents("form")[0].id.substr(6); }
function FormPostEle(ele) { FormPost(getFormID(ele),$(ele).attr("postevent"),(!$(ele).attr("postparm")?"":$(ele).attr("postparm")),(!$(ele).attr("postconfirm")?"":$(ele).attr("postconfirm")),$('#form__' + getFormID(ele)).formSerialize());}
function FormPost(form, ev, parm,cfm, data) { if (postIng) { window.alert("Allready posted ."); return; }; postIng = true; if(cfm!=""){if(confirm(cfm)!=true) {postIng = false; return;}} ; $.post("/" + form + getQueryParm() + '&_ekey_=' + ev + '&_eparm_=' + parm, data, function (r) { if (!r) { postIng = false; return; }; if (r.toString().substring(0, 6) == '_eval_') { eval(r.toString().substring(6)); } else { $('#form__' + form).html(r); } postIng = false; }); }

function changeselect(ele) { var fn = getFormID(ele); $("#form__" + fn + " input:checkbox").each(function (c) { var cid = $(this)[0].id; if (cid.substr(0, 12) == '_row_select_') { if ($(this)[0].checked) { $(this)[0].checked = '' } else { $(this)[0].checked = 'checked' } } }); }

function opendlgele(ele) { opendlg(getFormID(ele),$(ele).attr("tit"),$(ele).attr("url"),$(ele).attr("w"),$(ele).attr("h"),(!$(ele).attr("cfm")?"":$(ele).attr("cfm")),(!$(ele).attr("ele")?"":$(ele).attr("ele")),(!$(ele).attr("postevent")?"":$(ele).attr("postevent")),(!$(ele).attr("postparm")?"":$(ele).attr("postparm"))) }
function opendlg(formid,tit,url, ww, hh,cfm,ele,pevent,pparm) {
    if(cfm!=""){if(confirm(cfm)!=true) { return;}} ;
    dlgrettype = "";
    dlgform = formid;
    if(ele!=""){dlgrettype="ele"; dlgparm=ele;url = url + (url.indexOf("?")==-1?"?":"&") + "inputele=" + ele + "&searchstring=" + $(" input[ name='" + ele + "' ]").val() ;}
    else { if(pevent!="") {dlgrettype="event";dlgparm=pevent;dlgparm1=pparm} }
    if(pevent!="") {
        layer.open({
            type: 2,
            title: tit,
            shift:'',
            area: [ww + "px" , hh + "px"],
            content: url,
            cancel: function(index, layero){ 
                layer.close(index);
                FormPost(formid,pevent,"","",$('#form__' + formid).formSerialize());
              } 
        });
    }else{
        layer.open({
            type: 2,
            title: tit,
            shift:'',
            area: [ww + "px" , hh + "px"],
            content: url
        });
    }
}
function closedlg(retValue) { if (self != top) { parent.setRet(retValue); parent.layer.close(parent.layer.getFrameIndex(window.name));} else {window.opener=null;window.open(' ','_self');window.close();} }

function setRet(retValue) {
    switch (dlgrettype)
    {
        case "ele":
            $(" input[ name='" + dlgparm + "' ]").val(retValue);
            break;
        case "event":
            FormPost(dlgform,dlgparm,dlgparm1+"|"+retValue,"",$('#form__' + dlgform).formSerialize());
            break;
        default:
            break;
    }
}

function EleFold(eleid) {
    var oAObj = !document.getElementById(eleid) ? null : document.getElementById(eleid);
    if (!oAObj) return;
    var ipt = document.all(eleid + "hide") ;
    if (oAObj.style.display == "none") {
        oAObj.style.display = "";
        ipt.value = "False";
    }
    else {
        oAObj.style.display = "none";
        ipt.value = "True";
    }
}

function CheckBoxthree(ele)
{
    if (ele.value == "") { ele.value = "✓"; }
    else if (ele.value == "✓") { ele.value = "✗"; }
    else if (ele.value == "✗") { ele.value = ""; }
}

function dateinit(ele,t){
    if(!$(ele).attr("ii")) {
        $(ele).attr("ii","1");
        if (t==1) {
            laydate.render({elem: '#' + $(ele).attr("id") ,type: 'datetime',theme: 'molv',show: true});
        }
        else {
            laydate.render({elem: '#' + $(ele).attr("id") ,showBottom: false,theme: 'molv',show: true});
        }
    }
}

//Jquery Form
(function ($) {
    $.fn.formToArray = function (semantic) {
        var a = [];
        if (this.length == 0) return a;

        var form = this[0];
        var els = semantic ? form.getElementsByTagName('*') : form.elements;
        if (!els) return a;
        for (var i = 0, max = els.length; i < max; i++) {
            var el = els[i];
            var n = el.name;
            if (!n) continue;

            if (semantic && form.clk && el.type == "image") {
                if (!el.disabled && form.clk == el)
                    a.push({ name: n + '.x', value: form.clk_x }, { name: n + '.y', value: form.clk_y });
                continue;
            }

            var v = $.fieldValue(el, true);
            if (v && v.constructor == Array) {
                for (var j = 0, jmax = v.length; j < jmax; j++)
                    a.push({ name: n, value: v[j] });
            }
            else if (v !== null && typeof v != 'undefined')
                a.push({ name: n, value: v });
        }

        if (!semantic && form.clk) {
            var inputs = form.getElementsByTagName("input");
            for (var i = 0, max = inputs.length; i < max; i++) {
                var input = inputs[i];
                var n = input.name;
                if (n && !input.disabled && input.type == "image" && form.clk == input)
                    a.push({ name: n + '.x', value: form.clk_x }, { name: n + '.y', value: form.clk_y });
            }
        }
        return a;
    };

    $.fn.formSerialize = function (semantic) {
        try { editor.sync(); }
        catch (err) { }
        return $.param(this.formToArray(semantic));
    };

    $.fn.fieldValue = function (successful) {
        for (var val = [], i = 0, max = this.length; i < max; i++) {
            var el = this[i];
            var v = $.fieldValue(el, successful);
            if (v === null || typeof v == 'undefined' || (v.constructor == Array && !v.length))
                continue;
            v.constructor == Array ? $.merge(val, v) : val.push(v);
        }
        return val;
    };

    $.fieldValue = function (el, successful) {
        var n = el.name, t = el.type, tag = el.tagName.toLowerCase();
        if (typeof successful == 'undefined') successful = true;

        if (successful && (!n || el.disabled || t == 'reset' || t == 'button' ||
            (t == 'checkbox' || t == 'radio') && !el.checked ||
            (t == 'submit' || t == 'image') && el.form && el.form.clk != el ||
            tag == 'select' && el.selectedIndex == -1))
            return null;

        if (tag == 'select') {
            var index = el.selectedIndex;
            if (index < 0) return null;
            var a = [], ops = el.options;
            var one = (t == 'select-one');
            var max = (one ? index + 1 : ops.length);
            for (var i = (one ? index : 0); i < max; i++) {
                var op = ops[i];
                if (op.selected) {
                    var v = /msie/.test(navigator.userAgent.toLowerCase()) && !(op.attributes['value'].specified) ? op.text : op.value;
                    if (one) return v;
                    a.push(v);
                }
            }
            return a;
        }
        return el.value;
    };
})(jQuery);

