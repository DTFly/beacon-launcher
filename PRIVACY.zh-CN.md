# 隐私政策

最后更新：2026-10-29

Beacon Launcher 是一个开源的 Minecraft: Java Edition 桌面启动器。本政策说明启动器会处理哪些数据。

## 概要

- 不收集遥测、分析、广告或追踪数据。
- 不向开发者发送任何个人数据。
- 启动器设置只保存在用户自己的设备上。
- 微软账号登录（实现后）由用户、Microsoft、Xbox Live 和 Minecraft Services 直接完成。

## 本地数据

启动器把设置保存在 `%APPDATA%\.bl\config.json`：

- 启动器/标题栏名称
- 主题和强调色
- 窗口宽度和高度
- 用于配置迁移的主题版本号

这个文件不包含账号凭据。

## 微软账号登录（计划中）

当前版本还没有微软登录。实现后，启动器计划使用官方 Microsoft 身份平台（`consumers` 租户），请求 `XboxLive.signin offline_access` scope，然后调用官方 Xbox Live/XSTS 和 Minecraft Services API。

认证直接与 Microsoft 完成，开发者不会收到用户密码。如果启动器在本地保存 access token 或 refresh token 以保持登录，token 只保存在用户设备上；退出登录或删除本地数据即可移除。

## 网络请求

当前版本不发送遥测。未来版本只会在登录和启动游戏时访问 Microsoft、Xbox Live 和 Minecraft Services；启动器不运营自己的数据收集服务器。

## 数据共享

开发者不会出售、出租或共享个人数据。只有在登录和启动游戏所必需时，才会与上述官方服务交换数据。

## 用户控制

删除 `%APPDATA%\.bl` 即可随时移除启动器的本地数据。登录功能实现后，退出登录会移除本地保存的会话。

## 变更

如果本政策发生变化，新版本会提交到项目仓库，并更新“最后更新”日期。

## 联系方式

请在项目的 GitHub 仓库中提交 issue。

## 声明

非 Minecraft 官方产品，与 Mojang Studios 或 Microsoft 无关联、未获其批准。“Minecraft” 是 Mojang Synergies AB 的商标。
