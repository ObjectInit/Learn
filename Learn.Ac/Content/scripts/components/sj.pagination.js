/*
 * 基于jquery的pagination插件
 * 用法：通过dom渲染插件，$("#...").SJPagination({...});
 * 具体参数:total            总条目数                 必选参数，整数
            pageSize         每页显示的条目数             可选参数，默认是10
            pageNumber       默认页码                     可选参数，默认是1
            pageChange   页码变化之前回调方法
            $.SJPagination.total(n);  方法调用：设置总条数
            $.SJPagination.pageSize(n);  方法调用：设置每页显示多少条
            $.SJPagination.setPage(n);  方法调用：设置第几页
 * 依赖：jquery3.3.1
 * 作者：zcc
 * 日期：2019-03-18
 */
; (function ($, document) {

    // 默认参数 
    defaults = {
        total: 0,
        pageSize: 10,
        pageNumber: 1,
        pageChange: function (n) { }
    };

    // 构造函数 
    function Plugin(element, options) {
        this.element = element;
        this.settings = $.extend({}, defaults, options);
        this.init();

        //设置总条数
        this.total = function (n) {
            this.settings.total = n;
            this.init();
            this.seticon();
        }

        //设置页码
        this.setPage = function (n) {
            $('.pagination-input').val(n)

            // 页码发生变化 
            this.seticon();
        }

        // 设置每页显示多少条 
        this.pageSize = function (n) {
            this.settings.pageSize = n;
            this.init();
            this.seticon();
        }
    };

    // 添加属性方法 
    Plugin.prototype = {

        // 初始化 添加dom 
        init: function () {
            var e = this;
            var pageNo = Math.ceil(e.settings.total / e.settings.pageSize);
            this.settings.pageNo = pageNo;
            $('.pagination [target="currentPage"]').val(this.settings.pageNumber)
            $('.pagination [target="allPage"]').text(pageNo)
            $('.pagination [target="total"]').text(this.settings.total)
            e.setSelectPages();
            e.seticon();
        },

        // 添加方法 
        setSelectPages: function () {
            var e = this;

            // 添加快捷键
            window.onkeydown = function (event) {
                var pageNumber = parseInt($('.pagination-input').val());

                //翻至第一页（快捷键）
                if (event.altKey && event.keyCode == 70) {
                    if (pageNumber != 1) {
                        pageNumber = 1;
                        e.settings.pageChange(pageNumber)
                    }

                    //翻至上一页（快捷键）
                } else if (event.altKey && event.keyCode == 80) {
                    if (pageNumber > 1) {
                        pageNumber = pageNumber - 1;
                        e.settings.pageChange(pageNumber)
                    }

                    //翻至下一页（快捷键）
                } else if (event.altKey && event.keyCode == 78) {
                    if (pageNumber < Math.ceil(e.settings.total / e.settings.pageSize)) {
                        pageNumber = pageNumber + 1;
                        e.settings.pageChange(pageNumber)
                    }

                    //翻至最后一页（快捷键）
                } else if (event.altKey && event.keyCode == 76) {
                    if (pageNumber != Math.ceil(e.settings.total / e.settings.pageSize)) {
                        pageNumber = Math.ceil(e.settings.total / e.settings.pageSize);
                        e.settings.pageChange(pageNumber)
                    }
                }

                // 阻止关于ALT的浏览器默认快捷键 
                if (event.altKey) {
                    event.preventDefault();
                }
            }

            // 分页 切换 
            $(".pagination-icon").click(function (event) {

                var pageNumber = parseInt($('.pagination-input').val());

                //翻至第一页
                if ($(this).attr('key') == 'first') {
                    if (pageNumber != 1) {
                        pageNumber = 1
                    }

                    //翻至上一页
                } else if ($(this).attr('key') == 'prev') {
                    if (pageNumber > 1) {
                        pageNumber = pageNumber - 1;
                    }

                    //翻至下一页
                } else if ($(this).attr('key') == 'next') {
                    if (pageNumber < Math.ceil(e.settings.total / e.settings.pageSize)) {
                        pageNumber = pageNumber + 1;
                    }

                    //翻至最后一页
                } else if ($(this).attr('key') == 'last') {
                    if (pageNumber != Math.ceil(e.settings.total / e.settings.pageSize)) {
                        pageNumber = Math.ceil(e.settings.total / e.settings.pageSize);
                    }
                }
                e.settings.pageChange(pageNumber)
            });

            // 页码发生变化回调 
            $(".pagination-input").change(function () {
                //e.settings.changefunciton($(this).val())
            });

            // 改变每页显示条数 
            $(".pagination-select").change(function () {
                e.settings.pageSize = $(this).val();
                e.settings.selectfunciton($(this).val())
                e.init();
                e.seticon();
            });

            // enter键 
            $(".pagination-input").keyup(function (event) {
                if (event.keyCode == 13) {
                    e.settings.pageEnter($(this).val())
                    e.seticon();
                }
            });
        },

        // 设置按钮置灰 
        seticon: function () {
            var e = this;
            e.settings.pageNumber = $('.pagination-input').val();
            if ($('.pagination-input').val() > 1 && $('.pagination-input').val() < Math.ceil(e.settings.total / e.settings.pageSize)) {
                $('.page-first,.page-prev').removeClass('pagination-disabled');
                $('.page-next,.page-last').removeClass('pagination-disabled');
            } else if ($('.pagination-input').val() == 1 && $('.pagination-input').val() == Math.ceil(e.settings.total / e.settings.pageSize)) {
                $('.page-first,.page-prev').addClass('pagination-disabled');
                $('.page-next,.page-last').addClass('pagination-disabled');
            } else if ($('.pagination-input').val() > 1 && $('.pagination-input').val() == Math.ceil(e.settings.total / e.settings.pageSize)) {
                $('.page-first,.page-prev').removeClass('pagination-disabled');
                $('.page-next,.page-last').addClass('pagination-disabled');
            } else if ($('.pagination-input').val() == 1 && $('.pagination-input').val() < Math.ceil(e.settings.total / e.settings.pageSize)) {
                $('.page-first,.page-prev').addClass('pagination-disabled');
                $('.page-next,.page-last').removeClass('pagination-disabled');
            }
        }
    };
    $.fn.SJPagination = function (options) {
        var e = this;
        var plugin = new Plugin(this, options);
        $.SJPagination = plugin;
        return plugin;
    }

})(jQuery, document)