using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Profiling;

/// <summary>
/// 调试器
/// </summary>
public class Debugger : MonoSingleton<Debugger>
{
    bool enableDebugger;//是否启用调试器
    /// <summary>
    /// 启用调试器
    /// </summary>
    public void Enable()
    {
        enableDebugger = true;
        Application.logMessageReceived += LogCallBack;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= LogCallBack;
    }

    private void OnGUI()
    {
        if (enableDebugger)
        {
            if (showDebugger)
            {
                DrawDebugger();
            }
            else
            {
                if (GUILayout.Button("Show Debugger", GUILayout.MinWidth(100), GUILayout.MinHeight(100)))
                {
                    showDebugger = true;
                }
            }
        }
    }

    //窗口类型字符串
    string[] windowTypeStr = new string[]
    {
        "Log",       //Log
        "Memory",    //内存
        "Screen",    //屏幕
        "System",    //系统
        "Environment"//环境
    };
    int windowTypeIndex;//窗口类型下标
    bool showDebugger;

    /// <summary>
    /// 绘制调试器
    /// </summary>
    void DrawDebugger()
    {
        GUILayout.BeginVertical(GUILayout.MinWidth(Screen.width));
        windowTypeIndex = GUILayout.Toolbar(windowTypeIndex, windowTypeStr, GUILayout.MinHeight(Screen.height / 20));
        if (windowTypeIndex == 0)
        {
            //绘制Log窗口
            DrawLogWindow();
        }
        else if (windowTypeIndex == 1)
        {
            //绘制内存窗口
            DrawMemoryWindow();
        }
        else if (windowTypeIndex == 2)
        {
            //绘制屏幕窗口
            DrawScreenWindow();
        }
        else if (windowTypeIndex == 3)
        {
            //绘制系统窗口
            DrawSystemWindow();
        }
        else if (windowTypeIndex == 4)
        {
            //绘制环境窗口
            DrawEnviromentWindow();
        }
        if (GUILayout.Button("Hide", GUILayout.MinHeight(Screen.height / 20)))
        {
            showDebugger = false;
        }
        if (GUILayout.Button("Close", GUILayout.MinHeight(Screen.height / 20)))
        {
            enableDebugger = false;
            ClearLog();
        }
        GUILayout.EndVertical();
    }

    #region Log窗口

    //不同类型log的显示颜色
    Dictionary<LogType, Color> logColorMap = new Dictionary<LogType, Color>()
    {
        { LogType.Log,Color.white },
        { LogType.Warning,Color.yellow },
        { LogType.Error,Color.red },
        { LogType.Exception,Color.red },
        { LogType.Assert,Color.red },
    };

    List<LogInfo> logCache = new List<LogInfo>();//所有log信息
    /// <summary>
    /// 清空所有log
    /// </summary>
    public void ClearLog()
    {
        logCache.Clear();
        selectLog = null;
        logCounter = 0;
        warningCounter = 0;
        errorCounter = 0;
    }

    LogInfo selectLog;//当前选择的log
    string searchStr = "";//查询的字符串

    int logCounter;//log计数器
    int warningCounter;//warning计数器
    int errorCounter;//error计数器

    bool showLog = true;//是否显示log
    bool showWarning = true;//是否显示warning
    bool showError = true;//是否显示error

    Vector2 logScrollPos;//log区域滑动条位置
    Vector2 stackScrollPos;//堆栈区域滑动条位置
    const float logScrollSpeed = 0.02f;//log区域滑动条速度
    const float stackScrollSpeed = 0.02f;//log区域滑动条速度

    /// <summary>
    /// Log信息
    /// </summary>
    public class LogInfo
    {
        public string log;
        public string Log
        {
            get { return string.Format("[{0}] {1}", logTime.ToString("HH:mm:ss"), log); }
        }
        public string stackTrace;
        public LogType logType;
        public DateTime logTime;
    }

