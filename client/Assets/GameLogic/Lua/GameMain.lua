module("GameMain",package.seeall)

function OnLoadFinish(path,obj)

end

function GameInit()
    require("GameDebug")
    local func = System.Action_string_UnityEngine_Object(OnLoadFinish);
    GameCore.ResMgr.LoadAssetAsync("",func);
    GameCore.ResMgr.LoadAssetAsync("",func);
end

function GameQuit()

end
