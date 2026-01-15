using UnityEngine;
using System;

namespace ExoBeasts.Multiplayer.Auth
{
    /// <summary>
    /// Responsavel pela autenticacao de jogadores via Epic Online Services
    /// Suporta DevAuthTool, Device ID e Epic Account
    /// </summary>
    public class EOSAuthenticator : MonoBehaviour
    {
        // Eventos
        public event Action OnLoginSuccess;
        public event Action<string> OnLoginFailed;
        public event Action OnLogoutSuccess;

        [Header("Dev Settings")]
        [SerializeField] private string devCredentialName = "TestUser";
        [SerializeField] private bool useDevAuthTool = true;

        // TODO: Adicionar ProductUserId do jogador logado
        // private Epic.OnlineServices.ProductUserId localUserId;

        private bool isLoggedIn = false;

        /// <summary>
        /// Login usando DevAuthTool (apenas para desenvolvimento)
        /// </summary>
        public void LoginWithDevAuthTool(string credentialName = null)
        {
            string credential = credentialName ?? devCredentialName;
            Debug.Log($"[EOSAuthenticator] Tentando login com DevAuthTool: {credential}");

            // TODO: Implementar login via DevAuthTool
            // 1. Obter Connect Interface do EOSManager
            // 2. Criar LoginOptions com credenciais dev
            // 3. Chamar ConnectInterface.Login()
            // 4. No callback, armazenar ProductUserId e disparar OnLoginSuccess

            // Simulacao por enquanto
            isLoggedIn = true;
            OnLoginSuccess?.Invoke();
            Debug.Log("[EOSAuthenticator] Login com DevAuthTool bem-sucedido (simulado)");
        }

        /// <summary>
        /// Login anonimo usando Device ID
        /// </summary>
        public void LoginWithDeviceId()
        {
            Debug.Log("[EOSAuthenticator] Tentando login com Device ID...");

            // TODO: Implementar login via Device ID
            // 1. Obter Connect Interface
            // 2. Criar LoginOptions com tipo DeviceId
            // 3. Chamar ConnectInterface.Login()

            // Simulacao por enquanto
            isLoggedIn = true;
            OnLoginSuccess?.Invoke();
            Debug.Log("[EOSAuthenticator] Login com Device ID bem-sucedido (simulado)");
        }

        /// <summary>
        /// Login usando Epic Account (para producao)
        /// </summary>
        public void LoginWithEpicAccount()
        {
            Debug.Log("[EOSAuthenticator] Tentando login com Epic Account...");

            // TODO: Implementar login via Epic Account
            // 1. Obter Auth Interface
            // 2. Abrir browser para autenticacao
            // 3. Obter EpicAccountId
            // 4. Converter para ProductUserId via Connect Interface

            Debug.LogWarning("[EOSAuthenticator] Login com Epic Account ainda nao implementado");
        }

        /// <summary>
        /// Deslogar o usuario atual
        /// </summary>
        public void Logout()
        {
            Debug.Log("[EOSAuthenticator] Fazendo logout...");

            // TODO: Chamar ConnectInterface.Logout()

            isLoggedIn = false;
            // localUserId = null;
            OnLogoutSuccess?.Invoke();
            Debug.Log("[EOSAuthenticator] Logout bem-sucedido");
        }

        public bool IsLoggedIn()
        {
            return isLoggedIn;
        }

        // TODO: Metodo para obter o ProductUserId do usuario logado
        // public Epic.OnlineServices.ProductUserId GetLocalUserId()
        // {
        //     return localUserId;
        // }

        /// <summary>
        /// Obter nome de exibicao do jogador
        /// </summary>
        public string GetDisplayName()
        {
            // TODO: Obter nome real do EOS
            return isLoggedIn ? "Player" : "Guest";
        }
    }
}
