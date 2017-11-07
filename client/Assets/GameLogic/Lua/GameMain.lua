--游戏入口
require(jit and "debug.LuaDebugjit" or "debug.LuaDebug")("10.12.20.254",7003);
local function Update()
    print("hello lua");
end
UpdateBeat:Add(Update);