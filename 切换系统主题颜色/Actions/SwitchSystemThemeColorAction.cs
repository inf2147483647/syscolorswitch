using System;
using System.Threading.Tasks;
using ClassIsland.Core; // 新增，用于访问AppBase及相关UI方法
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Abstractions.Automation; // 新版ActionBase所在命名空间
using ClassIsland.Core.Attributes;
using FluentAvalonia.UI.Controls; // 新增，用于访问TaskDialog弹窗控件
using 切换系统主题颜色.Models;

namespace 切换系统主题颜色.Actions;

[ActionInfo(
    "switchsystemthemecolor.action", 
    "切换系统主题颜色", 
    "\uE790" // Fluent UI 颜色图标
)]
public class SwitchSystemThemeColorAction : ActionBase<SwitchSystemThemeColorSettings>
{
    protected override async Task OnInvoke()
    {
        try
        {
            // 调用API修改系统主题颜色
            SystemThemeHelper.SetAccent(Settings.TargetColor);
        }
        catch (Exception ex)
        {
            // 错误处理：2.0版本已移除旧的ShowAlertAsync，使用TaskDialog实现弹窗提示
            var dialog = new TaskDialog()
            {
                Title = "修改系统主题颜色失败",
                Content = ex.Message,
                XamlRoot = AppBase.Current.GetRootWindow(),
                Buttons = { new TaskDialogButton("确定", true) { IsDefault = true } }
            };
            await dialog.ShowAsync();
        }
        await Task.CompletedTask;
    }
}