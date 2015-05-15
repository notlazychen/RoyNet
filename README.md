# RoyNet

More Information:
http://ifchen.com/

##RoyNet是一个简单的游戏服务器。

最终目的是能够运行一个简单的MMORPG游戏。你可以通过地址来获取它的源码。

主要使用到的类库有：

+ SuperSocket
+ NetMQ
+ NancyFx
+ ProtoBuf-net

LoginServer是一个账号和令牌管理服务器
GateServer主要负责客户端的连接管理和游戏服间的转发
GameSever是游戏逻辑服务器

解决方案中包含了一个Client的项目，作为客户端的访问示例。
一个完整的登录包含以下几个步骤：

  一、向LoginServer发送账号密码，请求token和服务器列表
  
  二、获得token和服务器列表之后，选择服务器，连接改服务器的ip和port，按照协议发送token
  
  三、如果token验证成功，服务器返回1byte，登录成功。可进行玩法操作了。


可以通过修改LoginServer的App.Config来向服务器列表添加服务器。

支持一个GateServer通过同一个消息队列对应多个GameServer实例来分流玩家。

正在进行的一项改进是通过DestID，来让GateServer支持转发多个不同的游戏区服。

上述服务器没有开启先后顺序，但未避免通讯数据丢失，还是请在GameServer开启之后再启动GateServer和LoginServer。
