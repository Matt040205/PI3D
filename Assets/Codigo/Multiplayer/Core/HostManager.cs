using UnityEngine;
using Unity.Netcode;

namespace ExoBeasts.Multiplayer.Core
{
    /// <summary>
    /// Gerencia a logica do Host em modo P2P
    /// Host = Servidor + Cliente ao mesmo tempo
    /// </summary>
    public class HostManager : MonoBehaviour
    {
        private static HostManager _instance;
        public static HostManager Instance => _instance;

        [Header("Host Settings")]
        [SerializeField] private ushort hostPort = 7777;
        [SerializeField] private int maxPlayers = 4;

        private bool isHost = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Iniciar como Host (servidor + cliente)
        /// Chamado quando o jogador cria um lobby e inicia a partida
        /// </summary>
        public void StartAsHost()
        {
            Debug.Log($"[HostManager] Iniciando como Host (P2P) na porta {hostPort}...");

            // TODO: Configurar UnityTransport
            // var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            // transport.SetConnectionData("0.0.0.0", hostPort);

            // Iniciar como Host (servidor + cliente)
            if (NetworkManager.Singleton != null)
            {
                bool success = NetworkManager.Singleton.StartHost();
                if (success)
                {
                    isHost = true;
                    Debug.Log("[HostManager] Host iniciado com sucesso! Aguardando jogadores...");
                }
                else
                {
                    Debug.LogError("[HostManager] Falha ao iniciar Host!");
                }
            }
        }

        /// <summary>
        /// Parar de ser Host
        /// </summary>
        public void StopHost()
        {
            if (!isHost) return;

            Debug.Log("[HostManager] Encerrando Host...");

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
            }

            isHost = false;
        }

        public bool IsHost()
        {
            return isHost && NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        }

        public int GetMaxPlayers()
        {
            return maxPlayers;
        }

        public ushort GetHostPort()
        {
            return hostPort;
        }

        /// <summary>
        /// Obter numero de jogadores conectados (incluindo o host)
        /// </summary>
        public int GetConnectedPlayersCount()
        {
            if (NetworkManager.Singleton == null || !isHost) return 0;

            return (int)NetworkManager.Singleton.ConnectedClients.Count;
        }

        private void OnApplicationQuit()
        {
            if (isHost)
            {
                StopHost();
            }
        }
    }
}
