require(jit and 'debug.LuaDebugjit' or 'debug.LuaDebug')('localhost', 7003)
local traceback = debug.traceback
local print = print
local print_error = print_error

function log(format,...)
    xpcall(print,traceback,string.format(format,...))
end

function err(format,...)
    xpcall(print_error,traceback,string.format(format,...))
end