/**
 * sj popform 组件，用于初始化页面popform。 
 * 依赖 jquery3.3.1
 * @author jerem.wang
 * 
 */
;(function ($) {

    //#region 调用方法

    /** 注册事件 */
    function addEvent() {

        // 获取页面上所有的pop input
        var $input = $("[data-component='popform-input']");
        
        // 遍历挂载事件
        $input.each(function () {
            
            // 是否被禁用
            if ($(this).attr('readonly') !== 'readonly') {

                // input注册点击事件
                $(this).off('dblclick').dblclick(function () {

                    openWindow(this);
                });

                // 搜索小图标注册点击事件
                $(this).next().off('click').click(function () {

                    openWindow($(this).prev('input'));
                });
            }
        });
    }

    /**
     * 打开弹窗
     * @param {any} input input
     */
    function openWindow(input) {

        var that = input;
        var val = $(that).val();
        var url = $(that).attr('data-option-url');
        var param = $(that).attr('data-option-param');
        var title = $(that).attr('data-option-title');
        url = joinUrl(url + '?' + param, val);

        jQuery.dialog.open({
            width:450,
            url: url,
            title: title,
            messager: function (data) {
                // 设置值
                $(that).val(data);
            }
        });
    }

    /**
     * 组装url，进行传参
     * @param {String} url 请求链接
     * @param {String} val 需要传送的参数值
     * @returns {String} 组装后的url
     */
    function joinUrl(url, val) {

        url = sj.config.siteHost + url;

        if (url.indexOf('?') === -1) {
            url = url + '?popval=' + encodeURI(val);
        } else {
            url = url + '&popval=' + encodeURI(val);
        }

        return url;
    }
    
    //#endregion

    /** 设置表格上方搜索框的值 */
    function setVal() {

        // 获取页面上的gird-input
        var $input = $('[data-component="grid-input"]');

        //获取并设置 url参数
        $input.val(sj.util.getQueryString('popval'));

        //行双击事件
        $('[data-component="grid"]>tbody>tr').dblclick(function () {
            var code = $($(this).find('td')[0]).find('div').text().replace(/(^\s*)|(\s*$)/g, "");
            var name = $($(this).find('td')[1]).find('div').text().replace(/(^\s*)|(\s*$)/g, "");
            jQuery.dialog.popMessage(code + '.' + name);
        })
    }
    //#region 页面加载初始化页面控件

    $(function () {

        addEvent();

        setVal();
    });

    /** popform组件 */
    $.popTextBox = {};

    /** 初始化页面山的popform组件 */
    $.popTextBox.init = addEvent;
    
    //#endregion
    
})(jQuery)