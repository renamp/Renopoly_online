// vis2k: GUILayout instead of spacey += ...; removed Update hotkeys to avoid
// confusion if someone accidentally presses one.
using Mirror.Authenticators;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Mirror
{
    /// <summary>Shows NetworkManager controls in a GUI at runtime.</summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkManagerHUD")]
    [RequireComponent(typeof(NetworkManager))]
    [HelpURL("https://mirror-networking.gitbook.io/docs/components/network-manager-hud")]
    public class NetworkManagerHUD : MonoBehaviour
    {
        NetworkManager manager;
        public NewNetworkAuthenticator basicAuthenticator;

        // Deprecated 2021-02-24
        [Obsolete("showGUI will be removed unless someone has a valid use case. Simply use or don't use the HUD component.")]
        public bool showGUI = true;
        public bool showServer = true;
        public bool showHost = false;

        public int offsetX;
        public int offsetY;

        public string PlayerName;
        public Transform objCamera;
        public Transform objTabuleiro;
        public float rotationSpeed = 5f;

        private GameObject Button_Pg1_Server;
        private GameObject Button_Pg1_Host;

        //private bool stopRotation;

        public void ExitApplication()
        {
            Application.Quit();
        }

        public void setPlayerName(string playerName)
        {
            this.PlayerName = playerName;
        }
        public void setNetworkAddress(string networkAddress)
        {
            manager.networkAddress = networkAddress;
        }
        public void startHost()
        {
            PlayerName = GameObject.Find("InputField_playerName").GetComponent<InputField>().text;
            basicAuthenticator.PlayerName = PlayerName;
            string serverIp = Menus.serverIp.GetComponent<InputField>().text;
            if (serverIp != null && serverIp.Length > 1)
                manager.networkAddress = serverIp;
            Menus.connectingIp.GetComponent<Text>().text = manager.networkAddress;
            Menus.updateFile();
            manager.StartHost();
        }
        public void startClient()
        {
            PlayerName = GameObject.Find("InputField_playerName").GetComponent<InputField>().text;
            basicAuthenticator.PlayerName = PlayerName;
            string serverIp = Menus.serverIp.GetComponent<InputField>().text;
            if (serverIp != null && serverIp.Length > 1)
                manager.networkAddress = serverIp;
            Menus.connectingIp.GetComponent<Text>().text = manager.networkAddress;
            Menus.updateFile();
            manager.StartClient();
        }
        public void startServer()
        {
            manager.StartServer();
        }

        void Awake()
        {
            manager = GetComponent<NetworkManager>();
        }

        public void Start()
        {
            Button_Pg1_Server = GameObject.Find("Button_Pg1_Server");
            Button_Pg1_Host = GameObject.Find("Button_Pg1_Host");
            StartCoroutine("slowUpdate");
        }

        IEnumerator slowUpdate()
        {
            while (true)
            {
                Button_Pg1_Host.SetActive(showHost);
                Button_Pg1_Server.SetActive(showServer);
                yield return new WaitForSeconds(0.8f);
            }
        }

        private void Update()
        {
            if ( !NetworkClient.active && !NetworkServer.active)
            {
                Menus.setFirstScreenMenuPg1();
                objCamera.RotateAround(objTabuleiro.position, objTabuleiro.up, rotationSpeed * Time.deltaTime);
            }
            else if((NetworkClient.isConnected && NetworkClient.ready) )
            {
                Menus.setFirstScreenMenuPg3();
            }
        }

        void OnGUI()
        {
#pragma warning disable 618
            if (!showGUI) return;
#pragma warning restore 618

            GUILayout.BeginArea(new Rect(10 + offsetX, 40 + offsetY, 600, 9999));
            if (!NetworkClient.isConnected && !NetworkServer.active)
            {
                //StartButtons();
            }
            else
            {
                //StatusLabels();
            }

            // client ready
            if (NetworkClient.isConnected && !NetworkClient.ready)
            {
                if (GUILayout.Button("Client Ready"))
                {
                    NetworkClient.Ready();
                    if (NetworkClient.localPlayer == null)
                    {
                        NetworkClient.AddPlayer();
                    }
                }
            }

            //StopButtons();

            GUILayout.EndArea();
        }

        void StartButtons()
        {
            GUIStyle guiStyle1 = new GUIStyle(GUI.skin.button);
            GUIStyle guiStyle2 = new GUIStyle(GUI.skin.textField);
            guiStyle1.fontSize = 30;
            guiStyle2.fontSize = 28;
            guiStyle2.alignment = TextAnchor.MiddleCenter;

            if (!NetworkClient.active)
            {
                // Server + Client
                if ((Application.platform != RuntimePlatform.WebGLPlayer) && showServer)
                {
                    if (GUILayout.Button("Host (Server + Client)", guiStyle1, GUILayout.Height(80)))
                    {
                        startHost();

                    }
                }

                // Player name
                PlayerName = GUILayout.TextField(PlayerName, guiStyle2, GUILayout.Height(80));
                
                // Client + IP
                GUILayout.BeginHorizontal();
                manager.networkAddress = GUILayout.TextField(manager.networkAddress, guiStyle2, GUILayout.Height(80));
                if (GUILayout.Button("Conectar", guiStyle1, GUILayout.Height(80), GUILayout.Width(150)))
                {
                    startClient();
                }
                GUILayout.EndHorizontal();

                // Server Only
                if ((Application.platform == RuntimePlatform.WebGLPlayer) && showServer)
                {
                    // cant be a server in webgl build
                    GUILayout.Box("(  WebGL cannot be server  )");
                }
                else if(showServer)
                {
                    if (GUILayout.Button("Server Only", GUILayout.Height(80)))
                        manager.StartServer();
                }
            }
            else
            {
                // Connecting
                GUILayout.Label("Connecting to " + manager.networkAddress + "..");
                if (GUILayout.Button("Cancel Connection Attempt"))
                {
                    manager.StopClient();
                }
            }
        }

        void StatusLabels()
        {
            // host mode
            // display separately because this always confused people:
            //   Server: ...
            //   Client: ...
            if (NetworkServer.active && NetworkClient.active)
            {
                GUILayout.Label($"<b>Host</b>: running via {Transport.activeTransport}");
            }
            // server only
            else if (NetworkServer.active)
            {
                GUILayout.Label($"<b>Server</b>: running via {Transport.activeTransport}");
            }
            // client only
            else if (NetworkClient.isConnected)
            {
                GUILayout.Label($"<b>Client</b>: connected to {manager.networkAddress} via {Transport.activeTransport}");
            }
        }

        void StopButtons()
        {
            // stop host if host mode
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                if (GUILayout.Button("Stop Host", GUILayout.Height(80)))
                {
                    manager.StopHost();
                }
            }
            // stop client if client-only
            else if (NetworkClient.isConnected)
            {
                if (GUILayout.Button("Stop Client", GUILayout.Height(80)))
                {
                    manager.StopClient();
                }
            }
            // stop server if server-only
            else if (NetworkServer.active)
            {
                if (GUILayout.Button("Stop Server", GUILayout.Height(80)))
                {
                    manager.StopServer();
                }
            }
        }
    }
}
