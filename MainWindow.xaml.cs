using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;
using Windows.UI;

namespace IntelliCoreToolbox
{
    public sealed partial class MainWindow : Window
    {
        private AppWindow? _appWindow;
        
        // 主题颜色数据结构（与MainPage保持一致）
        public class ThemeColors
        {
            public Color PageBackground { get; set; }
            public Color TextForeground { get; set; }
            public Color ButtonBackground { get; set; }
            public Color ButtonBackgroundHover { get; set; }
            public Color ButtonBackgroundPressed { get; set; }
            public Color ButtonForeground { get; set; }
            public Color ThemeIconColor { get; set; }
        }

        public MainWindow()
        {
            this.InitializeComponent();

            // 🔥 Step 1: 基础设置 - 告诉XAML内容要延伸到标题栏
            this.ExtendsContentIntoTitleBar = true;

            // 获取AppWindow对象，这是WinUI 3中控制窗口属性的核心
            _appWindow = GetAppWindowForCurrentWindow();

            if (_appWindow != null)
            {
                // 🔥 Step 2: 隐藏最小化和最大化按钮，只保留关闭按钮
                if (_appWindow.Presenter is OverlappedPresenter presenter)
                {
                    presenter.IsMinimizable = false;  // 隐藏最小化按钮
                    presenter.IsMaximizable = false;  // 隐藏最大化按钮
                    presenter.IsResizable = true;     // 保持窗口可调整大小
                }

                // 🔥 Step 3 & 4: 标题栏透明化 + 深度自定义关闭按钮颜色
                if (_appWindow.TitleBar != null)
                {
                    var titleBar = _appWindow.TitleBar;
                    
                    // 确保标题栏区域可以被我们的内容交互
                    titleBar.ExtendsContentIntoTitleBar = true;
                    
                    // 设置标题栏背景为完全透明，让我们的XAML背景透出来
                    titleBar.BackgroundColor = Colors.Transparent;
                    titleBar.InactiveBackgroundColor = Colors.Transparent;
                    
                    // 🎨 深度自定义关闭按钮的颜色主题
                    // 正常状态：深色按钮，白色X图标
                    titleBar.ButtonBackgroundColor = Colors.Transparent;        // 按钮背景透明
                    titleBar.ButtonForegroundColor = Colors.DarkSlateGray;      // X符号为深灰色
                    
                    // 悬浮状态：红色背景，白色X图标（经典Windows风格）
                    titleBar.ButtonHoverBackgroundColor = Colors.Red;          // 悬浮时红色背景
                    titleBar.ButtonHoverForegroundColor = Colors.White;        // 悬浮时白色X符号
                    
                    // 按下状态：深红色背景，白色X图标
                    titleBar.ButtonPressedBackgroundColor = Colors.DarkRed;    // 按下时深红色
                    titleBar.ButtonPressedForegroundColor = Colors.White;      // 按下时白色X符号
                    
                    // 窗口非活动状态的按钮颜色
                    titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                    titleBar.ButtonInactiveForegroundColor = Colors.Gray;      // 非活动时灰色X符号
                    
                    // （可选）隐藏系统图标和菜单
                    try
                    {
                        titleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
                    }
                    catch { /* 在某些系统状态下可能失败，忽略即可 */ }
                }

                // 设置窗口初始大小
                _appWindow.Resize(new Windows.Graphics.SizeInt32(800, 600));
            }

            // （可选）移除Mica/Acrylic等系统背景，使用纯色背景
            try
            {
                this.SystemBackdrop = null;
            }
            catch { /* 忽略异常 */ }

            // 🔥 Step 5: 定义可拖拽区域 - 在窗口激活后执行
            this.Activated += MainWindow_Activated;
            
            // 导航到主页面
            MainFrame.Navigate(typeof(IntelliCoreToolbox.Views.MainPage));
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs e)
        {
            // 只在第一次激活时设置拖拽区域
            if (e.WindowActivationState != WindowActivationState.Deactivated)
            {
                SetupDraggableRegion();
                // 取消订阅，避免重复调用
                this.Activated -= MainWindow_Activated;
            }
        }

        private void SetupDraggableRegion()
        {
            if (_appWindow?.TitleBar != null)
            {
                // 获取当前窗口的客户区尺寸
                var clientSize = _appWindow.ClientSize;
                
                // 标题栏的高度通常是32像素（在标准DPI下）
                int titleBarHeight = 32;
                
                // 关闭按钮的宽度通常是46像素
                int closeButtonWidth = 46;
                
                // 左侧内容宽度（软件名 + 版本 + 主题按钮）约160像素
                int leftContentWidth = 160;
                
                // 定义可拖拽区域：左侧内容后面到关闭按钮前面的区域
                var dragRect = new Windows.Graphics.RectInt32(
                    leftContentWidth,                           // X: 从左侧内容后开始
                    0,                                          // Y: 从顶部开始  
                    clientSize.Width - leftContentWidth - closeButtonWidth, // Width: 中间空白区域
                    titleBarHeight                              // Height: 标题栏高度
                );
                
                _appWindow.TitleBar.SetDragRectangles(new[] { dragRect });
            }
        }

        /// <summary>
        /// 更新标题栏按钮颜色以匹配当前主题
        /// </summary>
        public void UpdateTitleBarButtonColors(ThemeColors theme)
        {
            if (_appWindow?.TitleBar != null)
            {
                var titleBar = _appWindow.TitleBar;
                
                // 更新关闭按钮颜色以匹配主题
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonForegroundColor = theme.TextForeground;
                
                // 悬浮状态使用主题的按钮颜色
                titleBar.ButtonHoverBackgroundColor = theme.ButtonBackground;
                titleBar.ButtonHoverForegroundColor = theme.ButtonForeground;
                
                // 按下状态使用更深的颜色
                titleBar.ButtonPressedBackgroundColor = theme.ButtonBackgroundPressed;
                titleBar.ButtonPressedForegroundColor = theme.ButtonForeground;
                
                // 非活动状态
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveForegroundColor = Colors.Gray;
            }
            
            // 更新MainWindow的背景颜色
            if (Content is Grid mainGrid)
            {
                mainGrid.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(theme.PageBackground);
            }
        }

        private void OnNavigationFailed(object sender, Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }
    }
}