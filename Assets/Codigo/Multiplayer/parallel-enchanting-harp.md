# Plano de Implementacao Multiplayer - ExoBeasts V3

## Especificacoes do Projeto

| Item | Valor |
|------|-------|
| **Engine** | Unity 6 (6000.0.52f1) |
| **Max Jogadores** | 4 jogadores |
| **Modelo de Conexao** | **P2P (Peer-to-Peer) com Host** |
| **Prazo** | 1-2 meses |
| **Escopo** | Sincronizacao completa (lobby, movimento, combate, torres, inimigos, moedas, habilidades) |

---

## Visao Geral da Arquitetura - P2P

```
[Epic Online Services - Lobby Service]
       |
       v
[Client 1 - HOST] <----+
   (Servidor + Cliente) |
       ^                |
       |                |
       +---> [Client 2] (Apenas Cliente)
       +---> [Client 3] (Apenas Cliente)
       +---> [Client 4] (Apenas Cliente)
```

**Modelo P2P (Peer-to-Peer):**
- Um jogador atua como **Host** (servidor + cliente simultaneamente)
- O Host processa toda a lógica autoritativa do jogo
- Outros jogadores conectam diretamente ao Host
- Epic Lobby Service gerencia matchmaking e conexões
- NAT Traversal facilitado pelo Epic P2P Service

**Vantagens do P2P:**
- ✅ Sem custo de servidor dedicado
- ✅ Setup mais simples
- ✅ Melhor para grupos pequenos (2-4 jogadores)
- ✅ Latência menor para jogadores próximos ao Host

**Considerações:**
- ⚠️ Host precisa de conexão estável
- ⚠️ Se Host desconectar, partida termina (Host Migration pode ser implementado depois)

---

## FASE 1: Fundacao e Configuracao

### 1.1 Configuracao no Epic Developer Portal

**URL:** https://dev.epicgames.com/portal

**Passos:**
1. Criar conta de desenvolvedor Epic
2. Criar nova Organizacao
3. Criar novo Produto "ExoBeasts"
4. Na aba **Product Settings**:
   - Anotar: Product ID
   - Anotar: Sandbox ID (usar Development para testes)
5. Na aba **Clients**:
   - Criar novo Client
   - Tipo: GameClient
   - Anotar: Client ID e Client Secret
6. Na aba **Deployments**:
   - Criar Deployment para Development sandbox
   - Anotar: Deployment ID
7. Na aba **Game Services**:
   - Ativar: **Lobbies** (OBRIGATORIO)
   - Ativar: **Peer-to-peer** (OBRIGATORIO para P2P - NAT traversal)
   - ~~Game Server Hosting (EGSH)~~ - NÃO necessário para P2P
8. Na aba **Brand Settings**:
   - Configurar nome e logo (obrigatorio para auth)

### 1.2 Instalacao de Pacotes Unity

**Modificar:** `Packages/manifest.json`

```json
{
  "dependencies": {
    "com.unity.netcode.gameobjects": "2.2.1",
    "com.unity.transport": "2.4.0",
    "com.unity.multiplayer.tools": "2.2.1",
    // ... demais pacotes existentes
  }
}
```

**Epic Online Services Plugin:**
- Opcao recomendada: PlayEveryWare EOS Plugin
- GitHub: https://github.com/PlayEveryWare/eos_plugin_for_unity
- Importar via .unitypackage ou git URL

### 1.3 Estrutura de Pastas

```
Assets/Codigo/Multiplayer/
  Core/
    NetworkBootstrap.cs           # Inicializacao geral
    EOSManager.cs                 # Wrapper do SDK Epic
    EOSConfig.cs                  # ScriptableObject para configuracoes (carrega de arquivo externo)
  Auth/
    EOSAuthenticator.cs           # Login/logout
    SessionManager.cs             # Gerencia sessao do usuario
  Lobby/
    LobbyManager.cs               # CRUD de lobbies
    LobbyUI.cs                    # Interface de lobbies
    LobbyData.cs                  # Structs de dados
  GameServer/
    GameServerManager.cs          # Logica do Host (para P2P)
    MatchManager.cs               # Gerencia partida
    PlayerRegistry.cs             # Registro de jogadores conectados
  Sync/
    NetworkedPlayerController.cs  # Controller de rede do jogador
    NetworkedCurrency.cs          # Moedas sincronizadas
    NetworkedBuilding.cs          # Torres sincronizadas
    NetworkedHorde.cs             # Waves sincronizadas
```

