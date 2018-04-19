module("GameMain",package.seeall)

function OnLoad(fileName,bytes)
    local x;
end

function GameInit()
    require("GameDebug")
    GameCore.ResMgr.LoadSceneAsync("start",OnLoad);
    --状态机
end

function GameQuit()

end
