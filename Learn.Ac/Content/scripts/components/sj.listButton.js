/*
 * 基于jquery的下拉菜单插件
 * 依赖：jquery3.3.1
 * 作者：zcc
 * 注册事件：
        $.listButton.list(function (data) {
            console.log(data)
        });
 * 注册下拉按钮：
        $.listButton.list();
 * 日期：2019-03-28
 */
; (function ($, document) {

    //页面加载完成后初始化插件
    $(document).ready(function () {
        $.listButton.list();
    });

    var funCallback;

    $.listButton = {

        // 遍历获取list 创建下拉菜单
        list: function (callback) {

            //方法赋值，点击时return给外部调用
            if (typeof callback == 'function') {
                funCallback = callback
                return;
            }

            var lists = $('[data-component="listButton"]');
            for (var i = 0; i < lists.length; i++) {
                this.createList(lists[i], i, $(lists[i]).attr('key'), lists);
            }
        },

        // 创建下拉菜单select 
        createList: function (listContainer, index, key, lists) {

            var other = $(listContainer).attr('default-text') || '其他';

            var tagList = $('<div class="lists-button"></div>');
            tagList.insertBefore(listContainer);

            //显示框listsshowbox,插入到创建的tagList中
            var listsshowbox = $('<div class="iconfont icon-zhankai6 list-showdata">' + other + ' </div>').appendTo(tagList); //显示框
            var sjList = $('<ul class="list-listed hide"></ul>').appendTo(tagList);
            //获取元素的所有自定义属性 并赋值给新创建的元素
            lists.each(function () {
                var that = this;
                $.each(that.attributes, function () {
                    if (this.name != "data-component") {
                        if (this.name != "width") {
                            $(that).prev().find('.list-showdata').css(this.name, this.value)
                        } else {
                            $(that).prev().find('.list-listed').css(this.name, this.value)
                        }
                    }
                });
            });
            this.createOptions(index, sjList, lists);

            //点击显示框
            $(document).bind("click", function (e) {
                if ($(e.target)[0] == listsshowbox[0]) {
                    sjList.toggle();
                    $('.list-showdata').removeClass("list-showdatahover");
                    $(this).addClass("list-showdatahover");
                } else {
                    $(listsshowbox).removeClass("select-showdatahover");
                    sjList.hide();
                }
            });

            var li_option = sjList.find('li');

            li_option.on('click', function (e) {
                $(listsshowbox).removeClass("select-showdatahover");
                $(this).addClass('listed').siblings().removeClass('listed');
                sjList.hide();
                try {
                    funCallback($(this));
                } catch (e) {
                }
                e.stopPropagation;
            });
            li_option.hover(function () {
                $(this).addClass('hover').siblings().removeClass('hover');
            }, function () {
                li_option.removeClass('hover');
            });
        },

        // 创建下拉菜单list 
        createOptions: function (index, _list, lists) {

            //获取被选中的元素并将其值赋值到显示框中
            var options = lists.eq(index).find('option'),
                selectedOption = options.filter(':selected'),
                selected_index = selectedOption.index(),
                showbox = _list.prev();

            //为每个option建立个li并赋值
            for (var n = 0; n < options.length; n++) {
                var txtOption = options.eq(n).text();
                var tagOption = $('<li class="list-option"></li>');
                tagOption.text(txtOption).appendTo(_list);

                //获取元素的所有自定义属性 并赋值给新创建的元素
                options.eq(n).each(function () {
                    $.each(this.attributes, function () {
                        tagOption.attr(this.name, this.value)
                    });
                });

                //为被选中的元素添加class为selected
                if (n == selected_index) {
                    tagOption.attr('class', 'selected');
                }
            }
            this.getPosition();
        },

        //获取定位
        getPosition: function () {

            // 下拉按钮定位 
            $(".select-showdata").click(function (e) {
                if ($(window).height() < $(this).next().height() + $(this).offset().top + 62) {
                    $(this).next().css({
                        "top": "auto",
                        "bottom": 25 + "px",
                    });
                }
            })
        },
    };
})(jQuery, document);