### 1.4 Cenas do Projeto

**Cenas existentes (manter):**
- MenuScene.unity - Menu principal
- EscolherPersonagem.unity - Selecao de personagem
- CenaMapaTeste.unity - Gameplay

**Cenas novas (criar):**
- NetworkBootstrap.unity - Inicializacao de rede
- LobbyScene.unity - Sistema de lobbies
- ~~DedicatedServer.unity~~ - NÃO necessário para P2P

**Fluxo de cenas P2P:**
```
[NetworkBootstrap] -> [MenuScene] -> [LobbyScene] -> [EscolherPersonagem] -> [CenaMapaTeste]
                          |              |                                         |
                          |              v                                         v
                          |         (Criar/Entrar Lobby)                    (Host ou Client)
                          v
                    (Login EOS)
```

**Diferença para P2P:**
- Host executa NetworkManager.StartHost() ao iniciar partida
- Clientes executam NetworkManager.StartClient() após receber dados de conexão
- Mesma cena é usada por Host e Clientes

---

## FASE 2: Autenticacao

### 2.1 EOSManager (Singleton Principal)

**Arquivo:** `Assets/Codigo/Multiplayer/Core/EOSManager.cs`

Responsabilidades:
- Inicializar SDK da Epic no Awake
- Manter referencia ao PlatformInterface
- Chamar Tick() no Update (obrigatorio)
- Shutdown no OnApplicationQuit

### 2.2 EOSAuthenticator

**Arquivo:** `Assets/Codigo/Multiplayer/Auth/EOSAuthenticator.cs`

**Metodos de login:**
1. `LoginWithDevAuthTool(string credentialName)` - Para desenvolvimento
2. `LoginWithDeviceId()` - Login anonimo
3. `LoginWithEpicAccount()` - Para producao

**Fluxo de autenticacao:**
```
1. EOSAuthenticator.LoginWithDevAuthTool()
2. Epic SDK -> Connect Interface -> Login
3. Recebe ProductUserId (PUID)
4. Armazena PUID localmente
5. Dispara evento OnLoginSuccess
6. UI navega para LobbyScene
```

### 2.3 DevAuthTool Setup

**Ferramenta:** `SDK/Tools/EOS_DevAuthTool.exe`

**Configuracao:**
1. Executar EOS_DevAuthTool.exe
2. Definir porta (default: 7878)
3. Criar credencial no portal Epic
4. Logar na ferramenta
5. Usar nome da credencial no jogo

### 2.4 UI de Login

**Modificar:** MenuScene.unity

Elementos:
- Panel: LoginPanel
  - InputField: CredentialName (texto)
  - Button: LoginDevAuth
  - Button: LoginAnonymous
  - Text: StatusMessage
  - Spinner: LoadingIndicator

---

## FASE 3: Sistema de Lobby

### 3.1 Estrutura de Dados

**Arquivo:** `Assets/Codigo/Multiplayer/Lobby/LobbyData.cs`

```csharp
[System.Serializable]
public class LobbyInfo
{
    public string lobbyId;
    public string lobbyName;
    public string hostDisplayName;
    public int currentPlayers;
    public int maxPlayers;        // 4
    public string mapName;
    public bool isPublic;
}

[System.Serializable]
public class LobbyMember
{
    public string productUserId;
    public string displayName;
    public int selectedCharacterIndex;  // Indice do personagem escolhido
    public bool isReady;
}

public enum LobbyState
{
    WaitingForPlayers,
    SelectingCharacters,
    StartingMatch,
    InGame
}
```

### 3.2 LobbyManager

**Arquivo:** `Assets/Codigo/Multiplayer/Lobby/LobbyManager.cs`

**Funcionalidades:**
- `CreateLobby(LobbySettings settings)` - Criar sala
- `SearchLobbies(SearchFilter filter)` - Buscar salas
- `JoinLobby(string lobbyId)` - Entrar em sala
- `LeaveLobby()` - Sair da sala
- `SetMemberAttribute(string key, string value)` - Ex: personagem escolhido
- `SetReady(bool ready)` - Marcar como pronto
- `StartMatch()` - Iniciar partida (solicita servidor)

