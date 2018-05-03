//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using UnityEngine;

//public class Options
//{
//	private enum LoadResult
//	{
//		INVALID,
//		SUCCESS,
//		FAIL
//	}

//	private string m_matchedOptionPath = "adapt_options";
//	private string m_localPath = "local_options";

////	private Options.LoadResult m_localLoadResult;
//	private Options.LoadResult m_optionLoadResult;

//	private Dictionary<Option, object> m_options = new Dictionary<Option, object>();

//	private ConfigUtlis m_localCfg = null;

//	private static Options s_instance = null;
//	public static Options Instance
//	{
//		get
//		{
//			if (s_instance == null)
//			{
//				s_instance = new Options();
//				s_instance.Initialize();
//			}

//			return s_instance;
//		}
//	}

//	private GraphicsQuality GetMatchedGraphicsQuality()
//	{
//#if UNITY_EDITOR
//		return GraphicsQuality.High;
//#else
//		if (SystemInfo.graphicsDeviceName.ToLower().Contains("sgx") 
//			&& (SystemInfo.graphicsDeviceName.Contains("544") || SystemInfo.graphicsDeviceName.Contains("540")))
//		{
//			Debug.Log(string.Format("Android Disable Golden Portraits: {0}", SystemInfo.graphicsDeviceName));
			
//			return GraphicsQuality.VeryLow;
//		}
//		else if (Screen.dpi != 0f && Screen.dpi < 180f)
//		{
//			return GraphicsQuality.Low;
//		}
//		else if(Screen.dpi >= 180 && Screen.dpi < 300f)
//		{
//			return GraphicsQuality.Medium;
//		}
//		else
//		{
//			return GraphicsQuality.High;
//		}
//#endif
//	}

//	private void Initialize()
//	{
//		this.Clear();
//		bool match = LoadMatchedOption();

//		if(!match)
//		{
//			Debug.LogError("LoadMatchedOption Failed!");
//		}

//		bool local = LoadLocalOption();
//		if(!local)
//		{
//			Debug.Log("LoadLocalOption Failed!");
//		}
//	}

//	private void Clear()
//	{
//		this.m_options.Clear();
//	}

//	public bool Has(Option key)
//	{
//		return this.m_options.ContainsKey(key);
//	}

//	public void Delete(Option key)
//	{
//		if (!this.m_options.Remove(key))
//		{
//			return;
//		}
//		m_localCfg.Delete(key.ToString());
//	}

//	public T Get<T>(Option key)
//	{
//		object obj;
//		if (!this.m_options.TryGetValue(key, out obj))
//		{
//			return default(T);
//		}
//		return (T)((object)obj);
//	}

//	public bool GetBool(Option key)
//	{
//		return this.Get<bool>(key);
//	}
//	public int GetInt(Option key)
//	{
//		return this.Get<int>(key);
//	}
//	public float GetFloat(Option key)
//	{
//		return this.Get<float>(key);
//	}
//	public string GetString(Option key)
//	{
//		return this.Get<string>(key);
//	}
//	public void Set(Option key, object val)
//	{
//		object obj;
//		if (this.m_options.TryGetValue(key, out obj))
//		{
//			if (obj == val)
//			{
//				return;
//			}
//			if (obj != null && obj.Equals(val))
//			{
//				return;
//			}
//		}

//		this.m_options[key] = val;

//		Type type = OptionDataTables.s_typeMap[key];
//		if(type == typeof(bool))
//		{
//			m_localCfg.SetBool(key.ToString(), (bool)val);
//		}
//		else if(type == typeof(int))
//		{
//			m_localCfg.SetInt(key.ToString(), (int)val);
//		}
//		else if(type == typeof(float))
//		{
//			m_localCfg.SetFloat(key.ToString(), (float)val);
//		}
//		else
//		{
//			m_localCfg.SetString(key.ToString(), (string)val);
//		}
//	}

//	private bool LoadMatchedOption()
//	{
//		TextAsset txt = Resources.Load<TextAsset>(m_matchedOptionPath);
//		if(null == txt)
//		{
////			this.m_localLoadResult = Options.LoadResult.FAIL;
//			Debug.LogError("addapt_option file is not exists!");
//			return false;
//		}

//		ConfigUtlis adapt = new ConfigUtlis(txt.bytes);

//		GraphicsQuality quality = GetMatchedGraphicsQuality();

//		string[] sections = adapt.GetSectionLines(quality.ToString());

//		if (!this.LoadAllLines(sections))
//		{
////			this.m_localLoadResult = Options.LoadResult.FAIL;
//			return false;
//		}

////		this.m_localLoadResult = Options.LoadResult.SUCCESS;

//		return true;
//	}

//	private bool LoadLocalOption()
//	{
//		m_localCfg = new ConfigUtlis(m_localPath);
//		string[] lines = m_localCfg.GetSectionLines("Default");

//		if (!this.LoadAllLines(lines))
//		{
////			this.m_localLoadResult = Options.LoadResult.FAIL;
//			return false;
//		}

////		this.m_localLoadResult = Options.LoadResult.SUCCESS;
		
//		return true;
//	}
//	private bool LoadAllLines(string[] lines)
//	{
//		for (int i = 0; i < lines.Length; i++)
//		{
//			string text = lines[i];
//			text = text.Trim();

//			if (text.Length != 0)
//			{
//				if (!text.StartsWith("#"))
//				{
//					Option key;
//					object value;
//					if (!this.LoadLine(text, out key, out value))
//					{
//						Debug.LogError(string.Format("Options.LoadAllLines() - Failed to load line {0}.", i + 1));

////						this.m_localLoadResult = Options.LoadResult.FAIL;
//						return false;
//					}
//					else
//					{
//						this.m_options[key] = value;
//					}
//				}
//			}
//		}

//		return true;
//	}
//	private bool LoadLine(string line, out Option key, out object val)
//	{
//		key = Option.INVALID;
//		val = null;
//		string text = null;
//		string text2 = null;

//		string[] strs = line.Split('=');
//		if(strs.Length != 2)
//		{
//			key = Option.INVALID;
//			val = text2;
//			return false;
//		}

//		text = strs[0].Trim();
//		text2 = strs[1].Trim();
//		string[] text2s = text2.Split('_');
//		if(text2s.Length == 2)
//		{
//			text2 = text2s[1];
//		}

//		Option option = Option.INVALID;
//		try
//		{
//			option = EnumUtils.GetEnum<Option>(text, StringComparison.OrdinalIgnoreCase);
//		}
//		catch (ArgumentException)
//		{
//			key = Option.INVALID;
//			val = text2;
//			return true;
//		}

//		Type type = OptionDataTables.s_typeMap[option];
//		if(type == typeof(bool))
//		{
//			val = bool.Parse(text2);
//		}
//		else if(type == typeof(int))
//		{
//			val = int.Parse(text2);
//		}
//		else if(type == typeof(float))
//		{
//			val = float.Parse(text2);
//		}
//		else
//		{
//			if (type != typeof(string))
//			{
//				return false;
//			}
//			val = text2;
//		}

//		key = option;
//		return true;
//	}
//}
