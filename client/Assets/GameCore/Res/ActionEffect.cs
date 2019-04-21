//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System;

namespace GameCore.Action
{

    //public class Effect : MonoBehaviour
    //{
    //    // 表格数据
    //    public ParticleData.ParticleData m_data;

    //    //属性
    //    public Vector3 m_StartPositionOffset = Vector3.zero;    //位置偏移
    //    public Vector3 m_StartRotationOffset = Vector3.zero;    //方向偏移  

    //    public int m_RootEntityID;
    //    public int m_TargetEntityID;
    //    public float lastTime;
    //    public Transform m_StartTransform = null;
    //    public Transform m_TargetTransform = null;
    //    public Vector3 m_TargetPoint;
    //    public Vector3 m_Direction;
    //    public int m_guid = -1;
    //	public int m_EntityID = 0;


    //    // 加速播放特效
    //    public float m_rate = 1.0f;

    //    private ParticleSystem[] m_tempParticle;                //特效上边的粒子系统,没必要每次启动都去获取
    //	private Animator[] m_tempAniamtor;                      //同上
    //	private WarningMeshDraw m_warn;
    //	private bool m_bPause = false;
    //    private bool m_initTemp = false;
    //    private int timerID = -1;

    //    void Awake()
    //    {
    //		m_warn = null;
    //    }

    //    public void Init(ParticleData.ParticleData edata, int nEntityID, int nTargetID, int nGuid, float rate)
    //    {
    //        this.transform.localPosition = Vector3.zero;

    //		// 通用赋值
    //		m_data = edata;
    //		m_guid = nGuid;
    //		m_rate = rate;
    //		m_EntityID = nEntityID;

    //		// transform
    //		if (edata.StartPositionOffset.Count == 3)
    //			m_StartPositionOffset = new Vector3(edata.StartPositionOffset[0], edata.StartPositionOffset[1], edata.StartPositionOffset[2]);
    //		if (edata.StartRotationOffset.Count == 3)
    //			m_StartRotationOffset = new Vector3(edata.StartRotationOffset[0], edata.StartRotationOffset[1], edata.StartRotationOffset[2]);

    //		if(edata.Scale > 0)
    //			transform.localScale *= m_data.Scale;

    //		Entity rootEnity = EntityManager.Instance.GetEntity(nEntityID);
    //		if(rootEnity == null)
    //		{
    //			Debug.Log("Effect.Init : rootEnity is null !");
    //			DoClear();
    //            this.gameObject.SetActive(false);
    //			return;
    //		}
    //		m_RootEntityID = nEntityID;

    //		// 获取特效挂点，如果挂点没填则放在rootEntity的位置，并且设置和角色朝向一致
    //        if (string.IsNullOrEmpty( m_data.StartHPoint ))
    //        {
    //            transform.position = rootEnity.Position;
    //            transform.rotation = rootEnity.Rotation;
    //            m_StartTransform = rootEnity.go.transform;
    //            transform.position += rootEnity.Rotation * m_StartPositionOffset;
    //            transform.eulerAngles += m_StartRotationOffset;
    //        }
    //        else
    //        {


    //            m_StartTransform = rootEnity.Model.GetHangingPointTransform( m_data.StartHPoint );
    //            if (m_StartTransform != null)
    //            {
    //                transform.SetParent( m_StartTransform, false );
    //                transform.position += m_StartPositionOffset;
    //                transform.eulerAngles += m_StartRotationOffset;
    //            }
    //            else if (rootEnity.Model.HasModel())
    //            {
    //                Debug.LogError(string.Format("can not found hangpoint : {0} : {1}",rootEnity.Name,m_data.StartHPoint ));
    //            }

    //        }

    //		Entity targetEnity = EntityManager.Instance.GetEntity(nTargetID);
    //		if(targetEnity != null)
    //		{
    //			m_TargetEntityID = nTargetID;

    //			m_TargetTransform = targetEnity.Model.GetHangingPointTransform(m_data.TargetHPoint);
    //			if(m_TargetTransform == null)
    //				m_TargetTransform = targetEnity.go.transform;
    //		}

    //		UpdatePosition();

    //		if(GraphicsManager.Instance.RenderQualityLevel >= GraphicsQuality.Medium)
    //		{
    //			EffectActive(true);
    //		}
    //		else
    //		{
    //			EffectActive(false);
    //		}

    //        timerID = Timer.Instance.AddTimer(() => DoClear(), edata.LifeTime);

    //		EventManager.Instance.AddListener<EventRenderQualityLevelChanged>(OnRenderQualityLevelChanged, -1);
    //	}

    //    public void Init(ParticleData.ParticleData edata, Vector3 TargetPoint, Vector3 direction, int nGuid, float rate)
    //    {
    //        m_guid = nGuid;
    //        m_data = edata;
    //        m_TargetPoint = TargetPoint;
    //        m_Direction = direction;
    //        m_rate = rate;

    //        // 初始参数赋值
    //        transform.position = TargetPoint;
    //        transform.eulerAngles = direction;

    //		if(GraphicsManager.Instance.RenderQualityLevel >= GraphicsQuality.Medium)
    //		{
    //			EffectActive(true);
    //		}
    //		else
    //		{
    //			EffectActive(false);
    //		}

    //        timerID = Timer.Instance.AddTimer(() => DoClear(), edata.LifeTime);

    //		EventManager.Instance.AddListener<EventRenderQualityLevelChanged>(OnRenderQualityLevelChanged, -1);
    //    }

    //    public void Clear()
    //    {
    //        m_RootEntityID = -1;
    //        m_TargetEntityID = -1;
    //        m_StartTransform = null;
    //        m_TargetTransform = null;
    //    }

    //	// 生存期结束加入销毁队列
    //	public void DoClear()
    //	{
    //		if (this == null)
    //			return;

