# Calypso监控系统

Calypso监控系统是一个基于ASP.NET Core和Blazor的Web应用程序，用于监控Zeiss Calypso测量机的状态和数据。该应用程序必须在运行Calypso软件的计算机上执行，以便访问测量数据。

## 功能特点

- 实时监控Calypso测量机状态（运行中、暂停、停止、完成、错误）
- 查看当前测量计划数据
- 查看测量结果
- 事件日志记录
- 支持Windows操作系统

## 系统要求

- Windows操作系统
- .NET 6.0或更高版本
- Zeiss Calypso软件

## 安装步骤

1. 确保已安装.NET 6.0或更高版本
2. 下载或克隆本项目
3. 使用Visual Studio 2022或更高版本打开解决方案
4. 构建并运行项目

## 配置说明

在`appsettings.json`文件中，您可以配置以下选项：

```json
{
  "Calypso": {
    "ObserverPath": "C:\\Users\\Public\\Documents\\Zeiss\\CMMObserver",
    "ApiUrl": "http://localhost:5000"
  }
}
```

- `ObserverPath`: Calypso CMMObserver文件夹的路径，通常位于`C:\Users\Public\Documents\Zeiss\CMMObserver`
- `ApiUrl`: Calypso WebAPI的URL地址

## 使用方法

1. 启动应用程序
2. 在首页点击"连接到Calypso"按钮
3. 连接成功后，您可以在不同的页面查看Calypso的状态和数据：
   - 首页：显示基本状态和事件日志
   - 状态监控：显示详细的状态信息和测量计划
   - 测量数据：显示测量结果和原始数据
   - 设置：配置应用程序参数

## 开发说明

本项目使用以下技术：

- ASP.NET Core 6.0
- Blazor Server
- Bootstrap 5.1.3
- CalypsoAPI库

## 许可证

本项目基于MIT许可证开源。 