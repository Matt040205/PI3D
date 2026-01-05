using UnityEngine;

public class MergulhoTintaLogic : MonoBehaviour
{
    private float _damage;
    private float _radius;

    // Armazena o estado original para restaurar depois
    private int _originalLayer;
    private string _originalTag;

    private GameObject _puddleInstance;
    private Renderer[] _renderers;
    private PlayerShooting _shootingScript;
    private CommanderAbilityController _abilityScript;
    private PlayerHealthSystem _playerHealth; // Para computar vampirismo/stats

    // Layer 2 é padrão da Unity "Ignore Raycast". Geralmente inimigos não batem nela.
    private const int GHOST_LAYER = 2;

    public void StartDive(float duration, float damage, float radius, GameObject puddlePrefab)
    {
        _damage = damage;
        _radius = radius;
        _renderers = GetComponentsInChildren<Renderer>();
        _shootingScript = GetComponent<PlayerShooting>();
        _abilityScript = GetComponent<CommanderAbilityController>();
        _playerHealth = GetComponent<PlayerHealthSystem>();

        // 1. Salvar estado original
        _originalLayer = gameObject.layer;
        _originalTag = gameObject.tag;

        // 2. Mudar para Layer Fantasma e Tag Null
        // Isso faz Inimigos que buscam "Player" pararem de te achar
        SetLayerRecursively(gameObject, GHOST_LAYER);
        gameObject.tag = "Untagged";

        // 3. TENTATIVA DE FORÇAR FÍSICA: 
        // Diz para a Unity: "A Layer 2 NÃO DEVE encostar na Layer Inimigo"
        // (Assumindo que seus inimigos estão na layer "Enemy" ou similar)
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
        {
            Physics.IgnoreLayerCollision(GHOST_LAYER, enemyLayer, true);
        }

        // 4. Desativar Controles e Visual
        if (_shootingScript) _shootingScript.enabled = false;
        if (_abilityScript) _abilityScript.enabled = false;
        foreach (var r in _renderers) r.enabled = false;

        // 5. Visual da Poça
        if (puddlePrefab != null)
        {
            Vector3 spawnPos = transform.position;
            spawnPos.y += 0.05f; // Levemente acima do chão
            _puddleInstance = Instantiate(puddlePrefab, spawnPos, Quaternion.identity);
        }

        // 6. Agenda o fim
        Invoke(nameof(EndDive), duration);
    }

    void Update()
    {
        // Poça segue o player
        if (_puddleInstance != null)
        {
            Vector3 targetPos = transform.position;
            targetPos.y += 0.05f;
            _puddleInstance.transform.position = targetPos;
        }
    }

    void EndDive()
    {
        // 1. Restaurar Colisão entre as layers (Importante desfazer!)
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
        {
            Physics.IgnoreLayerCollision(GHOST_LAYER, enemyLayer, false);
        }

        // 2. Restaurar Player
        SetLayerRecursively(gameObject, _originalLayer);
        gameObject.tag = _originalTag;

        if (_shootingScript) _shootingScript.enabled = true;
        if (_abilityScript) _abilityScript.enabled = true;
        foreach (var r in _renderers) r.enabled = true;

        if (_puddleInstance != null) Destroy(_puddleInstance);

        // 3. CAUSAR DANO (AQUI ESTÁ A CORREÇÃO PRINCIPAL)
        CausarDanoEmArea();

        Destroy(this);
    }

    void CausarDanoEmArea()
    {
        // Cria uma esfera invisível para achar inimigos
        Collider[] hits = Physics.OverlapSphere(transform.position, _radius);

        foreach (var hit in hits)
        {
            // Ignora a si mesmo
            if (hit.gameObject == gameObject) continue;

            // Busca o script de vida do inimigo
            EnemyHealthSystem enemyHealth = hit.GetComponent<EnemyHealthSystem>();

            if (enemyHealth != null)
            {
                // Usa os parâmetros exatos que vi no seu script:
                // Dano, Penetração de Armadura (0), Crítico (false)
                enemyHealth.TakeDamage(_damage, 0f, false);

                // Avisa ao player que causou dano (para passivas/vampirismo)
                if (_playerHealth != null)
                {
                    _playerHealth.TriggerDamageDealt(_damage);
                }

                Debug.Log($"SPLASH! Dano causado em {hit.name}");
            }
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}