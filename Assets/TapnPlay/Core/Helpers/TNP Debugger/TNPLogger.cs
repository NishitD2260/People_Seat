using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TapNPlay.Core.Utilities
{
    public static class TNPLogger
    {
        private const string DebugPrefix = "[TapNPlay Debug]";

        #region Default Logs
        public static void Log(object message, Object context = null)
        {
#if UNITY_EDITOR
            if (context != null)
                Debug.Log($"{DebugPrefix} {message}", context);
            else
                Debug.Log($"{DebugPrefix} {message}");
#endif
        }

        public static void LogWarning(object message, Object context = null)
        {
#if UNITY_EDITOR
            if (context != null)
                Debug.LogWarning($"{DebugPrefix} {message}", context);
            else
                Debug.LogWarning($"{DebugPrefix} {message}");
#endif
        }

        public static void LogError(object message, Object context = null)
        {
#if UNITY_EDITOR
            if (context != null)
                Debug.LogError($"{DebugPrefix} {message}", context);
            else
                Debug.LogError($"{DebugPrefix} {message}");
#endif
        }
        #endregion

        #region Colored Logs
        public static void LogWithColor(object message, string color = "#FFFFFF", Object context = null)
        {
#if UNITY_EDITOR
            string coloredMessage = $"<color={color}>{DebugPrefix} {message}</color>";
            if (context != null)
                Debug.Log(coloredMessage, context);
            else
                Debug.Log(coloredMessage);
#endif
        }

        public static void LogWarningWithColor(object message, string color = "#FFA500", Object context = null)
        {
#if UNITY_EDITOR
            string coloredMessage = $"<color={color}>{DebugPrefix} {message}</color>";
            if (context != null)
                Debug.LogWarning(coloredMessage, context);
            else
                Debug.LogWarning(coloredMessage);
#endif
        }
        #endregion

#if UNITY_EDITOR
        public static void LogEditorNotification(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                EditorUtility.DisplayDialog("Custom Debug Notification", message, "OK");
            }
        }
#endif
    }
}
