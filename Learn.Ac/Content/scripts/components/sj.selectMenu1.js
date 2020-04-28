/*
 * 基于jquery的下拉菜单插件
 * 依赖：jquery3.3.1
 * 作者：zcc
 * 注册事件：
        $.selectMenu.select(function (data) {
            console.log(data)
        });
 * 注册下拉按钮：
        $.selectMenu.select();
 * 日期：2019-03-28
 */
; (function ($, document) {

    //页面加载完成后初始化插件
    $(document).ready(function () {
        $.selectMenu.select();
    });

    var funCallback;

    $.selectMenu = {

        // 遍历获取select 创建下拉菜单 
        select: function (callback) {

            //方法赋值，点击时return给外部调用
            if (typeof callback == 'function') {
                funCallback = callback
                return;
            }

            var selects = $('[data-component="selectMenu"]');
            for (var i = 0; i < selects.length; i++) {
                this.createSelect(selects[i], i, $(selects[i]).attr('key'), selects);
            }
        },

        // 创建下拉菜单select 
        createSelect: function (selectContainer, index, key, selects) {
            var tagSelect = $('<div class="select-button"></div>');
            tagSelect.insertBefore(selectContainer);

            //显示框selectsshowbox,插入到创建的tagSelect中  
            var selectsshowbox = $('<div class="iconfont icon-zhankai6 select-showdata"></div>').appendTo(tagSelect); //显示框
            var sjSelect = $('<ul class="select-selected hide"></ul>').appendTo(tagSelect);
            //获取元素的所有自定义属性 并赋值给新创建的元素
            selects.each(function () {
                var that = this;
                $.each(that.attributes, function () {
                    if (this.name != "data-component") {
                        $(that).prev().find('.select-showdata').css(this.name, this.value)
                        if (this.name == "width") {
                            $(that).prev().find('.select-selected').css(this.name, this.value)
                        } else {
                            selectsshowbox.css('width', tagSelect.width())
                            sjSelect.css('width', tagSelect.width())
                        }
                    }
                });
            });
            this.createOptions(index, sjSelect, selects);

            //点击显示框
            $(document).bind("click", function (e) {
                if ($(e.target)[0] == selectsshowbox[0]) {
                    sjSelect.toggle();
                    $('.select-showdata').removeClass("select-showdatahover");
                    $(this).addClass("select-showdatahover");
                } else {
                    $(selectsshowbox).removeClass("select-showdatahover");
                    sjSelect.hide();
                }
            });

            var li_option = sjSelect.find('li');

            li_option.on('click', function (e) {
                $(selectsshowbox).removeClass("select-showdatahover");
                $(this).addClass('selected').siblings().removeClass('selected');
                sjSelect.hide();
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

        // 创建下拉菜单select 
        createOptions: function (index, _select, selects) {

            //获取被选中的元素并将其值赋值到显示框中
            var options = selects.eq(index).find('option'),
                selectedOption = options.filter(':selected'),
                selected_index = selectedOption.index(),
                showbox = _select.prev();

            //设置默认值
            selects.eq(index).prev().find('.select-showdata').text(options.eq(0).text())

            // 将用户设置的属性，设置到对应的"option"上。
            options.eq(0).each(function () {
                $.each(this.attributes, function () {
                    selects.prev().find('.select-showdata').attr(this.name, this.value)
                });
            });

            //为每个option建立个li并赋值
            for (var n = 0; n < options.length; n++) {
                var txtOption = options.eq(n).text();
                var tagOption = $('<li class="select-option"></li>');
                tagOption.text(txtOption).appendTo(_select);

                //获取元素的所有自定义属性 并赋值给新创建的元素
                options.eq(n).each(function () {
                    $.each(this.attributes, function () {
                        tagOption.attr(this.name, this.value)
                    });
                });

                //指定选中某一个
                if (tagOption.attr('selected')) {
                    selects.prev().find('.select-showdata').text(options.eq(n).text())
                    options.eq(n).each(function () {
                        $.each(this.attributes, function () {
                            selects.prev().find('.select-showdata').attr(this.name, this.value)
                        });
                    });
                }

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