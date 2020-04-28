/*
 * 说明：shiji 常用工具 辅助方法
 * 依赖： jquery3.3.1  layer3.1.1  
 * 作者：张耀
 * 日期：2019-05-15
 * */

;sj.util = (function () {
    'use strict';
    var t = {};

    /**
        * 创建iframe字符串
        * @param {any} url
        */
    t.createFrame = function (url) {
        var s = '<iframe name="contentFrame" src="' + url + '" width=100%  height=100% scrolling="no" frameborder="0" style="display:block;" ></iframe>';
        return s;
    };

    /**
    * 获取当前url中的值
    * @param {any} parms 参数名
    */
    t.getQueryString = function (parms) {
        var reg = new RegExp("(^|&)" + parms + "=([^&]*)(&|$)");
        var r = window.location.search.substr(1).match(reg);//search,查询？后面的参数，并匹配正则
        if (r != null) return decodeURI(r[2]); return null;
    }

    return t;
}());

/**
 * stirng 原型扩展
 * 利用jquery实现两头去空格
 */
Object.defineProperty(String.prototype, 'trim', {
    value: function (title) {
        return $.trim(this);
    },
    writable: true,
    enumerable: false,
    changeurable: true
});