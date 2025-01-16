using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq.Expressions;
using VNyanInterface;
using TMPro;

namespace JayoVRCBridge
{
    public class JayoVRCBridge : MonoBehaviour, VNyanInterface.IButtonClickedHandler
    {
        public GameObject windowPrefab;
        public GameObject window;

        private JayoVRCSender sender;
        private JayoVRCReceiver receiver;

        private VNyanPluginUpdater updater;

        //private TMP_Text senderStatusText;
        private Button enableSenderButton;
        private Button disableSenderButton;
        private Toggle autoStartSenderToggle;
        private TMP_InputField senderPortField;

        //private TMP_Text receiverStatusText;
        private Button enableReceiverButton;
        private Button disableReceiverButton;
        private Toggle autoStartReceiverToggle;
        private TMP_InputField receiverPortField;

        private Button testButton;
        private Button testButton2;

        private bool enableSenderOnStart;
        private int senderPort;

        private bool enableReceiverOnStart;
        private int receiverPort;

        private bool listenersRegistered = false;

        private string currentVersion = "v0.1.0";
        private string repoName = "jayo-exe/JayoVRCBridge";
        private string updateLink = "https://jayo-exe.itch.io/vrchat-osc-bridge-for-vnyan";

        private void OnApplicationQuit()
        {
            // Save settings
            savePluginSettings();
        }

