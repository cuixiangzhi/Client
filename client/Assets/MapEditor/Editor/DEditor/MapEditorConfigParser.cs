using System;
using System.IO;

using ProtoBuf;
using UnityEngine;

using ProtoBuf.Meta;

#if UNITY_EDITOR
public class MapEditorConfigParser
{
    public static T ParseDataFromStream<T>(TextAsset textAsset) where T : class
    {
        MemoryStream stream = new MemoryStream( textAsset.bytes );

        T data = Serializer.Deserialize<T>(stream);
        
        return data;
    }

	public static T ParseDataFromBytes<T>(byte[] byteAsset) where T : class
	{
		MemoryStream stream = new MemoryStream(byteAsset);

        T data = Serializer.Deserialize<T>(stream);

		return data;
	}

	public static object ParseDataFromStream(Type t,TextAsset textAsset)
	{
		MemoryStream stream = new MemoryStream( textAsset.bytes );

		object data = (object)RuntimeTypeModel.Default.Deserialize (stream, null, t);
		
		return data;
	}

	public static void SaveProtoDataToBinary<T>(T data,string filePath) where T : class
    {
        using(FileStream str = new FileStream( filePath , FileMode.Create ))
        {
            Serializer.Serialize<T>( str , data );

            str.Flush();

            str.Close();
        }
    }

    public static void SaveProtoDataToBinaryReplace<T>(T data,string filePath) where T : class
    {
        using(FileStream str = new FileStream( filePath , FileMode.Truncate ))
        {
            Serializer.Serialize<T>( str , data );
            
            str.Flush();
            
            str.Close();
        }
    }

    //public static void SaveWithCompress<T>(T data, string filePath) where T : class
    //{
    //    FileStream str = new FileStream(filePath, FileMode.Create);
        
    //    MemoryStream stream = new MemoryStream();
    //    MemoryStream destStream = new MemoryStream();
    //    Serializer.Serialize<T>(stream, data);
    //    stream.Seek(0, SeekOrigin.Begin);

    //    Compression7Z.CompressStream(stream, destStream);

    //    str.Write(destStream.GetBuffer(), 0, destStream.GetBuffer().Length);

    //    str.Flush();

    //    str.Close();
    //}

	public static MapEditConfigData.MapEditConfigData ParseDataFromStream(TextAsset txt)
	{
		int ver = MapEditorCons.CUR_MAP_VER - 1;

		while(ver > 0)
		{
			try
			{
				Debug.Log("尝试用" + ver + "版本解析数据");
				string editorClsName = "MapEditConfigData_ver" + ver + ".MapEditConfigData";
				System.Type clsType = System.Type.GetType( editorClsName );
				if(null == clsType)
				{
                    throw new Exception(string.Format("获取类型{0}失败", editorClsName));
				}

				object oldData = MapEditorConfigParser.ParseDataFromStream( clsType, txt );
				
				MapEditConfigData.MapEditConfigData data = new MapEditConfigData.MapEditConfigData();
				
				MapDataSerializerTools.AssignDataByFieldName(oldData, data);

				return data;
			}
			catch(Exception e)
			{
				ver--;
				Debug.Log("解析失败，" + e.ToString() + "尝试用" + ver + "版本解析数据");
			}
		}

		return null;
	}
}
#endif
