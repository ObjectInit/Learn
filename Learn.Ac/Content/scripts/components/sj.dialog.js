/*
 * 说明：shiji dialog 模块 包括弹出iframe窗口 dialog对话框
 * 依赖： jquery3.3.1  layer3.1.1  
 * 作者：luming
 * 日期：2019-04-10
 * 修改人：zcc
 * 修改日期：2019-04-23
 * 修改内容：dialog新增布局方式:参数：flex：row/col 布局方式，不传视为模态，模态情况下无最大最小化（98行判断）
 *          closeBtn：0/1 默认：1 显示关闭按钮
 *          shade:遮罩层透明度，设置为0 不显示遮罩层 默认0.3
 *          moveOut 移动
 *          maxmin 最大最小化
 * */
;(function (jQuery) {
    // 暴露给外部的对象
    var t = {};

    // 弹出层坐标位置
    var offset = [];
    var offsetTop = 0;
    var offsetLeft = 0;

    // 弹出层索引
    var _index = 1;
    /* 
     * layer 全局配置
     * 更多配置项查看layer api
     */
    layer.config({
        skin: 'sj-dialog',

    });

    /**
     * 弹出框
     * @param {any} options 弹框配置项
     * width 宽度 默认800
     * height 高度 默认600
     * title 标题  默认''
     * success 打开后的回调
     * close 点击关闭按钮的回调
     * messager 数据回调
     * dbCallback title双击回调（自定义）
     * moveEnd 拖动结束回调
     */
    t.open = function (options) {
        var opts = {
            url: '',
            width: 300,
            height: 200,
            title: '',
            parms: [{}],
            closeBtn: 1,
            shade: 0.3,
            tabs:false,
            moveOut: true,
            maxmin: true,
            success: function () { },
            close: function () { },
            moveEnd: function () { },
            dbCallback: function () { },
            message: function () { }
        }

        //方法赋值，点击时return给外部调用
        if (options == 'area') {
            return $.extend(opts, options);
        }

        // 覆盖配置项
        opts = $.extend(opts, options);
        if (!opts.url) {
            console.error('未设置popform地址');
            return;
        }

        // 随机数，作为当前窗口唯一标识符
        var random = Math.ceil(Math.random() * 10000000);
        // 把窗口的回调挂载到window对象中
        window.top.dialogCallback = window.top.dialogCallback || {};
        window.top.dialogCallback['dialog' + random] = opts.messager;

        for (var i = 0; i < opts.parms.length; i++) {
            var param = opts.url.substring(opts.url.indexOf('?')+1, opts.url.length);

            //去掉url后面的参数 将打开的url参数后添加当前打开窗口参数
            if (opts.url.indexOf('?') > -1) {
                opts.url = opts.url.substring(0, opts.url.indexOf('?'));
            }
            if (JSON.stringify(opts.parms[i]) == '{}') {
                opts.url = opts.url + '?dialogCallback=dialog' + random + '&' + param
            } else {
                for (let key in opts.parms[i]) {
                    opts.url = opts.url + '?dialogCallback=dialog' + random + '&' + key + '=' + opts.parms[i][key] + '&' + param;
                }
            }
            
            //设置弹出层坐标
            if (opts.flex == 'row') {
                offsetLeft = offsetLeft + opts.width;
                if (offsetLeft - opts.width > jQuery(window).width()) {
                    offsetTop = offsetTop + opts.height;
                    offsetLeft = opts.width;
                }
                offset[0] = offsetTop + 'px';
                offset[1] = offsetLeft - opts.width + 'px';
            } else if (opts.flex == 'col') {
                offsetTop = offsetTop + 36;
                offset = offsetTop;
            } else {
                opts.maxmin = false;
            }

            // 在顶层window打开框
            window.top.layer.open({
                type: 2,
                title: opts.title,
                area: [opts.width + 'px', opts.height + 'px'], //宽高
                content: opts.url,
                shade: opts.shade,
                moveOut: opts.moveOut,
                tabs:opts.tabs,
                maxmin: opts.maxmin,
                offset: offset,
                success: function (layero, index) {

                    //第几个弹出层
                    _index = _index + index

                    opts.success(layero, index);
                },
                cancel: function (index, layero) {

                    //最后一个弹出层则把坐标设置为0
                    _index = _index - index;
                    if (_index == 1) {
                        offsetTop = 0;
                        offsetLeft = 0;
                    }

                    opts.close(layero, index);
                },
                moveEnd: function (layero) {
                    opts.moveEnd(layero);
                },
                dbCallback: function (layero) {
                    opts.dbCallback(layero);
                }
            });
        }
    };

    /**
     * 提示消息
     * @param {any} 消息类型 错误提示error、普通提示info
     * @param {any} 消息内容
     */
    t.alert = function (type, message, title) {
        if (type == 'info') {
            layer.alert(message, { icon: 1, title: title });
        } else if (type == 'error') {
            layer.alert(message, { icon: 2, title: title });
        }

    };

    /**
     * 确认提示 
     * @param {any} 消息内容
     * @param {any} 确定的回调
     * @param {any} 取消的回调
     */
    t.confirm = function (message, title, yesCallback) {
        layer.confirm(message, { icon: 3, title: title }, function (index) {
            yesCallback(index);
            layer.close(index);
        })
    }

    /**
     * popform传值
     * @param {any} data 数据
     */
    t.popMessage = function (data) {

        window.top.dialogCallback[sj.util.getQueryString('dialogCallback')](data);

        this.close();
    }

    /** 
     * 获取popinput传过来的值 
     * @returns {String} 值
     */
    t.getParentMessage = function () {

        var val = sj.util.getQueryString('popval');

        // 如果存在"."，则取"."之前的代码，否则取全部。
        return val.split('.')[0];
    }

    /** 
     * 关闭方法，内部关闭当前窗口
     * 调用$.close()
     */
    t.close = function () {
        window.top.layer.close(window.top.layer.getFrameIndex(window.name));
    }

    /** 
     * 关闭方法，关闭当前窗口
     * 调用$.close()
     */
    t.closeNow = function (_index) {
        window.top.layer.close(_index);
    }
    // 挂载到jQuery
    $.dialog = t;


}(jQuery));