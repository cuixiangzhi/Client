module("GameCore",package.seeall)
local traceback = debug.traceback
local print = print
local print_error = print_error

function Log(format,...)
    xpcall(print,traceback,string.format(format,...))
end

function LogError(format,...)
    xpcall(print_error,traceback,string.format(format,...))
end

function LoadAsset()

end

function LoadBytes()

end