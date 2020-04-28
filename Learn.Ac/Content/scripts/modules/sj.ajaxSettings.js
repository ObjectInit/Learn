/*
 * 说明：Ajax请求全局设置
 * 依赖：Jquery3.3.1
 * 作者：张耀
 * 日期：2019-05-15
 * */
(function (window, $) {

    var errorTitle; 
  
    $.ajaxSetup({
        //关闭ajax缓存
        cache: false,
        // 请求开始之前调用
        beforeSend: function () {},
        // 请求完成结束调用
        error: function (jqXhr) {
            // 响应状态码
            var status = jqXhr.status;

            // 无权限，提示用户无权限操作
            if (status === 401) {
                $.dialog.alert('error', jqXhr.responseJSON.Message, '提示');
                return;
            }

            // 服务器发生错误
            if (status === 500) {
                $.dialog.alert('error', jqXhr.responseJSON.Message, '提示');
            }

            // 身份会话失效，重新登录
            if (status === 403) {
                //$.dialog.alert('error', '身份失效，请重新登录', '提示');
                //return;

                $.dialog.open({
                    url: sj.config.siteHost + '/Authentication/Modal',
                    title: 'Login',
                    width: 300,
                    height: 350,
                    shade: 0, 
                    success: function (a, b) { 
                    },
                    cancel: function (a, b) { 
                    },
                    close: function () {
                        //下面这段是刷新当前iframe框 在登录之后调用
                        //var _win = top.window;
                        //var currTab = _win.$('[data-sj-component="tabs-page-header"]').tabs('getSelected');    //获取选中的标签项
                        //var url = $(currTab.panel('options').content).attr('src');    //获取该选项卡中内容标签（iframe）的 src 属性  
                        //if (url != null) {
                        //    /* 重新设置该标签 */
                        //    _win.$('[data-sj-component="tabs-page-header"]').tabs('update', {
                        //        tab: currTab,
                        //        options: {
                        //            content: sj.util.createFrame(url),
                        //        }
                        //    })
                        //} 
                        //$.dialog.close();
                    }
                });
            }
        }
    });
}(window,$))