/*
 * 说明：全局js配置
 * 作者：zcc
 * 2019-03-20
 * */
;sj.config = (function () {
    'use strict';
    var SITEHOST = "http://localhost:63393",
        SUCCESS = "0";
    var t = {
        // 站点地址
        siteHost: SITEHOST,
        // 登录页
        loginUrl: '/Authentication/Login',
    };
    return t;
}());
