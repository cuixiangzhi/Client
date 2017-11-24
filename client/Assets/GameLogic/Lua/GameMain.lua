--游戏入口
module("GameMain",package.seeall)
--连接调试
require(jit and 'debug.LuaDebugjit' or 'debug.LuaDebug')('localhost', 7003)
--C# API
require "GameFunc"
--打开界面
GameFunc.OpenWindow()
