//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using UnityEngine;

//public class GraphicsManager
//{
//	private GraphicsQuality m_GraphicsQuality;
//	private static GraphicsManager s_instance = null;

//	public GraphicsQuality RenderQualityLevel
//	{
//		get
//		{
//			return this.m_GraphicsQuality;
//		}
//		set
//		{
//			this.m_GraphicsQuality = value;
//			Options.Instance.Set(Option.GFX_QUALITY, this.m_GraphicsQuality);
//			this.UpdateRenderQualitySettings();

//            EventManager.Instance.Trigger<EventRenderQualityLevelChanged>(EventRenderQualityLevelChanged.Get().reset(m_GraphicsQuality), -1);
//		}
//	}

//	public static GraphicsManager Instance
//	{
//		get
//		{
//			if (s_instance == null)
//			{
//				s_instance = new GraphicsManager();
//			}
			
//			return s_instance;
//		}
//	}

//	public void Initialize()
//	{
//		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		
//		Screen.orientation = ScreenOrientation.AutoRotation;
//		Screen.autorotateToLandscapeLeft = true;
//		Screen.autorotateToLandscapeRight = true;
//		Screen.autorotateToPortrait = false;
//		Screen.autorotateToPortraitUpsideDown = false;

//		this.m_GraphicsQuality = (GraphicsQuality)Options.Instance.GetInt(Option.GFX_QUALITY);
//		this.UpdateRenderQualitySettings();
//		this.LogSystemInfo();
//	}

//	public void TestSetQuality(int quality)
//	{
//		if(quality >= (int)GraphicsQuality.VeryLow && quality <= (int)GraphicsQuality.High)
//		{
//			m_GraphicsQuality = (GraphicsQuality)quality;
//			Options.Instance.Set(Option.GFX_QUALITY, quality);
//			UpdateRenderQualitySettings();
//		}
//		else
//		{
//			Debug.LogError("invalid quality level");
//		}
//	}

//	private void UpdateRenderQualitySettings()
//	{
//		int num = 101;

//		Debug.Log(string.Format("SetQualityLevel {0}", (int)m_GraphicsQuality));

//		QualitySettings.SetQualityLevel((int)m_GraphicsQuality, true);

//		switch(m_GraphicsQuality)
//		{
//		case GraphicsQuality.VeryLow:
//			num = 101;
//			break;
//		case GraphicsQuality.Low:
//			num = 201;
//			break;
//		case GraphicsQuality.Medium:
//			num = 301;
//			break;
//		case GraphicsQuality.High:
//			num = 401;
//			break;
//		}

//		if (this.m_GraphicsQuality == GraphicsQuality.VeryLow)
//		{
//			Shader.EnableKeyword("LOW_QUALITY");
//		}
//		else
//		{
//			Shader.DisableKeyword("LOW_QUALITY");
//		}

//#if !UNITY_EDITOR
//		int targetFrameRate = 30;

//		if (Options.Instance.Has(Option.GFX_TARGET_FRAME_RATE))
//		{
//			Application.targetFrameRate = Options.Instance.GetInt(Option.GFX_TARGET_FRAME_RATE);
//		}
//		else
//		{
//			Application.targetFrameRate = targetFrameRate;
//		}

//		Debug.Log(string.Format("Target frame rate: {0}", Application.targetFrameRate));
//#endif

//		Shader[] array5 = UnityEngine.Object.FindObjectsOfType(typeof(Shader)) as Shader[];
//		for (int k = 0; k < array5.Length; k++)
//		{
//			Shader shader = array5[k];
//			shader.maximumLOD = num;
//		}

