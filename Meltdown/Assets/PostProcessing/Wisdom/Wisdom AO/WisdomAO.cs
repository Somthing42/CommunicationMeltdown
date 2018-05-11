// ======================================================================
//  Copyright 2017 Cheng Cao (Bob Cao, bobcaocheng@gmail.com)
//  All rights reserved. Do not modify/distribute it without permission
// ----------------------------------------------------------------------
//  If you bought this from Unity Assest Store, you have the permission
//  to use, modify, and distribute in binary format it on any kind of 
//  Unity3D program. This permission is limited to Unity3D platform.
// ======================================================================

using System;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Wisdom Ambient Occulusion")]
[RequireComponent(typeof (Camera))]
[ImageEffectAllowedInSceneView]
public class WisdomAO : MonoBehaviour {

	/// Provides a shader property that is set in the inspector
	/// and a material instantiated from the shader
	private Material m_Material;

    public CommandBuffer cmd;

	public Shader m_shader;
	Shader shader {
		get	{
			if (!m_shader)
				m_shader = Shader.Find ("Hidden/WisdomAOV2");
			m_shader.hideFlags = HideFlags.None;
			return m_shader;
		}
	}

	void Start() {
		// Disable if we don't support image effects
		if (!SystemInfo.supportsImageEffects) {
			enabled = false;
			return;
		}

		// Disable the image effect if the shader can't
		// run on the users graphics card
		if (!shader || !shader.isSupported) {
			enabled = false;
			return;
		}
	}

	void OnPreCull () {
        Camera camera = GetComponent<Camera>();
        if (NormalSource == NormalSourceEnum.Use_NormalDepthTexture) {
            camera.depthTextureMode |= DepthTextureMode.DepthNormals;
            camera.depthTextureMode |= DepthTextureMode.Depth;
        }
	}

