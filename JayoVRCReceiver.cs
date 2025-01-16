using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using uOSC;

namespace JayoVRCBridge
{
    public class JayoVRCReceiver : MonoBehaviour
    {
        public event Action ReceiverConnected;
        public event Action ReceiverDisconnected;
        public event Action<string> ReceiverError;
        public bool IsRunning { get { return osc.isRunning; } }

        private int port;

        private uOscServer osc;
        private bool listenersRegistered = false;
        private static string prefix = "_xjvb_";

        public void Awake()
        {
            if (!listenersRegistered)
            {
                osc = gameObject.AddComponent<uOscServer>();
                osc.enabled = false;
                osc.onDataReceived.AddListener(OnDataReceived);

                osc.onServerStarted.AddListener((int a) => {
                    ReceiverConnected?.Invoke();
                    Logger.LogInfo($"Receiver started!");
                });

                osc.onServerStopped.AddListener((int a) => {
                    ReceiverDisconnected?.Invoke();
                    Logger.LogInfo($"Receiver stopped!");
                });

                listenersRegistered = true;
            }
        }

        public void Activate(int port)
        {
            if (osc == null) return;

            if (osc.isRunning)
            {
                Logger.LogInfo($"Receiver OSC already running!");
                return;
            }

            Logger.LogInfo($"Receiver initializing!");
            this.port = port;
            osc.port = this.port;
            osc.enabled = true;
        }

        public void Deactivate()
        {
            if (osc == null) return;

            if (!osc.isRunning)
            {
                Logger.LogInfo($"Receiver already stopped!");
                return;
            }

            Logger.LogInfo($"Receiver de-initializing!");
            osc.enabled = false;
        }

        private void OnDisable()
        {
            Deactivate();
        }

        private void OnDestroy()
        {
            Deactivate();
        }

        void OnDataReceived(Message message)
        {
            //Logger.LogInfo($"Inbound Message: {message.ToString()}");
            if (message.address.StartsWith("/avatar/parameters/"))
            {
                string[] addressParts = message.address.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                string parameterName = addressParts[2];
                var value = message.values[0];
                float floatValue = (value.GetType() == typeof(bool)) ? Convert.ToSingle(value) : float.Parse(value.ToString());
                //Logger.LogInfo($"Got Parameter Message: {parameterName}; {value}");
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat($"{prefix}{parameterName}", floatValue);
                VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger($"{prefix}paramChange", 0, 0, 0, parameterName, floatValue.ToString(), "");
            }
            else if (message.address.StartsWith("/avatar/change"))
            {
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString($"{prefix}avatar", message.values[0].ToString());
                VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger($"{prefix}avatarChange", 0, 0, 0, message.values[0].ToString(), "", "");
            }
        }
    }
}
