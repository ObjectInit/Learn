/**
 * shiji 登录模块
 * 依赖 jquery3.3.1、sj.core 
 */
;sj.login = (function () {
    'use strict';
    var t = {};
    var userName = function () { return $.trim($('.user-name').val()); }
    var userPassword = function () { return $.trim($('.user-password').val()); }
    var verificationCode = function () { $.trim($('.verification-code').val()); }

    /* 检测是否在框架页内，是则跳转*/
    t.iframeRedirectUrl=function iframeRedirectUrl() {
        var isContains = $('iframe', parent.document).length > 0;
        if (isContains) {
            //window.parent.location.href = sj.config.loginUrl;
        }
    }

    //登录初始化
    t.init = function () {
        //监控回车
        $(document).keydown(function (event) {           
            if (event.keyCode === 13) {
                t.loginSubmit();
            }
        });
        //挂在click事件
        $('.login-submit').click(function () {
            t.loginSubmit();
        })
    };

    //检查用户名
    t.checkUserName = function () {
       
        if (userName().length === 0 || userName() === "") {
            return false;
        } else {
            t.errorMessage('');
            return true;
        }
    };

    //检查密码
    t.checkUserPassword = function () {      
        if (userPassword().length === 0 || userPassword() === "") {
            return false;
        } else {
            t.errorMessage('');
            return true;
        }

    };

    //检查验证码
    t.checkVerificationCode = function () {

    };

    //登录提交
    t.loginSubmit = function () {
        var userNameFlag = t.checkUserName();
        var userPasswordFlag = t.checkUserPassword();
        // 前端验证码先去掉，以后再加 edit by 鲁明 20190410
        if (userNameFlag && userPasswordFlag) {
            t.errorMessage('');
            t.submit();
        } else if (!userNameFlag && !userPasswordFlag) {
            t.errorMessage('请输入用户名和密码');
        } else if (!userNameFlag) {
            t.errorMessage('请输入用户名');
        } else if (!userPasswordFlag) {
            t.errorMessage('请输入密码')
        }
    };

    //提交请求
    t.submit = function () {
        var userData = {
            loginName: userName,
            password: userPassword,
            //verificationCode: verificationCode
        };
        $.post(sj.config.siteHost + '/Authentication/LoginIn', userData, function (data) {
            if (data.Code == 0) {
                // 登录成功跳转到首页
                window.location = sj.config.siteHost + '/Home/Index';
                return;
            } else {
                t.errorMessage('登录失败');
            }
        });
    };

    //错误信息
    t.errorMessage = function (msg) {
        //$('.err-text').text(msg);
        if (msg != '') {
            alert(msg);
        }
    };  
    return t;
}());