**Atributos do Lobby (Epic):**
```
"LOBBY_NAME" = "Sala do Joao"
"MAP_NAME" = "CenaMapaTeste"
"MAX_PLAYERS" = "4"
"IS_PUBLIC" = "true"
"LOBBY_STATE" = "WaitingForPlayers"
```

### 3.3 Fluxo de Matchmaking P2P

```
1. Host cria lobby via Epic Lobby Service
2. Outros jogadores encontram e entram no lobby
3. Todos selecionam personagens
4. Todos marcam "Pronto"
5. Host clica "Iniciar Partida"
6. Host inicia NetworkManager.StartHost()
   - Host se torna servidor E cliente simultaneamente
   - Unity Transport usa Epic P2P para NAT traversal
7. LobbyManager envia dados de conexão P2P para membros do lobby
8. Clientes recebem dados e conectam ao Host via Epic P2P
9. Clientes iniciam NetworkManager.StartClient()
10. Host carrega cena de jogo
11. Clientes carregam cena automaticamente (Network Scene Management)
12. Partida começa com Host como autoridade
```

**Diferenças importantes do P2P:**
- ✅ Sem necessidade de servidor dedicado/EGSH
- ✅ Epic P2P Service facilita conexão (NAT Traversal automático)
- ✅ Configuração mais simples
- ⚠️ Host deve ter conexão estável (é o servidor)
- ⚠️ Host desconectar = fim da partida (sem Host Migration)

### 3.4 UI de Lobby

**Nova Cena:** LobbyScene.unity

**Panel: CreateLobbyPanel**
- InputField: LobbyName
- Slider: MaxPlayers (2-4)
- Toggle: IsPublic
- Button: CreateLobby

**Panel: LobbyListPanel**
- ScrollView: LobbyList
  - Prefab: LobbyListItem (Nome, Host, 2/4, Button Join)
- Button: RefreshList
- Button: CreateNew

**Panel: LobbyRoomPanel**
- Text: LobbyName
- List: PlayerSlots (4 slots)
  - Cada slot: Avatar, Nome, Personagem, Ready Status
- Button: SelectCharacter
- Toggle: Ready
- Button: StartGame (apenas host, apenas quando todos prontos)
- Button: LeaveLobby

---

## FASE 4: Configuração P2P e Host

### 4.1 Configuração do Host (P2P)

**No P2P, não há build separado:**
- Host e Clientes usam o mesmo build/executável
- Diferença é apenas o modo de inicialização:
  - **Host:** `NetworkManager.StartHost()` (servidor + cliente)
  - **Client:** `NetworkManager.StartClient()` (apenas cliente)

**Detecção de Host:**
```csharp
public bool IsHost()
{
    return NetworkManager.Singleton != null &&
           NetworkManager.Singleton.IsHost;
}

public bool IsClient()
{
    return NetworkManager.Singleton != null &&
           NetworkManager.Singleton.IsClient;
}
```

### 4.2 GameServerManager (Adaptado para P2P)

**Arquivo:** `Assets/Codigo/Multiplayer/GameServer/GameServerManager.cs`

**Responsabilidades (apenas no Host):**
- Iniciar NetworkManager.StartHost() quando criar lobby
- Aguardar conexões de clientes via Epic P2P
- Validar jogadores conectados
- Gerenciar estado da partida
- Processar toda lógica de jogo (dano, spawn, etc)
- Enviar atualizações para clientes

**Diferenças do Dedicated Server:**
- ✅ Host também é um jogador (tem personagem)
- ✅ Host processa input local E input remoto
- ✅ Sem necessidade de build headless

### 4.3 Epic P2P Service (Substituindo EGSH)

**Configuração no Portal:**
1. Game Services > Peer-to-peer
2. Ativar serviço
3. Configurar NAT traversal (automático)

