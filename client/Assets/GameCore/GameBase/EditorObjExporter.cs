///*
//Based on ObjExporter.cs, this "wrapper" lets you export to .OBJ directly from the editor menu.

//This should be put in your "Editor"-folder. Use by selecting the objects you want to export, and select
//the appropriate menu item from "Custom->Export". Exported models are put in a folder called
//"ExportedObj" in the root of your Unity-project. Textures should also be copied and placed in the
//same folder. */

//using UnityEngine;
//using UnityEditor;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using System;


//public class EditorObjExporter : EditorWindow
//{
//    const string GROUP_BACK_GROUND = "background";
//    const string GROUP_BLOCK = "block";
//    const string GROUP_OHTER_FLAGS = "door";
//    static string SCENE_IMPORT_PATH = Application.dataPath + "/ArtRes/Map";

//    //导出时的错误列表
//    static List<string> errorList = new List<string>();

//    private static int vertexOffset = 0;
//    private static int normalOffset = 0;
//    private static int uvOffset = 0;

//    //网格数据导出目录,unity相对于根目录
//    private static string MESH_FOLDER = "../tools/NavMeshGen/Meshes";
//    //批处理路径
//    private static string BAT_PATH = "../tools/NavMeshGen/NavMeshGen.bat";


//    [MenuItem("Uncle/NavMesh/导出选中物体的网格数据")]
//    public static void ExportWholeSelectionToSingleObj()
//    {
//        if (!CreateTargetFolder(MESH_FOLDER))
//            return;

//        Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);

//        List<MeshFilter> mfList = new List<MeshFilter>();

//        for (int i = 0; i < selection.Length; i++)
//        {
//            MeshFilter[] meshfilter = selection[i].GetComponentsInChildren<MeshFilter>();

//            for (int m = 0; m < meshfilter.Length; m++)
//            {
//                mfList.Add(meshfilter[m]);
//            }
//        }

//        ExMeshs(mfList, MESH_FOLDER, GetFileName());

//        //执行批处理，调用Raycastdemo生成寻路图
//        ExcuteBatCommand();
//    }


//    [MenuItem("Uncle/NavMesh/导出所有场景的网格数据")]
//    public static void ExportAllSceneMesh()
//    {
//        string[] scenes = Directory.GetFiles(SCENE_IMPORT_PATH, "*.unity", SearchOption.AllDirectories);
//        try
//        {
//            for (int i = 0; i < scenes.Length; i++)
//            {
//                if (EditorUtility.DisplayCancelableProgressBar("进度", EditorApplication.currentScene, (float)i / scenes.Length))
//                {
//                    EditorUtility.ClearProgressBar();
//                }

//                EditorApplication.OpenScene(scenes[i]);
//                ExportGroupToSingleObj();
//            }
//            //执行批处理，调用Raycastdemo生成寻路图
//            ExcuteBatCommand();
//        }
//        catch (System.Exception ex)
//        {
//            errorList.Add(ex.Message + ex.StackTrace);
//        }
//        finally
//        {
//            EditorUtility.ClearProgressBar();
//            if (errorList.Count == 0)
//            {
//                EditorUtility.DisplayDialog("SUCCESS", "生成成功", "确定");
//            }
//            else
//            {
//                StringBuilder sb = new StringBuilder();
//                for (int j = 0; j < errorList.Count; j++)
//                {
//                    sb.Append(errorList[j]);
//                }
//                EditorUtility.DisplayDialog("ERROR", sb.ToString(), "确定");
//            }
//        }
//    }


//    //导出单个场景的寻路数据
//    public static void ExportGroupToSingleObj()
//    {
//        if (!CreateTargetFolder(MESH_FOLDER))
//            return;

//        GameObject bg = GameObject.Find(GROUP_BACK_GROUND);
//        GameObject block = GameObject.Find(GROUP_BLOCK);
//        GameObject other = GameObject.Find(GROUP_OHTER_FLAGS);


//        if (bg != null)
//        {
//            //基本地图网格
//            List<MeshFilter> mfList = new List<MeshFilter>();

//            MeshFilter[] bgMF = bg.transform.GetComponentsInChildren<MeshFilter>();
//            for (int m = 0; m < bgMF.Length; m++)
//            {
//                mfList.Add(bgMF[m]);
//            }

//            if (block != null)
//            {
//                MeshFilter[] blockMF = block.transform.GetComponentsInChildren<MeshFilter>();
//                for (int n = 0; n < blockMF.Length; n++)
//                {
//                    mfList.Add(blockMF[n]);
//                }
//            }

//            ExMeshs(mfList, MESH_FOLDER, GetFileName());
//        }

//        if (other != null)
//        {
//            //标志物体网格
//            List<MeshFilter> mfList = new List<MeshFilter>();

//            MeshFilter[] meshfilter = other.transform.GetComponentsInChildren<MeshFilter>();
//            for (int m = 0; m < meshfilter.Length; m++)
//            {
//                mfList.Add(meshfilter[m]);
//            }
//            ExConvexArea(mfList, MESH_FOLDER, GetFileName());
//        }
//    }


//    //导出基本网格数据
//    private static void ExMeshs(List<MeshFilter> mf, string folder, string filename)
//    {
//        PrepareFileWrite();

//        if (mf == null || mf.Count == 0)
//        {
//            errorList.Add(string.Format("{0}: no meshfilter found", filename));
//            return;
//        }

//        using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".obj"))
//        {
//            sw.Write("mtllib ./" + filename + ".mtl\n");