        public void Awake()
        {

            Logger.LogInfo($"Plugin is Awake!");

            updater = new VNyanPluginUpdater(repoName, currentVersion, updateLink);
            updater.OpenUrlRequested += (url) => MainThreadDispatcher.Enqueue(() => { Application.OpenURL(url); });

            enableSenderOnStart = false;
            senderPort = 9000;

            enableReceiverOnStart = false;
            receiverPort = 9001;

            //Debug.Log($"Loading Settings");
            loadPluginSettings();
            updater.CheckForUpdates();

            //Debug.Log($"Beginning Plugin Setup");

            sender = gameObject.AddComponent<JayoVRCSender>();
            receiver = gameObject.AddComponent<JayoVRCReceiver>();

            //VRCBridgeTriggerHandler.EnableRequested += () => { enableOptimizer(); };
            //VRCBridgeTriggerHandler.DisableRequested += () => { disableOptimizer(); };

            if(!listenersRegistered)
            {
                VRCBridgeTriggerHandler.EnableSendRequested += OnEnableSendRequested;
                VRCBridgeTriggerHandler.DisableSendRequested += OnDisableSendRequested;
                VRCBridgeTriggerHandler.EnableReceiveRequested += OnEnableReceiveRequested;
                VRCBridgeTriggerHandler.DisableReceiveRequested += OnDisableReceiveRequested;
                VRCBridgeTriggerHandler.AvatarParameterChangeRequested += OnAvatarParameterChangeRequested;
                VRCBridgeTriggerHandler.InputAxisChangeRequested += OnInputAxisChangeRequested;
                VRCBridgeTriggerHandler.InputButtonChangeRequested += OnInputButtonChangeRequested;
                VRCBridgeTriggerHandler.InputChatboxChangeRequested += OnInputChatboxChangeRequested;
                VRCBridgeTriggerHandler.ChatboxTypingChangeRequested += OnChatboxTypingChangeRequested;
                VRCBridgeTriggerHandler.TrackerPositionChangeRequested += OnTrackerPositionChangeRequested;
                VRCBridgeTriggerHandler.TrackerRotationChangeRequested += OnTrackerRotationChangeRequested;
                VRCBridgeTriggerHandler.TrackerEyeClosedChangeRequested += OnTrackerEyeClosedChangeRequested;
                VRCBridgeTriggerHandler.TrackerEyePitchYawChangeRequested += OnTrackerEyePitchYawChangeRequested;
                VRCBridgeTriggerHandler.TrackerEyePitchYawDistanceChangeRequested += OnTrackerEyePitchYawDistanceChangeRequested;
                VRCBridgeTriggerHandler.TrackerEyeCenterVecChangeRequested += OnTrackerEyeCenterVecChangeRequested;
                VRCBridgeTriggerHandler.TrackerEyeCenterVecFullChangeRequested += OnTrackerEyeCenterVecFullChangeRequested;
                VRCBridgeTriggerHandler.TrackerEyeLeftRightPitchYawChangeRequested += OnTrackerEyeLeftRightPitchYawChangeRequested;
                VRCBridgeTriggerHandler.TrackerEyeLeftRightVecChangeRequested += OnTrackerEyeLeftRightVecChangeRequested;

                listenersRegistered = true;
            }
            VNyanInterface.VNyanInterface.VNyanTrigger.registerTriggerListener(VRCBridgeTriggerHandler.Instance);
            
            try
            {
                VNyanInterface.VNyanInterface.VNyanUI.registerPluginButton("VRChat OSC Bridge", this);
                window = (GameObject)VNyanInterface.VNyanInterface.VNyanUI.instantiateUIPrefab(windowPrefab);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }

            // Hide the window by default
            if (window != null)
            {
                //senderStatusText = window.transform.Find("Panel/SenderRow/StatusText").GetComponent<TMP_Text>();
                enableSenderButton = window.transform.Find("Panel/SenderRow/StartButton").GetComponent<Button>();
                disableSenderButton = window.transform.Find("Panel/SenderRow/StopButton").GetComponent<Button>();
                autoStartSenderToggle = window.transform.Find("Panel/SenderRow/AutoStart/FieldHead/AutoStartToggle").GetComponent<Toggle>();
                senderPortField = window.transform.Find("Panel/SenderRow/Port/PortField").GetComponent<TMP_InputField>();

                //receiverStatusText = window.transform.Find("Panel/ReceiverRow/StatusText").GetComponent<TMP_Text>();
                enableReceiverButton = window.transform.Find("Panel/ReceiverRow/StartButton").GetComponent<Button>();
                disableReceiverButton = window.transform.Find("Panel/ReceiverRow/StopButton").GetComponent<Button>();
                autoStartReceiverToggle = window.transform.Find("Panel/ReceiverRow/AutoStart/FieldHead/AutoStartToggle").GetComponent<Toggle>();
                receiverPortField = window.transform.Find("Panel/ReceiverRow/Port/PortField").GetComponent<TMP_InputField>();

                testButton = window.transform.Find("Panel/TestButton").GetComponent<Button>();
                testButton2 = window.transform.Find("Panel/TestButton2").GetComponent<Button>();

                window.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                window.SetActive(false);

                try
                {
                    //Debug.Log($"Preparing Plugin Window");

                    updater.PrepareUpdateUI(
                        window.transform.Find("Panel/UpdateRow/VersionText").gameObject,
                        window.transform.Find("Panel/UpdateRow/UpdateText").gameObject,
                        window.transform.Find("Panel/UpdateRow/UpdateButton").gameObject
                    );

                    window.transform.Find("Panel/TitleBar/CloseButton").GetComponent<Button>().onClick.AddListener(() => { closePluginWindow(); });
                    
                    enableSenderButton.onClick.AddListener(() => { enableSender(); });
                    disableSenderButton.onClick.AddListener(() => { disableSender(); });
                    autoStartSenderToggle.onValueChanged.AddListener((v) => { enableSenderOnStart = v; });
                    autoStartSenderToggle.SetIsOnWithoutNotify(enableSenderOnStart);
                    senderPortField.onValueChanged.AddListener((v) => { senderPort = Int32.Parse(v); });
                    senderPortField.SetTextWithoutNotify(senderPort.ToString());

                    enableReceiverButton.onClick.AddListener(() => { enableReceiver(); });
                    disableReceiverButton.onClick.AddListener(() => { disableReceiver(); });
                    autoStartReceiverToggle.onValueChanged.AddListener((v) => { enableReceiverOnStart = v; });
                    autoStartReceiverToggle.SetIsOnWithoutNotify(enableReceiverOnStart);
                    receiverPortField.onValueChanged.AddListener((v) => { receiverPort = Int32.Parse(v); });
                    receiverPortField.SetTextWithoutNotify(receiverPort.ToString());

                    testButton.onClick.AddListener(() => { doTest(); });
                    testButton2.onClick.AddListener(() => { doTest2(); });
                    testButton.gameObject.SetActive(false);
                    testButton2.gameObject.SetActive(false);

                    if (enableSenderOnStart) enableSender();
                    else disableSender();

                    if (enableReceiverOnStart) enableReceiver();
                    else disableReceiver();
                }
                catch (Exception e)
                {
                    Logger.LogError($"Couldn't prepare Plugin Window: {e.Message}");
                }
            }
        }
        public void OnSenderStatusChanged(string newStatus)
        {
            //MainThreadDispatcher.Enqueue(() => { senderStatusText.text = newStatus; });
        }