**No Código:**
```csharp
// Host cria lobby e inicia como Host
public void StartMatchAsHost()
{
    // Configurar Unity Transport com Epic P2P
    var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    // TODO: Configurar transport para usar Epic P2P

    // Iniciar como Host
    NetworkManager.Singleton.StartHost();

    Debug.Log("Host iniciado - aguardando conexões P2P");
}

// Clientes conectam ao Host via lobby
public void JoinMatchAsClient(string lobbyId)
{
    // Obter dados de conexão do lobby
    var connectionData = GetP2PConnectionDataFromLobby(lobbyId);

    // Configurar transport
    var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    // TODO: Configurar com dados do Host

    // Conectar ao Host
    NetworkManager.Singleton.StartClient();

    Debug.Log("Conectando ao Host via Epic P2P");
}
```

**Epic P2P facilita:**
- ✅ NAT Traversal automático (resolve problemas de firewall/router)
- ✅ Relay servers da Epic (fallback se P2P direto falhar)
- ✅ Conexões seguras e criptografadas

---

## FASE 5: Sincronizacao de Gameplay

### 5.1 Player Prefab Networked

**Modificar:** `Assets/Modelos/PreFab/Entidades/Player 1.prefab`

**Componentes a adicionar:**
1. `NetworkObject`
2. `NetworkTransform`
   - Sync Position X/Y/Z: true
   - Sync Rotation Y: true (apenas rotacao horizontal)
   - Interpolate: true
3. `NetworkAnimator`
   - Animator: referencia ao Animator do modelo
4. `NetworkedPlayerController` (novo script)

### 5.2 NetworkedPlayerController

**Arquivo:** `Assets/Codigo/Multiplayer/Sync/NetworkedPlayerController.cs`

```csharp
using Unity.Netcode;

public class NetworkedPlayerController : NetworkBehaviour
{
    // Referencias aos sistemas locais
    private PlayerMovement movement;
    private PlayerHealthSystem health;
    private PlayerShooting shooting;
    private PlayerCombatManager combat;
    private CommanderAbilityController abilities;

    // Dados sincronizados
    public NetworkVariable<float> NetworkHealth = new(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<int> NetworkAmmo = new(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<int> CharacterIndex = new(writePerm: NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        // Buscar componentes
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<PlayerHealthSystem>();
        shooting = GetComponent<PlayerShooting>();
        combat = GetComponent<PlayerCombatManager>();
        abilities = GetComponent<CommanderAbilityController>();

        // Desabilitar input para jogadores que nao sao donos
        if (!IsOwner)
        {
            movement.enabled = false;  // Outros jogadores nao controlam este
        }

        // Sincronizar vida inicial
        if (IsServer)
        {
            NetworkHealth.Value = health.currentHealth;
        }

        // Escutar mudancas
        NetworkHealth.OnValueChanged += OnHealthChanged;
    }

    private void OnHealthChanged(float oldValue, float newValue)
    {
        health.currentHealth = newValue;
        health.OnHealthChanged?.Invoke();
    }
}
```

### 5.3 Modificacoes em PlayerMovement.cs

**Arquivo:** `Assets/Codigo/Char scripts/Player/PlayerMovement.cs`

**Mudancas:**
```csharp
// Adicionar no topo:
using Unity.Netcode;

// Mudar heranca:
public class PlayerMovement : NetworkBehaviour  // Era: MonoBehaviour

// No Update(), adicionar verificacao:
private void Update()
{
    // CRITICO: Apenas o dono processa input
    if (!IsOwner) return;

    // ... resto do codigo existente
}
```

### 5.4 Modificacoes em PlayerHealthSystem.cs

**Arquivo:** `Assets/Codigo/Char scripts/Player/PlayerHealthSystem.cs`

**Mudancas:**
```csharp
using Unity.Netcode;

public class PlayerHealthSystem : NetworkBehaviour
{
    // Vida sincronizada (apenas servidor escreve)
    public NetworkVariable<float> networkHealth = new(
        writePerm: NetworkVariableWritePermission.Server
    );

    // Propriedade para manter compatibilidade
    public float currentHealth
    {
        get => networkHealth.Value;
        set
        {
            if (IsServer) networkHealth.Value = value;
        }
    }

    // Dano - chamar via ServerRpc
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage, ulong attackerId)
    {
        // Servidor calcula dano
        float finalDamage = damage * (1f - damageResistance);
        networkHealth.Value -= finalDamage;

        if (networkHealth.Value <= 0)
        {
            DieServerRpc();
        }
    }

    [ServerRpc]
    private void DieServerRpc()
    {
        // Servidor processa morte
        networkHealth.Value = characterData.maxHealth;
        RespawnClientRpc();
    }

    [ClientRpc]
    private void RespawnClientRpc()
    {
        // Todos os clientes veem o respawn
        // Teletransportar para spawn point
    }
}
```

