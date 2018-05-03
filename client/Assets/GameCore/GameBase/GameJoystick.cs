//using UnityEngine;
//using System.Collections;

//public class GameJoystick : MonoBehaviour
//{
//	//背景//
//	public UISprite _background;
	
//	//前景//
//	public UISprite _foreground;
	
//	//限制的最大的长度//
//	public float    _maxDistanceInPixels;
	
	
//	#region 动画参数
//	//松手后, 花多长时间让摇杆回到中心//
//	private float mSmoothBackPassedTime = 0f;
//	private const float SMOOTH_TIME = 0.2f;
	
//	//是否让拖柄归为原位了//
//	private bool mHasAlreadyReset = true;
	
//	//弹起手指时, 拖柄的位置//
//	private Vector3 mStartPos;
	
//	#endregion
	
	
//	//是否是按下状态//
//	//private bool mIsPressing = false;
	
//	//按下时的touchID//
//	private int mPressingTouchID = -10;
	
	
	
//	//缓冲的Transform//
//	private Transform mTrans = null;
//    private Vector3 mPos = Vector3.zero;
//	public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

//	//缓冲的gameobject//
//	private GameObject mGameObject = null;
//	public GameObject cachedObject { get { if (mGameObject == null) mGameObject = gameObject; return mGameObject; } } 
	
	
//	#region 通知
	
	
//	public delegate void JoystickDrag(bool isStart, float ratio, float radian);
//	public static JoystickDrag onJoystickDrag;
	
	
//	public delegate void JoystickEnd();
//	public static JoystickEnd onJoystickEnd;
	
//	#endregion

//    void Start()
//    {
//        onJoystickDrag = OnDrag;
//        onJoystickEnd = OnDragEnd;
//        mPos = cachedTransform.position;
//    }

//	void Update()
//	{
//		//非原位状态//
//		if(!mHasAlreadyReset)
//		{
			
//			//目标点//
//			Vector3 targetPos = cachedTransform.position;
			
			
//			mSmoothBackPassedTime += Time.deltaTime;
			
//			//计算补间的完成情况//
//			float t = mSmoothBackPassedTime / SMOOTH_TIME;
			
//			//设置位置//
//			Vector3 thisTimePos = Vector3.Lerp(mStartPos, targetPos, t);
			
//			//往回挪动//
//			_foreground.cachedTransform.position = thisTimePos;
			
//			//设置状态//
//			mHasAlreadyReset = t >= 1f;
//		}
//	}
	
//	void OnPress(bool isDown)
//	{
//		//不管刚按下 还是 刚弹起, 都设置一下//
//		mSmoothBackPassedTime = 0f;
		
//		if(!isDown)
//		{
//			/*mIsPressing = false;*/
//			mPressingTouchID = -10;
			
//			//弹起来的时候, 需要去检查一下//
//			mHasAlreadyReset = false;

//            cachedTransform.position = mPos;
			
//			//设置弹起的时候, 设置一下当前拖柄的位置//
//			mStartPos = _foreground.cachedTransform.position;
			
//			//发送结束通知//
//			if(onJoystickEnd != null) onJoystickEnd ();
//		}
//		else
//		{
//			mPressingTouchID = UICamera.currentTouchID;
//			/*mIsPressing = true;*/
			
//// 			//按下的时候, 是直接控制位置, 所以不需要Update去 慢慢移动//
//// 			mHasAlreadyReset = true;

//            cachedTransform.position = UICamera.currentCamera.ScreenToWorldPoint(UICamera.currentTouch.pos);
			
//			//重置//
//			mStartPos = Vector3.zero;
			
//// 			//设置初始位置//
//// 			UpdateJoystick(true);
//		}
		
//	}

//	private float _UpdateNGUIJoystick(ref Vector3 currentPos, ref Vector3 centerPos)
//	{
//		//向量相减//
//		Vector3 relative = currentPos - centerPos;
		
		
//		//计算角度//
//		float rad = Mathf.Atan2(relative.y, relative.x);
		
//		//计算长度
//		float magnitude = relative.magnitude;
		
//		//将像素转成单位//
//		//像素 * UIRoot对象的缩放 = 世界单位//
//		float maxMagnitude = _maxDistanceInPixels * cachedTransform.localToWorldMatrix.m00;
		
//		//如果长度已经大于,就要修正//
//		if(magnitude > maxMagnitude)
//		{
//			magnitude = maxMagnitude;
			
//			//重新修正地址//
//			currentPos.x = centerPos.x + Mathf.Cos(rad) * magnitude;
//			currentPos.y = centerPos.y + Mathf.Sin(rad) * magnitude;
//		}
		
		
//		//设置摇杆前景图片的位置//
//		_foreground.cachedTransform.position = currentPos;

//		return maxMagnitude > 0 ? (magnitude / maxMagnitude) : (0);
//	}

//	private float _CalculateSceneRadian(Vector3 current, Vector3 center)
//	{
//		Vector3 relative = current - center;

//		UnityEngine.Debug.DrawLine(current / 20f, center * 20f);

//		return Mathf.Atan2(relative.x, relative.z);
//	}
	
//	private void UpdateJoystick(bool isStart)
//	{
//		//屏幕转世界//
//		Ray ray = UICamera.currentCamera.ScreenPointToRay( UICamera.currentTouch.pos );
		
//		//计算位置点//
//		Vector3 currentPos = ray.GetPoint(0f);
//		currentPos.z = 0f;
		
//		//拿到中点//
//		Vector3 centerPos = cachedTransform.position;
//		centerPos.z = 0f;
		

//		float ratio = _UpdateNGUIJoystick(ref currentPos, ref centerPos);
		
//		//发通知//
//		if(onJoystickDrag != null)
//		{
//			float rad = _CalculateSceneRadian ( NGUI2ScenePosition(currentPos) , NGUI2ScenePosition(centerPos) );
//			onJoystickDrag(isStart ,ratio, rad);
//		}
		
//	}

//	Vector3 NGUI2ScenePosition(Vector3 pos)
//	{
//		//将NGUI的世界坐标转成屏幕坐标//
//		Vector3 scrPos = UICamera.currentCamera.WorldToScreenPoint(pos);


//		//将屏幕坐标转换成场景的世界坐标// 
//        Camera c = CameraManager.Instance.GetCamera().camera;
//        Vector3 worldPos = c.ScreenToWorldPoint(new Vector3(scrPos.x, scrPos.y, c.nearClipPlane));
//        return worldPos;
//	}

//	void OnDrag(Vector2 delta)
//	{
//		//不是按下时的那个手指//
//		if(UICamera.currentTouchID != mPressingTouchID)
//		{
//			return;
//		}
		
//		UpdateJoystick(false);
//	}

//    void OnDrag(bool isStart, float ratio, float radian)
//    {
//        App.SIM_DLL.Invoke<DEL_FaceTo>(StageManager.Instance.CurStageID, App.Player.ID, radian, (int)E_REPLAY_TYPE.RECORD);
//    }

//    void OnDragEnd()
//    {
//        App.SIM_DLL.Invoke<DEL_StopMove>(StageManager.Instance.CurStageID, App.Player.ID, (int)E_REPLAY_TYPE.RECORD);
//    }

//    void OnDisable()
//    {
//        if (App.Player != null)
//        {
//            App.SIM_DLL.Invoke<DEL_StopMove>(StageManager.Instance.CurStageID, App.Player.ID, (int)E_REPLAY_TYPE.RECORD);
//        }
//    }
//}
