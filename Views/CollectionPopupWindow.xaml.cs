using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Foundation;

namespace IntelliCoreToolbox.Views
{
    public sealed partial class CollectionPopupWindow : Window
    {
        private AppCollection _currentCollection;
        private const double BaseItemWidth = 120;  // 基准宽度
        private const double BaseItemHeight = 140; // 基准高度
        private const double MinItemWidth = 100;   // 最小宽度
        private const double MaxItemWidth = 160;   // 最大宽度

        public CollectionPopupWindow(AppCollection collection)
        {
            this.InitializeComponent();
            
            _currentCollection = collection;
            InitializeWindow();
            LoadCollectionData();
        }

        private void InitializeWindow()
        {
            // 设置窗口属性
            this.Title = "";
            
            // 隐藏标题栏
            this.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            this.AppWindow.TitleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
            this.AppWindow.TitleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
            
            // 设置窗口大小和位置
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(800, 600));
            
            // 居中显示
            var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(this.AppWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Nearest);
            if (displayArea != null)
            {
                var centerX = (displayArea.WorkArea.Width - 800) / 2;
                var centerY = (displayArea.WorkArea.Height - 600) / 2;
                this.AppWindow.Move(new Windows.Graphics.PointInt32(centerX, centerY));
            }

            // 设置窗口图标（可选）
            // this.AppWindow.SetIcon("Assets/app-icon.ico");
        }

        private void LoadCollectionData()
        {
            // 设置标题栏信息
            CollectionNameText.Text = _currentCollection.Name;
            CollectionCountText.Text = $"{_currentCollection.Apps.Count} 个应用";
            CollectionIconText.Text = _currentCollection.Icon;
            
            // 设置图标背景色
            if (ColorFromString(_currentCollection.Background) is SolidColorBrush brush)
            {
                CollectionIconBorder.Background = brush;
            }

            // 绑定应用数据到GridView
            CollectionAppsGridView.ItemsSource = _currentCollection.Apps;
        }

        // 🎯 辅助方法：将字符串颜色转换为SolidColorBrush
        private SolidColorBrush ColorFromString(string colorName)
        {
            try
            {
                var color = colorName.ToLower() switch
                {
                    "cornflowerblue" => Microsoft.UI.Colors.CornflowerBlue,
                    "forestgreen" => Microsoft.UI.Colors.ForestGreen,
                    "crimson" => Microsoft.UI.Colors.Crimson,
                    "darkorange" => Microsoft.UI.Colors.DarkOrange,
                    "mediumpurple" => Microsoft.UI.Colors.MediumPurple,
                    "blue" => Microsoft.UI.Colors.Blue,
                    "orange" => Microsoft.UI.Colors.Orange,
                    "red" => Microsoft.UI.Colors.Red,
                    "green" => Microsoft.UI.Colors.Green,
                    "purple" => Microsoft.UI.Colors.Purple,
                    "darkblue" => Microsoft.UI.Colors.DarkBlue,
                    "darkcyan" => Microsoft.UI.Colors.DarkCyan,
                    "darkmagenta" => Microsoft.UI.Colors.DarkMagenta,
                    "lightblue" => Microsoft.UI.Colors.LightBlue,
                    "brown" => Microsoft.UI.Colors.Brown,
                    "gold" => Microsoft.UI.Colors.Gold,
                    "gray" => Microsoft.UI.Colors.Gray,
                    "maroon" => Microsoft.UI.Colors.Maroon,
                    "teal" => Microsoft.UI.Colors.Teal,
                    "darkslateblue" => Microsoft.UI.Colors.DarkSlateBlue,
                    "darkgreen" => Microsoft.UI.Colors.DarkGreen,
                    _ => Microsoft.UI.Colors.Gray
                };
                return new SolidColorBrush(color);
            }
            catch
            {
                return new SolidColorBrush(Microsoft.UI.Colors.Gray);
            }
        }

        // 🎯 事件处理：应用项目点击
        private void CollectionAppsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ToolboxItem app)
            {
                // 这里可以添加启动应用的逻辑
                // 例如：启动应用、显示应用详情等
                ShowAppLaunchDialog(app);
            }
        }

        // 🎯 显示应用启动确认对话框
        private async void ShowAppLaunchDialog(ToolboxItem app)
        {
            var dialog = new ContentDialog()
            {
                Title = "启动应用",
                Content = $"是否要启动 {app.Name}？\n\n{app.Description}",
                PrimaryButtonText = "启动",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
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
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        // 🎯 事件处理：编辑合集按钮点击
        private async void EditCollectionButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog()
            {
                Title = "编辑合集",
                Content = $"编辑合集功能将在后续版本中实现。\n\n当前合集：{_currentCollection.Name}\n应用数量：{_currentCollection.Apps.Count}",
                CloseButtonText = "确定",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        // 🎯 事件处理：取消/关闭按钮点击（底部）
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // 🎯 在GridView加载完成后，主动触发一次尺寸调整
        private void CollectionAppsGridView_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is GridView gridView && gridView.ActualWidth > 0)
            {
                UpdateItemsLayout(new Size(gridView.ActualWidth, gridView.ActualHeight));
            }
        }

        // 🎯 在GridView尺寸变化时，触发尺寸调整
        private void CollectionAppsGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateItemsLayout(e.NewSize);
        }

        // 🎯 核心布局逻辑
        private void UpdateItemsLayout(Size newSize)
        {
            var gridView = this.CollectionAppsGridView;
            if (gridView == null) return;

            // 减去左边距20px，右边距现在是0
            double availableWidth = newSize.Width - 20; 
            
            int idealColumns = Math.Max(1, (int)(availableWidth / BaseItemWidth));
            
            double itemWidth = availableWidth / idealColumns;
            
            itemWidth = Math.Max(MinItemWidth, Math.Min(MaxItemWidth, itemWidth));
            
            double scaleFactor = itemWidth / BaseItemWidth;
            double itemHeight = BaseItemHeight * scaleFactor;
            
            if (gridView.ItemsPanelRoot is ItemsWrapGrid wrapGrid)
            {
                wrapGrid.ItemWidth = itemWidth;
                wrapGrid.ItemHeight = itemHeight;
            }
            
            foreach (var item in gridView.Items)
            {
                var container = gridView.ContainerFromItem(item) as GridViewItem;
                if (container != null)
                {
                    if (container.ContentTemplateRoot is Border border)
                    {
                        border.Width = itemWidth;
                        border.Height = itemHeight;

                        if (border.Child is StackPanel stackPanel)
                        {
                            if (stackPanel.Children.Count > 0 && stackPanel.Children[0] is Border iconBorder)
                            {
                                double iconSize = 64 * scaleFactor;
                                iconSize = Math.Max(48, Math.Min(80, iconSize));
                                iconBorder.Width = iconSize;
                                iconBorder.Height = iconSize;
                                
                                if (iconBorder.Child is FontIcon fontIcon)
                                {
                                    fontIcon.FontSize = iconSize * 0.5;
                                }
                            }
                            
                            if (stackPanel.Children.Count > 1 && stackPanel.Children[1] is TextBlock textBlock)
                            {
                                textBlock.FontSize = Math.Max(10, 12 * scaleFactor);
                            }
                        }
                    }
                }
            }
        }
    }
} 