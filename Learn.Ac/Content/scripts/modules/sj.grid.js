
/**
* grid 组件功能模块
* 依赖jquery3.3.1、layer、sj.pagination、sj.dialog、sj.tabs
* 作者:zcc
* 日期：2019-05-29
*/
; (function ($) {
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
     * 提交表单
     * url 表单提交地址
     * form 表单对象
     * @param {Function} CallBack 成功回调
     */
    function postform(url, form, completeCallBack) {

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

        // 发送到后端的数据对象
        var data = getFormData(form);

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
                // 回调
                if (typeof completeCallBack === 'function') {
                    completeCallBack(result);
                }
            },
            success: function (result) {
                if (result.Code != 0) {
                    $.dialog.alert('error', result.Message, '提示')
                    return;
                }
            }
        });
    }

    //页面加载完成
    $(document).ready(function () {

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
                if (currentTab.attr('issave') == "true") {
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
                if (nextTab.attr('isrefresh') == "true") {
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
         * 分页插件调用
         * */
        $(".pagination").SJPagination({
            total: 100,

            // 页码改变事件（改变之前）
            pageChange: function (n) {

                //获取当前tab内的form表单
                var $form = $('.tabs-item_state_active').find('form')
                postform($form.attr('action'), $form, function (result) {
                    if (result.responseJSON.Code != 0) {
                        return;
                    }

                    //请求成功调用翻页
                    $.SJPagination.setPage(n)
                });
            }
        })

        /**
         * 下拉按钮注册事件
         * */
        //$.listButton.list(function (obj) {
        //    if (obj.attr('code') == 3) {
        //        jQuery.dialog.open({
        //            width: 400,
        //            height: 140,
        //            url: 'LineNumber.html',
        //            title: obj.attr('dialog-title'),
        //            messager: function (data) {
                        
        //            }
        //        });
        //    }
        //});

        //设置行数页码的 保存并关闭按钮 获取被选中的input 关闭弹出层
        $('[data-component="setRow"]').on('click', function () {
            var val = $('input:radio[name="number"]:checked').val();
            console.log(val)
            jQuery.dialog.close();
        })
        $('[data-component="date"]').blur(function () {
            $.date();
        });
        $('[data-component="dateTime"]').blur(function () {
            $.dateTime();
        });
    });
}(jQuery))