/*
 * 基于jquery的tabs插件
 * 用法：$("...").sjTabs();
        $('.tabs-item') 元素同时支持设置url，Eg.:url="Demo1.html" ,url权重大于data-container-name标识
        切换tabs会有前后端交互，所以事件需要单独处理：
        $('[data-component="tabs"]').sjTabs({
            tabsBefore: function (oldTab, newTab) {
                $.sjTabs.setTabs(newTab.attr('data-tab-name'))
            }
        });
 * 依赖：jquery3.3.1
 * 作者：zcc
 * 日期：2019-04-16
 */
; (function (window, $) {

    // 默认参数 
    defaults = {
        tabsBefore: function (oldTab, newTab) { }
    };

    //页面加载完成后初始化插件
    $(document).ready(function () {
        $('[data-component="tabs"]').sjTabs();
    });

    // 构造函数 
    var Tabs = function (container, options) {

        this.container = container;
        this.options = $.extend({}, defaults, options);

        //设置tab切换
        this.setTabs = function (target) {
            target = decodeURIComponent(target);
            id = "#Button_" + target;
            $(id).addClass("tabs-tab_state_active").siblings().removeClass("tabs-tab_state_active");
            id = "#Tab_" + target;
            $(id).addClass("tabs-item_state_active").siblings().removeClass("tabs-item_state_active");
        }
    };
    Tabs.prototype = {

        // 初始化
        init: function () {
            var $container = this.container,
                $tabs = $container.find(".tabs-tab"),
                $items = $container.find(".tabs-item"),
                $this,
                name = $container.data("name"),
                id,
                _this = this;

            // 遍历tabs标签 添加标记 根据传入的参数判断是否更改tabs标签的内容
            $tabs.each(function (i) {
                $this = $(this);
                id = "Button_" + $this.data("tab-name");
                $this.prop("id", id);
            });

            // 遍历items 添加标记 根据传入的参数判断是否添加iframe
            $items.each(function (i) {
                $this = $(this);
                id = "Tab_" + $this.data("container-name");
                $this.prop("id", id);
                if ($this.attr('url')) {
                    $this.html('<iframe name="contentFrame" src="' + $this.attr('url') + '" width=100%  height=100% scrolling="no" frameborder="0" style="display:block;" ></iframe>')
                }
            });

            // tabs标签点击事件
            $tabs.on("click.tabs", function (e) {
                e.preventDefault();
                var oldtab = $('.tabs-head').find('.tabs-tab_state_active');

                try {
                    _this.options.tabsBefore(oldtab, $(this))
                } catch (e) { }
            });

            _this.setTabs($tabs.eq(0).data("tab-name"));
        }
    };

    // Plugin methods
    $.fn.sjTabs = function (options) {
        var $this = $(this),
            tabs = {},
            name;

        // 解决data取值的bug
        name = encodeURIComponent($this.data("container-name"));
        tabs[name] = new Tabs($this, options);
        tabs[name].init();
        $.sjTabs = tabs[name];
    };
}(window, $))