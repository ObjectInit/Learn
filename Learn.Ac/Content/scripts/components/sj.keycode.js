/*
 * 基于jquery的keycode插件
 * 依赖：jquery3.3.1
 * keycode对照表：65 新增、68 删除、83 保存、88 打印、71 刷新 翻页keycode在翻页组件中处理
 * 作者：zcc
 * 日期：2019-03-28
 */
;(function ($, document) {
    defaults = {
        onkeydown : function (keycode) { },
    };
    // 构造函数
    function Plugin(element,options) {
        this.element = element;
        this.settings = $.extend({}, defaults, options);
        this.init();
    };
    //添加属性方法
    Plugin.prototype = {
        //初始化
        init: function () {
            var e = this;
            document.onkeydown = function (event) {
                e.settings.onkeydown(event.keyCode);
                if (event.altKey) {
                    event.preventDefault();
                }
            }
        }
    };
    $.fn.keycode = function (options) {
        var e = this;
        $.data(e, new plugin(this, options));
        return e;
    }
})(jQuery, document)