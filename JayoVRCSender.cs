using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using uOSC;

namespace JayoVRCBridge
{
    public class JayoVRCSender : MonoBehaviour
    {
        public event Action SenderConnected;
        public event Action SenderDisconnected;
        public event Action<string> SenderError;
        public bool IsRunning { get { return osc.isRunning; } }

        private string address;
        private int port;

        private uOscClient osc;
        private bool listenersRegistered = false;

        public void Awake()
        {
            if(!listenersRegistered)
            {
                gameObject.SetActive(false);
                osc = gameObject.AddComponent<uOscClient>();
                osc.enabled = false;
                osc.maxQueueSize = 300;
                gameObject.SetActive(true);

                osc.onClientStarted.AddListener((string a, int b) => {
                    SenderConnected?.Invoke();
                    Logger.LogInfo($"Sender started!");
                });

                osc.onClientStopped.AddListener((string a, int b) => {
                    SenderDisconnected?.Invoke();
                    Logger.LogInfo($"Sender stopped!");
                });

                listenersRegistered = true;
            }
        }

        public void Activate(string address, int port)
        {
            if (osc == null) return;

            if (osc.isRunning)
            {
                Logger.LogInfo($"Sender OSC already running!");
                return;
            }

            Logger.LogInfo($"Sender initializing on port {port}!");
            this.address = address;
            this.port = port;
            osc.port = this.port;

            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(this.address);

                if (addresses.Length > 0)
                {
                    osc.address = addresses[0].ToString();
                }
                else
                {
                    SenderError?.Invoke("No IP addresses found for hostname");
                    Logger.LogError("EEP!");
                    return;
                }
            }
            catch (Exception ex)
            {
                SenderError?.Invoke(ex.Message);
                Logger.LogError("OOP!");
                return;
            }
            osc.enabled = true;
        }

        public void Deactivate()
        {
            if (osc == null) return;

            if (!osc.isRunning)
            {
                Logger.LogInfo($"Sender already stopped!");
                return;
            }

            Logger.LogInfo($"Sender de-initializing!");
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

        private void OnApplicationQuit()
        {
            Deactivate();
        }

        public void Send(string address, params object[] values)
        {
            if (osc == null) return;
            osc.Send(address, values);
        }

        public void Send(Message message)
        {
            if (osc == null) return;

            osc.Send(message);
        }

        public void Send(Bundle bundle)
        {
            if (osc == null) return;

            osc.Send(bundle);
        }
    }
}