### 5.5 Sincronizacao de Combate

**PlayerShooting.cs:**
```csharp
// Quando atirar (apenas owner):
if (IsOwner && Input.GetButton("Fire1"))
{
    ShootServerRpc(targetPosition, damage);
}

[ServerRpc]
private void ShootServerRpc(Vector3 target, float damage)
{
    // Servidor processa o tiro
    // Servidor spawna projetil (NetworkObject)
    // Servidor aplica dano se acertar
}
```

**MeleeCombatSystem.cs:**
```csharp
// Quando atacar (apenas owner):
if (IsOwner)
{
    MeleeAttackServerRpc(damage, targets);
}

[ServerRpc]
private void MeleeAttackServerRpc(float damage, ulong[] targetNetworkIds)
{
    // Servidor valida e aplica dano
}
```

### 5.6 Sincronizacao de Moedas (CurrencyManager)

**Arquivo:** `Assets/Codigo/Managers/CurrencyManager.cs`

```csharp
using Unity.Netcode;

public class CurrencyManager : NetworkBehaviour
{
    public static CurrencyManager Instance;

    // Moedas por jogador (Dictionary nao funciona direto, usar alternativa)
    // Ou moedas compartilhadas pelo time:
    public NetworkVariable<int> TeamGeodites = new(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<int> TeamDarkEther = new(writePerm: NetworkVariableWritePermission.Server);

    [ServerRpc(RequireOwnership = false)]
    public void AddGeoditeServerRpc(int amount)
    {
        TeamGeodites.Value += amount;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpendGeoditeServerRpc(int amount, ServerRpcParams rpcParams = default)
    {
        if (TeamGeodites.Value >= amount)
        {
            TeamGeodites.Value -= amount;
            // Aprovar compra
            ApproveSpendClientRpc(rpcParams.Receive.SenderClientId);
        }
        else
        {
            // Rejeitar compra
            RejectSpendClientRpc(rpcParams.Receive.SenderClientId);
        }
    }
}
```

### 5.7 Sincronizacao de Torres (BuildManager)

**Arquivo:** `Assets/Codigo/Tower scripts/BuildManager.cs`

```csharp
using Unity.Netcode;

public class BuildManager : NetworkBehaviour
{
    [ServerRpc(RequireOwnership = false)]
    public void PlaceTowerServerRpc(int towerIndex, Vector3 position, Quaternion rotation)
    {
        // Servidor valida posicao
        // Servidor verifica moedas
        // Servidor spawna torre (NetworkObject)
        GameObject tower = Instantiate(towerPrefabs[towerIndex], position, rotation);
        tower.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlaceTrapServerRpc(int trapIndex, Vector3 position, Quaternion rotation)
    {
        // Similar ao tower
    }
}
```

### 5.8 Sincronizacao de Waves (HordeManager)

**Arquivo:** `Assets/Codigo/Managers/HordeManager.cs`

```csharp
using Unity.Netcode;

public class HordeManager : NetworkBehaviour
{
    public NetworkVariable<int> CurrentWave = new();
    public NetworkVariable<int> EnemiesRemaining = new();

    // Apenas servidor controla spawn de inimigos
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(SpawnWaves());
        }
    }

    private IEnumerator SpawnWaves()
    {
        // Servidor spawna inimigos
        // Inimigos sao NetworkObjects
        // Clientes recebem automaticamente
    }

    private void SpawnEnemy(Vector3 position)
    {
        if (!IsServer) return;

        GameObject enemy = EnemyPoolManager.Instance.GetPooledEnemy();
        enemy.transform.position = position;
        enemy.GetComponent<NetworkObject>().Spawn();
    }
}
```

### 5.9 Sincronizacao de Habilidades

**Arquivo:** `Assets/Codigo/Char scripts/JP/CommanderAbilityController.cs`

