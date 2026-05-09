using Avalonia.Media;
namespace 切换系统主题颜色.Models;

public class SwitchSystemThemeColorSettings
{
    /// <summary>
    /// 要切换到的系统主题颜色
    /// </summary>
    public Color TargetColor { get; set; } = Color.FromRgb(0, 120, 215); // 默认Windows蓝色
}