//		Shader.globalMaximumLOD = num;
//	}
//	private void LogSystemInfo()
//	{
//		Debug.Log("System Info:");
//		Debug.Log(string.Format("SystemInfo - Device Name: {0}", SystemInfo.deviceName));
//		Debug.Log(string.Format("SystemInfo - Device Model: {0}", SystemInfo.deviceModel));
//		Debug.Log(string.Format("SystemInfo - OS: {0}", SystemInfo.operatingSystem));
//		Debug.Log(string.Format("SystemInfo - CPU Type: {0}", SystemInfo.processorType));
//		Debug.Log(string.Format("SystemInfo - CPU Cores: {0}", SystemInfo.processorCount));
//		Debug.Log(string.Format("SystemInfo - System Memory: {0}", SystemInfo.systemMemorySize));
//		Debug.Log(string.Format("SystemInfo - Screen Resolution: {0}x{1}", Screen.currentResolution.width, Screen.currentResolution.height));
//		Debug.Log(string.Format("SystemInfo - Screen DPI: {0}", Screen.dpi));
//		Debug.Log(string.Format("SystemInfo - GPU ID: {0}", SystemInfo.graphicsDeviceID));
//		Debug.Log(string.Format("SystemInfo - GPU Name: {0}", SystemInfo.graphicsDeviceName));
//		Debug.Log(string.Format("SystemInfo - GPU Vendor: {0}", SystemInfo.graphicsDeviceVendor));
//		Debug.Log(string.Format("SystemInfo - GPU Memory: {0}", SystemInfo.graphicsMemorySize));
//		Debug.Log(string.Format("SystemInfo - GPU Shader Level: {0}", SystemInfo.graphicsShaderLevel));
//		Debug.Log(string.Format("SystemInfo - GPU NPOT Support: {0}", SystemInfo.npotSupport));
//		Debug.Log(string.Format("SystemInfo - Graphics API (version): {0}", SystemInfo.graphicsDeviceVersion));
//		//Debug.Log(string.Format("SystemInfo - Graphics API (type): {0}", SystemInfo.graphicsDeviceType));
//		Debug.Log(string.Format("SystemInfo - Graphics Supported Render Target Count: {0}", SystemInfo.supportedRenderTargetCount));
//		Debug.Log(string.Format("SystemInfo - Graphics Supports 3D Textures: {0}", SystemInfo.supports3DTextures));
//		Debug.Log(string.Format("SystemInfo - Graphics Supports Compute Shaders: {0}", SystemInfo.supportsComputeShaders));
//		Debug.Log(string.Format("SystemInfo - Graphics Supports Image Effects: {0}", SystemInfo.supportsImageEffects));
//		Debug.Log(string.Format("SystemInfo - Graphics Supports Render Textures: {0}", SystemInfo.supportsRenderTextures));
//		Debug.Log(string.Format("SystemInfo - Graphics Supports Render To Cubemap: {0}", SystemInfo.supportsRenderToCubemap));
//		Debug.Log(string.Format("SystemInfo - Graphics Supports Shadows: {0}", SystemInfo.supportsShadows));
//		Debug.Log(string.Format("SystemInfo - Graphics Supports Sparse Textures: {0}", SystemInfo.supportsSparseTextures));
//		Debug.Log(string.Format("SystemInfo - Graphics Supports Stencil: {0}", SystemInfo.supportsStencil));
//		Debug.Log(string.Format("SystemInfo - Graphics RenderTextureFormat.ARGBHalf: {0}", SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)));
//		Debug.Log(string.Format("SystemInfo - Graphics Metal Support: {0}", SystemInfo.graphicsDeviceVersion.StartsWith("Metal")));
////		AndroidDeviceSettings androidDeviceSettings = AndroidDeviceSettings.Get();
////		Debug.Log("AndroidSettings - Pixel Width: " + androidDeviceSettings.widthPixels);
////		Debug.Log("AndroidSettings - Pixel Height: " + androidDeviceSettings.heightPixels);
////		Debug.Log("AndroidSettings - Physical Width: " + androidDeviceSettings.widthInches);
////		Debug.Log("AndroidSettings - Physical Height: " + androidDeviceSettings.heightInches);
////		Debug.Log("AndroidSettings - Diagonal: " + androidDeviceSettings.diagonalInches);
////		Debug.Log("AndroidSettings - Screen Layout: " + androidDeviceSettings.screenLayout);
//	}
//}
