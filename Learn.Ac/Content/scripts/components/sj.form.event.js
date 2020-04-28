
/**
* grid 组件功能模块
* 依赖jquery3.3.1、layer、sj.core、sj.dialog
* @author jerem.wang
 * 修改人:zcc
 * 修改内容：第一次加载、tab切换 交互
 * 修改日期：2019-04-29
*/

; sj.grid = (function () {
    "use strict";

    var t = {};


    /** 隐藏域name */
    var hiddenNames = {
        /** 旧的查询数据 */
        queryData : "Query.Original",
        /** 当前的操作按钮 */
        eventCode : "EventCode",
        /** 当前操作参数 */
        eventValue : "DataParameter",
        /** 当前页码 */
        // hiddenNames.formName 
        pageIndex : "PageIndex",
        /** 页大小 */
        pageSize : "PageSize",
        /** 总记录条数 */
        total : "Total"
    };

    $(function () {
        // 初始化注册事件
        registerEvent();
    });

    /**
     * 初始化注册组件事件
     * */
    function registerEvent() {

        // 表单提交事件
        formBtnEvent();

        // 过滤操作事件
        filterBtnEvent(); 
    }

    /**
     * 【过滤】点击事件
     * */
    function filterBtnEvent() {

        $('[data-component="filterBtn"]').click(function () {
            if ($('.fold-hide').hasClass('hide')) {
                $('.fold-hide').removeClass('hide')
            } else {
                $('.fold-hide').addClass('hide')
            } 
        }) 
    }

    /** 表单提交事件*/
    function formBtnEvent() {
        
        var $events = $('[data-eventcode]');
        // 判断是否有事件操作
        if ($events.length <= 0) {
            return;
        }

        // 防止重复绑定click事件
        $events.off('click').click(function () {

            // 如果当前操作为弹窗 则打开新窗口
            if ($(this).attr('data-component') == "dialog") { 
                jQuery.dialog.open({
                    width: $(this).attr("data-option-width"),
                    height: $(this).attr("data-option-height"),
                    url: $(this).attr("data-option-url") + "?" + $(this).attr("data-option-param"),
                    title: $(this).attr("data-option-title")
                });
                return;
            }

            // 操作事件码
            var eventCode = $(this).attr('data-eventcode'); 

            // 当前表单提交地址
            var url = $(this).parents('form').attr('action');

            // 当前操作的值
            if ($(this).attr('data-param') != undefined) {
                inputVal(hiddenNames.eventValue, $(this).attr('data-param'));
            }

            // 判断是否有确认提示 如果有确认提示则做确认操作处理
            if ($(this).attr('data-confirm') != undefined) {
                //提示文本
                var confirm = $(this).attr('data-confirm'); 
                jQuery.dialog.confirm(confirm, '提示', function () {
                    exec();
                });

            } else {
                exec();
            }

            function exec() {
                // 提交表单
                submit(url, eventCode, function (result) {
                });
            }
        });
    }


    /**
     * tabs插件调用
     * */
    $('[data-component="tabs"]').sjTabs({

        //切换之前
        tabsBefore: function (currentTab, nextTab) {
            var saveflag = false;
            var refreshflag = false;
            var saveRequestSuccessful = false;
            var refreshRequestSuccessful = false;

            //判断旧的tab是否保存   如需保存 判断保存是否成功
            if (currentTab.attr('data-issave') == "true") {
                var $form = $('[data-container-name="' + currentTab.data('tab-name') + '"]').find('form');

                //判断是否存在form
                if ($form.length > 0) {
                    postform($form.attr('action'), $form, function completeCallBack(result) {
                        saveRequestSuccessful = true;
                        if (result.responseJSON.Code != 0) {
                            saveflag = false;
                            return;
                        } else {
                            saveflag = true;
                        }
                    });
                } else {
                    saveRequestSuccessful = true;
                    saveflag = true;
                }
            } else {
                saveRequestSuccessful = true;
                saveflag = true;
            }

            //判断新的tab是否刷新   如需刷新 判断刷新是否成功
            if (nextTab.attr('data-isrefresh') == "true") {
                var $form = $('[data-container-name="' + nextTab.data('tab-name') + '"]').find('form');

                //判断是否存在form
                if ($form.length > 0) {
                    postform($form.attr('action'), $form, function completeCallBack(result) {
                        refreshRequestSuccessful = true;
                        if (result.responseJSON.Code != 0) {
                            refreshflag = false;
                            return;
                        } else {
                            refreshflag = true;
                        }
                    });
                } else {
                    refreshRequestSuccessful = true;
                    refreshflag = true;
                }
            } else {
                refreshRequestSuccessful = true;
                refreshflag = true;
            }
            //检查异步请求是否 全部请求成功
            var intervalTab = setInterval(function () {
                if (saveRequestSuccessful && refreshRequestSuccessful) {
                    //条件切换tab
                    if (saveflag && refreshflag) {
                        $.sjTabs.setTabs(nextTab.attr('data-tab-name'))
                    }
                    window.clearInterval(intervalTab);
                }
            }, 100)
        }
    });


    /**
     * 设置操作对象，并提交表单
     * @param {String} url 请求地址
     * @param {String} eventCode 当前操作
     * @param {Function} successCallBack 操作成功回调
     */
    function submit(url, eventCode, successCallBack) {

        // 更新当前操作事件码
        inputVal(hiddenNames.eventCode, eventCode);

        postform(url, successCallBack);
    }

    /**
     * 提交表单
     * @param {String} url 请求地址
     * @param {Function} successCallBack 成功回调
     */
    function postform(url, successCallBack) {

        /**
         * 说明：表单提交处理方法，负责提交表单，处理后端返回数据以及展示Loading效果。
         * 
         * 1.获取页面表单，所有的input，select。
         * 
         * 2.根据得到的input的name组装成表单数据提交。
         *      
         * 3.POST数据到后端接口。
         * 
         * */

        // 表单校验和表单数据对象组装

        var $form = $('form')

        // 发送到后端的数据对象
        var data = getFormData($form);

        $.ajax({
            url: sj.config.siteHost + url, // url：form表单上的action
            type: 'POST',
            cache: false,
            asnyc: false,
            data: data,
            // 发送请求之前，此处用于打开Loading效果
            beforeSend: function () {
                //$('body').sjLoading();
            },
            // 请求完成后，此处用于关闭Loading效果
            complete: function (result) {
                //$('body').sjLoading('destroy');
            },
            success: function (result) {
                if (result.Code !== undefined && result.Code !== 0) {
                    //$.dialog.alert('error', result.Message, '')
                    //return;
                }

                // 回调
                if (typeof successCallBack === 'function') {
                    successCallBack(result);
                }

                if (typeof t.successCallBack === 'function') {
                    t.successCallBack();
                } 
            }
        });
    }

    /**
     * 获取表单数据
     * @param {JQuery} $form jq表单对象
     * @returns {Object} data
     */
    function getFormData($form) {

        var data = {};

        $form.find('input,textarea').each(function () {

            var name = $(this).attr('name');
            var value = $(this).val();

            if (name !== undefined && name !== '' && name !== null) {
                data[name] = value;
            }
        });

        return data;
    }

    /**
     * 设置或者获取参数
     * @param {any} name input name
     * @param {any} val input value
     * @returns {String} value
     */
    function inputVal(name, val) {
        var $input = $('input[name="' + name + '"]');

        if (val === undefined || val === '') {
            return $input.val();
        } else {
            $input.val(val);
        }
    }

    t.successCallBack = function () { };

    return t;

}());