module("GameMain",package.seeall)

function OnLoadFinish(path,obj)

end

function GameInit()
    require("GameDebug")
    require("GameCoreA")
    reimport("GameCoreA")
end

function GameQuit()

end