//            for (int i = 0; i < mf.Count; i++)
//            {
//                sw.Write(MeshToString(mf[i]));
//            }
//        }
//    }

//    private static string MeshToString(MeshFilter mf)
//    {
//        Mesh m = mf.sharedMesh;

//        if (null == m)
//        {
//            Debug.Log("invalid object with null Mesh : " + mf.ToString());
//            EditorUtility.DisplayDialog("Error!", "invalid object with null Mesh : " + mf.ToString(), "");
//            return string.Empty;
//        }


//        StringBuilder sb = new StringBuilder();

//        sb.Append("g ").Append(mf.name).Append("\n");
//        foreach (Vector3 lv in m.vertices)
//        {
//            Vector3 wv = mf.transform.TransformPoint(lv);

//            //This is sort of ugly - inverting x-component since we're in
//            //a different coordinate system than "everyone" is "used to".
//            sb.Append(string.Format("v {0} {1} {2}\n", wv.x, wv.y, wv.z));
//        }

//        foreach (Vector3 lv in m.normals)
//        {
//            Vector3 wv = mf.transform.TransformDirection(lv);

//            sb.Append(string.Format("vn {0} {1} {2}\n", wv.x, wv.y, wv.z));
//        }

//        foreach (Vector3 v in m.uv)
//        {
//            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
//        }

//        for (int material = 0; material < m.subMeshCount; material++)
//        {

//            int[] triangles = m.GetTriangles(material);
//            for (int i = 0; i < triangles.Length; i += 3)
//            {
//                //Because we inverted the x-component, we also needed to alter the triangle winding.
//                sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n",
//                    triangles[i] + 1 + vertexOffset, triangles[i + 1] + 1 + normalOffset, triangles[i + 2] + 1 + uvOffset));
//            }
//        }

//        vertexOffset += m.vertices.Length;
//        normalOffset += m.normals.Length;
//        uvOffset += m.uv.Length;

//        return sb.ToString();
//    }

//    //导出特殊标志区域
//    private static void ExConvexArea(List<MeshFilter> mf, string folder, string filename)
//    {
//        if (mf == null || mf.Count == 0)
//        {
//            return;
//        }
//        using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".txt"))
//        {
//            //文件名
//            sw.Write("f " + "Meshes/" + filename + ".obj\n");

//            for (int i = 0; i < mf.Count; i++)
//            {
//                sw.Write(ConvexAreaToString(mf[i], (int)SamplePolyAreas.SAMPLE_POLYAREA_CUSTOM0 + i));
//            }
//        }

//    }

//    private static string ConvexAreaToString(MeshFilter mf, int areaFlag)
//    {
//        Mesh m = mf.sharedMesh;

//        if (null == m)
//        {
//            Debug.Log("invalid object with null Mesh : " + mf.ToString());
//            EditorUtility.DisplayDialog("Error!", "invalid object with null Mesh : " + mf.ToString(), "");
//            return string.Empty;
//        }

//        float minh = float.MaxValue;
//        float maxh = float.MinValue;

//        //计算出实际投影点
//        List<Vector3> vs = new List<Vector3>();
//        foreach (Vector3 lv in m.vertices)
//        {
//            Vector3 wv = mf.transform.TransformPoint(lv);

//            minh = Math.Min(minh, wv.y);
//            maxh = Math.Max(maxh, wv.y);

//            Ray ray = new Ray(wv, Vector3.down);
//            RaycastHit hit;
//            if (Physics.Raycast(ray, out hit, 1000.0f, 1 << LayerDefine.BackGroundLayer))
//            {
//                if (!vs.Contains(hit.point)) vs.Add(hit.point);
//            }
//        }

//        MathUtils.ClockwiseSortPoints(vs);

//        const float shapeDescent = 1.0f;

//        float shapeHeight = maxh - minh;
//        minh -= shapeDescent;
//        maxh = minh + shapeHeight;

//        StringBuilder sb = new StringBuilder();
//        //顶点数量，区域标志，最小高度，最大高度
//        sb.Append(string.Format("v {0} {1} {2} {3}\n", vs.Count, areaFlag, minh, maxh));

//        foreach (Vector3 lv in vs)
//        {
//            //顶点坐标
//            sb.Append(string.Format("{0} {1} {2}\n", lv.x, lv.y, lv.z));
//        }

//        return sb.ToString();
//    }


//    private static string GetFileName()
//    {
//        string filename = Path.GetFileName(EditorApplication.currentScene) + "_" + 1;

//        return filename;
//    }


//    private static void PrepareFileWrite()
//    {
//        vertexOffset = 0;
//        normalOffset = 0;
//        uvOffset = 0;
//    }


//    private static bool CreateTargetFolder(string path)
//    {
//        try
//        {
//            System.IO.Directory.CreateDirectory(path);
//        }
//        catch
//        {
//            errorList.Add("Failed to create target folder!"+path);
//            return false;
//        }

//        return true;
//    }

//    [MenuItem("Uncle/NavMesh/仅导出当前场景")]
//    public static void ExportCurScene( )
//    {
//        ExportGroupToSingleObj();
//        //执行批处理，调用Raycastdemo生成寻路图
//        ExcuteBatCommand();
//    }

//    static void ExcuteBatCommand()
//    {
//        string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
//        System.Diagnostics.Process p = System.Diagnostics.Process.Start(Path.GetFullPath(BAT_PATH), path);
//        p.WaitForExit();
//    }
//}