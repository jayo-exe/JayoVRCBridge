using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace JayoVRCBridge
{
    class VRCBridgeTriggerHandler : VNyanInterface.ITriggerHandler
    {
        // use events to communicate with the plugin
        // plugin and other modules will hook into these events to handle anything that needs to happen on thier side
        
        //enable/disable the sender or receiver via trigger
        public static event Action EnableSendRequested;
        public static event Action DisableSendRequested;
        public static event Action EnableReceiveRequested;
        public static event Action DisableReceiveRequested;

        //avatar parameters
        public static event Action<string, string> AvatarParameterChangeRequested;
        
        //input controls
        public static event Action<string, float> InputAxisChangeRequested;
        public static event Action<string, int> InputButtonChangeRequested;
        public static event Action<string, bool, bool> InputChatboxChangeRequested;
        public static event Action<bool> ChatboxTypingChangeRequested;

        //tracking
        public static event Action<int, float, float, float> TrackerPositionChangeRequested;
        public static event Action<int, float, float, float> TrackerRotationChangeRequested;

        public static event Action<float> TrackerEyeClosedChangeRequested;
        public static event Action<float, float> TrackerEyePitchYawChangeRequested;
        public static event Action<float, float, float> TrackerEyePitchYawDistanceChangeRequested;
        public static event Action<float, float, float> TrackerEyeCenterVecChangeRequested;
        public static event Action<float, float, float> TrackerEyeCenterVecFullChangeRequested;
        public static event Action<Vector2, Vector2> TrackerEyeLeftRightPitchYawChangeRequested;
        public static event Action<Vector3, Vector3> TrackerEyeLeftRightVecChangeRequested;

        public static VRCBridgeTriggerHandler Instance { get { return _instance; } }
        private static VRCBridgeTriggerHandler _instance = new();

        // the prefix used to denote triggers that this plugin should respond to
        private static string prefix = "_xjvb_";

        // named triggers and thier associated hander methods
        private static Dictionary<string, Action<int, int, int, string, string, string>> actionHandlers = new Dictionary<string, Action<int, int, int, string, string, string>>
        {
            ["enableSend"] = handleEnableSender,
            ["disableSend"] = handleDisableSender,
            ["enableRecv"] = handleEnableReceiver,
            ["disableRecv"] = handleDisableReceiver,
            ["setParam"] = handleSetParameter,
            ["setAxis"] = handleSetAxis,
            ["buttonOn"] = handleButtonOn,
            ["buttonOff"] = handleButtonOff,
            ["setChatbox"] = handleSetChatbox,
            ["setTyping"] = handleSetTyping,
            ["trackerPos"] = handleTrackerPosition,
            ["trackerRot"] = handleTrackerRotation,
            ["eyeClosed"] = handleEyeClosed,
            ["eyePitchYaw"] = handleEyePitchYaw,
            ["eyePitchYawDist"] = handleEyePitchYawDist,
            ["eyeCenterVec"] = handleEyeCenterVec,
            ["eyeCenterVecFull"] = handleEyeCenterVecFull,
            ["eyeLRPitchYaw"] = handleEyeLeftRightPitchYaw,
            ["eyeLRVec"] = handleEyeLeftRightVec
        };

        // general trigger router, sends relevant incoming triggers to matching handers
        public void triggerCalled(string triggerName, int value1, int value2, int value3, string text1, string text2, string text3)
        {
            if (!triggerName.StartsWith(prefix)) return;

            string triggerAction = triggerName.Substring(prefix.Length);
            if(actionHandlers.ContainsKey(triggerAction)) actionHandlers[triggerAction](value1, value2, value3, text1, text2, text3);
        }


        // handler for the _xjho_enable trigger, fires the event to signal the plugin about the request
        public static void handleEnableSender(int value1, int value2, int value3, string text1, string text2, string text3)
        {
            EnableSendRequested?.Invoke();
        }

        // handler for the _xjho_disable trigger, fires the event to signal the plugin about the request
        public static void handleDisableSender(int value1, int value2, int value3, string text1, string text2, string text3)
        {
            DisableSendRequested?.Invoke();
        }

        // handler for the _xjho_enable trigger, fires the event to signal the plugin about the request
        public static void handleEnableReceiver(int value1, int value2, int value3, string text1, string text2, string text3)
        {
            EnableReceiveRequested?.Invoke();
        }

        // handler for the _xjho_disable trigger, fires the event to signal the plugin about the request
        public static void handleDisableReceiver(int value1, int value2, int value3, string text1, string text2, string text3)
        {
            DisableReceiveRequested?.Invoke();
        }

        public static void handleSetParameter(int value1, int value2, int value3, string text1, string text2, string text3)
        {
            AvatarParameterChangeRequested?.Invoke(parseStringArgument(text1), parseStringArgument(text2));
        }

        public static void handleSetAxis(int value1, int value2, int value3, string text1, string text2, string text3) 
        {
            InputAxisChangeRequested?.Invoke(parseStringArgument(text1), parseFloatArgument(text2));
        }

        public static void handleButtonOn(int value1, int value2, int value3, string text1, string text2, string text3) 
        {
            InputButtonChangeRequested?.Invoke(parseStringArgument(text1), 1);
        }

        public static void handleButtonOff(int value1, int value2, int value3, string text1, string text2, string text3)
        {
            InputButtonChangeRequested?.Invoke(parseStringArgument(text1), 0);
        }

        public static void handleSetChatbox(int value1, int value2, int value3, string text1, string text2, string text3) 
        {
            InputChatboxChangeRequested?.Invoke(parseStringArgument(text1), value1 > 0, value2 > 0);
        }

        public static void handleSetTyping(int value1, int value2, int value3, string text1, string text2, string text3) 
        {
            ChatboxTypingChangeRequested?.Invoke(value1 > 0);
        }

        public static void handleTrackerPosition(int value1, int value2, int value3, string text1, string text2, string text3) 
        {
            TrackerPositionChangeRequested?.Invoke(value1, parseFloatArgument(text1), parseFloatArgument(text2), parseFloatArgument(text3));
        }

        public static void handleTrackerRotation(int value1, int value2, int value3, string text1, string text2, string text3) 
        {
            TrackerRotationChangeRequested?.Invoke(value1, parseFloatArgument(text1), parseFloatArgument(text2), parseFloatArgument(text3));
        }

        public static void handleEyeClosed(int value1, int value2, int value3, string text1, string text2, string text3) 
        {
            TrackerEyeClosedChangeRequested?.Invoke(parseFloatArgument(text1));
        }

        public static void handleEyePitchYaw(int value1, int value2, int value3, string text1, string text2, string text3) 
        {
            TrackerEyePitchYawChangeRequested?.Invoke(parseFloatArgument(text1), parseFloatArgument(text2));
        }

        public static void handleEyePitchYawDist(int value1, int value2, int value3, string text1, string text2, string text3) 
        {
            TrackerEyePitchYawDistanceChangeRequested?.Invoke(parseFloatArgument(text1), parseFloatArgument(text2), parseFloatArgument(text3));
        }

        public static void handleEyeCenterVec(int value1, int value2, int value3, string text1, string text2, string text3) 
        {
            TrackerEyeCenterVecChangeRequested?.Invoke(parseFloatArgument(text1), parseFloatArgument(text2), parseFloatArgument(text3));
        }

        public static void handleEyeCenterVecFull(int value1, int value2, int value3, string text1, string text2, string text3) 
        {
            TrackerEyeCenterVecFullChangeRequested?.Invoke(parseFloatArgument(text1), parseFloatArgument(text2), parseFloatArgument(text3));
        }

        public static void handleEyeLeftRightPitchYaw(int value1, int value2, int value3, string text1, string text2, string text3)
        {
            TrackerEyeLeftRightPitchYawChangeRequested?.Invoke(parseVector2Argument(text1), parseVector2Argument(text2));
        }

        public static void handleEyeLeftRightVec(int value1, int value2, int value3, string text1, string text2, string text3)
        {
            TrackerEyeLeftRightVecChangeRequested?.Invoke(parseVector3Argument(text1), parseVector3Argument(text2));
        }

        private static string parseStringArgument(string arg)
        {
            if (arg.StartsWith("<") && arg.EndsWith(">"))
            {
                return VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterString(arg.Substring(1, arg.Length - 2));
            }
            return arg;
        }

        private static float parseFloatArgument(string arg)
        {
            if (arg.StartsWith("[") && arg.EndsWith("]"))
            {
                //float param, just get and return the value directly
                return VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(arg.Substring(1, arg.Length - 2));
            }

            if (arg.StartsWith("<") && arg.EndsWith(">"))
            {
                //string param, set arg to the value from the parameters list before parsing
                arg = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterString(arg.Substring(1, arg.Length - 2));
            }

            //parse the value of arg into a float
            float returnVal = 0f;
            float.TryParse(arg, NumberStyles.Any, CultureInfo.InvariantCulture, out returnVal);
            return returnVal;
        }

        private static Vector2 parseVector2Argument(string arg)
        {
            string[] argParts = parseStringArgument(arg).Split(new string[] { "," }, StringSplitOptions.None);
            float x = argParts.Length >= 1 ? float.Parse(argParts[0]) : 0f;
            float y = argParts.Length >= 2 ? float.Parse(argParts[1]) : 0f;
            return new Vector2(x, y);
        }

        private static Vector3 parseVector3Argument(string arg)
        {
            string[] argParts = parseStringArgument(arg).Split(new string[] { "," }, StringSplitOptions.None);
            float x = argParts.Length >= 1 ? float.Parse(argParts[0]) : 0f;
            float y = argParts.Length >= 2 ? float.Parse(argParts[1]) : 0f;
            float z = argParts.Length >= 3 ? float.Parse(argParts[2]) : 0f;
            return new Vector3(x, y, z);
        }


    }
}
