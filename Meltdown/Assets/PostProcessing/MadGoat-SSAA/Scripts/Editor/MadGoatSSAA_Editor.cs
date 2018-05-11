using UnityEngine;
using UnityEditor;
namespace MadGoat_SSAA
{
	[CustomEditor(typeof(MadGoatSSAA))]
    public class MadGoatSSAA_Editor : Editor
    {
        SerializedObject serObj;
        SerializedProperty Mode;
        SerializedProperty FilterType;
        SerializedProperty Sharpness;
        SerializedProperty sampleDistance;
        SerializedProperty Multiplier;
        SerializedProperty UseShader;
        SerializedProperty mouseCompatiblity;
        //SerializedProperty vrCompatibility;
        SerializedProperty targetFramerate;
        SerializedProperty useVsyncTarget;
        SerializedProperty minMultiplier;
        SerializedProperty maxMultiplier;
        SerializedProperty targetTexture;
        protected SerializedProperty madGoatDebugger;

        protected SerializedProperty screenshotSettings;
        protected SerializedProperty panoramaSettings;
        protected SerializedProperty screenshotPath;
        protected SerializedProperty namePrefix;
        protected SerializedProperty useProductName;
        protected SerializedProperty JPGQuality;
        protected SerializedProperty EXR32;
        protected SerializedProperty ImageFormat;

        SerializedProperty SSAA_HALF;
        SerializedProperty SSAA_X2;
        SerializedProperty SSAA_X4;
        SerializedProperty ssaaUltra;
        SerializedProperty fssaaIntensity;
        SerializedProperty textureFormat;

        private string[] ssaaModes = new string[] { "Off", "0.5x", "2x", "4x" };
        protected EditorPanoramaRes panoramaRes = EditorPanoramaRes.Square1024;

        protected static Color accent_basic = new Color(0.5f, 0.1f, 0.1f);
        protected static Color accent_pro = new Color(1f, .5f, 0f);

        protected static Color normal_basic = Color.black;
        protected static Color normal_pro = Color.white;