```csharp
using Unity.Netcode;

public class CommanderAbilityController : NetworkBehaviour
{
    // Cooldowns sao locais (cada cliente rastreia os seus)
    // Efeitos das habilidades sao processados no servidor

    public void UseAbility1()
    {
        if (!IsOwner) return;
        if (IsOnCooldown(ability1)) return;

        UseAbilityServerRpc(1);
        StartCooldown(ability1);
    }

    [ServerRpc]
    private void UseAbilityServerRpc(int abilityIndex)
    {
        // Servidor processa efeito da habilidade
        // Ex: dano em area, buff, spawn de objeto, etc

        // Notifica todos os clientes para visual/audio
        PlayAbilityEffectClientRpc(abilityIndex);
    }

    [ClientRpc]
    private void PlayAbilityEffectClientRpc(int abilityIndex)
    {
        // Todos os clientes reproduzem efeito visual/sonoro
    }
}
```

---

## FASE 6: Polimento e Testes

### 6.1 ParrelSync para Testes Locais

**Instalacao:**
```
Window > Package Manager > + > Add package from git URL
https://github.com/VeriorPies/ParrelSync.git?path=/ParrelSync
```

**Uso:**
1. ParrelSync > Clones Manager
2. Create New Clone
3. Abrir o clone em outra instancia do Unity
4. Original: Rodar como servidor
5. Clone: Rodar como cliente

### 6.2 Tratamento de Desconexao

```csharp
// No NetworkManager
NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
NetworkManager.Singleton.OnServerStopped += OnServerStopped;

private void OnClientDisconnect(ulong clientId)
{
    if (clientId == NetworkManager.Singleton.LocalClientId)
    {
        // Eu fui desconectado
        ShowMessage("Desconectado do servidor");
        ReturnToMenu();
    }
}

private void OnServerStopped(bool wasHost)
{
    // Servidor parou
    ShowMessage("Servidor encerrado");
    ReturnToMenu();
}
```

### 6.3 Interpolacao e Suavizacao

**NetworkTransform Config:**
- Position Threshold: 0.001
- Rotation Threshold: 0.01
- Scale Threshold: 0.01
- Interpolate: true
- Use Quaternion Sync: true
- Use Unreliable Deltas: true (melhor para movimento)

### 6.4 Lag Compensation

Para combate responsivo:
```csharp
// Cliente envia timestamp com acoes
[ServerRpc]
void ShootServerRpc(Vector3 direction, float clientTimestamp)
{
    // Servidor usa timestamp para compensar latencia
    float latency = NetworkManager.Singleton.LocalTime.Time - clientTimestamp;
    // Rollback de posicoes de inimigos para o momento do tiro
}
```

### 6.5 Checklist de Verificacao

**Fase 1 - Setup:**
- [ ] Conta Epic Developer criada
- [ ] Produto configurado no portal
- [ ] Credenciais anotadas
- [ ] NGO instalado no Unity
- [ ] EOS Plugin instalado
- [ ] Projeto compila sem erros

**Fase 2 - Auth:**
- [ ] DevAuthTool funcionando
- [ ] Login no jogo funciona
- [ ] PUID exibido no console
- [ ] UI de login completa

**Fase 3 - Lobby:**
- [ ] Criar lobby funciona
- [ ] Buscar lobbies funciona
- [ ] Entrar em lobby funciona
- [ ] Sair de lobby funciona
- [ ] Lista de membros atualiza
- [ ] Status "pronto" funciona
- [ ] UI de lobby completa

**Fase 4 - Dedicated Server:**
- [ ] Build de servidor compila
- [ ] Servidor inicia sem erros
- [ ] Cliente conecta ao servidor
- [ ] EGSH configurado (se usando)
- [ ] Alocacao de servidor funciona

**Fase 5 - Gameplay:**
- [ ] Spawn de jogadores funciona
- [ ] Movimento sincronizado
- [ ] Combate ranged sincronizado
- [ ] Combate melee sincronizado
- [ ] Vida sincronizada
- [ ] Moedas sincronizadas
- [ ] Torres sincronizadas
- [ ] Traps sincronizadas
- [ ] Inimigos sincronizados
- [ ] Waves sincronizadas
- [ ] Habilidades sincronizadas
- [ ] Ultimate sincronizado

