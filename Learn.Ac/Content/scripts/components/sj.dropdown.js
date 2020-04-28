//下拉菜单
; (function ($, window) {
    $(function () {
        $('.dropdown-toggle').each(function (t, i) {
            if ($(this).find('.caret').length > 0) {
                $(this).css({
                    "paddingRight": '22px'
                });
            }

        });
        $('.dropdown').hover(function () {
            //元素底部距离页面底部的距离
            var scrollB = $(window).height() - $(this).find('.dropdown-toggle').offset().top - $(this).find('.dropdown-toggle').height() - $(this).find('.dropdown-menu').height()

            var alignright, alignleft;
            if ($(this).attr('area') == 'right') {
                alignright = 0;
                alignleft = "auto";
            } else {
                alignleft = 0;
                alignright = "auto";
            }
            $(this).find('.dropdown-menu').css({
                "minWidth": $(this).find('.dropdown-toggle').outerWidth()
            });
            //元素
            if (scrollB < 30) {
                $('.dropdown-menu').css({
                    "top": "auto",
                    "bottom": $(this).find('.dropdown-toggle').height() + 2 + "px",
                    "right": alignright,
                    "left": alignleft,
                });
            } else {
                $('.dropdown-menu').css({
                    "top": "100%",
                    "bottom": "auto",
                    "right": alignright,
                    "left": alignleft,
                });
            }
            //显示菜单
            $(this).find('.dropdown-menu').css({ "display": "block" })
            //是否替换选择选项
            var condition = $(this).attr('fill')
            var tog = $(this)
            //菜单点击项
            $(this).find('.dropdown-menu').find('a').click(function () {
                if (condition) {
                    $(tog).find('a').removeClass('dropdown-item-on')
                    $(this).addClass('dropdown-item-on')
                    $(tog).find('.toggle-text').html($(this).text())
                }
                $(tog).find('.dropdown-menu').css({ "display": "none" })
            })
        }, function () {
            $(this).find('.dropdown-menu').css({ "display": "none" })
        });
    })
}(jQuery, window))