    //		if (m_ctrl != null)
    //		{
    //			m_ctrl.doDisappear(m_ctrl.disappearTime);
    //		}

    //        timerID = -1;

    //		EventManager.Instance.RemoveListener<EventRenderQualityLevelChanged>(OnRenderQualityLevelChanged, -1);

    //		EffectManager.Instance.RemoveEffectByInstanceID(m_guid);

    //        Clear();
    //	}

    //    private void InitTempStuff ( )
    //    {
    //        if (m_particleScript != null)
    //        {
    //            m_initTemp = false;
    //        }
    //        if (false == m_initTemp)
    //        {
    //            m_tempParticle = GetComponentsInChildren<ParticleSystem>( true );
    //            m_tempAniamtor = GetComponentsInChildren<Animator>( true );
    //            m_particleScript = GetComponent<tp_prefabParticle>();
    //            m_particleRotateScript = GetComponent<Particle_PlayRotate>();
    //            m_particleTexScript = GetComponent<Particle_PlayTex>();
    //            m_initTemp = true;
    //        }
    //    }

    //    public void EffectPause(bool bPause)
    //    {
    //        InitTempStuff();

    //		if(m_bPause != bPause)
    //			m_bPause = bPause;
    //		else
    //			return;

    //		if(m_particleScript != null)
    //			m_particleScript.Pause(bPause);

    //        if (m_particleRotateScript != null)
    //            m_particleRotateScript.Pause(bPause);

    //        if (m_particleTexScript != null)
    //            m_particleTexScript.Pause(bPause);

    //		if (bPause)
    //        {
    //			for (int i = 0; i < m_tempParticle.Length; i++)
    //            {
    //				m_tempParticle[i].Pause(false);
    //            }
    //			for (int j = 0; j < m_tempAniamtor.Length; j++)
    //            {
    //				m_tempAniamtor[j].enabled = false;
    //            }
    //        }
    //        else
    //        {
    //			for (int i = 0; i < m_tempParticle.Length; i++)
    //			{
    //				m_tempParticle[i].Play(false);
    //			}
    //			for (int j = 0; j < m_tempAniamtor.Length; j++)
    //			{
    //				m_tempAniamtor[j].enabled = true;
    //			}
    //        }
    //    }

    //    // 激活特效
    //    public void EffectActive(bool bActive)
    //    {
    //        if (bActive)
    //        {
    //            Timer.Instance.DeleteTimer(timerID);
    //        }

    //        InitTempStuff();

    //        //单独为链接技能类激活
    //        if (m_data.Type == (int)Effect_Type.link)
    //        {
    //            return;
    //        }

    //        //重置particle
    //		for (int i = 0; i < m_tempParticle.Length; ++i)
    //        {
    //			m_tempParticle[i].Clear(false);
    //            if (bActive)
    //            {
    //				m_tempParticle[i].playbackSpeed = m_rate;
    //				m_tempParticle[i].Play(false);
    //            }
    //        }

    //		if (bActive)
    //		{
    //			//动作类特效
    //			for (int j = 0; j < m_tempAniamtor.Length; ++j)
    //	        {
    //				m_tempAniamtor[j].speed = m_rate;
    //				m_tempAniamtor[j].enabled = true;
    //            }
    //        }
    //    }

    //    private void UpdatePosition()
    //    {
    //        switch (m_data.Type)
    //        {
    //            case (int)Effect_Type.Normal:
    //                {
    //                    UpdataEffect_Normal();
    //                }
    //                break;

    //            case (int)Effect_Type.link:
    //                {
    //                    UpdataEffect_Link();
    //                }
    //                break;
    //            case (int)Effect_Type.Trajectory:
    //                {
    //                    UpdataEffect_Trajectory();
    //                }
    //                break;

    //            case (int)Effect_Type.Multistage:
    //                {
    //                    UpdateEffect_MultiStage();
    //                }
    //                break;
    //            case (int)Effect_Type.Warning:
    //                {
    //                    UpdateEffect_Warning();
    //                }
    //                break;
    //			case (int)Effect_Type.Healthy:
    //				{
    //					UpdateEffect_Healthy();
    //				}
    //				break;
    //        }
    //    }

    //    // normal状态下特效设置
    //    private void UpdataEffect_Normal()
    //    {

    //    }

    //    // link类特效
    //    private void UpdataEffect_Link()
    //    {
    //        if (m_StartTransform != null && m_TargetTransform != null)
    //        {
    //			if(m_ctrl == null)
    //				m_ctrl = GetComponentInChildren<LightingControl>();

    //			m_ctrl.SetTarget(transform, m_TargetTransform);    
    //        }
    //    }

    //    // trajectory类特效
    //    private void UpdataEffect_Trajectory()
    //    {

    //    }


    //    // multistage 特效
    //    private void UpdateEffect_MultiStage()
    //    {

    //    }

    //    // 预警 特效
    //    private void UpdateEffect_Warning()
    //    {
    //		if(m_warn == null)
    //			m_warn = GetComponentInChildren<WarningMeshDraw>();

    //		m_warn.DrawMesh();
    //    }

    //	private void UpdateEffect_Healthy()
    //	{
    //		if(m_ball == null)
    //			m_ball = GetComponentInChildren<HealthyBallControl>();

    //		m_ball.m_targetTrans = m_TargetTransform;
    //		m_ball.Play();
    //	}

    //	private void OnRenderQualityLevelChanged(EventRenderQualityLevelChanged e)
    //	{
    //		if(e.level < GraphicsQuality.Medium)
    //		{
    //			this.gameObject.SetActive(false);
    //		}
    //		else
    //		{
    //			this.gameObject.SetActive(true);
    //		}
    //	}
    //}

}
