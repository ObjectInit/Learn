/**
 * 说明：修改密码
 * 依赖：jquery3.3.1、sj.dialog.js
 * 作者: 霍秀梅
 * 日期：2019-06-03
 */


$(function () {
    "use strict";

    var oldPassword = function () { return $.trim($('[name="OldPassword"]').val()) };
    var newPassword = function () { return $.trim($('[name="NewPassword"]').val()) };
    var confirmPassword = function () { return $.trim($('[name="ConfirmPassword"]').val()); };

    // 校验旧密码
    function checkOldPassword() {
        if (oldPassword().length == 0 || oldPassword() == '') {
            return false;
        } else {

            $('[name="OldPassword"]').removeClass("input-error");
            return true;

        }
    }

    // 校验新密码
    function checkNewPassword() {
        if (newPassword().length == 0 || newPassword() == '') {
            return false;
        } else {

            $('[name="NewPassword"]').removeClass("input-error");
            return true;

        }
    }


    // 校验确认密码
    function checkConfirmPassword() {
        if (confirmPassword().length == 0 || confirmPassword() == '') {
            return false;
        } else {

            $('[name="ConfirmPassword"]').removeClass("input-error");
            return true;

        }
    }
    // 发送请求
    function sendRequest(userData) {
        $.post(sj.config.siteHost + '/User/UpdatePassword', userData, function (data) {
            console.log(data);
            if (data.Code == 0) {
                jQuery.dialog.close();
                return;
            } else {
                jQuery.dialog.alert('error', data.Message, '提示');
            }
        });

    }

    // 保存并关闭
    $('.save-close').click(function () {

        // 字段校验
        if (!checkOldPassword() && !checkNewPassword() && !checkConfirmPassword()) {

            $('tbody input').addClass("input-error");
        } else if (!checkOldPassword() && !checkNewPassword() ) {

            $('input:not([name="ConfirmPassword"])').addClass("input-error");
        } else if (!checkOldPassword() && !checkConfirmPassword()) {

            $('input:not([name="NewPassword"])').addClass("input-error");

        } else if (!checkNewPassword() && !checkConfirmPassword()) {

            $('input:not([name="OldPassword"])').addClass("input-error");
        }  else if (!checkNewPassword() ) {
            $('[name="NewPassword"]').addClass("input-error");

        } else if (!checkOldPassword()) {
            $('[name="OldPassword"]').addClass("input-error");

        } else if (!checkConfirmPassword()) {
            $('[name="ConfirmPassword"]').addClass("input-error");
        } else if (newPassword() != confirmPassword()) {
            jQuery.dialog.alert('error','两次输入的密码不一致','提示');
        }else {
            var userData = {               
                OldPassword: oldPassword(),
                NewPassword: newPassword(),
                ConfirmPassword: confirmPassword()
            }
            sendRequest(userData);
        }

    })




})