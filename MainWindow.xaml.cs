using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using WinRT.Interop;
using Windows.UI;

namespace IntelliCoreToolbox
{
    public sealed partial class MainWindow : Window
    {
        private AppWindow? _appWindow;
        private bool _isDarkTheme = false;
        private Button? _currentActiveButton = null;
        
        // 🎨 主题颜色定义
        private readonly ThemeColors _lightTheme = new ThemeColors
        {
            PageBackground = Color.FromArgb(255, 250, 250, 250),   // 主界面：白色系
            TextForeground = Colors.DarkSlateGray,
            ButtonBackground = Colors.DodgerBlue,
            ButtonBackgroundHover = Colors.RoyalBlue,
            ButtonBackgroundPressed = Colors.MediumBlue,
            ButtonForeground = Colors.White,
            ThemeIconColor = Colors.DarkSlateGray,
            SidebarBackground = Color.FromArgb(255, 240, 240, 240), // 侧边栏：浅灰色
            SidebarButtonForeground = Color.FromArgb(255, 80, 80, 80),
            TitleBarBackground = Color.FromArgb(255, 230, 230, 230) // 标题栏：更浅的灰色
        };

        private readonly ThemeColors _darkTheme = new ThemeColors
        {
            PageBackground = Color.FromArgb(255, 55, 55, 58),      // 主界面：原来的浅色（灰色系）
            TextForeground = Colors.LightGray,
            ButtonBackground = Colors.Orange,
            ButtonBackgroundHover = Colors.DarkOrange,
            ButtonBackgroundPressed = Colors.OrangeRed,
            ButtonForeground = Colors.Black,
            ThemeIconColor = Colors.LightGray,
            SidebarBackground = Color.FromArgb(255, 45, 45, 48),   // 侧边栏：中间灰色
            SidebarButtonForeground = Color.FromArgb(255, 224, 224, 224),
            TitleBarBackground = Color.FromArgb(255, 35, 35, 38)   // 标题栏：比侧边栏略深的灰色
        };

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
            public Color SidebarBackground { get; set; }
            public Color SidebarButtonForeground { get; set; }
            public Color TitleBarBackground { get; set; }
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
                // 🔥 Step 2: 隐藏最小化和最大化按钮，只保留关闭按钮，禁用双击全屏
                if (_appWindow.Presenter is OverlappedPresenter presenter)
                {
                    presenter.IsMinimizable = false;  // 隐藏最小化按钮
                    presenter.IsMaximizable = false;  // 隐藏最大化按钮
                    presenter.IsResizable = true;     // 保持窗口可调整大小
                    
                    // 禁用双击标题栏全屏/还原功能
                    try
                    {
                        // 通过设置最大化状态为禁用来阻止双击全屏
                        presenter.SetBorderAndTitleBar(true, true);
                    }
                    catch { /* 忽略可能的异常 */ }
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
            
            // 添加导航完成事件处理，确保新页面也应用主题
            ContentFrame.Navigated += ContentFrame_Navigated;
            
            // 初始化主题
            ApplyTheme(_lightTheme, "白色");
            
            // 导航到主页面（不设置任何按钮为激活状态，因为HomePage是独立的）
            ContentFrame.Navigate(typeof(IntelliCoreToolbox.Views.HomePage));
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
        }

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
                ApplyTheme(_lightTheme, "白色");
                // 白色主题时使用相同图标，通过颜色区分
                ThemeIcon.Glyph = "\uE771"; // 设置图标（更清晰）
            }
        }

        private void ApplyTheme(ThemeColors theme, string themeName)
        {
            // 更新根Grid和标题栏背景
            RootGrid.Background = new SolidColorBrush(theme.TitleBarBackground);
            TitleBarArea.Background = new SolidColorBrush(theme.TitleBarBackground);
            
            // 更新标题栏文本颜色
            AppNameText.Foreground = new SolidColorBrush(theme.TextForeground);
            AppVersionText.Foreground = new SolidColorBrush(theme.TextForeground);
            ThemeIcon.Foreground = new SolidColorBrush(theme.ThemeIconColor);
            
            // 更新侧边栏
            SidebarGrid.Background = new SolidColorBrush(theme.SidebarBackground);
            
            // 更新侧边栏按钮颜色
            var sidebarButtons = new[] { SearchButton, AppCenterButton, SnippetsButton, HotkeysButton, SettingsButton };
            foreach (var button in sidebarButtons)
            {
                // 只更新非激活状态的按钮
                if (button != _currentActiveButton)
                {
                    button.Foreground = new SolidColorBrush(theme.SidebarButtonForeground);
                    button.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
            
            // 如果有激活的按钮，更新其颜色
            if (_currentActiveButton != null)
            {
                SetButtonActiveState(_currentActiveButton, true);
            }
            
            // 更新主题状态文本
            ThemeStatusText.Text = $"当前主题: {themeName}";
            
            // 更新内容Frame的背景
            if (ContentFrame.Content is Page currentPage)
            {
                currentPage.Background = new SolidColorBrush(theme.PageBackground);
            }
            
            // 更新标题栏按钮颜色
            UpdateTitleBarButtonColors(theme);
        }

        private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            // 每当导航到新页面时，只更新页面主题，不重新应用整体主题
            var currentTheme = _isDarkTheme ? _darkTheme : _lightTheme;
            var themeName = _isDarkTheme ? "深色" : "白色";
            
            if (ContentFrame.Content is Page currentPage)
            {
                currentPage.Background = new SolidColorBrush(currentTheme.PageBackground);
                
                // 如果页面有特定的主题元素，可以在这里更新
                UpdatePageThemeElements(currentPage, currentTheme);
            }
            
            // 更新侧边栏主题状态文本
            ThemeStatusText.Text = $"当前主题: {themeName}";
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

        // 🎯 侧边栏导航事件处理器
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(IntelliCoreToolbox.Views.SearchPage), SearchButton);
        }

        private void AppCenterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(IntelliCoreToolbox.Views.AppCenterPage), AppCenterButton);
        }

        private void SnippetsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(IntelliCoreToolbox.Views.SnippetsPage), SnippetsButton);
        }

        private void HotkeysButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(IntelliCoreToolbox.Views.HotkeyManagerPage), HotkeysButton);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(IntelliCoreToolbox.Views.SettingsPage), SettingsButton);
        }

        // 🎯 导航和状态管理方法
        private void NavigateToPage(Type pageType, Button activeButton)
        {
            // 如果点击的是当前已激活的按钮，不做任何操作
            if (activeButton == _currentActiveButton)
            {
                return;
            }
            
            // 导航到指定页面
            ContentFrame.Navigate(pageType);
            
            // 更新按钮选中状态
            UpdateButtonActiveState(activeButton);
        }

        private void UpdateButtonActiveState(Button activeButton)
        {
            // 重置所有按钮状态
            ResetAllButtonStates();
            
            // 设置当前按钮为激活状态
            SetButtonActiveState(activeButton, true);
            _currentActiveButton = activeButton;
        }

        private void ResetAllButtonStates()
        {
            var sidebarButtons = new[] { SearchButton, AppCenterButton, SnippetsButton, HotkeysButton, SettingsButton };
            foreach (var button in sidebarButtons)
            {
                SetButtonActiveState(button, false);
            }
        }

        private void SetButtonActiveState(Button button, bool isActive)
        {
            var currentTheme = _isDarkTheme ? _darkTheme : _lightTheme;
            
            if (isActive)
            {
                // 激活状态：使用主题的按钮颜色
                button.Background = new SolidColorBrush(currentTheme.ButtonBackground);
                button.Foreground = new SolidColorBrush(currentTheme.ButtonForeground);
            }
            else
            {
                // 默认状态：透明背景，侧边栏前景色
                button.Background = new SolidColorBrush(Colors.Transparent);
                button.Foreground = new SolidColorBrush(currentTheme.SidebarButtonForeground);
            }
        }

         // 🎨 更新页面主题元素
         private void UpdatePageThemeElements(Page page, ThemeColors theme)
         {
             switch (page)
             {
                 case IntelliCoreToolbox.Views.HomePage homePage:
                     if (homePage.FindName("HomePageTitle") is TextBlock homeTitle)
                         homeTitle.Foreground = new SolidColorBrush(theme.TextForeground);
                     break;

                 case IntelliCoreToolbox.Views.SearchPage searchPage:
                     if (searchPage.FindName("PageTitle") is TextBlock searchTitle)
                         searchTitle.Foreground = new SolidColorBrush(theme.TextForeground);
                     break;

                 case IntelliCoreToolbox.Views.AppCenterPage appCenterPage:
                     if (appCenterPage.FindName("PageTitle") is TextBlock appCenterTitle)
                         appCenterTitle.Foreground = new SolidColorBrush(theme.TextForeground);
                     break;

                 case IntelliCoreToolbox.Views.SnippetsPage snippetsPage:
                     if (snippetsPage.FindName("PageTitle") is TextBlock snippetsTitle)
                         snippetsTitle.Foreground = new SolidColorBrush(theme.TextForeground);
                     break;

                 case IntelliCoreToolbox.Views.HotkeyManagerPage hotkeyPage:
                     if (hotkeyPage.FindName("PageTitle") is TextBlock hotkeyTitle)
                         hotkeyTitle.Foreground = new SolidColorBrush(theme.TextForeground);
                     break;

                 case IntelliCoreToolbox.Views.SettingsPage settingsPage:
                     if (settingsPage.FindName("PageTitle") is TextBlock settingsTitle)
                         settingsTitle.Foreground = new SolidColorBrush(theme.TextForeground);
                     break;
             }
         }
    }
}