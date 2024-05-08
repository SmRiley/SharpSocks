# SharpSocks
> 相当于LightSocks的C#版本(一个用于学习目的的Socks5代理)
>   
 使用C#语言基于.Net 8(服务端) / .Net Avalonia(本地端)实现的Socks5代理  
 可用于学习了解Socks代理原理和MAUI的简单使用
### 特点  

- ⚡:轻量级网络混淆代理，基于 SOCKS5 协议.
- 🙈:流量混淆转发,可绕过带内容控制的防火墙.
- 📦️:服务端基于.Net 8,客户端基于.Net Avalonia,可跨平台编译与部署.
- ✏️:默认为最基础的加解密流,可自定义加解密函数提高数据安全性.
- ✨:不会放大传输流量，传输流量更少更快.
- 🚀:多端口多用户支持.

### 使用
#### 下载
> 出于教学目的,不提供编译好的Release,需自行编译

1. 拉取源码并使用Visual Studio 2022打开(需安装.Net8和MAUI框架)  
2. 对所需平台进行编译(MAUI支持Windows/Android/IOS/Mac)
#### 安装
Client:按生成的对应文件直接安装  
Server:安装.Net环境,如果发布为独立程序可跳过这一步
1. [WIndows](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial/install "WIndows")
2. [Linux](https://docs.microsoft.com/zh-cn/dotnet/core/install/linux-package-manager-centos7 "Linux")

编辑Config.json,默认文件中为
```json
[
  {
    "Port": 3080,
    "Pass": "123456",
    "Udp": true
  },
  {
    "Port": 2080,
    "Pass": "123456",
    "Udp": false
  }
]
```
每一个数组对象代表一个代理端口,"Udp"表示是否开启UDP支持,如果不需要,则修改为false即可(如下面的2080对象)
按照实际需要修改即可.修改后请效验格式
#### 运行
Server:
- Windows:直接运行Server.exe
- Linux: `dotnet Server.dll` 或者 `./server.dll`(独立程序)

Local
填入Server端服务器信息(如图),点击启动

![Local](https://raw.githubusercontent.com/SmRiley/Imgs/master/SharpSocks/20220826114934.png)

![Server](https://raw.githubusercontent.com/SmRiley/Imgs/master/SharpSocks/20220826115141.png)
### 注意
1. **加解密仅仅为最基础的的凯撒密码,在现代密码学下是十分薄弱的,不可用于生产环境**
2. Local 和 Server 的 password 必须一致才能正常使用，password 不要泄露
3. 安全组只需要放行相应TCP端口,但是如果需要UDP支持,请在服务器中放行全部UDP端口的通行.
4. Local并不会像SS一样直接代理浏览器的HTTP(HTTPS)请求,可搭配[SwitchyOmega](https://github.com/FelisCatus/SwitchyOmega "SwitchyOmega")类似的插件或程序使用.

### 参考:
1. https://github.com/gwuhaolin/lightsocks
2. https://github.com/gwuhaolin/blog/issues/12
