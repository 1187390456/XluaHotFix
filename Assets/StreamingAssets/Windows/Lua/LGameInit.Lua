LGameInit = {}

-- 静态方法用. 其他方法用:
-- CS中的引用类型需要在LuaManager添加命名空间
local this = LGameInit

function LGameInit:GetInstance()
  return this
end

LGameInit.lp = function(name, process)
  print(name, process)
  if process >= 1 then
    CS.GameInit.Instance:TestFunc1()
  end
end

function LGameInit:Init()
  CS.AssetBundleManager.Instance:LoadAssetBundle("main", "UI", this.lp)
end
