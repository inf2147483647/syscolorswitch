using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Media;
namespace 切换系统主题颜色;
/// <summary>
/// 系统主题颜色帮助类，使用PowerShell修改注册表实现，确保所有颜色完整生效
/// 兼容所有Windows版本，解决UWP和窗口边框颜色未生效的问题
/// 优化：异步实现，避免阻塞UI线程
/// </summary>
internal static class SystemThemeHelper
{
    /// <summary>
    /// 设置系统强调色，通过PowerShell修改注册表实现
    /// </summary>
    /// <param name="color">目标颜色</param>
    /// <exception cref="System.ComponentModel.Win32Exception">当操作失败时抛出</exception>
    public static async Task SetAccent(Color color)
    {
        // 计算BGR格式的颜色值
        uint bgrColor = (uint)((color.B << 16) | (color.G << 8) | color.R);
        string bgrHex = $"0x{bgrColor:X8}";
        
        // 生成颜色板的字节数组，全部使用用户的原始颜色
        byte[] accentPalette = new byte[32];
        for (int i = 0; i < 8; i++)
        {
            accentPalette[i * 4 + 0] = color.R;
            accentPalette[i * 4 + 1] = color.G;
            accentPalette[i * 4 + 2] = color.B;
            accentPalette[i * 4 + 3] = 0xFF;
        }
        
        // 把字节数组转换成PowerShell能识别的格式：@(0xXX, 0xXX, ...)
        string paletteBytes = string.Join(", ", accentPalette.Select(b => $"0x{b:X2}"));
        try
        {
            // 构建PowerShell命令，通过命令行修改所有注册表配置
            string psCommand = @"
                # 修改Explorer的颜色配置
                Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Accent' -Name 'AccentPalette' -Value ([byte[]] @(" + paletteBytes + @")) -Force;
                Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Accent' -Name 'AccentColor' -Value " + bgrHex + @" -Force;
                Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Accent' -Name 'AutoColor' -Value 0 -Force;
                Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Accent' -Name 'ColorSetName' -Value 'Custom' -Force;
                
                # 修改Themes的颜色配置
                Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize' -Name 'AccentColor' -Value " + bgrHex + @" -Force;
                
                # 修改DWM的颜色配置，解决UWP和窗口边框颜色
                $colorization = [int](" + bgrHex + @" -bor 0xFF000000);
                Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\DWM' -Name 'ColorizationColor' -Value $colorization -Force;
                Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\DWM' -Name 'ColorizationColorBalance' -Value 100 -Force;
                Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\DWM' -Name 'AccentColor' -Value " + bgrHex + @" -Force;
                
                # 通知系统设置变更
                Add-Type -TypeDefinition @'
                using System;
                using System.Runtime.InteropServices;
                public class Win32 {
                    [DllImport(""user32.dll"", CharSet = CharSet.Unicode)]
                    public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);
                }
'@;
                [Win32]::SendMessageTimeout([IntPtr]0xffff, 0x001A, [IntPtr]::Zero, ""ImmersiveColorSet"", 0x0002, 1000, [ref] [IntPtr]::Zero);
                
                # 重启Explorer，保证所有界面更新
                Stop-Process -Name explorer -Force;
            ";
            // 启动PowerShell进程执行命令
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{psCommand.Replace("\"", "\\\"")}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            // 异步等待进程退出，不阻塞UI线程
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                // 异步读取错误输出
                string error = await process.StandardError.ReadToEndAsync();
                throw new Exception($"PowerShell命令执行失败: {error}");
            }
        }
        catch (Exception ex)
        {
            throw new System.ComponentModel.Win32Exception(ex.Message, ex);
        }
    }
}