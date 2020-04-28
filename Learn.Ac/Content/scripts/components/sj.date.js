/*
 * 说明：date 模块
 * 依赖： jquery3.3.1  layer3.1.1  
 * 作者：zcc
 * 日期：2019-04-24
 * 使用方法：dom上添加 data-component="date"或data-component="dateTime" 延迟加载的dom 需执行初始化：$.date();或$.dateTime();
 * */
; (function ($) {
    var myDate = new Date();
    //获取当前年
    var year = myDate.getFullYear();
    //获取当前月
    var month = myDate.getMonth() + 1;
    //获取当前日
    var date = myDate.getDate();
    //获取当时(0-23)
    var h = myDate.getHours();
    //获取当分(0-59)
    var m = myDate.getMinutes();
    //获取当秒
    var s = myDate.getSeconds();
    $.extend({
        date: function (document) {

            //遍历 执行layui laydate生成date
            $('[data-component="date"]').each(function () {
                var date = '';
                if ($(this).val().length > 0 && $(this).val().length < 3) {
                    date = year + '-' + month + '-' + $(this).val();
                }
                laydate.render({
                    elem: this,
                    type: 'date',
                    value: date,
                    isInitValue: true
                });
            })
        },
        dateTime: function (document) {
            //遍历 执行layui laydate生成dateTime
            $('[data-component="dateTime"]').each(function () {
                var date = '';
                if ($(this).val().length > 0 && $(this).val().length < 3) {
                    date = year + '-' + month + '-' + $(this).val() + ' ' + h + ':' + m + ':' + s;
                }
                laydate.render({
                    elem: this,
                    type: 'datetime',
                    value: date,
                    isInitValue: true
                });
            })
        }
    })

    $(document).ready(function () {

        //执行laydate实例
        $.date();
        $.dateTime();
    })
}(jQuery));