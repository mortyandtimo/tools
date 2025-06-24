using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

namespace IntelliCoreToolbox.Services
{
    // 🎯 数据模型：工具箱应用项 (从AppCenterPage迁移)
    public class ToolboxItem
    {
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Background { get; set; } = "";
        public string Description { get; set; } = "";
        public string Path { get; set; } = "";
        public bool IsFavorite { get; set; } = false;
    }

    // 🎯 用户配置数据结构（用于JSON序列化）
    public class UserAppConfig
    {
        public List<ToolboxItem> UserApps { get; set; } = new List<ToolboxItem>();
        public DateTime LastModified { get; set; } = DateTime.Now;
        public string Version { get; set; } = "1.0";
    }

    // 🎯 收藏配置数据结构（独立管理收藏状态）
    public class FavoriteConfig
    {
        public List<FavoriteItem> FavoriteApps { get; set; } = new List<FavoriteItem>();
        public DateTime LastModified { get; set; } = DateTime.Now;
        public string Version { get; set; } = "1.0";
    }

    // 🎯 收藏项数据结构
    public class FavoriteItem
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";        // 用户应用有路径，预设应用为空
        public bool IsPreset { get; set; } = false;   // 标识是否为预设应用
    }

    // 🎯 数据模型：应用合集 (从AppCenterPage迁移)
    public class AppCollection
    {
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Background { get; set; } = "";
        public string AppCount { get; set; } = "";
        public ObservableCollection<ToolboxItem> Apps { get; set; } = new ObservableCollection<ToolboxItem>();
    }

    // 🎯 数据初始化完成事件参数
    public class DataInitializationCompletedEventArgs : EventArgs
    {
        public int FavoriteAppsCount { get; set; }
        public int AllAppsCount { get; set; }
        public int CollectionsCount { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    // 🎯 核心数据服务：应用数据管理
    public class AppService : INotifyPropertyChanged
    {
        private static AppService? _instance;
        private static readonly object _lock = new object();

        // 核心数据集合
        public ObservableCollection<ToolboxItem> FavoriteApps { get; private set; }
        public ObservableCollection<ToolboxItem> AllApps { get; private set; }
        public ObservableCollection<AppCollection> Collections { get; private set; }
        public ObservableCollection<ToolboxItem> LoopingFavoriteApps { get; private set; }

        // 🎯 数据持久化相关
        private readonly string _configDirectory;
        private readonly string _userAppsFilePath;
        private readonly string _favoritesFilePath;
        private List<ToolboxItem> _userAddedApps;
        private bool _isUserAppsLoaded = false; // 🎯 防止重复加载标志
        private List<ToolboxItem>? _pendingFavoriteRestore; // 暂存待恢复的收藏应用

        // 🎯 数据初始化完成事件
        public event EventHandler<DataInitializationCompletedEventArgs>? DataInitializationCompleted;

        // 单例模式实现
        public static AppService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new AppService();
                    }
                }
                return _instance;
            }
        }

        private AppService()
        {
            FavoriteApps = new ObservableCollection<ToolboxItem>();
            AllApps = new ObservableCollection<ToolboxItem>();
            Collections = new ObservableCollection<AppCollection>();
            LoopingFavoriteApps = new ObservableCollection<ToolboxItem>();

            // 初始化配置目录
            _configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IntelliCoreToolbox");
            _userAppsFilePath = Path.Combine(_configDirectory, "user-apps.json");
            _favoritesFilePath = Path.Combine(_configDirectory, "favorites.json");
            _userAddedApps = new List<ToolboxItem>();

            // 确保配置目录存在
            EnsureConfigDirectoryExists();

            // 异步初始化数据
            _ = Task.Run(async () => await InitializeData());
        }

        private async Task InitializeData()
        {
            System.Diagnostics.Debug.WriteLine("🎯 AppService.InitializeData 开始");
            
            try
            {
                // 🎯 预设收藏已清空，现在初始为空列表（用户可通过右键菜单自行添加收藏）
                var favoriteAppsData = new ToolboxItem[] { };

                var allAppsData = new[]
                {
                    new ToolboxItem { Name = "Visual Studio", Icon = "&#xE943;", Background = "Blue", Description = "IDE开发环境", Path = "" },
                    new ToolboxItem { Name = "Notepad++", Icon = "&#xE8A5;", Background = "Orange", Description = "文本编辑器", Path = "" },
                    new ToolboxItem { Name = "Chrome", Icon = "&#xE774;", Background = "Red", Description = "网页浏览器", Path = "" },
                    new ToolboxItem { Name = "Git", Icon = "&#xE8AB;", Background = "Green", Description = "版本控制工具", Path = "" },
                    new ToolboxItem { Name = "Discord", Icon = "&#xE8BD;", Background = "Purple", Description = "聊天通讯工具", Path = "" },
                    new ToolboxItem { Name = "Postman", Icon = "&#xE968;", Background = "DarkBlue", Description = "API开发测试", Path = "" },
                    new ToolboxItem { Name = "Docker", Icon = "&#xE8C7;", Background = "DarkCyan", Description = "容器化平台", Path = "" },
                    new ToolboxItem { Name = "Slack", Icon = "&#xE8F2;", Background = "DarkMagenta", Description = "团队协作", Path = "" },
                    new ToolboxItem { Name = "VS Code", Icon = "&#xE943;", Background = "LightBlue", Description = "轻量级编辑器", Path = "" },
                    new ToolboxItem { Name = "Figma", Icon = "&#xE8EF;", Background = "Brown", Description = "UI设计工具", Path = "" },
                    new ToolboxItem { Name = "Adobe XD", Icon = "&#xE8F0;", Background = "Crimson", Description = "原型设计", Path = "" },
                    new ToolboxItem { Name = "Sketch", Icon = "&#xE8F1;", Background = "Gold", Description = "界面设计", Path = "" },
                    new ToolboxItem { Name = "Notion", Icon = "&#xE8A5;", Background = "Gray", Description = "笔记协作", Path = "" }
                };

                // 🎯 批量添加数据以避免逐个触发CollectionChanged事件
                System.Diagnostics.Debug.WriteLine("🎯 开始批量加载收藏应用数据...");
                foreach (var item in favoriteAppsData)
                {
                    FavoriteApps.Add(item);
                }
                System.Diagnostics.Debug.WriteLine($"🎯 已批量添加 {favoriteAppsData.Length} 个收藏应用，当前数量: {FavoriteApps.Count}");

                // 创建循环收藏列表
                CreateLoopingFavorites();
                System.Diagnostics.Debug.WriteLine($"🎯 已创建循环收藏列表，数量: {LoopingFavoriteApps.Count}");

                // 批量添加全部应用
                System.Diagnostics.Debug.WriteLine("🎯 开始批量加载全部应用数据...");
                foreach (var item in allAppsData)
                {
                    AllApps.Add(item);
                }
                System.Diagnostics.Debug.WriteLine($"🎯 已批量添加 {allAppsData.Length} 个全部应用，当前数量: {AllApps.Count}");

                // 初始化合集
                InitializeCollections();
                System.Diagnostics.Debug.WriteLine($"🎯 已初始化合集，数量: {Collections.Count}");

                // 加载用户添加的应用
                System.Diagnostics.Debug.WriteLine("🎯 开始加载用户应用...");
                await LoadUserApps();
                System.Diagnostics.Debug.WriteLine($"🎯 用户应用加载完成，最终AllApps数量: {AllApps.Count}");

                // 🎯 最后加载收藏状态（确保所有应用都已加载完成）
                System.Diagnostics.Debug.WriteLine("🎯 开始加载收藏状态...");
                System.Diagnostics.Debug.WriteLine($"🎯 当前AllApps应用列表:");
                foreach (var app in AllApps)
                {
                    System.Diagnostics.Debug.WriteLine($"   - {app.Name} (路径: '{app.Path}', 预设: {string.IsNullOrEmpty(app.Path)})");
                }
                await LoadFavorites();
                System.Diagnostics.Debug.WriteLine($"🎯 收藏状态加载完成，最终FavoriteApps数量: {FavoriteApps.Count}");

                // 🎯 所有数据加载完成，触发完成事件
                var eventArgs = new DataInitializationCompletedEventArgs
                {
                    FavoriteAppsCount = FavoriteApps.Count,
                    AllAppsCount = AllApps.Count,
                    CollectionsCount = Collections.Count,
                    Success = true,
                    Message = "数据初始化成功完成"
                };

                System.Diagnostics.Debug.WriteLine($"🎯 数据初始化完成！准备触发完成事件 - 收藏应用:{eventArgs.FavoriteAppsCount}, 全部应用:{eventArgs.AllAppsCount}, 合集:{eventArgs.CollectionsCount}");
                
                // 直接在当前线程触发事件，让订阅者决定线程调度
                DataInitializationCompleted?.Invoke(this, eventArgs);
                System.Diagnostics.Debug.WriteLine("🎯 DataInitializationCompleted事件已触发！");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🎯 数据初始化失败: {ex.Message}");
                
                // 触发失败事件
                var errorEventArgs = new DataInitializationCompletedEventArgs
                {
                    Success = false,
                    Message = $"数据初始化失败: {ex.Message}"
                };

                DataInitializationCompleted?.Invoke(this, errorEventArgs);
            }
        }

        private void InitializeCollections()
        {
            Collections.Add(new AppCollection
            {
                Name = "设计工具",
                Icon = "🎨",
                Background = "CornflowerBlue",
                AppCount = "8 个应用",
                Apps = new ObservableCollection<ToolboxItem>
                {
                    new ToolboxItem { Name = "Figma", Icon = "&#xE8EF;", Background = "Brown", Description = "UI设计工具", Path = "" },
                    new ToolboxItem { Name = "Adobe XD", Icon = "&#xE8F0;", Background = "Crimson", Description = "原型设计", Path = "" },
                    new ToolboxItem { Name = "Sketch", Icon = "&#xE8F1;", Background = "Gold", Description = "界面设计", Path = "" },
                    new ToolboxItem { Name = "Photoshop", Icon = "&#xE91B;", Background = "DarkCyan", Description = "图像编辑", Path = "" },
                    new ToolboxItem { Name = "Illustrator", Icon = "&#xE91C;", Background = "Orange", Description = "矢量绘图", Path = "" },
                    new ToolboxItem { Name = "Canva", Icon = "&#xE8EF;", Background = "Green", Description = "在线设计", Path = "" },
                    new ToolboxItem { Name = "Blender", Icon = "&#xE7F8;", Background = "DarkOrange", Description = "3D建模", Path = "" },
                    new ToolboxItem { Name = "GIMP", Icon = "&#xE91B;", Background = "Purple", Description = "免费图像编辑", Path = "" }
                }
            });

            Collections.Add(new AppCollection
            {
                Name = "开发工具",
                Icon = "💻",
                Background = "ForestGreen",
                AppCount = "12 个应用",
                Apps = new ObservableCollection<ToolboxItem>
                {
                    new ToolboxItem { Name = "Visual Studio", Icon = "&#xE943;", Background = "Blue", Description = "IDE开发环境", Path = "" },
                    new ToolboxItem { Name = "VS Code", Icon = "&#xE943;", Background = "LightBlue", Description = "轻量级编辑器", Path = "" },
                    new ToolboxItem { Name = "IntelliJ IDEA", Icon = "&#xE943;", Background = "Maroon", Description = "Java IDE", Path = "" },
                    new ToolboxItem { Name = "Git", Icon = "&#xE8AB;", Background = "Green", Description = "版本控制工具", Path = "" },
                    new ToolboxItem { Name = "Docker", Icon = "&#xE8C7;", Background = "DarkCyan", Description = "容器化平台", Path = "" },
                    new ToolboxItem { Name = "Postman", Icon = "&#xE968;", Background = "DarkBlue", Description = "API开发测试", Path = "" }
                }
            });

            Collections.Add(new AppCollection
            {
                Name = "游戏娱乐",
                Icon = "🎮",
                Background = "Crimson",
                AppCount = "5 个应用",
                Apps = new ObservableCollection<ToolboxItem>
                {
                    new ToolboxItem { Name = "Steam", Icon = "&#xE8C1;", Background = "DarkBlue", Description = "游戏平台", Path = "" },
                    new ToolboxItem { Name = "Discord", Icon = "&#xE8BD;", Background = "Purple", Description = "游戏聊天", Path = "" },
                    new ToolboxItem { Name = "Unity", Icon = "&#xE7F8;", Background = "DarkSlateBlue", Description = "游戏引擎", Path = "" },
                    new ToolboxItem { Name = "OBS Studio", Icon = "&#xE714;", Background = "DarkGreen", Description = "直播录制", Path = "" },
                    new ToolboxItem { Name = "Twitch", Icon = "&#xE8BD;", Background = "MediumPurple", Description = "直播平台", Path = "" }
                }
            });

            Collections.Add(new AppCollection
            {
                Name = "系统工具",
                Icon = "🔧",
                Background = "DarkOrange",
                AppCount = "15 个应用",
                Apps = new ObservableCollection<ToolboxItem>
                {
                    new ToolboxItem { Name = "7-Zip", Icon = "&#xE8B5;", Background = "DarkBlue", Description = "压缩解压", Path = "" },
                    new ToolboxItem { Name = "Everything", Icon = "&#xE721;", Background = "Green", Description = "文件搜索", Path = "" },
                    new ToolboxItem { Name = "PowerToys", Icon = "&#xE8C1;", Background = "Blue", Description = "系统增强", Path = "" },
                    new ToolboxItem { Name = "TaskManager", Icon = "&#xE7EF;", Background = "Red", Description = "任务管理器", Path = "" }
                }
            });

            Collections.Add(new AppCollection
            {
                Name = "办公软件",
                Icon = "📊",
                Background = "MediumPurple",
                AppCount = "6 个应用",
                Apps = new ObservableCollection<ToolboxItem>
                {
                    new ToolboxItem { Name = "Microsoft Office", Icon = "&#xE8D7;", Background = "Blue", Description = "办公套件", Path = "" },
                    new ToolboxItem { Name = "Slack", Icon = "&#xE8F2;", Background = "DarkMagenta", Description = "团队协作", Path = "" },
                    new ToolboxItem { Name = "Zoom", Icon = "&#xE8AA;", Background = "DarkBlue", Description = "视频会议", Path = "" },
                    new ToolboxItem { Name = "Notion", Icon = "&#xE8A5;", Background = "Gray", Description = "笔记协作", Path = "" }
                }
            });
        }

        // 🎯 核心功能：添加新应用（重构版本，支持数据持久化）
        public async Task<AddApplicationResult> AddApplication(string filePath, string? customName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    return new AddApplicationResult { Success = false, Message = "文件不存在或路径无效" };

                // 检查是否已存在（基于路径）
                var existingApp = AllApps.FirstOrDefault(app => app.Path.Equals(filePath, StringComparison.OrdinalIgnoreCase));
                if (existingApp != null)
                {
                    return new AddApplicationResult 
                    { 
                        Success = false, 
                        Message = $"应用 \"{existingApp.Name}\" 已经存在于工具箱中",
                        IsAlreadyExists = true,
                        ExistingApp = existingApp
                    };
                }

                // 提取文件信息
                var appInfo = await ExtractApplicationInfo(filePath, customName);
                if (appInfo == null) 
                    return new AddApplicationResult { Success = false, Message = "无法提取应用信息" };

                // 添加到用户应用列表和全部应用列表
                _userAddedApps.Add(appInfo);
                AllApps.Add(appInfo);

                // 🎯 立即保存到文件
                await SaveUserApps();

                // 触发属性变更通知
                OnPropertyChanged(nameof(AllApps));

                return new AddApplicationResult 
                { 
                    Success = true, 
                    Message = $"成功添加应用 \"{appInfo.Name}\"",
                    AddedApp = appInfo
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"添加应用失败: {ex.Message}");
                return new AddApplicationResult { Success = false, Message = $"添加失败: {ex.Message}" };
            }
        }

        // 🎯 添加应用结果类
        public class AddApplicationResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = "";
            public bool IsAlreadyExists { get; set; }
            public ToolboxItem? ExistingApp { get; set; }
            public ToolboxItem? AddedApp { get; set; }
        }

        // 🎯 提取应用信息
        private async Task<ToolboxItem?> ExtractApplicationInfo(string filePath, string? customName = null)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                
                // 🎯 统一使用完整文件名（不含扩展名）作为显示名称，不再使用ProductName覆盖
                string appName = customName ?? Path.GetFileNameWithoutExtension(filePath);
                string description = "用户添加的应用";
                string iconGlyph = "&#xE8FC;"; // 默认应用图标
                string backgroundColor = GetRandomBackgroundColor();

                // 处理快捷方式文件
                if (fileInfo.Extension.Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                {
                    var targetPath = ResolveShortcut(filePath);
                    if (!string.IsNullOrEmpty(targetPath))
                    {
                        filePath = targetPath;
                        if (string.IsNullOrEmpty(customName))
                            appName = Path.GetFileNameWithoutExtension(targetPath);
                    }
                }

                // 🎯 仅从文件版本信息获取描述信息，不覆盖应用名称
                try
                {
                    var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(filePath);
                    // 移除ProductName覆盖逻辑，确保使用完整文件名
                    if (!string.IsNullOrEmpty(versionInfo.FileDescription))
                        description = versionInfo.FileDescription;
                }
                catch
                {
                    // 忽略版本信息读取失败
                }

                return new ToolboxItem
                {
                    Name = appName, // 确保使用完整文件名
                    Icon = iconGlyph,
                    Background = backgroundColor,
                    Description = description,
                    Path = filePath
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"提取应用信息失败: {ex.Message}");
                return null;
            }
        }

        // 🎯 解析快捷方式目标路径 (使用简化方案)
        private string? ResolveShortcut(string shortcutPath)
        {
            try
            {
                // 简化实现：对于.lnk文件，暂时返回null，等待后续使用Win32 API实现
                // 这样可以避免重量级的COM依赖
                return null;
            }
            catch
            {
                return null;
            }
        }

        // 🎯 获取随机背景颜色
        private string GetRandomBackgroundColor()
        {
            var colors = new[] { "Blue", "Green", "Red", "Purple", "Orange", "DarkBlue", "DarkCyan", "Brown", "Crimson", "Gold", "ForestGreen", "DarkOrange", "MediumPurple", "DarkMagenta", "Maroon", "Teal" };
            var random = new Random();
            return colors[random.Next(colors.Length)];
        }

        // 🎯 重新创建循环收藏列表（兼容无限滚动功能）
        public void CreateLoopingFavorites()
        {
            LoopingFavoriteApps.Clear();

            if (FavoriteApps == null || FavoriteApps.Count == 0) return;

            // 将整个队列重复三次
            for (int i = 0; i < 3; i++)
            {
                foreach (var item in FavoriteApps)
                {
                    LoopingFavoriteApps.Add(item);
                }
            }

            OnPropertyChanged(nameof(LoopingFavoriteApps));
        }

        // 🎯 更新循环收藏列表（页面重新导航时使用）
        public void UpdateLoopingCollection()
        {
            CreateLoopingFavorites();
        }

        /// <summary>
        /// 切换应用的收藏状态
        /// </summary>
        /// <param name="item">要切换收藏状态的应用项</param>
        public void ToggleFavoriteStatus(ToolboxItem item)
        {
            if (item == null) return;

            System.Diagnostics.Debug.WriteLine($"🎯 [ToggleFavoriteStatus] 开始处理应用: {item.Name}");
            System.Diagnostics.Debug.WriteLine($"🎯 [ToggleFavoriteStatus] 当前收藏状态: {item.IsFavorite}");
            System.Diagnostics.Debug.WriteLine($"🎯 [ToggleFavoriteStatus] 应用路径: '{item.Path}'");
            System.Diagnostics.Debug.WriteLine($"🎯 [ToggleFavoriteStatus] 是否预设应用: {string.IsNullOrEmpty(item.Path)}");

            // 切换收藏状态
            item.IsFavorite = !item.IsFavorite;
            System.Diagnostics.Debug.WriteLine($"🎯 [ToggleFavoriteStatus] 切换后收藏状态: {item.IsFavorite}");

            if (item.IsFavorite)
            {
                // 添加到收藏集合
                if (!FavoriteApps.Contains(item))
                {
                    FavoriteApps.Add(item);
                    System.Diagnostics.Debug.WriteLine($"🎯 [ToggleFavoriteStatus] 已添加到收藏: {item.Name}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"🎯 [ToggleFavoriteStatus] 应用已在收藏中: {item.Name}");
                }
            }
            else
            {
                // 从收藏集合中移除
                if (FavoriteApps.Contains(item))
                {
                    FavoriteApps.Remove(item);
                    System.Diagnostics.Debug.WriteLine($"🎯 [ToggleFavoriteStatus] 已从收藏移除: {item.Name}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"🎯 [ToggleFavoriteStatus] 应用不在收藏中: {item.Name}");
                }
            }

            // 更新循环收藏列表
            CreateLoopingFavorites();
            System.Diagnostics.Debug.WriteLine($"🎯 [ToggleFavoriteStatus] 当前FavoriteApps数量: {FavoriteApps.Count}");

            // 保存数据
            System.Diagnostics.Debug.WriteLine($"🎯 [ToggleFavoriteStatus] 开始保存数据...");
            _ = Task.Run(async () => 
            {
                await SaveUserApps();   // 保存用户应用数据（用户添加的应用）
                await SaveFavorites();  // 保存收藏状态（所有应用）
                System.Diagnostics.Debug.WriteLine($"🎯 [ToggleFavoriteStatus] 数据保存完成");
            });
        }

        /// <summary>
        /// 从工具箱中移除应用
        /// </summary>
        /// <param name="item">要移除的应用项</param>
        public void RemoveApplication(ToolboxItem item)
        {
            if (item == null) return;

            // 从AllApps集合中移除
            if (AllApps.Contains(item))
            {
                AllApps.Remove(item);
                System.Diagnostics.Debug.WriteLine($"🎯 已从全部应用移除: {item.Name}");
            }

            // 从用户添加的应用列表中移除
            var userApp = _userAddedApps.FirstOrDefault(app => 
                app.Path == item.Path || app.Name == item.Name);
            if (userApp != null)
            {
                _userAddedApps.Remove(userApp);
                System.Diagnostics.Debug.WriteLine($"🎯 已从用户应用列表移除: {userApp.Name}");
            }

            // 如果是收藏应用，也要从收藏集合中移除
            if (item.IsFavorite && FavoriteApps.Contains(item))
            {
                FavoriteApps.Remove(item);
                System.Diagnostics.Debug.WriteLine($"🎯 已从收藏应用移除: {item.Name}");
            }

            // 更新循环收藏列表
            CreateLoopingFavorites();

            // 保存数据
            _ = Task.Run(async () => 
            {
                await SaveUserApps();   // 保存用户应用数据（用户添加的应用）
                await SaveFavorites();  // 保存收藏状态（所有应用）
            });
        }

        // 🎯 数据持久化方法
        
        /// <summary>
        /// 确保配置目录存在
        /// </summary>
        private void EnsureConfigDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_configDirectory))
                {
                    Directory.CreateDirectory(_configDirectory);
                    System.Diagnostics.Debug.WriteLine($"配置目录已创建: {_configDirectory}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建配置目录失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存用户添加的应用到文件
        /// </summary>
        private async Task SaveUserApps()
        {
            try
            {
                var config = new UserAppConfig
                {
                    UserApps = _userAddedApps.ToList(),
                    LastModified = DateTime.Now,
                    Version = "1.0"
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var jsonString = JsonSerializer.Serialize(config, options);
                await File.WriteAllTextAsync(_userAppsFilePath, jsonString);
                
                System.Diagnostics.Debug.WriteLine($"用户应用数据已保存: {_userAppsFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存用户应用数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 从文件加载用户添加的应用
        /// </summary>
        private async Task LoadUserApps()
        {
            try
            {
                // 🎯 防止重复加载
                if (_isUserAppsLoaded)
                {
                    System.Diagnostics.Debug.WriteLine("🎯 用户应用已加载，跳过重复加载");
                    return;
                }
                
                Console.WriteLine($"🎯 文件存在检查: {File.Exists(_userAppsFilePath)}");
                
                if (!File.Exists(_userAppsFilePath))
                {
                    _isUserAppsLoaded = true; // 🎯 标记为已加载（即使没有文件）
                    return;
                }

                var jsonString = File.ReadAllText(_userAppsFilePath);
                
                Console.WriteLine($"🎯 JSON内容预览: {(jsonString.Length > 100 ? jsonString.Substring(0, 100) + "..." : jsonString)}");
                
                var config = JsonSerializer.Deserialize<UserAppConfig>(jsonString);
                

                if (config?.UserApps != null)
                {
                    _userAddedApps.Clear();
                    _userAddedApps.AddRange(config.UserApps);
                    

                    // 🎯 将用户应用添加到AllApps集合中，使用从Path提取的文件名作为显示名称
                    int addedToAllAppsCount = 0;
                    foreach (var app in config.UserApps)
                    {
                        try
                        {
                            // 🎯 更安全的文件名提取方式，处理长文件名和特殊字符
                            string displayName;
                            try
                            {
                                if (!string.IsNullOrEmpty(app.Path))
                                {
                                    // 先尝试使用Path.GetFileNameWithoutExtension
                                    string fileName = Path.GetFileName(app.Path);
                                    if (!string.IsNullOrEmpty(fileName))
                                    {
                                        int lastDot = fileName.LastIndexOf('.');
                                        displayName = lastDot > 0 ? fileName.Substring(0, lastDot) : fileName;
                                        
                                        // 如果文件名过长，截断到合理长度
                                        if (displayName.Length > 50)
                                        {
                                            displayName = displayName.Substring(0, 47) + "...";
                                        }
                                    }
                                    else
                                    {
                                        displayName = app.Name;
                                    }
                                }
                                else
                                {
                                    displayName = app.Name;
                                }
                            }
                            catch (Exception pathEx)
                            {
                                
                                displayName = app.Name;
                            }
                            
                            var newApp = new ToolboxItem
                            {
                                Name = displayName, // 使用安全提取的文件名
                                Icon = app.Icon,
                                Background = app.Background,
                                Description = app.Description,
                                Path = app.Path,
                                IsFavorite = false // 🎯 初始化为false，收藏状态由LoadFavorites方法单独处理
                            };
                            
                            // 🎯 检查路径重复以避免重复添加
                            var existingApp = AllApps.FirstOrDefault(a => 
                                !string.IsNullOrEmpty(a.Path) && 
                                !string.IsNullOrEmpty(newApp.Path) && 
                                a.Path.Equals(newApp.Path, StringComparison.OrdinalIgnoreCase));
                            
                            if (existingApp == null)
                            {
                                AllApps.Add(newApp);
                                addedToAllAppsCount++;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"🎯 跳过重复应用: {newApp.Name} (路径: {newApp.Path})");
                            }
                            
                            
                        }
                        catch (Exception ex)
                        {
                            
                            // 创建备份应用项，确保不会因为单个应用失败而中断整个加载
                            try 
                            {
                                var fallbackApp = new ToolboxItem
                                {
                                    Name = app?.Name ?? "应用",
                                    Icon = "&#xE8FC;",
                                    Background = "Gray",
                                    Description = "用户添加的应用",
                                    Path = app?.Path ?? "",
                                    IsFavorite = false // 🎯 初始化为false，收藏状态由LoadFavorites方法单独处理
                                };
                                // 🎯 检查路径重复以避免重复添加
                                var existingFallbackApp = AllApps.FirstOrDefault(a => 
                                    !string.IsNullOrEmpty(a.Path) && 
                                    !string.IsNullOrEmpty(fallbackApp.Path) && 
                                    a.Path.Equals(fallbackApp.Path, StringComparison.OrdinalIgnoreCase));
                                
                                if (existingFallbackApp == null)
                                {
                                    AllApps.Add(fallbackApp);
                                    addedToAllAppsCount++;
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"🎯 跳过重复备用应用: {fallbackApp.Name} (路径: {fallbackApp.Path})");
                                }
                            }
                            catch (Exception)
                            {
                                // 如果连备份都失败，继续处理下一个应用
                            }
                        }
                    }

                    // 触发属性更改通知，通知UI刷新AllApps显示
                    OnPropertyChanged(nameof(AllApps));
                }
                
                // 🎯 标记用户应用加载完成
                _isUserAppsLoaded = true;
                System.Diagnostics.Debug.WriteLine($"🎯 用户应用加载完成，总计添加: {config?.UserApps?.Count ?? 0} 个应用");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 加载用户应用数据失败: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ 异常详情: {ex}");
                // 如果加载失败，创建备份并重置用户应用列表
                await CreateBackupAndReset();
                // 🎯 即使失败也标记为已加载，避免重试
                _isUserAppsLoaded = true;
            }
        }

        /// <summary>
        /// 保存收藏状态到文件
        /// </summary>
        private async Task SaveFavorites()
        {
            try
            {
                var favoriteItems = new List<FavoriteItem>();

                // 遍历所有收藏应用，保存其收藏状态
                foreach (var app in FavoriteApps)
                {
                    favoriteItems.Add(new FavoriteItem
                    {
                        Name = app.Name,
                        Path = app.Path ?? "",
                        IsPreset = string.IsNullOrEmpty(app.Path) // 预设应用Path为空
                    });
                }

                var config = new FavoriteConfig
                {
                    FavoriteApps = favoriteItems,
                    LastModified = DateTime.Now,
                    Version = "1.0"
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var jsonString = JsonSerializer.Serialize(config, options);
                await File.WriteAllTextAsync(_favoritesFilePath, jsonString);
                
                System.Diagnostics.Debug.WriteLine($"🎯 收藏状态已保存: {_favoritesFilePath}, 收藏数量: {favoriteItems.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 保存收藏状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 从文件加载收藏状态
        /// </summary>
        private async Task LoadFavorites()
        {
            try
            {
                if (!File.Exists(_favoritesFilePath))
                {
                    System.Diagnostics.Debug.WriteLine("🎯 收藏配置文件不存在，使用默认空收藏列表");
                    return;
                }

                var jsonString = await File.ReadAllTextAsync(_favoritesFilePath);
                var config = JsonSerializer.Deserialize<FavoriteConfig>(jsonString);

                if (config?.FavoriteApps == null)
                {
                    System.Diagnostics.Debug.WriteLine("🎯 收藏配置为空");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"🎯 开始加载收藏状态，配置中收藏数量: {config.FavoriteApps.Count}");
                await File.WriteAllTextAsync("debug_step.txt", $"步骤1: 开始处理，config.FavoriteApps.Count = {config.FavoriteApps.Count}\n");

                // 先在后台线程收集要恢复的收藏应用
                var itemsToRestore = new List<ToolboxItem>();
                await File.AppendAllTextAsync("debug_step.txt", "步骤2: 开始收集收藏应用数据\n");

                // 根据收藏配置查找应用
                await File.AppendAllTextAsync("debug_step.txt", "步骤3: 即将进入foreach循环\n");
                await File.WriteAllTextAsync("foreach_test.txt", $"foreach循环开始，共{config.FavoriteApps.Count}项\n");
                foreach (var favoriteItem in config.FavoriteApps)
                {
                    await File.AppendAllTextAsync("foreach_test.txt", $"处理: {favoriteItem.Name}\n");
                    System.Diagnostics.Debug.WriteLine($"🎯 [LoadFavorites] 处理收藏项: {favoriteItem.Name}");
                    System.Diagnostics.Debug.WriteLine($"🎯 [LoadFavorites] 收藏项路径: '{favoriteItem.Path}'");
                    System.Diagnostics.Debug.WriteLine($"🎯 [LoadFavorites] 是否预设: {favoriteItem.IsPreset}");

                    ToolboxItem? targetApp = null;

                    if (favoriteItem.IsPreset && string.IsNullOrEmpty(favoriteItem.Path))
                    {
                        // 预设应用：根据名称查找
                        System.Diagnostics.Debug.WriteLine($"🎯 [LoadFavorites] 查找预设应用: {favoriteItem.Name}");
                        targetApp = AllApps.FirstOrDefault(app => 
                            app.Name == favoriteItem.Name && string.IsNullOrEmpty(app.Path));
                        
                        if (targetApp == null)
                        {
                            // 尝试在所有预设应用中查找（调试用）
                            var presetApps = AllApps.Where(app => string.IsNullOrEmpty(app.Path)).ToList();
                            System.Diagnostics.Debug.WriteLine($"🎯 [LoadFavorites] 当前预设应用:");
                            foreach (var preset in presetApps)
                            {
                                System.Diagnostics.Debug.WriteLine($"   - {preset.Name}");
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(favoriteItem.Path))
                    {
                        // 用户应用：根据路径查找
                        System.Diagnostics.Debug.WriteLine($"🎯 [LoadFavorites] 查找用户应用: {favoriteItem.Name}");
                        targetApp = AllApps.FirstOrDefault(app => 
                            !string.IsNullOrEmpty(app.Path) && 
                            app.Path.Equals(favoriteItem.Path, StringComparison.OrdinalIgnoreCase));
                        
                        if (targetApp == null)
                        {
                            // 尝试在所有用户应用中查找（调试用）
                            var userApps = AllApps.Where(app => !string.IsNullOrEmpty(app.Path)).ToList();
                            System.Diagnostics.Debug.WriteLine($"🎯 [LoadFavorites] 当前用户应用:");
                            foreach (var userApp in userApps)
                            {
                                System.Diagnostics.Debug.WriteLine($"   - {userApp.Name} (路径: {userApp.Path})");
                            }
                        }
                    }

                    if (targetApp != null)
                    {
                        targetApp.IsFavorite = true;
                        // 添加到临时列表，稍后在UI线程中批量更新
                        itemsToRestore.Add(targetApp);
                        System.Diagnostics.Debug.WriteLine($"🎯 [LoadFavorites] ✅ 找到收藏应用: {targetApp.Name} (预设: {favoriteItem.IsPreset})");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"🎯 [LoadFavorites] ❌ 未找到收藏应用: {favoriteItem.Name} (路径: '{favoriteItem.Path}')");
                    }
                }

                // 在UI线程中批量更新收藏列表
                await File.AppendAllTextAsync("debug_step.txt", $"步骤4: 找到{itemsToRestore.Count}个收藏应用，准备UI更新\n");
                
                // 获取主窗口的DispatcherQueue进行UI更新
                var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
                if (dispatcherQueue == null)
                {
                    // 如果无法获取当前线程的Dispatcher，尝试使用静态方法
                    await File.AppendAllTextAsync("debug_step.txt", "步骤5: 当前线程无Dispatcher，需要在UI线程执行\n");
                    System.Diagnostics.Debug.WriteLine("🎯 [LoadFavorites] 警告：无法获取DispatcherQueue，将通过回调在UI线程更新");
                    
                    // 暂存收藏数据，稍后在UI线程更新
                    _pendingFavoriteRestore = itemsToRestore;
                }
                else
                {
                    await File.AppendAllTextAsync("debug_step.txt", "步骤5: 在UI线程更新收藏列表\n");
                    dispatcherQueue.TryEnqueue(() =>
                    {
                        // 清空并重新添加
                        FavoriteApps.Clear();
                        foreach (var item in itemsToRestore)
                        {
                            FavoriteApps.Add(item);
                        }
                        // 更新循环收藏列表
                        CreateLoopingFavorites();
                        System.Diagnostics.Debug.WriteLine($"🎯 收藏状态恢复完成，收藏数量: {FavoriteApps.Count}");
                    });
                }

                System.Diagnostics.Debug.WriteLine($"🎯 收藏状态加载完成，找到收藏应用: {itemsToRestore.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 加载收藏状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 在UI线程中恢复收藏状态（当无法获取DispatcherQueue时的备用方案）
        /// </summary>
        private void RestoreFavoritesInUIThread(List<ToolboxItem> itemsToRestore)
        {
            try 
            {
                // 这个方法应该在UI线程中调用
                FavoriteApps.Clear();
                foreach (var item in itemsToRestore)
                {
                    FavoriteApps.Add(item);
                }
                CreateLoopingFavorites();
                System.Diagnostics.Debug.WriteLine($"🎯 [RestoreFavoritesInUIThread] 收藏状态恢复完成，收藏数量: {FavoriteApps.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🎯 [RestoreFavoritesInUIThread] 恢复收藏失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 完成收藏恢复（在UI线程中调用）
        /// </summary>
        public void CompleteFavoriteRestore()
        {
            if (_pendingFavoriteRestore != null)
            {
                try 
                {
                    // 在UI线程中更新收藏列表
                    FavoriteApps.Clear();
                    foreach (var item in _pendingFavoriteRestore)
                    {
                        FavoriteApps.Add(item);
                    }
                    CreateLoopingFavorites();
                    System.Diagnostics.Debug.WriteLine($"🎯 [CompleteFavoriteRestore] 收藏状态恢复完成，收藏数量: {FavoriteApps.Count}");
                    
                    // 清除暂存
                    _pendingFavoriteRestore = null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"🎯 [CompleteFavoriteRestore] 恢复收藏失败: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("🎯 [CompleteFavoriteRestore] 没有待恢复的收藏数据");
            }
        }

        /// <summary>
        /// 创建配置文件备份并重置
        /// </summary>
        private async Task CreateBackupAndReset()
        {
            try
            {
                if (File.Exists(_userAppsFilePath))
                {
                    var backupPath = $"{_userAppsFilePath}.backup.{DateTime.Now:yyyyMMdd_HHmmss}";
                    File.Copy(_userAppsFilePath, backupPath);
                    System.Diagnostics.Debug.WriteLine($"配置文件已备份到: {backupPath}");
                }
                
                _userAddedApps.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建备份失败: {ex.Message}");
            }
        }

        // INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 
