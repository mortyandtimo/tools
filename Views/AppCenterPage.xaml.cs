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
using IntelliCoreToolbox.Services;
using Windows.Foundation;

namespace IntelliCoreToolbox.Views
{
    // 🎯 数据模型已迁移到 IntelliCoreToolbox.Services 命名空间

    // 🎯 ViewModel：应用中心页面 (重构为使用AppService)
    public class AppCenterViewModel : INotifyPropertyChanged
    {
        private readonly AppService _appService;

        public ObservableCollection<ToolboxItem> FavoriteApps => _appService.FavoriteApps;
        public ObservableCollection<ToolboxItem> AllApps => _appService.AllApps;
        public ObservableCollection<AppCollection> Collections => _appService.Collections;
        public ObservableCollection<ToolboxItem> LoopingFavoriteApps => _appService.LoopingFavoriteApps;

        public AppCenterViewModel()
        {
            _appService = AppService.Instance;
        }

        // 🎯 数据初始化已迁移到AppService

        // 🎯 CreateLoopingFavorites已迁移到AppService

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

            // 🎯 监听FavoriteApps数据加载完成
            ViewModel.FavoriteApps.CollectionChanged += FavoriteApps_CollectionChanged;
            _originalFavoriteCount = ViewModel.FavoriteApps.Count;

            // 绑定页面级别的SizeChanged事件
            this.SizeChanged += AppCenterPage_SizeChanged;

            // 为页面添加点击事件，用于取消选中
            this.Tapped += AppCenterPage_Tapped;
            
            // 初始状态设置为非无限滚动
            FavoritesRepeater.ItemsSource = ViewModel.FavoriteApps;
            UpdateScrollState(false); // 强制初始为锁定状态
            
            // 🎯 页面加载后直接检查无限滚动状态
            this.Loaded += AppCenterPage_Loaded;
        }

        // 🎯 页面加载完成后的初始化
        private void AppCenterPage_Loaded(object sender, RoutedEventArgs e)
        {
            // 确保数据已加载
            if (ViewModel.FavoriteApps.Count > 0)
            {
                _originalFavoriteCount = ViewModel.FavoriteApps.Count;
                System.Diagnostics.Debug.WriteLine($"页面加载完成，FavoriteApps数量: {_originalFavoriteCount}");
                
                // 延迟检查以确保UI布局完成
                DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                {
                    CheckAndUpdateScrollState();
                });
            }
        }

        // 🎯 检查并更新滚动状态的统一方法
        private void CheckAndUpdateScrollState()
        {
            if (FavoritesScrollViewer == null || _originalFavoriteCount == 0)
            {
                System.Diagnostics.Debug.WriteLine($"CheckAndUpdateScrollState跳过: ScrollViewer={FavoritesScrollViewer != null}, Count={_originalFavoriteCount}");
                return;
            }

            const double itemWidth = 80;
            const double spacing = 20;
            double requiredWidth = (_originalFavoriteCount * itemWidth) + ((_originalFavoriteCount - 1) * spacing);
            
            bool needsInfiniteScroll = FavoritesScrollViewer.ActualWidth < requiredWidth;

            System.Diagnostics.Debug.WriteLine($"滚动状态检查: ViewerWidth={FavoritesScrollViewer.ActualWidth}, RequiredWidth={requiredWidth}, NeedsInfinite={needsInfiniteScroll}, Current={_isInfiniteScrollActive}");

            if (needsInfiniteScroll != _isInfiniteScrollActive)
            {
                UpdateScrollState(needsInfiniteScroll);
            }
        }

        // 🎯 处理FavoriteApps数据加载完成事件
        private void FavoriteApps_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"FavoriteApps_CollectionChanged: Action={e.Action}, CurrentCount={ViewModel.FavoriteApps.Count}, OriginalCount={_originalFavoriteCount}");
            
            // 只在首次数据加载时处理
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && _originalFavoriteCount == 0 && ViewModel.FavoriteApps.Count > 0)
            {
                _originalFavoriteCount = ViewModel.FavoriteApps.Count;
                System.Diagnostics.Debug.WriteLine($"FavoriteApps数据加载完成: Count={_originalFavoriteCount}");
                
                // 延迟执行以确保UI布局完成
                DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                {
                    if (FavoritesScrollViewer != null && FavoritesScrollViewer.ActualWidth > 0)
                    {
                        // 直接调用滚动状态检查逻辑
                        const double itemWidth = 80;
                        const double spacing = 20;
                        double requiredWidth = (_originalFavoriteCount * itemWidth) + ((_originalFavoriteCount - 1) * spacing);
                        
                        bool needsInfiniteScroll = FavoritesScrollViewer.ActualWidth < requiredWidth;
                        
                        System.Diagnostics.Debug.WriteLine($"数据加载后滚动状态检查: ViewerWidth={FavoritesScrollViewer.ActualWidth}, RequiredWidth={requiredWidth}, NeedsInfinite={needsInfiniteScroll}");

                        if (needsInfiniteScroll != _isInfiniteScrollActive)
                        {
                            UpdateScrollState(needsInfiniteScroll);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"ScrollViewer未准备就绪: ViewerNull={FavoritesScrollViewer == null}, ActualWidth={FavoritesScrollViewer?.ActualWidth ?? -1}");
                    }
                });
            }
        }

        private void AppCenterPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 使用统一的检查方法
            CheckAndUpdateScrollState();
        }

        private void UpdateScrollState(bool activateInfiniteScroll)
        {
            _isInfiniteScrollActive = activateInfiniteScroll;

            System.Diagnostics.Debug.WriteLine($"UpdateScrollState: 激活无限滚动={activateInfiniteScroll}, LoopingApps数量={ViewModel.LoopingFavoriteApps.Count}");

            if (_isInfiniteScrollActive)
            {
                // 切换到无限滚动
                FavoritesScrollViewer.ViewChanged += FavoritesScrollViewer_ViewChanged;
                FavoritesRepeater.ItemsSource = ViewModel.LoopingFavoriteApps;
                FavoritesScrollViewer.HorizontalScrollMode = ScrollMode.Auto;
                FavoritesRepeaterContainer.HorizontalAlignment = HorizontalAlignment.Left; // 确保内容靠左对齐以进行滚动
                DispatcherQueue.TryEnqueue(ResetScrollViewPosition);
                System.Diagnostics.Debug.WriteLine("已切换到无限滚动模式");
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
                System.Diagnostics.Debug.WriteLine("已切换到居中锁定模式");
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