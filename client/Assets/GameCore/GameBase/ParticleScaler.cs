//This script will only work in editor mode. You cannot adjust the scale dynamically in-game!
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ParticleScaler : MonoBehaviour
{
    public float particleScale = 1.0f;
    public bool alsoScaleGameobject = true;

    float prevScale;

    void Start()
    {
        prevScale = particleScale;
    }

    void Update()
    {
#if UNITY_EDITOR
        //check if we need to update
        if (prevScale != particleScale && particleScale > 0)
        {
            if (alsoScaleGameobject)
                transform.localScale = new Vector3(particleScale, particleScale, particleScale);

            float scaleFactor = particleScale / prevScale;

            //scale legacy particle systems
            ScaleLegacySystems(scaleFactor);

            //scale shuriken particle systems
            ScaleShurikenSystems(scaleFactor);

            //scale trail renders
            ScaleTrailRenderers(scaleFactor);

            prevScale = particleScale;
        }
#endif

        if (this.transform.localScale.x != prevScale || this.transform.localScale.y != prevScale || this.transform.localScale.z != prevScale)
        {
            transform.localScale = new Vector3(particleScale, particleScale, particleScale);
        }

    }

    void ScaleShurikenSystems(float scaleFactor)
    {
#if UNITY_EDITOR
        //get all shuriken systems we need to do scaling on
        ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();

        for (int i = 0; i < systems.Length; i++)
        {

            systems[i].startSpeed *= scaleFactor;
            systems[i].startSize *= scaleFactor;
            systems[i].gravityModifier *= scaleFactor;

            //some variables cannot be accessed through regular script, we will acces them through a serialized object
            SerializedObject so = new SerializedObject(systems[i]);

            //unity 4.0 and onwards will already do this one for us
#if UNITY_3_5 
			so.FindProperty("ShapeModule.radius").floatValue *= scaleFactor;
			so.FindProperty("ShapeModule.boxX").floatValue *= scaleFactor;
			so.FindProperty("ShapeModule.boxY").floatValue *= scaleFactor;
			so.FindProperty("ShapeModule.boxZ").floatValue *= scaleFactor;
#endif

            so.FindProperty("VelocityModule.x.scalar").floatValue *= scaleFactor;
            so.FindProperty("VelocityModule.y.scalar").floatValue *= scaleFactor;
            so.FindProperty("VelocityModule.z.scalar").floatValue *= scaleFactor;
            so.FindProperty("ClampVelocityModule.magnitude.scalar").floatValue *= scaleFactor;
            so.FindProperty("ClampVelocityModule.x.scalar").floatValue *= scaleFactor;
            so.FindProperty("ClampVelocityModule.y.scalar").floatValue *= scaleFactor;
            so.FindProperty("ClampVelocityModule.z.scalar").floatValue *= scaleFactor;
            so.FindProperty("ForceModule.x.scalar").floatValue *= scaleFactor;
            so.FindProperty("ForceModule.y.scalar").floatValue *= scaleFactor;
            so.FindProperty("ForceModule.z.scalar").floatValue *= scaleFactor;
            so.FindProperty("ColorBySpeedModule.range").vector2Value *= scaleFactor;
            so.FindProperty("SizeBySpeedModule.range").vector2Value *= scaleFactor;
            so.FindProperty("RotationBySpeedModule.range").vector2Value *= scaleFactor;

            so.ApplyModifiedProperties();
        }
#endif
    }

    void ScaleLegacySystems(float scaleFactor)
    {
#if UNITY_EDITOR
        //get all emitters we need to do scaling on
        ParticleEmitter[] emitters = GetComponentsInChildren<ParticleEmitter>();

        //get all animators we need to do scaling on
        ParticleAnimator[] animators = GetComponentsInChildren<ParticleAnimator>();

        //apply scaling to emitters
        for (int i = 0; i < emitters.Length; i++)
        {
            emitters[i].minSize *= scaleFactor;
            emitters[i].maxSize *= scaleFactor;
            emitters[i].worldVelocity *= scaleFactor;
            emitters[i].localVelocity *= scaleFactor;
            emitters[i].rndVelocity *= scaleFactor;

            //some variables cannot be accessed through regular script, we will acces them through a serialized object
            SerializedObject so = new SerializedObject(emitters[i]);

            so.FindProperty("m_Ellipsoid").vector3Value *= scaleFactor;
            so.FindProperty("tangentVelocity").vector3Value *= scaleFactor;
            so.ApplyModifiedProperties();
        }

        //apply scaling to animators
        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].force *= scaleFactor;
            animators[i].rndForce *= scaleFactor;
        }
#endif
    }

    void ScaleTrailRenderers(float scaleFactor)
    {
        //get all animators we need to do scaling on
        TrailRenderer[] trails = GetComponentsInChildren<TrailRenderer>();

        //apply scaling to animators
        for (int i = 0; i < trails.Length; i++)
        {
            trails[i].startWidth *= scaleFactor;
            trails[i].endWidth *= scaleFactor;
        }
    }
}
