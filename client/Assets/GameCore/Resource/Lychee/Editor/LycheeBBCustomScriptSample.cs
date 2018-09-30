using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

public class LycheeBBCustomScriptSample : LycheeBB.CustomScript
{
    public void Awake()
    {
    }

    public bool HasUI()
    {
        return true;
    }

    public void DrawUI()
    {
        EditorGUILayout.LabelField("Yes you can add custom UI here !");
    }

    public bool PreBuildEvent(LycheeBB.BuildArguments args)
    {
        groups_ = new List<LycheeBB.Pair<int, string[]>>();

        groups_.Add(LycheeBB.Utility.MakePair(1000, new string[] {
            "Assets/Players/Liam/Liam.prefab",
        }));

        groups_.Add(LycheeBB.Utility.MakePair(1001, new string[] {
            "Assets/Players/Jacob/Jacob.prefab",
        }));

        groups_.Add(LycheeBB.Utility.MakePair(1002, new string[] {
            "Assets/Players/Emma/Emma.prefab",
        }));

        groups_.Add(LycheeBB.Utility.MakePair(1003, new string[] {
            "Assets/Monsters/Sphinx/Sphinx.prefab",
            "Assets/Monsters/Cyclops/Cyclops.prefab",
            "Assets/Monsters/Chimera/Chimera.prefab",
            "Assets/Monsters/Empusa/Empusa.prefab",
        }));

        groups_.Add(LycheeBB.Utility.MakePair(1004, new string[] {
            "Assets/Prefabs/Towers/Laser/LaserTower.prefab",
            "Assets/Prefabs/Towers/Laser/LaserTower_0.prefab",
            "Assets/Prefabs/Towers/Laser/LaserTower_1.prefab",
            "Assets/Prefabs/Towers/Laser/LaserTower_2.prefab",
        }));

        customFiles_ = new List<string[]>();

        customFiles_.Add(new string[] {
            "Stage/Scripts.zip",
            "Stage/Tables.zip",
        });

        customFiles_.Add(new string[] {
            "Stage/Other.ext",
        });

        index_ = 0;

        return true;
    }

    public void PostBuildEvent(LycheeBB.BuildResult result)
    {
        foreach (string p in result.GetSceneBundles())
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(result.GetBundleAssets(p)[0]);
            sb.Append(": ");

            string[] dependencies = result.GetBundleDependencies(p, true);

            sb.Append("(");
            sb.Append(dependencies.Length);
            sb.Append(") ");

            for (int i = 0; i < dependencies.Length; ++i)
            {
                if (i > 0) {
                    sb.Append(", ");
                }

                sb.Append(dependencies[i]);
            }

            Debug.Log(sb.ToString());
        }
    }

    public bool AddGroup(out int groupId, out string[] assets)
    {
        if (index_ < groups_.Count)
        {
            groupId = groups_[index_].first;
            assets = groups_[index_].second;

            ++index_;

            return true;
        }
        else
        {
            groupId = 0;
            assets = null;

            index_ = 0;

            return false;
        }
    }

    public bool AddCustomFiles(out string[] files)
    {
        if (index_ < customFiles_.Count)
        {
            files = customFiles_[index_];

            ++index_;

            return true;
        }
        else
        {
            files = null;
            index_ = 0;

            return false;
        }
    }

    private List<LycheeBB.Pair<int, string[]>> groups_;
    private List<string[]> customFiles_;
    private int index_;
}

