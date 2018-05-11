using UnityEngine;
using System.Threading;
namespace MadGoat_SSAA
{
	public enum Mode
    {
        SSAA = 0,
        ResolutionScale = 1,
        AdaptiveResolution = 3,
        Custom = 2
    }
    public enum SSAAMode
    {
        SSAA_OFF = 0,
        SSAA_HALF = 1,
        SSAA_X2 = 2,
        SSAA_X4 = 3
    }
    public enum Filter
    {
        NEAREST_NEIGHBOR,
        BILINEAR,
        BICUBIC
    }
    public enum ImageFormat
    {
        JPG,
        PNG,
        #if UNITY_5_6_OR_NEWER
        EXR
        #endif
    }
    public enum EditorPanoramaRes
    {
        Square128 = 128,
        Square256 = 256,
        Square512 = 512,
        Square1024 = 1024,
        Square2048 = 2048,
        Square4096 = 4096,

    }
    [System.Serializable]
    public class SsaaProfile
    {
        [HideInInspector]
        public float multiplier;

        public bool useFilter;
        [Tooltip("Which type of filtering to be used (only applied if useShader is true)")]
        public Filter filterType = Filter.BILINEAR;
        [Tooltip("The sharpness of the filtered image (only applied if useShader is true)")]
        [Range(0, 1)]
        public float sharpness;
        [Tooltip("The distance between the samples (only applied if useShader is true)")]
        [Range(0.5f, 2f)]
        public float sampleDistance;

        public SsaaProfile(float mul, bool useDownsampling)
        {
            multiplier = mul;

            useFilter = useDownsampling;
            sharpness = useDownsampling ? 0.85f : 0;
            sampleDistance = useDownsampling ? 0.65f : 0;
        }
        public SsaaProfile(float mul, bool useDownsampling, Filter filterType, float sharp, float sampleDist)
        {
            multiplier = mul;

            this.filterType = filterType;
            useFilter = useDownsampling;
            sharpness = useDownsampling ? sharp : 0;
            sampleDistance = useDownsampling ? sampleDist : 0;
        }
    }
    [System.Serializable]
    public class ScreenshotSettings
    {
        [HideInInspector]
        public bool takeScreenshot = false;

        [Range(1, 4)]
        public int screenshotMultiplier = 1;
        public Vector2 outputResolution = new Vector2(1920, 1080);

        public bool useFilter = true;
        [Range(0, 1)]
        public float sharpness = 0.85f;
    }
    [System.Serializable]
    public class PanoramaSettings
    {
        public PanoramaSettings(int size, int mul)
        {
            panoramaMultiplier = mul;
            panoramaSize = size;
        }
        public int panoramaSize;

        [Range(1,4)]
        public int panoramaMultiplier;

        public bool useFilter = true;
        [Range(0, 1)]
        public float sharpness = 0.85f;
    }
    public class DebugData
    {
        public MadGoatSSAA instance;

        public Mode renderMode
        {
            get { return instance.renderMode; }
        }
        public float multiplier
        {
            get { return instance.multiplier; }
        }
        public bool fssaa
        {
            get { return instance.ssaaUltra; }
        }

        // Constructor
        public DebugData(MadGoatSSAA instance)
        {
            this.instance = instance;
        }
    }
    public static class MadGoatSSAA_Utils
    {
        public const string ssaa_version = "1.4"; // Don't forget to change me when pushing updates!

        /// <summary>
        /// Makes this camera's settings match the other camera and assigns a custom target texture
        /// </summary>
        public static void CopyFrom(this Camera current, Camera other, RenderTexture rt)
        {
            current.CopyFrom(other);
            current.targetTexture = rt;
        }

    }
    public class TextureScale
    {
        public class ThreadData
        {
            public int start;
            public int end;
            public ThreadData(int s, int e)
            {
                start = s;
                end = e;
            }
        }

        private static Color[] texColors;
        private static Color[] newColors;
        private static int w;
        private static float ratioX;
        private static float ratioY;
        private static int w2;
        private static int finishCount;
        private static Mutex mutex;

        public static void Point(Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight, false);
        }

        public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight, true);
        }

        private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear)
        {
            texColors = tex.GetPixels();
            newColors = new Color[newWidth * newHeight];
            if (useBilinear)
            {
                ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
                ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
            }
            else
            {
                ratioX = ((float)tex.width) / newWidth;
                ratioY = ((float)tex.height) / newHeight;
            }
            w = tex.width;
            w2 = newWidth;
            var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
            var slice = newHeight / cores;

            finishCount = 0;
            if (mutex == null)
            {
                mutex = new Mutex(false);
            }
            if (cores > 1)
            {
                int i = 0;
                ThreadData threadData;
                for (i = 0; i < cores - 1; i++)
                {
                    threadData = new ThreadData(slice * i, slice * (i + 1));
                    ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
                    Thread thread = new Thread(ts);
                    thread.Start(threadData);
                }
                threadData = new ThreadData(slice * i, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
                while (finishCount < cores)
                {
                    Thread.Sleep(1);
                }
            }
            else
            {
                ThreadData threadData = new ThreadData(0, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
            }

            tex.Resize(newWidth, newHeight);
            tex.SetPixels(newColors);
            tex.Apply();

            texColors = null;
            newColors = null;
        }

        public static void BilinearScale(System.Object obj)
        {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                int yFloor = (int)Mathf.Floor(y * ratioY);
                var y1 = yFloor * w;
                var y2 = (yFloor + 1) * w;
                var yw = y * w2;

                for (var x = 0; x < w2; x++)
                {
                    int xFloor = (int)Mathf.Floor(x * ratioX);
                    var xLerp = x * ratioX - xFloor;
                    newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
                                                           ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp),
                                                           y * ratioY - yFloor);
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        public static void PointScale(System.Object obj)
        {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                var thisY = (int)(ratioY * y) * w;
                var yw = y * w2;
                for (var x = 0; x < w2; x++)
                {
                    newColors[yw + x] = texColors[(int)(thisY + ratioX * x)];
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
        {
            return new Color(c1.r + (c2.r - c1.r) * value,
                              c1.g + (c2.g - c1.g) * value,
                              c1.b + (c2.b - c1.b) * value,
                              c1.a + (c2.a - c1.a) * value);
        }
    }
}