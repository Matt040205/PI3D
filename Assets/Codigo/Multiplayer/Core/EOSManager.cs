using UnityEngine;

namespace ExoBeasts.Multiplayer.Core
{
    /// <summary>
    /// Singleton principal para gerenciar o SDK da Epic Online Services
    /// Responsavel por inicializar, manter e fazer Tick() no SDK
    /// </summary>
    public class EOSManager : MonoBehaviour
    {
        private static EOSManager _instance;
        public static EOSManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<EOSManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("EOSManager");
                        _instance = go.AddComponent<EOSManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Epic Credentials")]
        [SerializeField] private string productId = "";
        [SerializeField] private string sandboxId = "";
        [SerializeField] private string deploymentId = "";
        [SerializeField] private string clientId = "";
        [SerializeField] private string clientSecret = "";

        // TODO: Adicionar referencia ao PlatformInterface do EOS SDK
        // private Epic.OnlineServices.Platform.PlatformInterface platformInterface;

        private bool isInitialized = false;

        private void Awake()
        {
            // Singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeEOS();
        }

        private void InitializeEOS()
        {
            Debug.Log("[EOSManager] Inicializando Epic Online Services SDK...");

            // TODO: Inicializar o SDK da Epic
            // platformInterface = Epic.OnlineServices.Platform.PlatformInterface.Initialize(...);

            isInitialized = true;
            Debug.Log("[EOSManager] EOS SDK inicializado com sucesso!");
        }

        private void Update()
        {
            if (isInitialized)
            {
                // CRITICO: Tick() deve ser chamado todo frame
                // TODO: platformInterface?.Tick();
            }
        }

        private void OnApplicationQuit()
        {
            ShutdownEOS();
        }

        private void ShutdownEOS()
        {
            Debug.Log("[EOSManager] Encerrando Epic Online Services SDK...");

            // TODO: Fazer shutdown do SDK
            // platformInterface?.Release();
            // platformInterface = null;

            isInitialized = false;
        }

        public bool IsInitialized()
        {
            return isInitialized;
        }

        // TODO: Adicionar metodo para obter a interface do SDK
        // public Epic.OnlineServices.Platform.PlatformInterface GetPlatformInterface()
        // {
        //     return platformInterface;
        // }
    }
}
