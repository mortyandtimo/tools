using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace IntelliCoreToolbox.Views
{
    /// <summary>
    /// A simple page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainPage : Page
    {
        int count = 0;
        private bool _isDarkTheme = false;

        // 🎨 主题颜色定义
        private readonly ThemeColors _lightTheme = new ThemeColors
        {
            PageBackground = Colors.LightBlue,
            TextForeground = Colors.DarkSlateGray,
            ButtonBackground = Colors.Red,
            ButtonBackgroundHover = Colors.DarkRed,
            ButtonBackgroundPressed = Colors.Crimson,
            ButtonForeground = Colors.White,
            ThemeIconColor = Colors.DarkSlateGray
        };

        private readonly ThemeColors _darkTheme = new ThemeColors
        {
            PageBackground = Colors.DarkSlateGray,
            TextForeground = Colors.LightGray,
            ButtonBackground = Colors.Orange,
            ButtonBackgroundHover = Colors.DarkOrange,
            ButtonBackgroundPressed = Colors.OrangeRed,
            ButtonForeground = Colors.Black,
            ThemeIconColor = Colors.LightGray
        };

        public MainPage()
        {
            this.InitializeComponent();
            
            // 初始化为浅色主题
            ApplyTheme(_lightTheme, "浅色");
        }

        private void OnCountClicked(object sender, RoutedEventArgs e)
            => txtCount.Text = $"Current count: {++count}";

        private void OnThemeToggleClicked(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;
            
            if (_isDarkTheme)
            {
                ApplyTheme(_darkTheme, "深色");
                // 深色主题时显示设置图标
                ThemeIcon.Glyph = "\uE771"; // 设置图标（更清晰）
            }
            else
            {
                ApplyTheme(_lightTheme, "浅色");
                // 浅色主题时使用相同图标，通过颜色区分
                ThemeIcon.Glyph = "\uE771"; // 设置图标（更清晰）
            }

            // 通知主窗口更新标题栏按钮颜色
            NotifyMainWindowThemeChanged();
        }

        private void ApplyTheme(ThemeColors theme, string themeName)
        {
            // 更新整个页面背景（包括标题栏区域）
            this.Background = new SolidColorBrush(theme.PageBackground);
            MainContentArea.Background = new SolidColorBrush(theme.PageBackground);
            TitleBarArea.Background = new SolidColorBrush(theme.PageBackground);
            
            // 更新文本颜色
            AppNameText.Foreground = new SolidColorBrush(theme.TextForeground);
            AppVersionText.Foreground = new SolidColorBrush(theme.TextForeground);
            ThemeStatusText.Text = $"当前主题: {themeName}";
            ThemeStatusText.Foreground = new SolidColorBrush(theme.TextForeground);
            
            // 更新主题图标颜色
            ThemeIcon.Foreground = new SolidColorBrush(theme.ThemeIconColor);
        }

        private void NotifyMainWindowThemeChanged()
        {
            // 简化的主窗口查找方式
            try
            {
                var mainWindow = (Application.Current as App)?.m_window;
                if (mainWindow != null)
                {
                    var currentTheme = _isDarkTheme ? _darkTheme : _lightTheme;
                    var themeColors = new MainWindow.ThemeColors
                    {
                        PageBackground = currentTheme.PageBackground,
                        TextForeground = currentTheme.TextForeground,
                        ButtonBackground = currentTheme.ButtonBackground,
                        ButtonBackgroundHover = currentTheme.ButtonBackgroundHover,
                        ButtonBackgroundPressed = currentTheme.ButtonBackgroundPressed,
                        ButtonForeground = currentTheme.ButtonForeground,
                        ThemeIconColor = currentTheme.ThemeIconColor
                    };
                    mainWindow.UpdateTitleBarButtonColors(themeColors);
                }
            }
            catch
            {
                // 如果获取主窗口失败，忽略标题栏颜色更新
            }
        }

        // 主题颜色数据结构
        private class ThemeColors
        {
            public Color PageBackground { get; set; }
            public Color TextForeground { get; set; }
            public Color ButtonBackground { get; set; }
            public Color ButtonBackgroundHover { get; set; }
            public Color ButtonBackgroundPressed { get; set; }
            public Color ButtonForeground { get; set; }
            public Color ThemeIconColor { get; set; }
        }
    }
}