**Fase 6 - Polimento:**
- [ ] Desconexao tratada
- [ ] Movimento suave
- [ ] Sem erros no console
- [ ] Funciona em 2+ maquinas
- [ ] Performance aceitavel

---

## Cronograma Sugerido (8 semanas)

### Semana 1-2: Fundacao
- Criar conta Epic
- Configurar portal
- Instalar pacotes
- Criar estrutura de pastas
- Testar inicializacao do SDK

### Semana 3: Autenticacao
- Implementar EOSManager
- Implementar EOSAuthenticator
- Criar UI de login
- Testar DevAuthTool

### Semana 4: Lobby
- Implementar LobbyManager
- Criar/buscar/entrar lobbies
- Criar UI de lobby
- Testar fluxo completo

### Semana 5: Dedicated Server
- Configurar build de servidor
- Implementar GameServerManager
- Configurar EGSH
- Testar servidor local

### Semana 6: Gameplay Basico
- Networkar Player Prefab
- Modificar PlayerMovement
- Modificar PlayerHealthSystem
- Testar movimento e vida

### Semana 7: Gameplay Completo
- Sincronizar combate
- Sincronizar moedas
- Sincronizar torres
- Sincronizar inimigos
- Sincronizar habilidades

### Semana 8: Polimento
- Tratamento de erros
- Otimizacao de rede
- Testes em multiplas maquinas
- Preparar para apresentacao

---

## Arquivos a Serem Criados

```
Assets/Codigo/Multiplayer/
  Core/
    NetworkBootstrap.cs
    EOSManager.cs
    DedicatedServerManager.cs
  Auth/
    EOSAuthenticator.cs
    SessionManager.cs
  Lobby/
    LobbyManager.cs
    LobbyUI.cs
    LobbyData.cs
    LobbyItemUI.cs
  GameServer/
    GameServerManager.cs
    MatchManager.cs
    PlayerRegistry.cs
  Sync/
    NetworkedPlayerController.cs
    NetworkedCurrency.cs
    NetworkedBuilding.cs
    NetworkedHorde.cs

Assets/Scenes/
  NetworkBootstrap.unity
  LobbyScene.unity
  DedicatedServer.unity
```

## Arquivos a Serem Modificados

```
Packages/manifest.json                              # Adicionar NGO, Transport

Assets/Scenes/MenuScene.unity                       # UI de login

Assets/Codigo/Char scripts/Player/
  PlayerMovement.cs                                 # NetworkBehaviour, IsOwner
  PlayerHealthSystem.cs                             # NetworkVariable, ServerRpc
  PlayerShooting.cs                                 # ServerRpc para dano
  MeleeCombatSystem.cs                              # ServerRpc para dano

Assets/Codigo/Char scripts/JP/
  CommanderAbilityController.cs                     # ServerRpc para habilidades

Assets/Codigo/Managers/
  CurrencyManager.cs                                # NetworkVariable
  HordeManager.cs                                   # Servidor controla spawns
  PauseControl.cs                                   # Pausar nao funciona em rede (remover ou adaptar)

Assets/Codigo/Tower scripts/
  BuildManager.cs                                   # ServerRpc para construir

Assets/Modelos/PreFab/Entidades/
  Player 1.prefab                                   # NetworkObject, NetworkTransform, etc.
```

---

## Riscos e Mitigacoes

| Risco | Probabilidade | Impacto | Mitigacao |
|-------|---------------|---------|-----------|
| EGSH complexo demais | Media | Alto | Comecar com servidor local, migrar depois |
| Bugs de sincronizacao | Alta | Medio | Testar incrementalmente, usar logs |
| Latencia alta | Media | Medio | Interpolacao, client-side prediction |
| EOS SDK complicado | Media | Alto | Usar PlayEveryWare wrapper |
| Prazo apertado | Baixa | Alto | Priorizar funcionalidades core |

---

## Proximos Passos Imediatos

1. **Criar conta no Epic Developer Portal** (tarefa externa)
2. **Instalar Netcode for GameObjects** via Package Manager
3. **Baixar e importar PlayEveryWare EOS Plugin**
4. **Criar cena NetworkBootstrap** com inicializacao basica
5. **Testar se SDK Epic inicializa** (verificar console)
