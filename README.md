# SharpSocks
## 使用C#语言基于.Net Core3.1(服务端) / Framework 4.7(本地端)实现的Socks5代理
### 特点
 一个轻量级网络混淆代理，基于 SOCKS5 协议，可用来代替 Shadowsocks。
- 流量混淆转发,可绕过带内容控制的防火墙.
- 服务端基于.net core3.1的跨平台,可跨平台部署.
- UDP转发阈值限制
- 默认为基础的加解密流,可自定义加解密函数提高数据安全性.
- 不会放大传输流量，传输流量更少更快
- 多端口多用户支持

### 使用
#### 下载
去 [releases](https://github.com/SmRiley/SharpSocks5/releases "releases") 页下载最新的可执行文件，注意选择正确的操作系统和位数（Mac 系统内核为 darwin）。 解压后会看到2个可执行文件，分别是：

Socks5Local：用于运行在本地电脑的客户端，用于桥接本地浏览器和远程代理服务，传输前会混淆数据；
Socks5Server：用于运行在代理服务器的客户端，会还原混淆数据(**Linux服务器请下载Linux-Server.zip,Windows服务器请下载Windows-Server.zip**)；

#### 安装
Local:直接运行exe文件,正常更新的Windows10应当内置了Framework4.72,否则会提示需要下载安装,按照提示安装即可.
Server:安装.Net Core3.1运行环境,
1. [WIndows](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial/install "WIndows")
2. [Linux](https://docs.microsoft.com/zh-cn/dotnet/core/install/linux-package-manager-centos7 "Linux")

编辑Config.json,默认文件中为
```json
[
    {
        "Port": "1080",
        "Pass": "123456",
        "UDP":"true"
    }
,
    {
        "Port": "2080",
        "Pass": "123456",
        "UDP":"false"
    }
]
```
每一个对象格式代表一个代理端口,"UDP"表示是否开启UDP支持,如果不需要,则修改为false即可(如下面的2080对象)
按照实际需要修改即可.修改后建议进行效验(http://tool.chinaz.com/Tools/jsonformat.aspx)
#### 运行
Server:
- Windows:直接运行Socks5Server.exe
- Linux: dotnet Socks5Server.dll
Local
填入Server端服务器信息(如图),点击启动
![Local.jpg](https://cdn.sauyoo.com/2020/01/22/1579691572.jpg)

*开始科学的上网吧~*
![TIM截图20200122192851.jpg](https://cdn.sauyoo.com/2020/01/22/1579692549.jpg)
### 注意
1. Local 和 Server 的 password 必须一致才能正常使用，password 不要泄露
2. 安全组只需要放行相应TCP端口,但是如果需要UDP支持,请在服务器中放行全部UDP端口的通行.
3. Local并不会像SS一样直接代理浏览器的HTTP(HTTPS)请求,建议Chrome搭配[SwitchyOmega](https://github.com/FelisCatus/SwitchyOmega "SwitchyOmega")使用.