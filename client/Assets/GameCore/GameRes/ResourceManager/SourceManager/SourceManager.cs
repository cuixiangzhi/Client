/********************************************************************************
*	创建人：	 
*	创建时间：   2015-06-11   作废
*
*	功能说明： 测试使用
*	
*	修改记录：
*********************************************************************************/
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using GetMD5;

public class SourceManager : GameCore.BaseManager<SourceManager> 
{
	public GameSourceData localSource;
	public GameSourceData newSource;
	private Dictionary<string,SourceData> LocalSouceData;
	private Dictionary<string,SourceData> NewSouceData;
		
	public SourceManager()
	{
		LocalSouceData = new Dictionary<string, SourceData>();
		NewSouceData = new Dictionary<string, SourceData>();
	}

    public void Init()
    {
        loadFromConfig();
    }

	private bool loadFromConfig ()
	{
		string OldConfigPath = Path.Combine(Application.persistentDataPath,"VerRes.txt");
		localSource = ReadSourceFile(OldConfigPath);
		if(localSource != null) {
			foreach(string name in localSource.SourceDict.Keys) {
                LocalSouceData.Add(name, localSource.SourceDict[name]);
			}
		}
        //string NewConfigPath = Path.Combine(Application.persistentDataPath, "Config/VerRes.txt");
        //newSource = ReadSourceFile(NewConfigPath);
        //if(newSource != null && newSource.Files != null)
        //{
        //    foreach(SourceData data in newSource.Files)
        //        NewSouceData.Add(data.FileName,data);
        //}
			
		return true;
	}

	//读取资源表(与其他表格式不一样,单独处理)
	public GameSourceData ReadSourceFile(string path)
	{		
		GameSourceData data = new GameSourceData();
        data.Version = "";
        data.Platform = "";
        data.SourceDict.Clear();
			
		if(File.Exists(path))
		{	
			try
			{
				using (StreamReader sr = new StreamReader(path))
				{
                    string strLine = "";
                    strLine = sr.ReadLine();
                    while (strLine != null)
                    {
                        TotalBundleJson json = SimpleJson.SimpleJson.DeserializeObject<TotalBundleJson>(strLine);
                        if (json != null)
                        {
                            SourceData sourcedata = new SourceData();
                            sourcedata.Num = json.version;
                            sourcedata.FileName = json.ResourceName;
                            sourcedata.Size = "";

                            string[] namekeyarray = json.ResourceName.Split(new string[]{"%","$","%"},StringSplitOptions.None);
                            string[] namekeyend = namekeyarray[0].Split(new char[]{'/'});
                            string namekey = namekeyend[namekeyend.Length - 1].Replace('_', '/');
                            if (namekey.Contains(".unity3d"))
                                namekey = namekey.Substring(0, namekey.LastIndexOf('.'));
                            data.SourceDict.Add(namekey, sourcedata);
                        }
                        strLine = sr.ReadLine();
                    }
                    sr.Close();

				}
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
			}
		}
		return data;
	}

	public string getSourceSize(string souceName){
		if(NewSouceData.ContainsKey(souceName))
			return NewSouceData[souceName].Size;
		else
			return "0";
	}
		
	public int getLocalNum(string oldsouceName)
	{
		if(LocalSouceData.ContainsKey(oldsouceName))
			return LocalSouceData[oldsouceName].Num;
		else
			return -1;
	}
		
	public int getNewNum(string newsouceName)
	{
		if(NewSouceData.ContainsKey(newsouceName))
			return NewSouceData[newsouceName].Num;
		else
			return -1;
	}

    public SourceData GetSourcedata(string namepath)
    {
        if (LocalSouceData != null && LocalSouceData.Count != 0)
        {
            if (LocalSouceData.ContainsKey(namepath))
                return LocalSouceData[namepath];
        }
        return null;
    }
		
	//添加一个下载记录
	public void AddDownloadRecord(SourceData data) {
		if(!LocalSouceData.ContainsKey(data.FileName)) {
			LocalSouceData.Add(data.FileName,data);
		} else {
			LocalSouceData[data.FileName] = data;
		}
	}
	//添加一个下载记录,并立刻保存到本地磁盘
	public void AddDownloadRecordAndSaveToLocaldisk(SourceData data)
	{
		AddDownloadRecord(data);
		SaveToLocaldisk();
	}
		
	//保存到本地磁盘
	//全部覆盖
	public void SaveToLocaldisk()
	{
		if( localSource == null ) localSource = new GameSourceData();
		localSource.Platform = newSource.Platform;
		localSource.Version = newSource.Version;
			
		int count = LocalSouceData.Values.Count;
        //localSource.Files = new SourceData[count];
			
		//int i = 0;
		foreach(SourceData sd in LocalSouceData.Values) {
            //localSource.Files[i ++] = sd;
		}
			
		string jsonStr = SimpleJson.SimpleJson.SerializeObject(localSource);
        string localConfigPath = Path.Combine(Application.persistentDataPath, "VerRes.txt");
			
		try
		{
			using (StreamWriter sr = new StreamWriter(localConfigPath))
			{
				sr.Write(jsonStr);
				sr.Close();
			}
		} catch (Exception e) {
			Debug.Log(e.Message);
		}
	}

	/// <summary>
	/// 获取当前版本需要的列表
	/// </summary>
	/// <returns>The update mode.</returns>
	public List<SourceData> GetUpdateModes() {
        List<SourceData> curModes = new List<SourceData>();
        foreach (KeyValuePair<string, SourceData> pair in NewSouceData)
        {
			SourceData date = null;
			if(LocalSouceData.TryGetValue(pair.Key,out date)){
				if(NewSouceData[pair.Key].Num != date.Num){
					curModes.Add(date);
				}
			}else{
				curModes.Add(pair.Value);
			}
		}
		return curModes;
	}

}

public class SourceData
{
	public string FileName;
	public int Num;
	public string Size;
	public SourceData(){}
	public SourceData(string filename,int num,string size)
	{
		FileName = filename;
		Num = num;
		Size = size;
	}
}

public class GameSourceData
{
	public string Version;
	public string Platform;
	public Dictionary<string, SourceData> SourceDict = new Dictionary<string,SourceData>();

}