        protected GUIStyle normal_style = new GUIStyle();
        protected GUIStyle accent_style = new GUIStyle();
        int tab;
        protected int sstab;
        private bool Extend;
        private int scale;
        private int mode;
        private bool ssaa_extend_half = false;
        private bool ssaa_extend_2 = false;
        private bool ssaa_extend_4 = false;
        void OnEnable()
        {
            accent_style.normal.textColor = Application.HasProLicense() ? accent_pro : accent_basic;
            accent_style.fontSize = 16;

            normal_style.normal.textColor = Application.HasProLicense() ? normal_pro : normal_basic;
            normal_style.fontSize = 12;

            serObj = new SerializedObject(target);
            Mode = serObj.FindProperty("renderMode");
            FilterType = serObj.FindProperty("filterType");
            Sharpness = serObj.FindProperty("sharpness");
            sampleDistance = serObj.FindProperty("sampleDistance");
            ssaaUltra = serObj.FindProperty("ssaaUltra");
            fssaaIntensity = serObj.FindProperty("fssaaIntensity");
            Multiplier = serObj.FindProperty("multiplier");
            UseShader = serObj.FindProperty("useShader");
            mouseCompatiblity = serObj.FindProperty("mouseCompatibilityMode");

            targetFramerate = serObj.FindProperty("targetFramerate");
            useVsyncTarget = serObj.FindProperty("useVsyncTarget");
            minMultiplier = serObj.FindProperty("minMultiplier");
            maxMultiplier = serObj.FindProperty("maxMultiplier");
            screenshotSettings = serObj.FindProperty("screenshotSettings");
            panoramaSettings = serObj.FindProperty("panoramaSettings");
            screenshotPath = serObj.FindProperty("screenshotPath");
            namePrefix = serObj.FindProperty("namePrefix");
            useProductName = serObj.FindProperty("useProductName");
            JPGQuality = serObj.FindProperty("JPGQuality");
            ImageFormat = serObj.FindProperty("imageFormat");
            EXR32 = serObj.FindProperty("EXR32");
            targetTexture = serObj.FindProperty("targetTexture");
            madGoatDebugger = serObj.FindProperty("madGoatDebugger");

            SSAA_HALF = serObj.FindProperty("SSAA_HALF");
            SSAA_X2 = serObj.FindProperty("SSAA_X2");
            SSAA_X4 = serObj.FindProperty("SSAA_X4");
            textureFormat = serObj.FindProperty("textureFormat");
        }
        public override void OnInspectorGUI()
        {
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serObj.Update();
            
           
            EditorGUILayout.Separator();
            GUILayout.Label(Title, accent_style);
            EditorGUILayout.Separator();
            tab = GUILayout.Toolbar(tab, new string[] { "SSAA", "Screenshot", "General" });

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            switch (tab)
            {
                case 0:
                    DrawTab1();
                    break;
                case 1:
                    DrawTab2();
                    break;
                case 2:
                    DrawTab3();
                    break;
            }
            RefreshButton();
            accent_style.fontSize = 8;
            EditorGUILayout.Separator();
            GUILayout.Label("Version: " + MadGoatSSAA_Utils.ssaa_version, accent_style);
            accent_style.fontSize = 16;
            // Apply modifications
            serObj.ApplyModifiedProperties();
        }
        private int Getmode()
        {
            return (int)(target as MadGoatSSAA).ssaaMode;
        }
        private void DrawFSSAA()
        {
            EditorGUILayout.PropertyField(ssaaUltra, new GUIContent("Ultra Quality (FSSAA)"));
            if(ssaaUltra.boolValue)
                EditorGUILayout.PropertyField(fssaaIntensity, new GUIContent("Intensity"));
        }
        public virtual void RefreshButton()
        {

        }
        public virtual void DrawTab1()
        {
            accent_style.fontSize = 12;
            EditorGUILayout.PropertyField(Mode, new GUIContent("Operation mode"), true);
            if (Mode.intValue == 1) // Resolution scale
            {
                EditorGUILayout.HelpBox("Rise or lower the render resolution by percent", MessageType.Info);
                Multiplier.floatValue = EditorGUILayout.Slider("Resolution Scale (%)", Multiplier.floatValue * 100f, 50, 200) / 100f;
                if (Multiplier.floatValue > 1)
                {
                    DrawFSSAA();
                }
            }
            else if (Mode.intValue == 0) // SSAA presets
            {
                mode = Getmode();
                EditorGUILayout.HelpBox("Conventional SSAA settings. Higher settings produces better quality at the cost of performance. x0.5 boost the performance, but reduces the resolution.", MessageType.Info);
                mode = EditorGUILayout.Popup("SSAA Mode", mode, ssaaModes);
                switch (mode)
                {
                    case 0: // off
                        (target as MadGoatSSAA).SetAsSSAA(SSAAMode.SSAA_OFF);
                        break;
                    case 1: // x0.5
                        (target as MadGoatSSAA).SetAsSSAA(SSAAMode.SSAA_HALF);
                        break;
                    case 2: // x2
                        (target as MadGoatSSAA).SetAsSSAA(SSAAMode.SSAA_X2);
                        break;
                    case 3: // x4
                        (target as MadGoatSSAA).SetAsSSAA(SSAAMode.SSAA_X4);
                        break;
                }
                if (mode > 1)
                {
                    DrawFSSAA();
                }
                EditorGUILayout.Separator();
                accent_style.fontSize = 12;
                GUILayout.Label("Edit SSAA Presets", accent_style);
                EditorGUILayout.Separator();

                ssaa_extend_half = EditorGUILayout.Foldout(ssaa_extend_half, "SSAA x0.5");
                if (ssaa_extend_half)
                {
                    EditorGUILayout.PropertyField(SSAA_HALF.FindPropertyRelative("useFilter"));
                    if (SSAA_HALF.FindPropertyRelative("useFilter").boolValue)
                    {
                        EditorGUILayout.PropertyField(SSAA_HALF.FindPropertyRelative("filterType"));
                        if (SSAA_HALF.FindPropertyRelative("filterType").enumValueIndex == 0)
                            EditorGUILayout.PropertyField(SSAA_HALF.FindPropertyRelative("sharpness"));
                        else if (SSAA_HALF.FindPropertyRelative("filterType").enumValueIndex == 2)
                            EditorGUILayout.PropertyField(SSAA_HALF.FindPropertyRelative("sharpness"));
                        else
                        {
                            EditorGUILayout.PropertyField(SSAA_HALF.FindPropertyRelative("sharpness"));
                            EditorGUILayout.PropertyField(SSAA_HALF.FindPropertyRelative("sampleDistance"));

                        }

                    }
                }
                ssaa_extend_2 = EditorGUILayout.Foldout(ssaa_extend_2, "SSAA x2");
                if (ssaa_extend_2)
                {
                    EditorGUILayout.PropertyField(SSAA_X2.FindPropertyRelative("useFilter"));
                    if (SSAA_X2.FindPropertyRelative("useFilter").boolValue)
                    {
                        EditorGUILayout.PropertyField(SSAA_X2.FindPropertyRelative("filterType"));
                        if (SSAA_X2.FindPropertyRelative("filterType").enumValueIndex == 0)
                            EditorGUILayout.PropertyField(SSAA_X2.FindPropertyRelative("sharpness"));
                        else if (SSAA_X2.FindPropertyRelative("filterType").enumValueIndex == 2)
                            EditorGUILayout.PropertyField(SSAA_X2.FindPropertyRelative("sharpness"));
                        else
                        {
                            EditorGUILayout.PropertyField(SSAA_X2.FindPropertyRelative("sharpness"));
                            EditorGUILayout.PropertyField(SSAA_X2.FindPropertyRelative("sampleDistance"));

                        }

                    }
                }
                ssaa_extend_4 = EditorGUILayout.Foldout(ssaa_extend_4, "SSAA x4");
                if (ssaa_extend_4)
                {
                    EditorGUILayout.PropertyField(SSAA_X4.FindPropertyRelative("useFilter"));
                    if (SSAA_X4.FindPropertyRelative("useFilter").boolValue)
                    {
                        EditorGUILayout.PropertyField(SSAA_X4.FindPropertyRelative("filterType"));
                        if (SSAA_X4.FindPropertyRelative("filterType").enumValueIndex == 0)
                            EditorGUILayout.PropertyField(SSAA_X4.FindPropertyRelative("sharpness"));
                        else if (SSAA_X4.FindPropertyRelative("filterType").enumValueIndex == 2)
                            EditorGUILayout.PropertyField(SSAA_X4.FindPropertyRelative("sharpness"));
                        else
                        {
                            EditorGUILayout.PropertyField(SSAA_X4.FindPropertyRelative("sharpness"));
                            EditorGUILayout.PropertyField(SSAA_X4.FindPropertyRelative("sampleDistance"));
                        }
                    }
                }

                if (GUILayout.Button("Reset SSAA preset to defaults"))
                {
                    // Reset
                    (target as MadGoatSSAA).SSAA_X2 = new SsaaProfile(1.5f, true, Filter.BILINEAR, 0.8f, 0.5f);
                    (target as MadGoatSSAA).SSAA_X4 = new SsaaProfile(2f, true, Filter.BICUBIC, 0.725f, .95f);
                    (target as MadGoatSSAA).SSAA_HALF = new SsaaProfile(.5f, false);
                }
            }
            else if (Mode.intValue == 2)// Custom 
            {
                EditorGUILayout.HelpBox("Experimental. Only use if you know what you're doing.\nValues over 4 not recommended, higher values (depending on current screen size) may cause system instability or engine crashes.", MessageType.Warning);

                Extend = EditorGUILayout.Toggle("Don't limit the multiplier", Extend);
                if (Extend) EditorGUILayout.PropertyField(Multiplier, new GUIContent("Resolution Multiplier"), true);
                else Multiplier.floatValue = EditorGUILayout.Slider("Resolution Multiplier", Multiplier.floatValue, 0.2f, 4f);
                if (Multiplier.floatValue > 1)
                {
                    DrawFSSAA();
                }
            }
            else // Adaptive
            {
                EditorGUILayout.HelpBox("Adaptive mode allows for automatic resolution adjustment based on a target frame rate.", MessageType.Info);

                EditorGUILayout.PropertyField(useVsyncTarget, new GUIContent("Refresh Rate is target"));
                if (!useVsyncTarget.boolValue)
                    EditorGUILayout.PropertyField(targetFramerate, new GUIContent(" Frame Rate Target"));

                float min = minMultiplier.floatValue * 100, max = maxMultiplier.floatValue * 100;
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Resolution scale range");
                EditorGUILayout.MinMaxSlider(ref min, ref max, 50, 150);
                normal_style.fontStyle = FontStyle.Bold;
                EditorGUILayout.LabelField("Minimum resolution scale: " + (int)min + "%",normal_style);
                EditorGUILayout.LabelField("Maximum resolution scale: " + (int)max + "%", normal_style);
                EditorGUILayout.LabelField("Current resolution scale: " + (int)(Multiplier.floatValue * 100f) + "%", normal_style);
                normal_style.fontStyle = FontStyle.Normal;
                minMultiplier.floatValue = min / 100;
                maxMultiplier.floatValue = max / 100;
            }
            // Draw the shader stuff
            if (Mode.intValue != 0)
            {
                EditorGUILayout.Separator();
                accent_style.fontSize = 12;
                GUILayout.Label("Downsampling", accent_style);
                EditorGUILayout.Separator();

                EditorGUILayout.HelpBox("If using image filtering, the render image will be passed through a custom downsampling filter. If not, it will be resized as is.", MessageType.Info);
                UseShader.boolValue = EditorGUILayout.Toggle("Use Filter", UseShader.boolValue);
                if (UseShader.boolValue)
                {
                    EditorGUILayout.PropertyField(FilterType);
                    Sharpness.floatValue = EditorGUILayout.Slider("Downsample sharpness", Sharpness.floatValue, 0f, 1f);

                    if (FilterType.intValue == 1)
                        sampleDistance.floatValue = EditorGUILayout.Slider("Distance between samples", sampleDistance.floatValue, 0.5f, 2f);
                }
            }
        }
        public virtual void DrawTab2()
        {

            EditorGUILayout.PropertyField(screenshotPath, new GUIContent("Save path"));
            EditorGUILayout.PropertyField(useProductName);
            if(!useProductName.boolValue)
            EditorGUILayout.PropertyField(namePrefix, new GUIContent("File Name Prefix"));
            // the screenshot module
            EditorGUILayout.PropertyField(ImageFormat, new GUIContent("Output Image Format"));

            EditorGUI.indentLevel++;
            if (ImageFormat.enumValueIndex == 0)
                EditorGUILayout.PropertyField(JPGQuality, new GUIContent("JPG Quality"));
            if (ImageFormat.enumValueIndex == 2)
                EditorGUILayout.PropertyField(EXR32, new GUIContent("32-bit EXR"));

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            sstab = GUILayout.Toolbar(sstab, new string[] { "Frame", "360 Panorama" });
            if (sstab == 0)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.PropertyField(screenshotSettings.FindPropertyRelative("outputResolution"), new GUIContent("Screenshot Resolution"));
                EditorGUILayout.PropertyField(screenshotSettings.FindPropertyRelative("screenshotMultiplier"), new GUIContent("Render Resolution Multiplier"));
                accent_style.fontSize = 12;
                GUILayout.Label("*Render Resolution: " + (target as MadGoatSSAA).screenshotSettings.outputResolution * (target as MadGoatSSAA).screenshotSettings.screenshotMultiplier, accent_style);
                
                EditorGUILayout.Separator();
                EditorGUILayout.PropertyField(screenshotSettings.FindPropertyRelative("useFilter"), new GUIContent("Use Filter"));
                if (screenshotSettings.FindPropertyRelative("useFilter").boolValue)
                {
                    screenshotSettings.FindPropertyRelative("sharpness").floatValue = EditorGUILayout.Slider("   Sharpness", screenshotSettings.FindPropertyRelative("sharpness").floatValue, 0, 1);
                }
                if (GUILayout.Button(Application.isPlaying ? "Take Screenshot" : "Only available in play mode"))
                {
                    if (Application.isPlaying)
                    {
                        MadGoatSSAA tg = (target as MadGoatSSAA);
                        if (screenshotSettings.FindPropertyRelative("useFilter").boolValue)
                            tg.TakeScreenshot(
                                tg.screenshotPath,
                                tg.screenshotSettings.outputResolution,
                                tg.screenshotSettings.screenshotMultiplier,
                                tg.screenshotSettings.sharpness
                                );
                        else
                            tg.TakeScreenshot(
                                tg.screenshotPath,
                                tg.screenshotSettings.outputResolution,
                                tg.screenshotSettings.screenshotMultiplier
                                );
                    }
                }
            }
            else
            {
                //EditorGUILayout.PropertyField(panoramaSettings, true);
                
                panoramaRes = (EditorPanoramaRes)EditorGUILayout.EnumPopup(new GUIContent("Panorama Face Resolution"), panoramaRes);

                panoramaSettings.FindPropertyRelative("panoramaSize").intValue = (int)panoramaRes;

                panoramaSettings.FindPropertyRelative("panoramaMultiplier").intValue = EditorGUILayout.IntSlider(new GUIContent("Resolution Multiplier"), panoramaSettings.FindPropertyRelative("panoramaMultiplier").intValue, 1, panoramaRes == EditorPanoramaRes.Square4096 ? 2 : 4);

                accent_style.fontSize = 12;

                GUILayout.Label("*Render Resolution: " + panoramaSettings.FindPropertyRelative("panoramaSize").intValue* panoramaSettings.FindPropertyRelative("panoramaMultiplier").intValue + " x " + panoramaSettings.FindPropertyRelative("panoramaSize").intValue * panoramaSettings.FindPropertyRelative("panoramaMultiplier").intValue + " x 6 faces", accent_style);

                EditorGUILayout.PropertyField(panoramaSettings.FindPropertyRelative("useFilter"));
                if (panoramaSettings.FindPropertyRelative("useFilter").boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(panoramaSettings.FindPropertyRelative("sharpness"));
                    EditorGUI.indentLevel--;
                }
                if (GUILayout.Button(Application.isPlaying ? "Take Screenshot" : "Only available in play mode"))
                {
                    if (Application.isPlaying)
                    {
                        MadGoatSSAA tg = (target as MadGoatSSAA);
                        if(panoramaSettings.FindPropertyRelative("useFilter").boolValue)
                            tg.TakePanorama(
                                tg.screenshotPath,
                                tg.panoramaSettings.panoramaSize,
                                tg.panoramaSettings.panoramaMultiplier,
                                tg.panoramaSettings.sharpness
                                );
                        else
                            tg.TakePanorama(
                               tg.screenshotPath,
                               tg.panoramaSettings.panoramaSize,
                               tg.panoramaSettings.panoramaMultiplier
                               );
                    }
                }
            }
        }
        public virtual void DrawTab3()
        {

            if (!Application.isPlaying)
                EditorGUILayout.PropertyField(textureFormat, new GUIContent("Render Texture Format"));
            EditorGUILayout.PropertyField(targetTexture, new GUIContent("Target Render Texture"));
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.HelpBox("Enables compatibility with OnClick() and other mouse events, at the cost of performance impact (scene is rendered second time for collider lookup).", MessageType.Info);
            mouseCompatiblity.boolValue = EditorGUILayout.Toggle("OnClick() Compatibility Mode", mouseCompatiblity.boolValue);
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            EditorGUILayout.HelpBox("Set the MadGoat Debugger gameobject from your scene in order to use the debugging features", MessageType.Info);

            EditorGUILayout.PropertyField(madGoatDebugger, new GUIContent("MadGoatDebugger object"));
            if (madGoatDebugger.objectReferenceValue == null)
                if (GUILayout.Button("Get MadGoat Debugger & Benchmark"))
                    Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/content/45279");
            EditorGUILayout.Separator();
            if (GUILayout.Button("Open online documentation"))
                    Application.OpenURL("https://drive.google.com/open?id=1QZ0XVhIteEjvne1BoiEBYD5s3Kzsn73_AdWNtCqCj5I");


        }
        public virtual string Title
        {
            get { return "MadGoat SuperSampling"; }
        }
    }
}