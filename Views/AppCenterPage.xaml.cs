using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;

namespace IntelliCoreToolbox.Views
{
    public sealed partial class AppCenterPage : Page
    {
        private const double BaseItemWidth = 120;  // 基准宽度
        private const double BaseItemHeight = 140; // 基准高度
        private const double MinItemWidth = 100;   // 最小宽度
        private const double MaxItemWidth = 160;   // 最大宽度
        
        public AppCenterPage()
        {
            this.InitializeComponent();
            
            // 为页面添加点击事件，用于取消选中
            this.Tapped += AppCenterPage_Tapped;
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