    void LogCallBack(string condition, string stackTrace, LogType logType)
    {
        LogInfo logInfo = new LogInfo();
        logInfo.log = condition;
        logInfo.stackTrace = stackTrace;
        logInfo.logType = logType;
        if (logType == LogType.Error
            || logType == LogType.Exception
            || logType == LogType.Assert)
        {
            errorCounter++;
        }
        else if (logType == LogType.Warning)
        {
            warningCounter++;
        }
        else
        {
            logCounter++;
        }
        logInfo.logTime = DateTime.Now;
        logCache.Add(logInfo);
    }

    /// <summary>
    /// 绘制Log窗口
    /// </summary>
    void DrawLogWindow()
    {
        #region Log区域

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal("box", GUILayout.MinWidth(Screen.width * 0.85f));
        GUILayout.Label("Search");
        searchStr = GUILayout.TextField(searchStr, "box", GUILayout.MaxWidth(Screen.width / 5));
        GUILayout.FlexibleSpace();
        showLog = GUILayout.Toggle(showLog, string.Format("log [{0}] ", logCounter));
        showWarning = GUILayout.Toggle(showWarning, string.Format("warning [{0}] ", warningCounter));
        showError = GUILayout.Toggle(showError, string.Format("error [{0}] ", errorCounter));
        if (GUILayout.Button("Clear"))
        {
            ClearLog();
        }
        GUILayout.EndHorizontal();

        GUIStyle logStyle_info = new GUIStyle();
        logStyle_info.fontSize = 24;
        logStyle_info.normal.textColor = Color.white;
        logScrollPos = GUILayout.BeginScrollView(logScrollPos, "box", GUILayout.MinHeight(Screen.height * 0.2f), GUILayout.MinWidth(Screen.width * 0.85f));
        foreach (var logInfo in logCache)
        {
            LogType logType = logInfo.logType;
            if (!IsShowLog(logType)) continue;

            if (logInfo.log.ToLower().Contains(searchStr))
            {
                GUI.contentColor = logInfo == selectLog
                    ? Color.cyan
                    : logColorMap[logType];
                if (GUILayout.Button(logInfo.Log, "label"))
                {
                    selectLog = logInfo;
                }
            }
        }
        GUI.contentColor = Color.white;
        GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.RepeatButton("Up", GUILayout.MinWidth(100), GUILayout.MinHeight(50)))
        {
            logScrollPos += Screen.height * logScrollSpeed * Vector2.down;
        }
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.RepeatButton("Left", GUILayout.MinWidth(50), GUILayout.MinHeight(100)))
        {
            logScrollPos += Screen.width * logScrollSpeed * Vector2.left;
        }
        if (GUILayout.RepeatButton("Right", GUILayout.MinWidth(50), GUILayout.MinHeight(100)))
        {
            logScrollPos += Screen.width * logScrollSpeed * Vector2.right;
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        if (GUILayout.RepeatButton("Down", GUILayout.MinWidth(100), GUILayout.MinHeight(50)))
        {
            logScrollPos += Screen.height * logScrollSpeed * Vector2.up;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        #endregion

        #region Stack区域

        if (selectLog == null) return;

        GUILayout.BeginHorizontal();

        stackScrollPos = GUILayout.BeginScrollView(stackScrollPos, "box", GUILayout.MinHeight(Screen.height * 0.15f), GUILayout.MinWidth(Screen.width * 0.85f));
        GUILayout.Label(selectLog.Log);
        GUILayout.Space(10);
        GUILayout.Label(selectLog.stackTrace);
        GUILayout.EndScrollView();

        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.RepeatButton("Up", GUILayout.MinWidth(100), GUILayout.MinHeight(50)))
        {
            stackScrollPos += Screen.height * stackScrollSpeed * Vector2.down;
        }
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.RepeatButton("Left", GUILayout.MinWidth(50), GUILayout.MinHeight(50)))
        {
            stackScrollPos += Screen.width * stackScrollSpeed * Vector2.left;
        }
        if (GUILayout.RepeatButton("Right", GUILayout.MinWidth(50), GUILayout.MinHeight(50)))
        {
            stackScrollPos += Screen.width * stackScrollSpeed * Vector2.right;
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        if (GUILayout.RepeatButton("Down", GUILayout.MinWidth(100), GUILayout.MinHeight(50)))
        {
            stackScrollPos += Screen.height * stackScrollSpeed * Vector2.up;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        #endregion
    }

    /// <summary>
    /// 是否显示Log
    /// </summary>
    bool IsShowLog(LogType logType)
    {
        switch (logType)
        {
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                return showError;
            case LogType.Warning:
                return showWarning;
            case LogType.Log:
                return showLog;
            default:
                return showLog;
        }
    }

    #endregion

    #region 内存窗口

    /// <summary>
    /// 绘制内存窗口
    /// </summary>
    void DrawMemoryWindow()
    {
        GUILayout.BeginVertical("Box");
        GUILayout.Label("总内存：" + Profiler.GetTotalReservedMemoryLong() / 1000000 + "MB");
        GUILayout.Label("已占用内存：" + Profiler.GetTotalAllocatedMemoryLong() / 1000000 + "MB");
        GUILayout.Label("空闲中内存：" + Profiler.GetTotalUnusedReservedMemoryLong() / 1000000 + "MB");
        GUILayout.Label("总Mono堆内存：" + Profiler.GetMonoHeapSizeLong() / 1000000 + "MB");
        GUILayout.Label("已占用Mono堆内存：" + Profiler.GetMonoUsedSizeLong() / 1000000 + "MB");
        GUILayout.EndVertical();
    }


    #endregion

    #region 屏幕窗口

    float FPS;//帧率
    float fpsCounter;//fps计数器
    float lastUpdateFpsTime;//上一次更新帧率的时间

    private void Update()
    {
        fpsCounter++;
        if (Time.realtimeSinceStartup - lastUpdateFpsTime >= 1)
        {
            lastUpdateFpsTime = Time.realtimeSinceStartup;
            FPS = fpsCounter;
            fpsCounter = 0;
        }
    }

    /// <summary>
    /// 绘制屏幕窗口
    /// </summary>
    void DrawScreenWindow()
    {
        GUILayout.BeginVertical("Box");
        GUILayout.Label(string.Format("<color={0}>FPS：{1}</color>",
           FPS < 30 ? "#FF0000" : "#00FF00",
           FPS));
        GUILayout.Label("DPI：" + Screen.dpi);
        GUILayout.Label("分辨率：" + Screen.currentResolution.ToString());
        GUILayout.EndVertical();
    }

    #endregion

    #region 系统窗口

    /// <summary>
    /// 绘制系统窗口
    /// </summary>
    void DrawSystemWindow()
    {
        GUILayout.BeginVertical("Box");
        GUILayout.Label("设备模式：" + SystemInfo.deviceModel);
        GUILayout.Label("设备名称：" + SystemInfo.deviceName);
        GUILayout.Label("设备类型：" + SystemInfo.deviceType);
        GUILayout.Label("设备标识：" + SystemInfo.deviceUniqueIdentifier);
        GUILayout.Label("");
        GUILayout.Label("显卡：" + SystemInfo.graphicsDeviceName);
        GUILayout.Label("显卡类型：" + SystemInfo.graphicsDeviceType);
        GUILayout.Label("显存：" + SystemInfo.graphicsMemorySize + "MB");
        GUILayout.Label("");
        GUILayout.Label("处理器：" + SystemInfo.processorType);
        GUILayout.Label("处理器数量：" + SystemInfo.processorCount);
        GUILayout.Label("");
        GUILayout.Label("操作系统：" + SystemInfo.operatingSystem);
        GUILayout.Label("系统内存：" + SystemInfo.systemMemorySize + "MB");
        GUILayout.Label("");
        GUILayout.Label("是否支持多线程渲染：" + SystemInfo.graphicsMultiThreaded);
        GUILayout.Label("支持最大图片尺寸：" + SystemInfo.maxTextureSize);
        GUILayout.EndVertical();
    }

    #endregion

    #region 环境窗口

    /// <summary>
    /// 绘制环境窗口
    /// </summary>
    void DrawEnviromentWindow()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label("项目名称：" + Application.productName);
        GUILayout.Label("项目包名：" + Application.identifier);
        GUILayout.Label("项目版本：" + Application.version);
        GUILayout.Label("Unity版本：" + Application.unityVersion);
        GUILayout.Label("公司名称：" + Application.companyName);
        GUILayout.EndVertical();
    }

    #endregion
}