        public void OnReceiverStatusChanged(string newStatus)
        {
            //MainThreadDispatcher.Enqueue(() => { senderStatusText.text = newStatus; });
        }

        public void loadPluginSettings()
        {
            Dictionary<string, string> settings = VNyanInterface.VNyanInterface.VNyanSettings.loadSettings("JayoVRMHairOptimizerPlugin.cfg");
            if (settings != null)
            {
                string enableSenderOnStartValue;
                settings.TryGetValue("EnableSenderOnStart", out enableSenderOnStartValue);
                if (enableSenderOnStartValue != null) enableSenderOnStart = Boolean.Parse(enableSenderOnStartValue);

                string senderPortValue;
                settings.TryGetValue("SenderPort", out senderPortValue);
                if (senderPortValue != null) senderPort = Int32.Parse(senderPortValue);

                string enableReceiverOnStartValue;
                settings.TryGetValue("EnableReceiverOnStart", out enableReceiverOnStartValue);
                if (enableReceiverOnStartValue != null) enableReceiverOnStart = Boolean.Parse(enableReceiverOnStartValue);

                string receiverPortValue;
                settings.TryGetValue("ReceiverPort", out receiverPortValue);
                if (receiverPortValue != null) receiverPort = Int32.Parse(receiverPortValue);
            }
        }

        public void savePluginSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings["EnableSenderOnStart"] = enableSenderOnStart.ToString();
            settings["SenderPort"] = senderPort.ToString();
            settings["EnableReceiverOnStart"] = enableReceiverOnStart.ToString();
            settings["ReceiverPort"] = receiverPort.ToString();

            VNyanInterface.VNyanInterface.VNyanSettings.saveSettings("JayoVRMHairOptimizerPlugin.cfg", settings);
        }

        public void pluginButtonClicked()
        {
            // Flip the visibility of the window when plugin window button is clicked
            if (window != null)
            {
                window.SetActive(!window.activeSelf);
                if (window.activeSelf)
                {
                    window.transform.SetAsLastSibling();
                }
                window.transform.SetAsLastSibling();
            }
        }

        public void closePluginWindow()
        {
            window.SetActive(false);
        }

        public void doTest()
        {
            Debug.Log("Test Clicked");
            sendMessage("/avatar/parameters/is_lit", 0);
            sendMessage("/input/Jump", 0);
            VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger("_xjvb_buttonOn", 0, 0, 0, "Jump", "", "");
            VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger("_xjvb_setParam", 0, 0, 0, "is_lit", "1", "");
        }
        public void doTest2()
        {
            Debug.Log("Test2 Clicked");
            VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger("_xjvb_setParam", 0, 0, 0, "is_lit", "0", "");
            VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger("_xjvb_buttonOff", 0, 0, 0, "Jump", "", "");
        }

        public void enableSender()
        {
            if (sender.IsRunning) return;
            sender.Activate("127.0.0.1", senderPort);
            enableSenderButton.gameObject.SetActive(false);
            disableSenderButton.gameObject.SetActive(true);
        }

