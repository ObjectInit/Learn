/*
 * 基于jquery的loading插件
 * 用法：$("...").sjLoading();
 * 依赖：jquery3.3.1
 * 作者：lwh
 * 日期：2019-04-18
 */
; (function (window, $) {

    //构造函数
    $.sjLoading = function (element, options) {
        //判断是否构造实例 
        if (!(this instanceof $.sjLoading)) {
            return new $.sjLoading(element, options);
        }

        var self = this;

        self.$container = element;

        //绑定实例和jquery实例
        self.$container.data("sjLoading", self);


        //实例参数
        self.options = $.extend({}, $.sjLoading.defaultOptions, options);

        //load jquery 实例
        self.$load = $(self.options.dom);

        //是否改变了元素定位
        self.isChangePosition = false;

        self.render();
    };

    //sjLoading全局默认参数
    $.sjLoading.defaultOptions = {
        dom: '<div id="sj-loading-bg"><div class="sj-loading"></div>'
    }


    //实例方法
    $.sjLoading.prototype = {
        //组件渲染  事件 结构
        render: function () {
            var self = this;
            self.renderHtml();
        },
        //渲染html
        renderHtml: function () {
            var self = this;
            //判断节点是否有定位
            var p = self.$container.css("position");
            if (p === "static") {
                //没有定位则加上相对定位
                self.$container.css("position", "relative");
                self.isChangePosition = true;
            }
            self.$container.prepend(self.$load);
        }, 
        destroy: function () {
            var self = this;
            self.$load.remove();
            if (self.isChangePosition) {
                self.$container.css("position","static");
            }
            self.$container.removeData("sjLoading");
        },
        //用于扩展调用方法
        callMethod: function (method, options) {
            var self = this;
            switch (method) {
                case 'option':
                    self.options = $.extend({}, self.options, options);
                    self.render();
                    break;
                case 'destroy':
                    self.destroy();
                    break;
                default:
                    throw new Error('[sjLoading] method "' + method + '" does not exist');
            }
        }
    }


    // Plugin methods
    $.fn.sjLoading = function (options) {

        var self = this;

        var args = Array.prototype.slice.call(arguments);
        //判断实例是否初始化过组件
        var $instance = $(self).data('sjLoading');

        if (typeof args[0] === "string") {  
            $instance&&$instance.callMethod(args[0], args[1]);
        } else {
            //组件只需实例化一次
            if ($instance) return $instance;
            //初始化插件实例
            return new $.sjLoading(self, args[0]);
        }
    }


})(window, $)