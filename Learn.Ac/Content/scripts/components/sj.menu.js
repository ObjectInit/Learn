/*
 * 导航菜单关联 tabs展示不开，右侧出现下拉菜单 和dialog结合 拖动||双击触发 支持右键操作
 * 用法：window.top.$.tabs.addTabs([{url:'',title:'',type:''}]);
 *      type = "page" ? 新开页面 ： 新开tab
 * 作者：zcc
 * 日期：2019-05-10
 */

; (function (window, $) {

    //变量声明
    var t = {}, tabMapper = {}, arrTabs = [], arrTabsDropDown = [], homeDom = $('.tabs-header').html();

    //新增tab
    t.addTabs = function (tabsArr) {
        if (window.top.location.href.indexOf('Index') > 0) {
            for (var i = 0; i < tabsArr.length; i++) {
                t.setData(tabsArr[i].url, tabsArr[i].title, tabsArr[i].type)
            }
        } else {
            for (var i = 0; i < tabsArr.length; i++) {
                window.open(tabsArr[i].url); 
            }
        }
    }

    //根据参数设置数组 dom等
    t.setData = function (url,title,type) {

        //url存在
        if (url && url != '#') {

            var url = url;
            var title = title;

            //取消tab全部选择 隐藏选择绑定区域
            $('.tab').removeClass('tabs-selected');
            $('.tab').attr('selected', false);
            $('.iframe-container').css('display', 'none');

            var maxlength = Math.floor($(window).width() / 127);
            var obj = {};
            obj.url = url;
            obj.title = title;
            obj.type = type;

            for (var i = 0; i < arrTabs.length; i++) {
                if (arrTabs[i].url == url) {
                    arrTabs.splice(i, 1)
                }
            }

            arrTabs.unshift(obj)
            if (arrTabs.length + 1 > maxlength) {
                arrTabsDropDown.unshift(arrTabs[$('.tab').length])
                arrTabs.splice($('.tab').length, 1)
            }

            for (var i = 0; i < arrTabsDropDown.length; i++) {
                if (arrTabsDropDown[i].url == url) {
                    arrTabsDropDown.splice(i, 1)
                }
            }

            //判断是否显示下拉按钮
            t.dropDown();

            //设置dom
            t.setDom(url);
        }

        //显示隐藏首页内容
        t.homePage();
    }

    //打开弹出层
    t.openDialog = function (provisional) {
        //调用弹出层组件
        $.dialog.open({
            url: sj.config.siteHost + provisional.url,
            title: provisional.title,
            width: 1000,
            height: 600,
            shade: 0,
            tabs: true,
            //flex: 'row',//col row 
            moveEnd: function (layero) {
                //判断如果移入到tabs区域 则关闭弹出层 并加入tabs
                if (65 < layero.offset().top && layero.offset().top < 100) {
                    $.dialog.closeNow(layero.attr('times'))
                    arrTabs.unshift(provisional)
                    arrTabs = arrTabs.reverse()
                    t.addTabs(arrTabs);
                }
            },
            dbCallback: function (layero) {
                //新开2个BUG 暂未处理
                $.dialog.closeNow(layero.attr('times'))
                arrTabs.unshift(provisional)
                arrTabs = arrTabs.reverse()
                t.addTabs(arrTabs);
            }
        });
    }

    //tab拖动打开dialog等操作
    t.moveTab = function () {

        //鼠标点击的位置距离DIV左边的距离
        var disX = 0;

        //鼠标点击的位置距离DIV顶部的距离
        var disY = 0;
        $('.tabs-header > .tab').mousedown(function (e) {
            var e = e || window.event;
            disX = e.clientX;
            disY = e.clientY;
            var that = this;
            var domLeft = $('.tabs-header > .tab').offset().left;
            var domTop = $('.tabs-header > .tab').offset().top;
            var provisional = arrTabs[$('.tab').index(that)];
            document.onmousemove = function (e) {
                var e = e || window.event;

                // 横轴坐标
                var leftX = e.clientX - disX;

                // 纵轴坐标
                var topY = e.clientY - disY;
                if (leftX < 0) {
                    leftX = 0;
                }

                // 获取浏览器视口大小 document.document.documentElement.clientWidth
                else if (leftX > document.documentElement.clientWidth - $('.tabs-header > .tab').width()) {
                    leftX = document.document.documentElement.clientWidth - $('.tabs-header > .tab').width();
                }

                if (topY < 0) {
                    topY = 0;
                }
                else if (topY > document.documentElement.clientHeight - $('.tabs-header > .tab').height()) {
                    topY = document.documentElement.clientHeight - $('.tabs-header > .tab').height();
                }
                $('.tabs-header > .tab').css('left', leftX + "px");
                $('.tabs-header > .tab').css('top', topY + "px");
            }
            document.onmouseup = function () {
                try {
                    if (Math.abs(domLeft - $('.tabs-header > .tab').offset().left) > 10 || Math.abs(domTop - $('.tabs-header > .tab').offset().top) > 10) {
                        //从数组中删除被点击的项
                        arrTabs.splice($('.tab').index(that), 1)

                        //判断是否存在下拉菜单
                        if (arrTabsDropDown.length > 0) {

                            //菜单的  下拉菜单数组中 取出一项 放入 tab数组中
                            arrTabs.push(arrTabsDropDown[0])

                            //删除 菜单的  下拉菜单数组中的第一项
                            arrTabsDropDown.splice(0, 1)

                        }

                        var url = $(that).attr('url')

                        //删除该项的iframe
                        var _id = url.replace(/\//g, '');
                        _id = tabMapper[_id];
                        $('#' + _id).remove();

                        //判断是否显示下拉按钮
                        t.dropDown();

                        //设置dom
                        t.setDom(url)

                        //显示隐藏首页内容
                        t.homePage();

                        //选中第一项
                        t.selectFirstItem();
                        t.openDialog(provisional)
                    }
                }
                catch (err) {
                    console.log('关闭已触发')
                }
                document.onmousemove = null;
                $('.tabs-header > .tab').mousedown = null;
            }
        });
    }

    //判断是否显示下拉按钮以及下拉内容
    t.dropDown = function () {
        if (arrTabsDropDown.length < 1) {
            $('.tab-dropdown-button').hide();
            $('.tab-list').hide();
        } else {
            $('.tab-dropdown-button').show();
            $('.tab-list').show();
        }
    }

    //设置dom 重新生成tab 和iframe
    t.setDom = function (url) {
        $('.tab-list').html('');
        for (var i = 0; i < arrTabsDropDown.length; i++) {
            $('.tab-list').append('<li class="tab-list-item" title=' + arrTabsDropDown[i].title + ' url=' + arrTabsDropDown[i].url + '>' + arrTabsDropDown[i].title + '</li>')
            $('.tab-dropdown-button').show();
        }

        $('.tabs-header').html(homeDom);
        $('.other-page-container').html('');
        for (var i = 0; i < arrTabs.length; i++) {

            var isSelected = url == arrTabs[i].url;

            var _id = arrTabs[i].url.replace(/\//g, '');
            t.setBindId(_id);

            if (isSelected) {
                $('.tabs-header').append('<div class="tab tabs-selected" title=' + arrTabs[i].title + '  selected="selected" url=' + arrTabs[i].url + '><div>' + arrTabs[i].title + '</div><span url=' + arrTabs[i].url + ' class="iconfont icon-cha tab-close"></span></div>');
            } else {
                $('.tabs-header').append('<div class="tab" title=' + arrTabs[i].title + ' url=' + arrTabs[i].url + '><div>' + arrTabs[i].title + '</div><span url=' + arrTabs[i].url + ' class="iconfont icon-cha tab-close"></span></div>');
            }

            $('.other-page-container').append('<div class="iframe-container" style="display:' + (isSelected ? 'block' : 'none') + ';" id=' + tabMapper[_id] + '>' + sj.util.createFrame(arrTabs[i].url) + '</div>')
        }
        t.moveTab();
    }

    //获取tab项 显示隐藏首页内容
    t.homePage = function () {
        if ($('.tab').length == 0) {
            $('.tab-fixed').attr('selected', true);
            $('.tab-fixed').addClass('tabs-selected');
            $('.home-page-container').show();
        } else {
            $('.home-page-container').hide();
            $('.tab-fixed').attr('selected', false);
            $('.tab-fixed').removeClass('tabs-selected');
        }
    }

    //在当前节点中，获取id
    t.getBindId = function () {
        var _id = $(this).attr('url').replace(/\//g, '');
        return tabMapper[_id];
    }

    //设置节点id
    t.setBindId = function (_id) {
        tabMapper[_id] = Math.floor(Math.random() * 100000000 + 1);
    }

    //选中第一项
    t.selectFirstItem = function () {
        $('.tab').each(function (index, dom) {
            if (index == 0) {
                $(dom).click();
            }
        });
    }

    //初始化事件
    t.initTabDropDown = function (tabs) {

        //判断页面打开方式 page新开页码  tab tabs打开
        if ($('[data-component="tabs"]').attr('type') == "page") {

            //遍历打开页面
            if (tabs) {
                for (var i = 0; i < tabs.length; i++) {
                    window.open(tabs[i].url); 
                }
            }
            
            return;
        }

        //菜单点击事件
        $('.nav-menu-item > a').on('click.menu', function (e) {
            e.preventDefault();
            var url = $(this).attr('href');
            var title = $(this).text();
            var type = $(this).attr('type');

            //临时存放的对象 dialog时使用
            var provisional = { url: url,title:title,type:type};

            //遍历打开页面
            if (type == 'page') {
                window.open(url);
                return;
            }

            //遍历打开页面
            if (type == 'dialog') {
                if (JSON.stringify(arrTabs).indexOf(JSON.stringify(provisional)) == -1) {
                    t.openDialog(provisional)
                }
                return;
            }

            var tabsArr = [{
                url: url,
                title: title,
                type: type
            }]
            t.addTabs(tabsArr);
        });

        //菜单->下拉菜单项点击事件
        $(document).on('click', '.tab-list-item', function (e) {

            var url = $(this).attr('url');
            arrTabs.unshift(arrTabsDropDown[$('.tab-list-item').index(this)])
            arrTabsDropDown.splice($('.tab-list-item').index(this), 1)

            arrTabsDropDown.unshift(arrTabs[$('.tab').length])
            arrTabs.splice($('.tab').length, 1)

            //判断是否显示下拉按钮
            t.dropDown();

            //设置dom
            t.setDom(url);

            //显示隐藏首页内容
            t.homePage();

        });

        //home点击事件
        $('.tabs-header').on("click", ".tab-fixed", function () {
            //隐藏其他iframe项
            $('.iframe-container').css('display', 'none');

            //隐藏其他tab项
            $('.tab').attr('selected', false);
            $('.tab').removeClass('tabs-selected');
            $('.tab-fixed').attr('selected', false);
            $('.tab-fixed').addClass('tabs-selected');
            $('.home-page-container').show();
        });

        //tab点击事件
        $('.tabs-header').on("click", ".tab", function () {
            //获取tab绑定iframe的ID 展示当前 隐藏其他项
            var _id = t.getBindId.call(this);

            $('.iframe-container').css('display', 'none');
            $('#' + _id).css('display', 'block');

            //如果点击的已经被展示 return
            if ($(this).attr('selected')) {
                return;
            }

            //tab展示当前 隐藏其他项
            $('.tab').attr('selected', false);
            $('.tab').removeClass('tabs-selected');
            $('.tab-fixed').attr('selected', false);
            $('.tab-fixed').removeClass('tabs-selected');
            $('.home-page-container').hide();
            $(this).addClass('tabs-selected');
            $(this).attr('selected', true);
        });

        //点击关闭事件
        $('.tabs-header').on("click",".tab-close" ,function () {

            //从数组中删除被点击的项
            arrTabs.splice($('.tab-close').index(this), 1)

            //判断是否存在下拉菜单
            if (arrTabsDropDown.length > 0) {

                //菜单的  下拉菜单数组中 取出一项 放入 tab数组中
                arrTabs.push(arrTabsDropDown[0])

                //删除 菜单的  下拉菜单数组中的第一项
                arrTabsDropDown.splice(0, 1)

            }

            var url = $(this).attr('url')

            //删除该项的iframe
            var _id = url.replace(/\//g, '');
            _id = tabMapper[_id];
            $('#' + _id).remove();

            //判断是否显示下拉按钮
            t.dropDown();

            //设置dom
            t.setDom(url)

            //显示隐藏首页内容
            t.homePage();

            //选中第一项
            t.selectFirstItem();

            //阻止冒泡
            return false;
        });

        //取消右击X号的事件
        $('.tabs-header').on("contextmenu", ".tab-close", function () {
            return false;
        });

        //打开右键操作栏
        $('.tabs-header').on("contextmenu", ".tab", function (e) {

            $('.tab-operates').css({ 'top': e.pageY - 2, 'left': e.pageX - 2, 'display': 'block' }).attr('url', $(this).attr('url'));

            //获取tab绑定iframe的ID 展示当前 隐藏其他项
            var _id = $(this).attr('url').replace(/\//g, '');

            _id = tabMapper[_id];

            $('.iframe-container').css('display', 'none');
            $('#' + _id).css('display', 'block');

            //tab展示当前 隐藏其他项
            $('.tab').attr('selected', false);
            $('.tab').removeClass('tabs-selected');
            $('.tab-fixed').attr('selected', false);
            $('.tab-fixed').removeClass('tabs-selected');
            $(this).addClass('tabs-selected');
            $(this).attr('selected', true);

            //tab项的隐藏显示
            var flag = true;
            $(this).mouseout(function () {
                $(".tab-operates").mouseover(function () {
                    $(".tab-operates").show();
                    flag = false;
                }).mouseout(function () {
                    flag = true;
                    $(".tab-operates").hide();
                });
                if (flag) {
                    $(".tab-operates").hide();
                }
            });
            return false;
        });

        //关闭当前
        $("#closeCurrent").click(function () {

            //关闭右击操作栏
            $('.tab-operates').hide();

            //从数组中删除被点击的项
            var _index = 0;
            $('.tab').each(function (index, dom) {
                if ($(dom).attr('selected')) {
                    _index = index;
                }
            });
            arrTabs.splice(_index, 1)

            //判断是否存在下拉菜单
            if (arrTabsDropDown.length > 0) {

                //菜单的  下拉菜单数组中 取出一项 放入 tab数组中
                arrTabs.push(arrTabsDropDown[0])

                //删除 菜单的  下拉菜单数组中的第一项
                arrTabsDropDown.splice(0, 1)

            }

            var url = $(this).parent().attr('url')

            //删除该项的iframe
            var _id = url.replace(/\//g, '');
            _id = tabMapper[_id];
            $('#' + _id).remove();

            //判断是否显示下拉按钮
            t.dropDown();

            //设置dom
            t.setDom(url)

            //显示隐藏首页内容
            t.homePage();

            //选中第一项
            t.selectFirstItem();

            //阻止冒泡
            return false;
        });

        //刷新
        $("#refreshCurrent").click(function () {

            //关闭右击操作栏
            $('.tab-operates').hide();

            //刷新iframe
            var _id = t.getBindId.call($('.tab-operates'));
            $('#' + _id).children().attr('src', $('.tab-operates').attr('url'));
        });

        //关闭所有
        $("#closeAll").click(function () {

            //关闭右击操作栏
            $('.tab-operates').hide();

            //从数组中删除所有项
            arrTabs = [];
            arrTabsDropDown = [];

            //删除所有项的iframe
            $('.iframe-container').remove();

            //重新生成菜单
            $('.tab-list').html('');

            $('.other-page-container').html('');

            $('.tabs-header').html(homeDom);

            //判断是否显示下拉按钮
            t.dropDown();

            //显示隐藏首页内容
            t.homePage();

            //阻止冒泡
            return false;
        });

        //关闭其他
        $("#closeOthers").click(function () {

            //关闭右击操作栏
            $('.tab-operates').hide();

            //从数组中删除被点击之外的项
            for (var i = 0; i < arrTabs.length; i++) {
                if (arrTabs[i].url != $('.tab-operates').attr('url')) {
                    arrTabs.splice(i, 1);
                    i --
                }
            }
            arrTabsDropDown = [];

            //删除其他项的iframe
            var _id = t.getBindId.call($('.tab'));
            $('#' + _id).remove();
            $('.iframe-container').each(function (index, dom) {
                if ($(this).attr('id') != _id) {
                    $(this).remove();
                }
            });

            //重新生成菜单
            $('.tab-list').html('');
            $('.other-page-container').html('');
            $('.tabs-header').html(homeDom);
            for (var i = 0; i < arrTabs.length; i++) {
                $('.tabs-header').append('<div class="tab tabs-selected"  selected="selected" url=' + arrTabs[i].url + '><div>' + arrTabs[i].title + '</div><span url=' + arrTabs[i].url + ' class="iconfont icon-cha tab-close"></span></div>');

                var _id = arrTabs[i].url.replace(/\//g, '');
                t.setBindId(_id);

                $('.other-page-container').append('<div class="iframe-container" style="display:block;" id=' + tabMapper[_id] + '>' + sj.util.createFrame(arrTabs[i].url) + '</div>')
            }

            //判断是否显示下拉按钮
            t.dropDown();

            //显示隐藏首页内容
            t.homePage();

            //阻止冒泡
            return false;
        });

        //显示隐藏下拉内容
        $('.tab-dropdown-button').click(function () {
            $('.tab-list').toggle(500);
        })

        //双击关闭tab 弹出dialog
        $('.tabs-header').on("dblclick", ".tab", function () {

            //临时存放的对象 dialog时使用
            var provisional = arrTabs[$('.tab').index(this)];

            //从数组中删除被点击的项
            arrTabs.splice($('.tab').index(this), 1)

            //判断是否存在下拉菜单
            if (arrTabsDropDown.length > 0) {

                //菜单的  下拉菜单数组中 取出一项 放入 tab数组中
                arrTabs.push(arrTabsDropDown[0])

                //删除 菜单的  下拉菜单数组中的第一项
                arrTabsDropDown.splice(0, 1)

            }

            var url = $(this).attr('url')

            //删除该项的iframe
            var _id = url.replace(/\//g, '');
            _id = tabMapper[_id];
            $('#' + _id).remove();

            //判断是否显示下拉按钮
            t.dropDown();

            //设置dom
            t.setDom(url)

            //显示隐藏首页内容
            t.homePage();

            //选中第一项
            t.selectFirstItem();

            //调用弹出层组件
            t.openDialog(provisional)

            //阻止冒泡
            return false;
        });
    }

    $.tabs = t;

    // 初始化菜单
    $(function () {
        $.tabs.initTabDropDown();

    });

}(window,$))