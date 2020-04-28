/**
 *  快捷键模块
 */
(function (window, $) {
    $(function () {
        /** 绑定快捷键 */
        $('body').keycode({
            onkeydown: function (keyCode) {
                if (keyCode == 65) {
                    console.log('新增')
                } else if (keyCode == 68) {
                    console.log('删除')
                } else if (keyCode == 83) {
                    console.log('保存')
                } else if (keyCode == 88) {
                    console.log('打印')
                } else if (keyCode == 71) {
                    console.log('刷新')
                } else if (keyCode == 76) {
                    console.log('导入')
                } else if (keyCode == 69) {
                    console.log('导出')
                }
            }
        });
    });
}(window, $))