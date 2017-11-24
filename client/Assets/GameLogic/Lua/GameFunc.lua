module("GameFunc",package.seeall)
local traceback = debug.traceback
local print = print
function LoadAsset()
end

function Log(format,...)
    xpcall(print,traceback,string.format(format,...))
end

function LogError(format,...)
    xpcall(GameCore.LogMgr.Log,traceback,string.format(format,...))
end