	Material material {
		get {
			if (m_Material == null) {
				m_Material = new Material (shader);
				m_Material.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_Material;
		}
	}

    public bool Deferred_PBR_Injection = true;
    public bool ColorBleed = false;
		
	void OnDisable() {
		if (material) {
			DestroyImmediate(material);
		}
		if (m_Material) {
			DestroyImmediate(m_Material);
		}
        Camera camera = GetComponent<Camera>();
    }

    public bool useBlur = true;

	public enum FrequencyMultiplierEnum {
		Single = 0,
		Double = 1,
	}
	public FrequencyMultiplierEnum FrequencyMultiplier = FrequencyMultiplierEnum.Double;

	public enum SamplesAmountEnum {
		Low = 0,
		Med = 1,
		High = 2,
		Ultra = 3,
	}
	public SamplesAmountEnum SamplesAmount = SamplesAmountEnum.High;

	// ===================================================================
	//  Wisdom AO options
	// ===================================================================
	[Range(0.1f, 3.0f)]
	public float SecondFrequencyStep = 1.0f;

	[Range(0.5f, 0.999f)]
	public float maxDistance = 0.999f;

	[Range(0.5f, 2.0f)]
	public float downSample = 1.0f;

	[Range(0.3f, 2.0f)]
	public float SampleDistribution = 1.0f;

	[Range(0.01f,1.0f)]
	public float aoRadius = 0.01f;

	[Range(0.0f,0.9f)]
	public float angleBias = 0.1f;

	[Range(0.5f, 3.0f)]
	public float intensity = 1.0f;

    [Range(0.1f, 10.0f)]
    public float ColorBleedIntensity = 1.0f;

    [Range(0.1f, 2.0f)]
	public float AO_Exponential = 0.5f;

	[Range(0.01f, 10.0f)]
	public float distanceBias = 3.0f;

    [Range(0.1f, 10.0f)]
    public float scene_scale = 1.0f;

    public enum NormalSourceEnum {
        Use_Generated_Normal = 0,
        Use_GBuffers_Normal = 1,
        Use_NormalDepthTexture = 2,
    }
    public NormalSourceEnum NormalSource = NormalSourceEnum.Use_GBuffers_Normal;

	[Range(0.1f,4.0f)]
	public float blurRadius = 1.0f;

	public bool showAOImageOnly = false;

	void refreshConstant () {
		material.SetFloat ("maxDepth", maxDistance);
		material.SetFloat ("intensity", intensity);
		material.SetFloat ("angleBias", angleBias);
		material.SetFloat ("distanceBias", distanceBias * 0.01f);
        material.SetFloat ("ColorBleedIntensity", ColorBleedIntensity);
        material.SetFloat ("scene_scale", 1.0f / scene_scale);
	}

	const int   WAO = 0;
	const int	BlurAdvancedH = 1;
	const int	BlurBleedH = 2;
	const int	Composite = 3;
	const int	AOOnly = 4;
	const int   FrqMul = 5;
	const int   WAOGN = 6;
	const int   FrqMulGN = 7;
    const int   BlurAdvancedV = 8;
    const int   WAOGB = 9;
    const int   FrqMulGB = 10;
    const int   BlurBleedV = 11;

    void sampleLow() {
		cmd.EnableShaderKeyword ("WAOSamples_Low");
		cmd.DisableShaderKeyword ("WAOSamples_Med");
		cmd.DisableShaderKeyword ("WAOSamples_High");
		cmd.DisableShaderKeyword ("WAOSamples_Ultra");
	}
	void sampleMed() {
		cmd.DisableShaderKeyword ("WAOSamples_Low");
		cmd.EnableShaderKeyword ("WAOSamples_Med");
		cmd.DisableShaderKeyword ("WAOSamples_High");
		cmd.DisableShaderKeyword ("WAOSamples_Ultra");
	}
	void sampleHigh() {
		cmd.DisableShaderKeyword ("WAOSamples_Low");
		cmd.DisableShaderKeyword ("WAOSamples_Med");
		cmd.EnableShaderKeyword ("WAOSamples_High");
		cmd.DisableShaderKeyword ("WAOSamples_Ultra");
	}
	void sampleUltra() {
		cmd.DisableShaderKeyword ("WAOSamples_Low");
		cmd.DisableShaderKeyword ("WAOSamples_Med");
		cmd.DisableShaderKeyword ("WAOSamples_High");
		cmd.EnableShaderKeyword ("WAOSamples_Ultra");
	}

    void OnGUI() {
        Camera camera = GetComponent<Camera>();
        if (Deferred_PBR_Injection && camera.renderingPath != RenderingPath.DeferredShading && camera.renderingPath != RenderingPath.DeferredLighting)
            Deferred_PBR_Injection = false;

        if (camera.renderingPath == RenderingPath.Forward && NormalSource == NormalSourceEnum.Use_GBuffers_Normal)
            NormalSource = NormalSourceEnum.Use_NormalDepthTexture;
    }

    // ===================================================================
    //  Real render is here
    // ===================================================================
    void OnPreRender() {
        Camera camera = GetComponent<Camera>();
        foreach (var buf in camera.GetCommandBuffers(CameraEvent.BeforeLighting)) {
            if (buf.name == "Wisdom Ambient Occlusion") {
                camera.RemoveCommandBuffer(CameraEvent.BeforeLighting, buf);
            }
        }

        if (cmd != null) {
            cmd.Clear();
        } else {
            cmd = new CommandBuffer();
            cmd.name = "Wisdom Ambient Occlusion";
        }

        if (Deferred_PBR_Injection) {
            camera.AddCommandBuffer(CameraEvent.BeforeLighting, cmd);

            var src_id = Shader.PropertyToID("WAO_InjectFrame");
            var default_format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            cmd.GetTemporaryRT(src_id, camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Point, camera.targetTexture ? camera.targetTexture.format : default_format);
            var src = new RenderTargetIdentifier(src_id);
            cmd.CopyTexture(BuiltinRenderTextureType.CameraTarget, src);
            doRender(camera.pixelWidth, camera.pixelHeight, src, BuiltinRenderTextureType.CameraTarget);
        }
    }

    private void doRender(int w, int h, RenderTargetIdentifier source, RenderTargetIdentifier destination,
        BuiltinRenderTextureType rt = BuiltinRenderTextureType.None)
    {
        downSample = (float)Math.Floor(downSample * 2.0f) * 0.5f;

        Camera camera = GetComponent<Camera>();

        refreshConstant();

        material.SetFloat("screenWidth", (float)camera.pixelWidth / downSample);
        material.SetFloat("screenHeight", (float)camera.pixelHeight / downSample);

        material.SetFloat("Distribution", FrequencyMultiplier == FrequencyMultiplierEnum.Single ? SampleDistribution * 2.0f : SampleDistribution);

        if (SamplesAmount == SamplesAmountEnum.Low)
        {
            sampleLow();
        }
        else if (SamplesAmount == SamplesAmountEnum.Med)
        {
            sampleMed();
        }
        else if (SamplesAmount == SamplesAmountEnum.High)
        {
            sampleHigh();
        }
        else if (SamplesAmount == SamplesAmountEnum.Ultra)
        {
            sampleUltra();
        }

        if (ColorBleed) {
            cmd.EnableShaderKeyword("WAOColorBleed");
        } else {
            cmd.DisableShaderKeyword("WAOColorBleed");
        }

        Matrix4x4 P = camera.projectionMatrix;
        //Matrix4x4 invP = P.inverse;
        Vector4 projInfo = new Vector4
            ((-2.0f / P[0, 0]),
             (-2.0f / P[1, 1]),
             ((1.0f - P[0, 2]) / P[0, 0]),
             ((1.0f + P[1, 2]) / P[1, 1]));

#if UNITY_5_5_OR_NEWER || UNITY_5_5 || UNITY_5_6
        if (camera.stereoEnabled)
        {
            Matrix4x4 P0 = camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            Matrix4x4 P1 = camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

            Vector4 projInfo0 = new Vector4
                ((-2.0f / (P0[0, 0])),
                 (-2.0f / (P0[1, 1])),
                 ((1.0f - P0[0, 2]) / P0[0, 0]),
                 ((1.0f + P0[1, 2]) / P0[1, 1]));

            Vector4 projInfo1 = new Vector4
                ((-2.0f / (P1[0, 0])),
                 (-2.0f / (P1[1, 1])),
                 ((1.0f - P1[0, 2]) / P1[0, 0]),
                 ((1.0f + P1[1, 2]) / P1[1, 1]));

            material.SetVector("_ProjInfoLeft", projInfo0);
            material.SetVector("_ProjInfoRight", projInfo1);
        }
#endif

        material.SetVector("_ProjInfo", projInfo); // used for unprojection
                                                   //material.SetMatrix ("_InvProj", camera.cameraToWorldMatrix);

        // Render into texture
        var rtAO_id = Shader.PropertyToID("WAO_rtAO");
        var rtAO = new RenderTargetIdentifier(rtAO_id);
        cmd.GetTemporaryRT(rtAO_id, (int)(w / downSample), (int)(h / downSample), 0, ColorBleed ? FilterMode.Point : FilterMode.Trilinear, ColorBleed ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.ARGBHalf);

        material.SetFloat("aoexp", AO_Exponential);

        if (FrequencyMultiplier == FrequencyMultiplierEnum.Single)
        {
            material.SetFloat("Radius", aoRadius);
            if (NormalSource == NormalSourceEnum.Use_GBuffers_Normal)
                cmd.Blit(ColorBleed ? source : BuiltinRenderTextureType.None, rtAO, material, WAOGB);
            else if (NormalSource == NormalSourceEnum.Use_NormalDepthTexture)
                cmd.Blit(ColorBleed ? source : BuiltinRenderTextureType.None, rtAO, material, WAO);
            else
                cmd.Blit(ColorBleed ? source : BuiltinRenderTextureType.None, rtAO, material, WAOGN);
        }
        else if (FrequencyMultiplier == FrequencyMultiplierEnum.Double)
        {
            material.SetFloat("Radius", aoRadius);
            material.SetFloat("Radius2", aoRadius + SecondFrequencyStep);
            if (NormalSource == NormalSourceEnum.Use_GBuffers_Normal)
                cmd.Blit(ColorBleed ? source : BuiltinRenderTextureType.None, rtAO, material, FrqMulGB);
            else if (NormalSource == NormalSourceEnum.Use_NormalDepthTexture)
                cmd.Blit(ColorBleed ? source : BuiltinRenderTextureType.None, rtAO, material, FrqMul);
            else
                cmd.Blit(ColorBleed ? source : BuiltinRenderTextureType.None, rtAO, material, FrqMulGN);
        }

        // Blur
        var blurTex_id = Shader.PropertyToID("WAO_blurAO");
        var blurTex = new RenderTargetIdentifier(blurTex_id);
        cmd.GetTemporaryRT(blurTex_id, w, h, 0, ColorBleed ? FilterMode.Point : FilterMode.Trilinear, ColorBleed ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.ARGBHalf);
        if (useBlur) {
            cmd.SetGlobalFloat("blurRadius", blurRadius / (float)camera.pixelWidth);
            cmd.Blit(rtAO, blurTex, material, ColorBleed ? BlurBleedH : BlurAdvancedH);

            cmd.SetGlobalFloat("blurRadius", blurRadius / (float)camera.pixelHeight);
            cmd.Blit(blurTex, rtAO, material, ColorBleed ? BlurBleedV : BlurAdvancedV);
        }

        if (showAOImageOnly)
        {
            // Final composite
            cmd.Blit(null, destination, material, AOOnly);
        }
        else
        {
            cmd.Blit(source, destination, material, Composite);
        }

        cmd.ReleaseTemporaryRT(rtAO_id);
        cmd.ReleaseTemporaryRT(blurTex_id);
    }

    [ImageEffectOpaque]
	void OnRenderImage (RenderTexture source, RenderTexture destination) {
        if (!Deferred_PBR_Injection) {
            doRender(source.width, source.height, source, destination);
            Graphics.ExecuteCommandBuffer(cmd);
        } else {
            Graphics.Blit(source, destination);
        }
        return;
    }
}
