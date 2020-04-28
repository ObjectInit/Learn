/*
 * 基于jquery的下拉菜单插件
 * 依赖：jquery3.3.1
 * 作者：zcc
 * 注册事件：
        $.selectMenu.init(function (data) {
            console.log(data)
        });
 * 注册下拉按钮：
        $.selectMenu.init();
 * 日期：2019-03-28
 */
; (function ($, document) {

    //页面加载完成后初始化插件
    $(document).ready(function () {
        $.selectMenu.init();
    });

    var funCallback;

    $.selectMenu = {

        // 初始化下拉菜单
        init: function (callback) {

            //方法赋值，点击时return给外部调用
            if (typeof callback == 'function') {
                funCallback = callback
                return;
            }

            var lists = $('[data-component="selectMenu"]');
            for (var i = 0; i < lists.length; i++) {
                this.createList(lists[i], i, $(lists[i]).attr('key'), lists);
            }
        },

        // 创建下拉菜单select 
        createList: function (listContainer, index, key, lists) {

            var tagList = $('<div class="select-container"></div>');
            tagList.insertBefore(listContainer);

            //显示框listsshowbox,插入到创建的tagList中
            var listsshowbox;
            if ($(listContainer).attr("default-text") != "" && $(listContainer).attr("default-text") != undefined) {
                listsshowbox = $('<div class="iconfont icon-zhankai6 select-button">' + $(listContainer).attr("default-text") + ' </div>').appendTo(tagList); //显示框
            } else {
                listsshowbox = $('<div class="iconfont icon-zhankai6 select-button white select-icon"></div>').appendTo(tagList); //显示框
            }
            var sjList = $('<ul class="select-list-container hide"></ul>').appendTo(tagList);

            //获取元素的所有自定义属性 并赋值给新创建的元素
            lists.each(function () {
                var that = this;
                $.each(that.attributes, function () {
                    if (this.name != "data-component" && this.name != "default-text") {
                        if (this.name != "width") {
                            $(that).prev().find('.select-button').css(this.name, this.value)
                        } else {
                            $(that).prev().find('.select-list-container').css(this.name, this.value)
                        }
                    }
                });
            });
            this.createOptions(index, sjList, lists, listContainer);

            //鼠标移入显示框
            var isShowFlag;
            listsshowbox.hover(function (e) {
                sjList.show();
                $('.split-showdata').removeClass("split-showdatahover");
                $(this).addClass("split-showdatahover");
            }, function () {
                isShowFlag = true;
                sjList.hover(function (e) {
                    sjList.show();
                    isShowFlag = false;
                }, function () {
                    sjList.hide();
                    isShowFlag = true;
                });
                if (isShowFlag) {
                    sjList.hide();
                }
            });

            var li_option = sjList.find('li');

            //子项点击事件
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
        createOptions: function (index, _list, lists, listContainer) {

            //获取被选中的元素并将其值赋值到显示框中
            var options = lists.eq(index).find('option'),
                selectedOption = options.filter(':selected'),
                selected_index = selectedOption.index(),
                showbox = _list.prev();

            //为每个option建立个li并赋值
            for (var n = 0; n < options.length; n++) {
                var txtOption = options.eq(n).text();
                var tagOption = $('<li class="select-list-option"></li>');
                tagOption.text(txtOption).appendTo(_list);

                //获取元素的所有自定义属性 并赋值给新创建的元素
                options.eq(n).each(function () {
                    $.each(this.attributes, function () {
                        tagOption.attr(this.name, this.value)
                    });
                });

                //是否绑定点击事件
                if ($(listContainer).attr("fill") == "true" && $(listContainer).attr("default-text") != "" && $(listContainer).attr("default-text") != undefined) {

                    //点击显示框
                    tagOption.on('click', function (e) {
                        $(this).parent().prev().text($(this).text())

                        //获取元素的所有自定义属性 并赋值给新创建的元素
                        $(this).each(function () {
                            var _this = this;
                            $.each(this.attributes, function () {
                                if (this.name != 'class') {
                                    $(_this).parent().prev().attr(this.name, this.value)
                                }
                            });
                        });
                    });
                }

                //为被选中的元素添加class为selected
                if (n == selected_index) {
                    tagOption.attr('class', 'selected');
                }
            }
            var _this = this;

            // 分裂式按钮定位 
            $(".select-button").hover(function (e) {
                _this.getPosition(this);
            })
        },

        //获取定位
        getPosition: function (e) {

            // 下拉按钮定位 
            $('.select-list-container').css({
                "top": $(e).offset().top + 24,
                "right": $(window).width() - $(e).offset().left - $(e).width() - 12 + "px",
                "bottom": "auto",
            });
            if ($(window).height() < $('.select-list-container').height() + $(e).offset().top + 60) {
                $('.select-list-container').css({
                    "top": "auto",
                    "bottom": $(window).height() - $(e).offset().top + "px",
                });
            }
        },
    };
})(jQuery, document);