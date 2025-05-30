﻿using UnityEngine;

namespace JayoVRCBridge
{
    static class Logger
    {
        private static string identifier = "VRC Bridge";
        public static void LogInfo(object message) => Debug.Log($"[{identifier}] {message}");
        public static void LogWarning(object message) => Debug.LogWarning($"[{identifier}] {message}");
        public static void LogError(object message) => Debug.LogError($"[{identifier}] {message}");
    }
}
