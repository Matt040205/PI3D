# Sistema Multiplayer - ExoBeasts V3

## 📋 Visão Geral

Sistema multiplayer completo usando **Unity Netcode for GameObjects** + **Epic Online Services**.

**Arquitetura:** P2P (Peer-to-Peer) com Host
**Max Jogadores:** 4
**Serviços Epic:** Lobbies + P2P

---

## 📁 Estrutura de Pastas

```
Multiplayer/
├── Core/                      # Núcleo do sistema
│   ├── NetworkBootstrap.cs    # Inicialização de rede
│   ├── EOSManager.cs           # Gerenciador Epic SDK (Wrapper)
│   ├── EOSConfig.cs            # Configurações (carrega credenciais)
│   ├── WindowsPlatformSpecifics.cs  # Workaround Windows
│   ├── EOSConfig_Main.asset   # ScriptableObject de config
│   ├── EOSManager.prefab      # Prefab do manager
│   └── HostManager.cs          # Gerenciador de Host P2P
│
├── Auth/                      # Autenticação
│   ├── EOSAuthenticator.cs     # Login via Device ID
│   └── SessionManager.cs       # Sessão do usuário
│
├── Lobby/                     # Sistema de Lobbies
│   ├── LobbyData.cs            # Estruturas de dados
│   ├── LobbyManager.cs         # CRUD de lobbies
│   ├── LobbyUI.cs              # Interface principal
│   └── LobbyItemUI.cs          # Item da lista
│
├── GameServer/                # Lógica do Host
│   ├── GameServerManager.cs    # Gerenciamento do Host
│   ├── MatchManager.cs         # Estado da partida
│   └── PlayerRegistry.cs       # Registro de jogadores
│
├── Sync/                      # Sincronização
│   ├── NetworkedPlayerController.cs  # Jogador
│   ├── NetworkedCurrency.cs         # Moedas
│   ├── NetworkedBuilding.cs         # Torres/Traps
│   └── NetworkedHorde.cs            # Waves
│
├── Testing/                   # Testes
│   └── EOSAuthTest.cs          # Script de teste de autenticação
│
├── EOSAuthTest.unity          # Cena de teste
│
└── Docs/                      # Documentação
    ├── AUTHENTICATION_GUIDE.md # Guia de autenticação (Passo 2.1)
    ├── SETUP_INSTRUCTIONS.md   # Guia de configuração
    ├── CREDENTIALS_SETUP.md    # Segurança de credenciais
    └── EOSCredentials.json.example  # Template
```

---

## 🚀 Como Começar

### 1. Configurar Credenciais Epic

Leia: [`CREDENTIALS_SETUP.md`](CREDENTIALS_SETUP.md)

**Resumo:**
1. Criar `EOSCredentials.json` na **raiz do projeto** (não em Assets!)
2. Preencher com credenciais do Epic Developer Portal
3. Verificar que está no `.gitignore`

### 2. Configurar Unity

Leia: [`SETUP_INSTRUCTIONS.md`](SETUP_INSTRUCTIONS.md)

**Resumo:**
1. Pacotes NGO já estão no `manifest.json`
2. Instalar EOS Plugin (PlayEveryWare)
3. Criar ScriptableObject: `Create → Multiplayer → EOS Config`
4. Carregar credenciais no Inspector

---

## 🎮 Fluxo de Jogo P2P

```
[Login EOS] → [Criar/Entrar Lobby] → [Selecionar Personagem] → [Iniciar Partida]
                                                                          ↓
                                                           Host: StartHost()
                                                           Clients: StartClient()
```

---

## 📚 Documentação Completa

- **Plano Detalhado:** `parallel-enchanting-harp.md` (na raiz desta pasta)
- **Autenticação:** [`Docs/AUTHENTICATION_GUIDE.md`](Docs/AUTHENTICATION_GUIDE.md) - Guia completo do Passo 2.1
- **Setup:** `SETUP_INSTRUCTIONS.md`
- **Segurança:** `CREDENTIALS_SETUP.md`

---

## 🔐 Segurança

**NUNCA commitar:**
- `EOSCredentials.json` (arquivo real com credenciais)
- `.env` files

**Sempre verificar `.gitignore` antes de commits!**

---

## 🛠️ Status de Desenvolvimento

### ✅ Fase 1: Fundação (Concluído)
- [x] Estrutura de pastas completa
- [x] 17 scripts base com TODOs
- [x] Sistema de credenciais seguro
- [x] Documentação completa
- [x] Cenas básicas criadas

### ✅ Fase 2.1: Autenticação Device ID (Concluído)
- [x] Instalar EOS Plugin (PlayEveryWare)
- [x] Implementar EOSManagerWrapper (integração com SDK)
- [x] Implementar WindowsPlatformSpecifics (workaround Windows)
- [x] Implementar EOSAuthenticator (login via Device ID)
- [x] Implementar SessionManager (gerenciamento de sessão)
- [x] Criar cena de teste EOSAuthTest
- [x] Documentação AUTHENTICATION_GUIDE.md

**Status:** Funcional - Login via Device ID operacional

### 🚧 Próximos Passos
- [ ] Polir sistema de autenticação (retry, tratamento de erros)
- [ ] Implementar UI de login para usuário final
- [ ] Implementar lobby system (Fase 3)
- [ ] Integrar com gameplay existente (Fase 5)

---

## 📞 Suporte

- Epic Online Services: https://dev.epicgames.com/docs
- Unity Netcode: https://docs-multiplayer.unity3d.com
- Equipe de desenvolvimento

---

**Versão:** 1.1
**Última atualização:** Janeiro 2025
**Fase atual:** 2.1 - Autenticação Device ID (Concluído)