        public void disableSender()
        {
            if (!sender.IsRunning) return;
            sender.Deactivate();
            enableSenderButton.gameObject.SetActive(true);
            disableSenderButton.gameObject.SetActive(false);
        }

        public void enableReceiver()
        {
            if (receiver.IsRunning) return;
            receiver.Activate(receiverPort);
            enableReceiverButton.gameObject.SetActive(false);
            disableReceiverButton.gameObject.SetActive(true);
        }

        public void disableReceiver()
        {
            if (!receiver.IsRunning) return;
            receiver.Deactivate();
            enableReceiverButton.gameObject.SetActive(true);
            disableReceiverButton.gameObject.SetActive(false);
        }

        private void sendMessage(string address, params object[] values)
        {
            if (!sender.IsRunning) return;
            sender.Send(address, values);
        }

        public void OnEnableSendRequested()
        {
            enableSender();
        }

        public void OnDisableSendRequested() {
            disableSender();
        }

        public void OnEnableReceiveRequested() {
            enableReceiver();
        }

        public void OnDisableReceiveRequested() {
            disableReceiver();
        }

        public void OnAvatarParameterChangeRequested(string name, string value) {

            if (value.ToLower() == "true")
            {
                sendMessage($"/avatar/parameters/{name}", true);
            }
            else if (value.ToLower() == "false")
            {
                sendMessage($"/avatar/parameters/{name}", false);
            }
            else if (float.TryParse(value, out float floatValue))
            {
                if (!value.Contains("."))
                {
                    sendMessage($"/avatar/parameters/{name}", (int)floatValue);
                } else
                {
                    sendMessage($"/avatar/parameters/{name}", floatValue);
                }
            }
        }

        public void OnInputAxisChangeRequested(string name, float value) {
            sendMessage($"/input/{name}", value);
        }

        public void OnInputButtonChangeRequested(string name, int state) {
            sendMessage($"/input/{name}", state);
        }

        public void OnInputChatboxChangeRequested(string chatText, bool sendImmediate, bool notify) {
            sendMessage($"/chatbox/input", chatText, sendImmediate, notify);
        }

        public void OnChatboxTypingChangeRequested(bool newStatus) {
            sendMessage($"/chatbox/typing", newStatus);
        }

        public void OnTrackerPositionChangeRequested(int trackerID, float x, float y, float z) {
            string trackerName = trackerID > 0 ? trackerID.ToString() : "head";
            sendMessage($"tracking/trackers/{trackerName}/position", x, y, z);
        }

        public void OnTrackerRotationChangeRequested(int trackerID, float x, float y, float z) {
            string trackerName = trackerID > 0 ? trackerID.ToString() : "head";
            sendMessage($"tracking/trackers/{trackerName}/rotation", x, y, z);
        }

        public void OnTrackerEyeClosedChangeRequested(float newValue) {
            sendMessage($"tracking/eye/EyesClosedAmount", newValue);
        }

        public void OnTrackerEyePitchYawChangeRequested(float x, float y) {
            sendMessage($"tracking/eye/CenterPitchYaw", x, y);
        }

        public void OnTrackerEyePitchYawDistanceChangeRequested(float x, float y, float z) {
            sendMessage($"tracking/eye/CenterPitchYawDist", x, y, z);
        }

        public void OnTrackerEyeCenterVecChangeRequested(float x, float y, float z) {
            sendMessage($"tracking/eye/CenterVec", x, y, z);
        }

        public void OnTrackerEyeCenterVecFullChangeRequested(float x, float y, float z) {
            sendMessage($"tracking/eye/CenterVecFull", x, y, z);
        }

        public void OnTrackerEyeLeftRightPitchYawChangeRequested(Vector2 left, Vector2 right) {
            sendMessage($"tracking/eye/LeftRightPitchYaw", left.x, left.y, right.x, right.y);
        }

        public void OnTrackerEyeLeftRightVecChangeRequested(Vector3 left, Vector3 right) {
            sendMessage($"tracking/eye/LeftRightVec", left.x, left.y, left.z, right.x, right.y, right.z);
        }

    }
}
