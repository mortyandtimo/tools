using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Dispatching;
using System.Collections.Generic;

namespace IntelliCoreToolbox.Views
{
    // 🎯 数据模型：工具箱应用项
    public class ToolboxItem
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Background { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
    }

    // 🎯 数据模型：应用合集
    public class AppCollection
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Background { get; set; }
        public string AppCount { get; set; }
        public ObservableCollection<ToolboxItem> Apps { get; set; } = new ObservableCollection<ToolboxItem>();
    }

    // 🎯 ViewModel：应用中心页面
    public class AppCenterViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ToolboxItem> FavoriteApps { get; set; }
        public ObservableCollection<ToolboxItem> AllApps { get; set; }
        public ObservableCollection<AppCollection> Collections { get; set; }
        public ObservableCollection<ToolboxItem> LoopingFavoriteApps { get; set; }

        public AppCenterViewModel()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            // 初始化收藏应用
            FavoriteApps = new ObservableCollection<ToolboxItem>
            {
                new ToolboxItem { Name = "Visual Studio Code", Icon = "VS", Background = "LightBlue", Description = "代码编辑器" },
                new ToolboxItem { Name = "Notepad++", Icon = "N++", Background = "Orange", Description = "文本编辑器" },
                new ToolboxItem { Name = "Git", Icon = "Git", Background = "Green", Description = "版本控制" },
                new ToolboxItem { Name = "Discord", Icon = "DC", Background = "Purple", Description = "聊天工具" },
                new ToolboxItem { Name = "Chrome", Icon = "Chr", Background = "Red", Description = "浏览器" },
                new ToolboxItem { Name = "Postman", Icon = "PM", Background = "DarkBlue", Description = "API测试" },
                new ToolboxItem { Name = "Docker", Icon = "Doc", Background = "Teal", Description = "容器平台" },
                new ToolboxItem { Name = "Figma", Icon = "Fig", Background = "Brown", Description = "设计工具" },
                // 为了实现无限滚动，添加更多项目
                new ToolboxItem { Name = "Unity", Icon = "Uni", Background = "DarkSlateBlue", Description = "游戏引擎" },
                new ToolboxItem { Name = "Blender", Icon = "Bln", Background = "DarkOrange", Description = "3D建模" },
                new ToolboxItem { Name = "Photoshop", Icon = "PS", Background = "DarkCyan", Description = "图像编辑" },
                new ToolboxItem { Name = "IntelliJ", Icon = "IJ", Background = "Maroon", Description = "Java IDE" }
            };

            LoopingFavoriteApps = new ObservableCollection<ToolboxItem>();
            CreateLoopingFavorites();

            // 初始化全部应用
            AllApps = new ObservableCollection<ToolboxItem>
            {
                new ToolboxItem { Name = "Visual Studio", Icon = "&#xE943;", Background = "Blue", Description = "IDE开发环境" },
                new ToolboxItem { Name = "Notepad++", Icon = "&#xE8A5;", Background = "Orange", Description = "文本编辑器" },
                new ToolboxItem { Name = "Chrome", Icon = "&#xE774;", Background = "Red", Description = "网页浏览器" },
                new ToolboxItem { Name = "Git", Icon = "&#xE8AB;", Background = "Green", Description = "版本控制工具" },
                new ToolboxItem { Name = "Discord", Icon = "&#xE8BD;", Background = "Purple", Description = "聊天通讯工具" },
                new ToolboxItem { Name = "Postman", Icon = "&#xE968;", Background = "DarkBlue", Description = "API开发测试" },
                new ToolboxItem { Name = "Docker", Icon = "&#xE8C7;", Background = "DarkCyan", Description = "容器化平台" },
                new ToolboxItem { Name = "Slack", Icon = "&#xE8F2;", Background = "DarkMagenta", Description = "团队协作" },
                new ToolboxItem { Name = "VS Code", Icon = "&#xE943;", Background = "LightBlue", Description = "轻量级编辑器" },
                new ToolboxItem { Name = "Figma", Icon = "&#xE8EF;", Background = "Brown", Description = "UI设计工具" },
                new ToolboxItem { Name = "Adobe XD", Icon = "&#xE8F0;", Background = "Crimson", Description = "原型设计" },
                new ToolboxItem { Name = "Sketch", Icon = "&#xE8F1;", Background = "Gold", Description = "界面设计" },
                new ToolboxItem { Name = "Notion", Icon = "&#xE8A5;", Background = "Gray", Description = "笔记协作" }
            };

            // 初始化合集
            Collections = new ObservableCollection<AppCollection>
            {
                new AppCollection 
                { 
                    Name = "设计工具", 
                    Icon = "🎨", 
                    Background = "CornflowerBlue", 
                    AppCount = "8 个应用",
                    Apps = new ObservableCollection<ToolboxItem>
                    {
                        new ToolboxItem { Name = "Figma", Icon = "&#xE8EF;", Background = "Brown", Description = "UI设计工具" },
                        new ToolboxItem { Name = "Adobe XD", Icon = "&#xE8F0;", Background = "Crimson", Description = "原型设计" },
                        new ToolboxItem { Name = "Sketch", Icon = "&#xE8F1;", Background = "Gold", Description = "界面设计" },
                        new ToolboxItem { Name = "Photoshop", Icon = "&#xE91B;", Background = "DarkCyan", Description = "图像编辑" },
                        new ToolboxItem { Name = "Illustrator", Icon = "&#xE91C;", Background = "Orange", Description = "矢量绘图" },
                        new ToolboxItem { Name = "Canva", Icon = "&#xE8EF;", Background = "Green", Description = "在线设计" },
                        new ToolboxItem { Name = "Blender", Icon = "&#xE7F8;", Background = "DarkOrange", Description = "3D建模" },
                        new ToolboxItem { Name = "GIMP", Icon = "&#xE91B;", Background = "Purple", Description = "免费图像编辑" }
                    }
                },
                new AppCollection 
                { 
                    Name = "开发工具", 
                    Icon = "💻", 
                    Background = "ForestGreen", 
                    AppCount = "12 个应用",
                    Apps = new ObservableCollection<ToolboxItem>
                    {
                        new ToolboxItem { Name = "Visual Studio", Icon = "&#xE943;", Background = "Blue", Description = "IDE开发环境" },
                        new ToolboxItem { Name = "VS Code", Icon = "&#xE943;", Background = "LightBlue", Description = "轻量级编辑器" },
                        new ToolboxItem { Name = "IntelliJ IDEA", Icon = "&#xE943;", Background = "Maroon", Description = "Java IDE" },
                        new ToolboxItem { Name = "Git", Icon = "&#xE8AB;", Background = "Green", Description = "版本控制工具" },
                        new ToolboxItem { Name = "Docker", Icon = "&#xE8C7;", Background = "DarkCyan", Description = "容器化平台" },
                        new ToolboxItem { Name = "Postman", Icon = "&#xE968;", Background = "DarkBlue", Description = "API开发测试" }
                    }
                },
                new AppCollection 
                { 
                    Name = "游戏娱乐", 
                    Icon = "🎮", 
                    Background = "Crimson", 
                    AppCount = "5 个应用",
                    Apps = new ObservableCollection<ToolboxItem>
                    {
                        new ToolboxItem { Name = "Steam", Icon = "&#xE8C1;", Background = "DarkBlue", Description = "游戏平台" },
                        new ToolboxItem { Name = "Discord", Icon = "&#xE8BD;", Background = "Purple", Description = "游戏聊天" },
                        new ToolboxItem { Name = "Unity", Icon = "&#xE7F8;", Background = "DarkSlateBlue", Description = "游戏引擎" },
                        new ToolboxItem { Name = "OBS Studio", Icon = "&#xE714;", Background = "DarkGreen", Description = "直播录制" },
                        new ToolboxItem { Name = "Twitch", Icon = "&#xE8BD;", Background = "MediumPurple", Description = "直播平台" }
                    }
                },
                new AppCollection 
                { 
                    Name = "系统工具", 
                    Icon = "🔧", 
                    Background = "DarkOrange", 
                    AppCount = "15 个应用",
                    Apps = new ObservableCollection<ToolboxItem>
                    {
                        new ToolboxItem { Name = "7-Zip", Icon = "&#xE8B5;", Background = "DarkBlue", Description = "压缩解压" },
                        new ToolboxItem { Name = "Everything", Icon = "&#xE721;", Background = "Green", Description = "文件搜索" },
                        new ToolboxItem { Name = "PowerToys", Icon = "&#xE8C1;", Background = "Blue", Description = "系统增强" },
                        new ToolboxItem { Name = "TaskManager", Icon = "&#xE7EF;", Background = "Red", Description = "任务管理器" }
                    }
                },
                new AppCollection 
                { 
                    Name = "办公软件", 
                    Icon = "📊", 
                    Background = "MediumPurple", 
                    AppCount = "6 个应用",
                    Apps = new ObservableCollection<ToolboxItem>
                    {
                        new ToolboxItem { Name = "Microsoft Office", Icon = "&#xE8D7;", Background = "Blue", Description = "办公套件" },
                        new ToolboxItem { Name = "Slack", Icon = "&#xE8F2;", Background = "DarkMagenta", Description = "团队协作" },
                        new ToolboxItem { Name = "Zoom", Icon = "&#xE8AA;", Background = "DarkBlue", Description = "视频会议" },
                        new ToolboxItem { Name = "Notion", Icon = "&#xE8A5;", Background = "Gray", Description = "笔记协作" }
                    }
                }
            };
        }

        private void CreateLoopingFavorites()
        {
            if (FavoriteApps == null || FavoriteApps.Count == 0) return;

            // 将整个队列重复三次，而不是将每个项目重复三次
            for (int i = 0; i < 3; i++)
            {
                foreach (var item in FavoriteApps)
                {
                    LoopingFavoriteApps.Add(item);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public sealed partial class AppCenterPage : Page
    {
        private const double BaseItemWidth = 120;  // 基准宽度
        private const double BaseItemHeight = 140; // 基准高度
        private const double MinItemWidth = 100;   // 最小宽度
        private const double MaxItemWidth = 160;   // 最大宽度

        // 🎯 ViewModel实例
        public AppCenterViewModel ViewModel { get; set; }

        private int _originalFavoriteCount = 0;
        private bool _isInfiniteScrollActive = false;
        private bool _isProgrammaticallyScrolling = false;

        public AppCenterPage()
        {
            this.InitializeComponent();

            // 初始化ViewModel
            ViewModel = new AppCenterViewModel();
            this.DataContext = ViewModel;

            _originalFavoriteCount = ViewModel.FavoriteApps.Count;

            // 绑定页面级别的SizeChanged事件
            this.SizeChanged += AppCenterPage_SizeChanged;

            // 为页面添加点击事件，用于取消选中
            this.Tapped += AppCenterPage_Tapped;
            
            // 初始状态设置为非无限滚动
            FavoritesRepeater.ItemsSource = ViewModel.FavoriteApps;
            UpdateScrollState(false); // 强制初始为锁定状态
        }

        private void AppCenterPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (FavoritesScrollViewer == null || _originalFavoriteCount == 0) return;

            const double itemWidth = 80;
            const double spacing = 20;
            double requiredWidth = (_originalFavoriteCount * itemWidth) + ((_originalFavoriteCount - 1) * spacing);
            
            bool needsInfiniteScroll = FavoritesScrollViewer.ActualWidth < requiredWidth;

            if (needsInfiniteScroll != _isInfiniteScrollActive)
            {
                UpdateScrollState(needsInfiniteScroll);
            }
        }

        private void UpdateScrollState(bool activateInfiniteScroll)
        {
            _isInfiniteScrollActive = activateInfiniteScroll;

            if (_isInfiniteScrollActive)
            {
                // 切换到无限滚动
                FavoritesScrollViewer.ViewChanged += FavoritesScrollViewer_ViewChanged;
                FavoritesRepeater.ItemsSource = ViewModel.LoopingFavoriteApps;
                FavoritesScrollViewer.HorizontalScrollMode = ScrollMode.Auto;
                FavoritesRepeaterContainer.HorizontalAlignment = HorizontalAlignment.Left; // 确保内容靠左对齐以进行滚动
                DispatcherQueue.TryEnqueue(ResetScrollViewPosition);
            }
            else
            {
                // 切换到视觉居中锁定模式
                FavoritesScrollViewer.ViewChanged -= FavoritesScrollViewer_ViewChanged;

                const double itemWidthWithSpacing = 80 + 20;
                double viewportCenterAbs = FavoritesScrollViewer.HorizontalOffset + (FavoritesScrollViewer.ActualWidth / 2);
                int centerIndexInLoop = (int)(viewportCenterAbs / itemWidthWithSpacing);
                int centerIndexInOriginal = _originalFavoriteCount > 0 ? centerIndexInLoop % _originalFavoriteCount : 0;
                
                FavoritesRepeater.ItemsSource = ViewModel.FavoriteApps;
                
                // 延迟以确保ItemsSource更新后布局完成
                DispatcherQueue.TryEnqueue(() =>
                {
                    double targetOffset = (centerIndexInOriginal * itemWidthWithSpacing) + (itemWidthWithSpacing / 2) - (FavoritesScrollViewer.ActualWidth / 2);
                    FavoritesScrollViewer.ChangeView(targetOffset, null, null, true);
                    FavoritesScrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
                    FavoritesRepeaterContainer.HorizontalAlignment = HorizontalAlignment.Center; // 在滚动结束后再居中
                });
            }
        }

        private void ResetScrollViewPosition()
        {
            if (!_isInfiniteScrollActive || _originalFavoriteCount == 0) return;
            const double itemWidthWithSpacing = 80 + 20;
            double offset = itemWidthWithSpacing * _originalFavoriteCount;
            FavoritesScrollViewer.ChangeView(offset, null, null, true);
        }

        private void FavoritesScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (e.IsIntermediate || !_isInfiniteScrollActive || _isProgrammaticallyScrolling) return;

            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer == null) return;

            const double itemWidthWithSpacing = 80 + 20;
            double sectionWidth = itemWidthWithSpacing * _originalFavoriteCount;
            double offset = scrollViewer.HorizontalOffset;

            if (offset >= sectionWidth * 2)
            {
                _isProgrammaticallyScrolling = true;
                scrollViewer.ChangeView(offset - sectionWidth, null, null, true);
                DispatcherQueue.TryEnqueue(() => _isProgrammaticallyScrolling = false);
            }
            else if (offset < sectionWidth)
            {
                _isProgrammaticallyScrolling = true;
                scrollViewer.ChangeView(offset + sectionWidth, null, null, true);
                DispatcherQueue.TryEnqueue(() => _isProgrammaticallyScrolling = false);
            }
        }

        // 🎯 4.1 实现收藏区的无限循环滚动（暂时注释，后续可完善）
        /*
        private void FavoritesScrollViewer_PointerWheelChanged(object sender, PointerEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer == null) return;

            // 仅当内容总宽度大于可视宽度时才启用滚动逻辑
            if (scrollViewer.ScrollableWidth <= 0) return;

            // 获取鼠标滚轮的垂直滚动增量
            var delta = e.GetCurrentPoint(scrollViewer).Properties.MouseWheelDelta;
            
            // 设置滚动速度倍数（可调整）
            double scrollMultiplier = 2.0;
            double horizontalOffset = scrollViewer.HorizontalOffset - (delta * scrollMultiplier);

            // 🎯 实现无限循环滚动逻辑
            if (horizontalOffset >= scrollViewer.ScrollableWidth)
            {
                // 向右滚动到底，回到开头
                scrollViewer.ChangeView(0, null, null, false);
            }
            else if (horizontalOffset <= 0)
            {
                // 向左滚动到头，跳到末尾
                scrollViewer.ChangeView(scrollViewer.ScrollableWidth, null, null, false);
            }
            else
            {
                // 正常滚动
                scrollViewer.ChangeView(horizontalOffset, null, null, false);
            }

            // 标记事件已处理，防止默认滚动行为
            e.Handled = true;
        }
        */

        // 🎯 4.2 应用GridView点击事件处理
        private async void AppsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ToolboxItem app)
            {
                // 显示应用启动确认对话框
                await ShowAppLaunchDialog(app);
            }
        }

        // 🎯 显示应用启动确认对话框
        private async System.Threading.Tasks.Task ShowAppLaunchDialog(ToolboxItem app)
        {
            var dialog = new ContentDialog()
            {
                Title = "启动应用",
                Content = $"是否要启动 {app.Name}？\n\n{app.Description}",
                PrimaryButtonText = "启动",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // 这里添加实际的应用启动逻辑
                // 例如：Process.Start(app.Path) 或其他启动方式
                await ShowLaunchResultDialog(app);
            }
        }

        // 🎯 显示启动结果
        private async System.Threading.Tasks.Task ShowLaunchResultDialog(ToolboxItem app)
        {
            var dialog = new ContentDialog()
            {
                Title = "启动结果",
                Content = $"应用 {app.Name} 启动成功！\n\n注意：这是演示版本，实际启动功能需要在正式版本中实现。",
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        // 🎯 4.3 实现合集区的弹窗交互
        private async void CollectionsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is AppCollection collection)
            {
                // 创建并显示合集弹窗
                var collectionPopup = new CollectionPopupWindow(collection);
                collectionPopup.Activate();
            }
        }

        private void AppsGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var gridView = sender as GridView;
            if (gridView == null) return;
            
            // 获取可用宽度（减去左右边距：16px左边距 + 1px右边距）
            double availableWidth = e.NewSize.Width - 17; // 16px左边距 + 1px右边距
            
            // 计算理想的列数（基于基准宽度）
            int idealColumns = Math.Max(1, (int)(availableWidth / BaseItemWidth));
            
            // 计算实际的项目宽度（等分可用宽度）
            double itemWidth = availableWidth / idealColumns;
            
            // 限制在最小和最大宽度之间
            itemWidth = Math.Max(MinItemWidth, Math.Min(MaxItemWidth, itemWidth));
            
            // 按比例调整高度
            double scaleFactor = itemWidth / BaseItemWidth;
            double itemHeight = BaseItemHeight * scaleFactor;
            
            // 查找并更新ItemsWrapGrid的ItemWidth和ItemHeight
            if (gridView.ItemsPanelRoot is ItemsWrapGrid wrapGrid)
            {
                wrapGrid.ItemWidth = itemWidth;
                wrapGrid.ItemHeight = itemHeight;
            }
            
            // 更新所有GridViewItem的内容尺寸
            foreach (var item in gridView.Items)
            {
                var container = gridView.ContainerFromItem(item) as GridViewItem;
                if (container?.Content is Border border)
                {
                    border.Width = itemWidth;
                    border.Height = itemHeight;
                    
                    // 同时调整内部图标大小
                    if (border.Child is StackPanel stackPanel && 
                        stackPanel.Children.Count > 0 && 
                        stackPanel.Children[0] is Border iconBorder)
                    {
                        double iconSize = 64 * scaleFactor;
                        iconSize = Math.Max(48, Math.Min(80, iconSize)); // 限制图标大小范围
                        iconBorder.Width = iconSize;
                        iconBorder.Height = iconSize;
                        
                        // 调整图标内的FontIcon大小
                        if (iconBorder.Child is FontIcon fontIcon)
                        {
                            fontIcon.FontSize = iconSize * 0.5; // 图标占图标容器的50%
                        }
                    }
                }
            }
        }
        
        private void AppCenterPage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // 检查点击位置是否不在GridView或ListView项目上
            var element = e.OriginalSource as FrameworkElement;
            
            // 如果点击的不是GridViewItem或ListViewItem，则取消选中
            if (element != null)
            {
                // 检查是否点击在GridViewItem或其子元素上
                var gridViewItem = FindParent<GridViewItem>(element);
                var listViewItem = FindParent<ListViewItem>(element);
                
                if (gridViewItem == null && listViewItem == null)
                {
                    // 点击在空白区域，取消所有选中
                    AppsGridView.SelectedItem = null;
                    CollectionsListView.SelectedItem = null;
                }
            }
        }
        
        // 辅助方法：查找指定类型的父元素
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null) return null;
            
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            
            if (parentObject is T parent)
                return parent;
            
            return FindParent<T>(parentObject);
        }
    }
} 