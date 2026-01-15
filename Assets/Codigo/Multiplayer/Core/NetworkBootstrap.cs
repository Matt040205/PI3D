using UnityEngine;
using Unity.Netcode;

namespace ExoBeasts.Multiplayer.Core
{
    /// <summary>
    /// Responsavel pela inicializacao geral do sistema de rede
    /// Deve estar na cena NetworkBootstrap.unity
    /// </summary>
    public class NetworkBootstrap : MonoBehaviour
    {
        [Header("Configuracoes")]
        [Tooltip("Iniciar automaticamente como Host (P2P)")]
        [SerializeField] private bool autoStartHost = false;

        [Tooltip("Iniciar automaticamente como Client")]
        [SerializeField] private bool autoStartClient = false;

        [Header("Modo P2P")]
        [Tooltip("IMPORTANTE: Em modo P2P, use Host ao inves de Dedicated Server")]
        [SerializeField] private bool useP2PMode = true;

        private void Awake()
        {
            // Garantir que este objeto persista entre cenas
            DontDestroyOnLoad(gameObject);

            Debug.Log($"[NetworkBootstrap] Inicializando sistema de rede... (P2P: {useP2PMode})");
        }

        private void Start()
        {
            // Inicializar componentes de rede
            InitializeNetworking();

            // Auto-start para testes (se configurado)
            if (autoStartHost && useP2PMode)
            {
                StartHost();
            }
            else if (autoStartClient)
            {
                StartClient();
            }
        }

        private void InitializeNetworking()
        {
            // TODO: Inicializar NetworkManager
            // TODO: Configurar callbacks de conexao
            Debug.Log("[NetworkBootstrap] Sistema de rede inicializado");
        }

        /// <summary>
        /// DEPRECATED: Use StartHost() para P2P
        /// </summary>
        public void StartServer()
        {
            Debug.LogWarning("[NetworkBootstrap] StartServer() esta deprecated! Use StartHost() para P2P");
            // Dedicated Server nao e usado em P2P
        }

        public void StartClient()
        {
            Debug.Log("[NetworkBootstrap] Iniciando como cliente...");
            // TODO: NetworkManager.Singleton.StartClient();
        }

        public void StartHost()
        {
            Debug.Log("[NetworkBootstrap] Iniciando como host...");
            // TODO: NetworkManager.Singleton.StartHost();
        }

        public void Shutdown()
        {
            Debug.Log("[NetworkBootstrap] Encerrando conexao...");
            // TODO: NetworkManager.Singleton.Shutdown();
        }
    }
}
