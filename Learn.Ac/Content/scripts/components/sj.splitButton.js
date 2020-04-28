/*
 * 基于jquery的分裂式按钮
 * 依赖：jquery3.3.1
 * 作者：zcc
 * 用法：
 * 注册事件：
        $.splitButton.split( function (data) {
                console.log(data)
            }
        );
 * 注册分裂式按钮：
        $.splitButton.split();
 * 日期：2019-03-28
 */
; (function ($, document) {

    //页面加载完成后初始化插件
    $(document).ready(function () {
        $.splitButton.split();
    });

    var funCallback;

    $.splitButton = {

        // 遍历获取select 创建分裂式按钮 
        split: function (callback) {

            //方法赋值，点击时return给外部调用
            if (typeof callback == 'function') {
                funCallback = callback
                return;
            }

            var split = $('[data-component="splitButton"]');

            // 获取select 
            for (var i = 0; i < split.length; i++) {
                this.createSelect(split[i], i, $(split[i]).attr('key'), split);
            }
        },

        // 创建分裂式按钮select 
        createSelect: function (selectContainer, index, key, split) {
            var tagSelect = $('<div class="split-button-container"></div>');
            tagSelect.insertBefore(selectContainer);

            // 显示框selectShowbox,插入到创建的tagSelect中 
            console.log($(split).attr('text'))
            var selectShowbox = $('<div class="iconfont icon-zhankai6 split-showdata white"></div>').appendTo(tagSelect); //显示框
            var sjSelect = $('<ul class="split-select"></ul>').appendTo(tagSelect);
            if ($(split).attr("panelweight")) {
                sjSelect.css('width', $(selects).attr("panelweight"))
            } else {
                sjSelect.css('min-width', 55)
            }
            // 创建option 
            this.createSplitOptions(index, sjSelect, split);
            $(sjSelect).css({
                "right": $(window).width() - $(tagSelect).offset().left - $(selectShowbox).width() - 2 + "px",
                "display": "none"
            });

            // 判断下拉展开window高度是否满足  预留 7个像素 
            if ($(window).height() < $(sjSelect).height() + $(tagSelect).offset().top + 37 + 30) {
                $(sjSelect).css({
                    "top": "auto",
                });
            }

            //鼠标移入显示框
            var isShowFlag;
            selectShowbox.hover(function (e) {
                sjSelect.show();
                $('.split-showdata').removeClass("split-showdatahover");
                $(this).addClass("split-showdatahover");
            }, function () {
                isShowFlag = true;
                sjSelect.hover(function (e) {
                    sjSelect.show();
                    isShowFlag = false;
                }, function () {
                    sjSelect.hide();
                    isShowFlag = true;
                });
                if (isShowFlag) {
                    sjSelect.hide();
                }
            });

            var li_option = sjSelect.find('li');

            li_option.on('click', function (e) {
                $(selectShowbox).removeClass("split-showdatahover");
                $(this).addClass('selected').siblings().removeClass('selected');
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

        // 创建分裂式按钮options 
        createSplitOptions: function (index, _select, split) {

            // 获取被选中的元素并将其值赋值到显示框中 
            var options = split.eq(index).find('option'),
                selectedOption = options.filter(':selected'),
                selected_index = selectedOption.index(),
                showbox = _select.prev();

            // 为每个option建立个li并赋值 
            for (var n = 0; n < options.length; n++) {
                var txtOption = options.eq(n).text();
                var OptionTitle = options.eq(n).attr("title");
                var OptionCode = options.eq(n).attr("code");
                var tagOption = $('<li class="split-option-item"></li>');
                tagOption.text(txtOption).appendTo(_select);

                //获取元素的所有自定义属性 并赋值给新创建的元素
                options.eq(n).each(function () {
                    $.each(this.attributes, function () {
                        if (this.attributes != "class") {
                            tagOption.attr(this.name, this.value)
                        }
                    });
                });

                
            }

            var _this = this;
            // 分裂式按钮定位 
            $(".split-showdata").hover(function (e) {
                _this.getPosition(this);
            })
        },

        //获取定位
        getPosition: function (e) {
            $('.split-select').css({
                "top": $(e).offset().top + 24,
                "right": $(window).width() - $(e).offset().left - 22 + "px",
                "bottom": "auto",
            });
            if ($(window).height() < $('.split-select').height() + $(e).offset().top + 60) {
                $('.split-select').css({
                    "top": "auto",
                    "bottom": $(window).height() - $(e).offset().top + "px",
                });
            }
        },
    };
})(jQuery, document);