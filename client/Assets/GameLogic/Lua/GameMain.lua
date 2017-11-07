--游戏入口
require "Debug.LuaDebug"
local DEBUGGER_FILE = "LuaDebug";
if jit then
    DEBUGGER_FILE = "LuaDebugjit";
end
local breakInfoFunc = require(DEBUGGER_FILE)(host or "localhost",port or 7003)
local timer = Timer.New(breakInfoFunc,0.01,-1,1);
timer:Start();
print("hello lua");