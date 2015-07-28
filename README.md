# RoyNet

More Information:
http://ifchen.com/

## RoyNet是一个简单的游戏服务器。

最终目的是能够运行一个简单的MMORPG游戏。你可以通过地址来获取它的源码。

主要使用到的类库有：

+ SuperSocket
+ NancyFx
+ ProtoBuf-net

Server.Login是一个账号和令牌管理服务器

Server.Gate 主要负责客户端的连接管理和游戏服间的转发

Server.GameEngine 主要是游戏逻辑服务器传输层代码

Server.Game 引用了Server.GameEngine，是书写业务逻辑的地方

解决方案中包含了一个Client的项目，作为客户端的访问示例。还有一个ChatModule作为模块开发的示例，ChatModule.ChatCommand中有消息接收和群发回复、定点回复的示例。

## 创建和启动一组服务器

在机能允许的情况下登录服可以共用服务于多个游戏。Gate应该服务于单个区服多个场景Game服（如地形分或者副本分或者战斗-世界分）。

上述三个Login、Gate、Game合称一组服务器，其核心3个类库Server.Login\Server.Gate\Server.Game中都有一个继承自ServerBase的类型。启动一个服务器，你只需要在Main函数中创建改类型的实例并调用Configure()、Startup()方法即可。关闭的时候调用Stop()方法。

你也可以使用解决方案中包含的Launcher项目生成的Launcher.exe，将所有配置都包含到royNet配置节点中，使用它将通过配置中的内容反射生成服务器实例，并将服务器一并启动。所以请将引用到的所有类库都放置到同目录下。
通过Launcher启动的服务器之间是应用程序域隔离的。
启动后可以通过指令关闭或重启指定的服务器。

具体可以直接看Launcher.Test，不过它添加了所有解决方案中的一用，是个测试用项目，方便VS调试。是个很好的示例。

## 一个完整的登录包含以下几个步骤：

  一、向LoginServer发送账号密码，请求token和服务器列表
  
  二、获得token和服务器列表之后，选择服务器，连接改服务器的ip和port，按照协议发送token
  
  三、如果token验证成功，服务器返回1byte，登录成功。可进行玩法操作了。

正在进行的一项改进是通过DestID，来让GateServer支持转发多个不同的游戏区服（已存在配置项）。

上述服务器没有开启先后顺序，但未避免通讯数据丢失，还是请在GameServer开启之后再启动GateServer和LoginServer。

