//using UnityEngine;

//[RequireComponent(typeof(Camera))]
//public class BlackEffect : LensEffectBase
//{	
//	private RenderTexture accumTexture;
//	public Color sceneColor;
	
//	override protected void Start()
//	{
//		if(!SystemInfo.supportsRenderTextures)
//		{
//			enabled = false;
//			return;
//		}
//		base.Start();
//	}
	
//	override protected void OnDisable()
//	{
//		base.OnDisable();
//		DestroyImmediate(accumTexture);
//	}
	
//	// Called by camera to apply image effect
//	void OnRenderImage (RenderTexture source, RenderTexture destination)
//	{
//		// Create the accumulation texture
//		if (accumTexture == null)
//		{
//			DestroyImmediate(accumTexture);
//			accumTexture = new RenderTexture(1024, 1024, 0);
//			accumTexture.hideFlags = HideFlags.HideAndDontSave;
//			Graphics.Blit( source, accumTexture );
//		}
		
//		// Setup the texture and floating point values in the shader
//		material.SetTexture("_MainTex", accumTexture);
//		material.SetColor("_Color", sceneColor);
		
//		// We are accumulating motion over frames without clear/discard
//		// by design, so silence any performance warnings from Unity
//		//accumTexture.MarkRestoreExpected(); 
//		accumTexture.MarkRestoreExpected();
		
//		// Render the image using the motion blur shader
//		Graphics.Blit (source, accumTexture, material);
//		Graphics.Blit (accumTexture, destination);
//	}
//}


