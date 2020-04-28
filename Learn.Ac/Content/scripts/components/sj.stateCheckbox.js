/*
 * 基于jquery的checkbox-state插件
 * 用法：
 * 依赖：jquery3.3.1
 * 作者：megan
 * 日期：2019-04-09
 * 修改人：zcc
 * 修改内容：重构（重构原因：页面dom过多）
 * 修改日期：2019-05-17
 */
;(function (window, $) {
    
    //#region 内部方法

    /** 复选框类型 */
    var cheboxType = {

        /**
         * 默认复选框
         */
        defalut: 'checkbox-default', //勾 空

        /**
         * 两态框
         */
        twoState: 'checkbox-twoState', //勾 叉

        /**
         * 三态框
         */
        threeState: 'checkbox-threeState'//勾 叉 空
    }
    
   /**
    * 初始化复选框
    */
    function init() {

        $('.checkbox-state').each(function () { 
            initState(this);

            addClickEvent(this);
        });
    }

    /**
     * 根据input的值渲染状态
     * @param { Object } input input
     */
    function initState(div) {

        var _div = $(div);

        var _input = $(div).prev();
        switch (_input.attr('data-component')) {
            case 'checkbox-default':
                if (_input.val() === "") {
                    nullState(div);
                } else if (_input.val() === "1") {
                    selectedState(div);
                }
                break;
            case 'checkbox-twoState':
                if (_input.val() === "0") {
                    notSelectedState(div);
                } else if (_input.val() === "1") {
                    selectedState(div);
                }
                break;
            case 'checkbox-threeState':
                if (_input.val() === "0") {
                    notSelectedState(div);
                } else if (_input.val() === "1") {
                    selectedState(div);
                } else if (_input.val() === "") {
                    nullState(div);
                }
                break;
        }
    }

    /**
     * 添加点击事件
     * @param {any} 
     */
    function addClickEvent(div) {

        var _div = $(div);
        
        var _input = $(div).prev();
        _div.off('click').on('click', function () {
            switch (_input.attr('data-component')) {
                case 'checkbox-default':
                    if (_input.val() === "") {
                        _input.val("1")
                    } else if (_input.val() === "1") {
                        _input.val("")
                    }
                    break;
                case 'checkbox-twoState':
                    if (_input.val() === "0") {
                        _input.val("1")
                    } else if (_input.val() === "1") {
                        _input.val("0")
                    }
                    break;
                case 'checkbox-threeState':
                    if (_input.val() === "0") {
                        _input.val("")
                    } else if (_input.val() === "1") {
                        _input.val("0")
                    } else if (_input.val() === "") {
                        _input.val("1")
                    }
                    break;
            }
            initState(div);
            if (_input.attr('name') == "code-all") {
                $('[name="code"]').each(function () {
                    var div = $(this).next();
                    var input = $(this);

                    if (input.val() === "1") {
                        $(div).removeClass('checkbox-hook');
                        input.val("")
                    } else if (input.val() === "") {
                        $(div).addClass('checkbox-hook');
                        input.val("1")
                    }
                });
            }
        });
    }

    /**
    * 【√】选中状态
    */
    function selectedState(_div) {
        $(_div).removeClass('checkbox-fork');
        $(_div).addClass('checkbox-hook');
    }

    /**
     * 【×】不选中状态
     */
    function notSelectedState(_div) {
        $(_div).addClass('checkbox-fork');
        $(_div).removeClass('checkbox-hook');
    }

    /**
     * 【□】 空状态
     */
    function nullState(_div) {
        $(_div).removeClass('checkbox-fork');
        $(_div).removeClass('checkbox-hook');
    }

    /**
     * 设置值
     */
    function setValue(div, value) {
        $(div).prev().val(value);
        initState(div);
    }
    // 页面加载完成，初始化组件
    $(document).ready(function () {
        
        init();
    });

    /**
     * 扩展jQuery方法
     * @param { Object } value 当前页面上input的value值
     */
    $.fn.setStateCheckbox = function (value) {
        setValue(this, value);

        return this;
    }

    /**
     * 多态复选框
     * */
    $.StateCheckbox = {};

    /**
     * 初始化多态复选框组件
     * */
    $.StateCheckbox.init = init;
    
    //#endregion
    
})(window, jQuery)