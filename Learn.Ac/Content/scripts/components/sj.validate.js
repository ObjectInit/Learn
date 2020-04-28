/*
 * 基于jquery的表单验证插件
 * 依赖：jquery3.3.1
 * 作者：jerem.wang
 * 日期：2019-04-24
 */
(function ($) {

    $.validate = {

        /** 表单验证方法 */
        methods: {

            /** 必填验证 */
            required: function (element) {
                
                var $element = $(element);

                // 如果存在多个表单
                if ($element.length !== 1) {
                    throw new Error('jquery selector is null !');
                }

                // 判断是否为form表单
                var isForm = $element.is('form');

                // 是否有必填框未填
                var isEmpty = false;

                if (isForm) {

                    // 禁用浏览器默认验证
                    $element.attr('novalidate', 'novalidate');

                    $element.find('input[required],textarea[required]').each(function () {
                        var length = $.trim($(this).val()).length;
                        
                        if (length === 0) {
                            isEmpty = true;
                            $(this).addClass('input-error');
                        } else {
                            $(this).removeClass('input-error');
                        }
                    });
                } else {
                    throw new Error('jquery selector not is form element !');
                }

                return !isEmpty;
            }
        }
    };

    /** 表单验证组件 */
    $.fn.validate = function () {

        return this;
    }

    /** 立即验证表单 */
    $.fn.valid = function () {

        return $.validate.methods['required'](this);
    }

})(jQuery)