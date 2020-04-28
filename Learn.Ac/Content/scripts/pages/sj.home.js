/**
 * 说明：首页框架功能模块， 包括首页初始化、切换语种、修改密码、打开标签页、退出、等功能;
 * 依赖：jquery3.3.1、sj.dialog.js、sj.login.js
 * 作者: luming
 * 日期：2019-04-15
 */

; (function (window, $) {

    // 切换并设置语种
    function changeLanguage() { }

    // 修改密码
    function changePassword() { }

    // 打开标签页
    function addMenuTab() { }

    // 用户退出
    function signOut() { }

    // 根据布局设置表格高度
    function gridResize() { }

    // 打开个人信息
    function openUserInfo() { }

    // 打开设置
    function openSettings() { }

    //左右滚动tabs
    function initTab() {

        //菜单存放数组
        var arrTabs = [];

        //滚动数值
        var scrolNumber = 0;

        //菜单点击事件
        $('.nav-menu-item > a').on('click.menu', function (e) {
            e.preventDefault();
            var url = $(this).attr('href');

            //url存在
            if (url && url != '#') {
                var _id = url.replace(/\//g, '');
                if (_id.indexOf("?") != -1) {
                    _id = _id.split("?")[0];
                }

                //取消tab全部选择 隐藏选择绑定区域
                $('.tab').removeClass('tabs-selected');
                $('.tab').attr('selected', false);
                $('.iframe-container').css('display', 'none');

                //如果数组中存在 则不再添加 转而选中
                if (arrTabs.indexOf(url) == -1) {
                    arrTabs.push(url)
                    $('.tabs-header').append('<div class="tab tabs-selected"  selected="selected" url=' + url + '><div>' + $(this).text() + '</div><span url=' + url + ' class="iconfont icon-cha tab-close"></span></div>');
                    $('.tabs-header').after('<div class="iframe-container" style="display:block;" id=' + _id + '>' + sj.util.createFrame(url) + '</div>')

                    var tabWidth = 127;
                    $('.tab').each(function (i, t) {
                        tabWidth = tabWidth + $(t).width() + 27;
                    })
                    if (tabWidth > $(window).width()) {
                        if (tabWidth - $(window).width() > scrolNumber) {
                            scrolNumber = tabWidth - $(window).width();
                            $(".tabs-header").animate({ right: tabWidth - $(window).width() + "px" }, "fast");
                        }
                        $('.tab-button').show();
                    }
                } else {
                    $('#' + _id).css('display', 'block');
                    $('.tab').each(function (index, dom) {
                        if ($(dom).attr('url') == url) {
                            $(dom).click();
                            if ($(dom).offset().left < 0) {
                                $(".tabs-header").animate({ right: - $(dom).offset().left + "px" }, "fast");
                                scrolNumber = - $(dom).offset().left;
                            } else if ($(dom).offset().left > $(window).width() - 127) {
                                scrolNumber = $(dom).offset().left - $(window).width() + 252
                                $(".tabs-header").animate({ right: scrolNumber + "px" }, "fast");
                            }
                        }
                    });
                }
                $('.tab-page').removeClass('tabs-selected');
                $('.tab-page').attr('selected', false);
                $('.home-page').hide();
            }

            //判断是否显示首页内容
            homePage();
        });

        //home点击事件
        $('.tabs-header').on("click", ".tab-home", function () {
            //隐藏其他iframe项
            $('.iframe-container').css('display', 'none');

            //隐藏其他tab项
            $('.tab').attr('selected', false);
            $('.tab').removeClass('tabs-selected');
            $('.tab-home').attr('selected', false);
            $('.tab-home').addClass('tabs-selected');
            $('.home-page-container').show();
        });

        //tab点击事件
        $('.tabs-header').on("click", ".tab", function () {

            //获取tab绑定iframe的ID 展示当前 隐藏其他项
            var _id = $(this).attr('url').replace(/\//g, '');
            if (_id.indexOf("?") != -1) {
                _id = _id.split("?")[0];
            }
            $('.iframe-container').css('display', 'none');
            $('#' + _id).css('display', 'block');

            //如果点击的已经被展示 return
            if ($(this).attr('selected')) {
                return;
            }

            //tab展示当前 隐藏其他项
            $('.tab').attr('selected', false);
            $('.tab').removeClass('tabs-selected');
            $('.tab-home').attr('selected', false);
            $('.tab-home').removeClass('tabs-selected');
            $(this).addClass('tabs-selected');
            $(this).attr('selected', true);
        });

        //点击关闭事件
        $('.tabs-header').on("click", ".tab-close", function () {

            //从数组中删除被点击的项
            for (var i = 0; i < arrTabs.length; i++) {
                if (arrTabs[i] == $(this).attr('url')) {
                    arrTabs.splice(i, 1);
                }
            }

            //删除该项以及该项的iframe
            var _id = $(this).attr('url').replace(/\//g, '');
            if (_id.indexOf("?") != -1) {
                _id = _id.split("?")[0];
            }
            $('#' + _id).remove();
            $(this).parent().remove();
            if (arrTabs[arrTabs.length - 1]) {
                var lastId = arrTabs[arrTabs.length - 1].replace(/\//g, '');
                $('#' + lastId).css('display', 'block');
            }

            //选中最后一项
            $('.tab').each(function (index, dom) {
                if (index == $('.tab').length - 1) {
                    $(this).addClass('tabs-selected');
                    $(this).attr('selected', true);
                }
            });

            //判断是否显示两侧按钮
            var tabWidth = 127;
            $('.tab').each(function (i, t) {
                tabWidth = tabWidth + $(t).width() + 27;
            })

            if (tabWidth > $(window).width()) {
                $('.tab-button').show();

                //左移tabs
                $(".tabs-header").animate({ right: tabWidth - $(window).width() + "px" }, "fast");
            } else {
                $('.tab-button').hide();
            }

            homePage();

            //阻止冒泡
            return false;
        });

        //取消右击X号的事件
        $(".tabs-header").on("contextmenu", ".tab-close", function () {
            return false;
        });

        //打开右键操作栏
        $(".tabs-header").on("contextmenu", ".tab", function (e) {
            $('#tab-operate-menu').css({ 'top': e.pageY - 2, 'left': e.pageX - 2, 'display': 'block' }).attr('url', $(this).attr('url'));

            //获取tab绑定iframe的ID 展示当前 隐藏其他项
            var _id = $(this).attr('url').replace(/\//g, '');
            if (_id.indexOf("?") != -1) {
                _id = _id.split("?")[0];
            }
            $('.iframe-container').css('display', 'none');
            $('#' + _id).css('display', 'block');

            //tab展示当前 隐藏其他项
            $('.tab').attr('selected', false);
            $('.tab').removeClass('tabs-selected');
            $('.tab-home').attr('selected', false);
            $('.tab-home').removeClass('tabs-selected');
            $(this).addClass('tabs-selected');
            $(this).attr('selected', true);

            //tab项的隐藏显示
            var flag = true;
            $(this).mouseout(function () {
                $("#tab-operate-menu").mouseover(function () {
                    $("#tab-operate-menu").show();
                    flag = false;
                }).mouseout(function () {
                    flag = true;
                    $("#tab-operate-menu").hide();
                });
                if (flag) {
                    $("#tab-operate-menu").hide();
                }
            });
            return false;
        });

        //关闭当前
        $("#closeCurrent").click(function () {

            //关闭右击操作栏
            $('#tab-operate-menu').hide();

            //从数组中删除被点击的项
            for (var i = 0; i < arrTabs.length; i++) {
                if (arrTabs[i] == $(this).parent().attr('url')) {
                    arrTabs.splice(i, 1);
                }
            }

            //删除tab项
            $('.tab').each(function (index, dom) {
                if ($(dom).attr('url') == $('#tab-operate-menu').attr('url')) {
                    $(this).remove();
                }
            });

            //删除tab项的iframe
            var _id = $(this).parent().attr('url').replace(/\//g, '');
            if (_id.indexOf("?") != -1) {
                _id = _id.split("?")[0];
            }
            $('#' + _id).remove();

            //选中最后一个tab项的iframe
            if (arrTabs[arrTabs.length - 1]) {
                var lastId = arrTabs[arrTabs.length - 1].replace(/\//g, '');
                $('#' + lastId).css('display', 'block');
            }

            //选中最后一个tab项
            $('.tab').each(function (index, dom) {
                if (index == $('.tab').length - 1) {
                    $(this).addClass('tabs-selected');
                    $(this).attr('selected', true);
                }
            });

            //判断是否显示两侧按钮
            var tabWidth = 127;
            $('.tab').each(function (i, t) {
                tabWidth = tabWidth + $(t).width() + 27;
            })

            if (tabWidth > $(window).width()) {
                $('.tab-button').show();

                //左移tabs
                $(".tabs-header").animate({ right: tabWidth - $(window).width() + "px" }, "fast");
            } else {
                $('.tab-button').hide();
            }

            //调用左移按钮
            $('.tab-left-button').click();

            //显示隐藏首页内容
            homePage();
        });

        //刷新
        $("#refreshCurrent").click(function () {

            //关闭右击操作栏
            $('#tab-operate-menu').hide();

            //刷新iframe
            var _id = $('#tab-operate-menu').attr('url').replace(/\//g, '');
            if (_id.indexOf("?") != -1) {
                _id = _id.split("?")[0];
            }
            $('#' + _id).children().attr('src', $('#tab-operate-menu').attr('url'));
        });

        //关闭所有
        $("#closeAll").click(function () {

            //关闭右击操作栏
            $('#tab-operate-menu').hide();

            //从数组中删除被点击的项
            for (var i = 0; i < arrTabs.length; i++) {
                arrTabs.splice(i, 1);
            }

            //删除tab项 
            $('.tab').each(function (index, dom) {
                $(this).remove();
            });

            //删除tab项的iframe
            $('.iframe-container').each(function (index, dom) {
                $(this).remove();
            });

            //隐藏2侧按钮
            $('.tab-button').hide();

            //tabs组滚动至最左边
            $(".tabs-header").animate({ right: "0px" }, "normal");

            //显示隐藏首页内容
            homePage();
        });

        //关闭其他
        $("#closeOthers").click(function () {

            //关闭右击操作栏
            $('#tab-operate-menu').hide();

            //从数组中删除被点击之外的项并删除
            for (var i = 0; i < arrTabs.length; i++) {
                if (arrTabs[i] != $(this).parent().attr('url')) {
                    arrTabs.splice(i, 1);
                }
            }

            //删除tab项
            $('.tab').each(function (index, dom) {
                if ($(dom).attr('url') != $('#tab-operate-menu').attr('url')) {
                    $(this).remove();
                }
            });

            //删除tab项的iframe
            var _id = $(this).parent().attr('url').replace(/\//g, '');
            if (_id.indexOf("?") != -1) {
                _id = _id.split("?")[0];
            }
            $('.iframe-container').each(function (index, dom) {
                if ($(dom) != $('#' + _id)) {
                    $(this).remove();
                }
            });

            //隐藏2侧按钮
            $('.tab-button').hide();

            //tabs组滚动至最左边
            $(".tabs-header").animate({ right: "0px" }, "normal");

            //显示隐藏首页内容
            homePage();
        });

        //左移按钮
        $('.tab-left-button').click(function () {
            var tabWidth = 127;
            $('.tab').each(function (i, t) {
                tabWidth = tabWidth + $(t).width() + 27;
            })

            //根据宽度判断左移 127位元素加边距宽度
            if (scrolNumber > 0) {
                if (scrolNumber < 127) {
                    scrolNumber = 127
                }
                scrolNumber = scrolNumber - 127;
                $(".tabs-header").animate({ right: scrolNumber + "px" }, "fast");
                return;
            }
        });

        //右移按钮
        $('.tab-right-button').click(function () {
            var tabWidth = 127;
            $('.tab').each(function (i, t) {
                tabWidth = tabWidth + $(t).width() + 27;
            })

            //根据宽度判断右移 127位元素加边距宽度
            if (tabWidth > $(window).width()) {
                if (tabWidth - $(window).width() > scrolNumber) {
                    scrolNumber = scrolNumber + 127;
                    $(".tabs-header").animate({ right: scrolNumber + "px" }, "fast");
                    return;
                }
            }
        });

        //获取tab项 显示隐藏首页内容
        function homePage() {
            if ($('.tab').length == 0) {
                $('.tab-home').attr('selected', true);
                $('.tab-home').addClass('tabs-selected');
                $('.home-page-container').show();
            } else {
                $('.home-page-container').hide();
                $('.tab-home').attr('selected', false);
                $('.tab-home').removeClass('tabs-selected');
            }
        }
    }

    /**
    * 切换语种
    * @param {code,name} 选中的语种
    */
    function changeLanguage(item) {
        $.cookie('sjLanguage', item.name)
        location.reload(true);
    };

    // 页面加载完成后执行
    $(function () {
        // initTab();

        // initTabDropDown();

        $.watchWindowHeight();
        if (window.location.href.indexOf('Login/Index') > -1 && $('iframe', top.document).length > 0) {
            window.parent.location.href = sj.config.loginUrl;
        }
    });

    //监听页面大小 调用 设置窗体高度
    $(window).resize(function () {
        $.watchWindowHeight();
    });
}(window, $))


;$.extend({

    //监听屏幕大小变化
    watchWindowHeight: function () {
        var h = top.$(window).height() - 66;

        //设置窗体高度 最外部不出现滚动条
        $(".layout-main").attr("style", "height:" + h + "px");
        this._watchGridHeight();
    },

    //监听表格大小变化
    _watchGridHeight: function () {
        var h = 0;

        //获取表格外区域的高度
        $('div[data-sj-component="section"]').each(function (i, t) {
            if ($(t).hasClass('hide')) {
                h += 0
            } else {
                h += $(t).outerHeight();
            }
        });
        $('.sj-grid-action').each(function (i, t) {
            h += $(t).outerHeight();
        });
        var grid_height = top.$(window).height() - h - 28;

        //表格高度
        $('.sj-grid').css("height", grid_height);
    }
});