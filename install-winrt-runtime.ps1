# Windows App Runtime 1.7 安装脚本
# 适用于解决 WinUI 3 应用程序的依赖问题

Write-Host "=== Windows App Runtime 1.7 安装器 ===" -ForegroundColor Green
Write-Host ""

# 检测系统架构
$arch = $env:PROCESSOR_ARCHITECTURE
Write-Host "检测到系统架构: $arch" -ForegroundColor Yellow

# 根据架构选择下载URL
switch ($arch) {
    "AMD64" { 
        $url = "https://aka.ms/windowsappsdk/1.7/1.7.250606001/windowsappruntimeinstall-x64.exe"
        $filename = "windowsappruntimeinstall-x64.exe"
        Write-Host "使用 x64 版本" -ForegroundColor Cyan
    }
    "x86" { 
        $url = "https://aka.ms/windowsappsdk/1.7/1.7.250606001/windowsappruntimeinstall-x86.exe"
        $filename = "windowsappruntimeinstall-x86.exe"
        Write-Host "使用 x86 版本" -ForegroundColor Cyan
    }
    "ARM64" { 
        $url = "https://aka.ms/windowsappsdk/1.7/1.7.250606001/windowsappruntimeinstall-arm64.exe"
        $filename = "windowsappruntimeinstall-arm64.exe"
        Write-Host "使用 ARM64 版本" -ForegroundColor Cyan
    }
    default { 
        $url = "https://aka.ms/windowsappsdk/1.7/1.7.250606001/windowsappruntimeinstall-x64.exe"
        $filename = "windowsappruntimeinstall-x64.exe"
        Write-Host "使用默认 x64 版本" -ForegroundColor Cyan
    }
}

$output = "$env:TEMP\$filename"

try {
    Write-Host ""
    Write-Host "正在下载 Windows App Runtime 1.7..." -ForegroundColor Yellow
    Write-Host "下载地址: $url" -ForegroundColor Gray
    
    # 下载文件
    Invoke-WebRequest -Uri $url -OutFile $output -UseBasicParsing
    
    Write-Host "✅ 下载完成！" -ForegroundColor Green
    Write-Host "文件位置: $output" -ForegroundColor Gray
    Write-Host ""
    
    # 询问是否立即安装
    $install = Read-Host "是否立即安装？(Y/N)"
    
    if ($install -eq "Y" -or $install -eq "y" -or $install -eq "是") {
        Write-Host "正在启动安装程序..." -ForegroundColor Yellow
        Write-Host "请按照安装向导完成安装。" -ForegroundColor Cyan
        
        # 启动安装程序
        Start-Process -FilePath $output -Wait
        
        Write-Host ""
        Write-Host "✅ 安装完成！" -ForegroundColor Green
        Write-Host "现在您可以运行您的 WinUI 3 应用程序了。" -ForegroundColor Cyan
    } else {
        Write-Host "安装程序已下载到: $output" -ForegroundColor Yellow
        Write-Host "您可以稍后手动运行它来完成安装。" -ForegroundColor Cyan
    }
    
} catch {
    Write-Host "❌ 错误: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "请尝试手动下载:" -ForegroundColor Yellow
    Write-Host $url -ForegroundColor Cyan
}

Write-Host ""
Write-Host "安装完成后，请重新运行您的应用程序。" -ForegroundColor Green
Write-Host "按任意键继续..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") 