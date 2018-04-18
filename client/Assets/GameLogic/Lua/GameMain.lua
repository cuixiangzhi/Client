module("GameMain",package.seeall)

function OnLoad(fileName,bytes)
    local x;
end

function GameInit()
    require("GameDebug")
    local val1 = GameCore.ResMgr.LoadSceneAsync("0",OnLoad);
    local val2 = GameCore.ResMgr.LoadSceneAsync("0",OnLoad);
    local val3 = GameCore.ResMgr.LoadBytesAsync("0",OnLoad);
end

function GameQuit()

end
