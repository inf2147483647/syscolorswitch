using ClassIsland.Core;
using ClassIsland.Core.Abstractions; // 新增，用于访问PluginBase
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Extensions.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using 切换系统主题颜色.Controls.ActionSettingsControls;
using 切换系统主题颜色.Actions;
using 切换系统主题颜色.Models;

namespace 切换系统主题颜色;

public class Plugin : PluginBase
{
    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        // 注册自定义行动
        services.AddAction<SwitchSystemThemeColorAction, SwitchSystemThemeColorActionSettingsControl>();
    }
}