module("GameFunc",package.seeall)
local traceback = debug.traceback
local print = print
local print_error = print_error
function load()
    local function OnAssetLoad(func,asset)

    end
end

function log(format,...)
    xpcall(print,traceback,string.format(format,...))
end

function error(format,...)
    xpcall(print_error,traceback,string.format(format,...))
end