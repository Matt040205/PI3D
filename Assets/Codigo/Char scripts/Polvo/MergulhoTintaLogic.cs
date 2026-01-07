using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class MergulhoTintaLogic : MonoBehaviour
{
    private float _damage;
    private float _radius;

    private int _originalLayer;
    private string _originalTag;
    private Collider _myCollider;

    private GameObject _puddleInstance;
    private Renderer[] _renderers;
    private PlayerShooting _shootingScript;
    private CommanderAbilityController _abilityScript;
    private PlayerHealthSystem _playerHealth;

    // Layer 2 é "Ignore Raycast". Geralmente colide com chão, mas vamos configurar para ignorar inimigos.
    private const int GHOST_LAYER = 2;
    private const string POCA_TAG = "Poca";

    public void StartDive(float duration, float damage, float radius, GameObject puddlePrefab)
    {
        _damage = damage;
        _radius = radius;
        _renderers = GetComponentsInChildren<Renderer>();
        _shootingScript = GetComponent<PlayerShooting>();
        _abilityScript = GetComponent<CommanderAbilityController>();
        _playerHealth = GetComponent<PlayerHealthSystem>();

        _myCollider = GetComponent<Collider>();
        if (_myCollider == null) _myCollider = GetComponent<CharacterController>();

        // 1. Salvar estado original
        _originalLayer = gameObject.layer;
        _originalTag = gameObject.tag;

        // 2. Mudar Layer e Tag
        SetLayerRecursively(gameObject, GHOST_LAYER);
        gameObject.tag = POCA_TAG;

        // 3. Ignorar colisão física entre a Layer Fantasma e Inimigos
        // Isso garante que você não empurre eles enquanto é poça
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) Physics.IgnoreLayerCollision(GHOST_LAYER, enemyLayer, true);

        // 4. Desativar Controles
        if (_shootingScript) _shootingScript.enabled = false;
        if (_abilityScript) _abilityScript.enabled = false;
        foreach (var r in _renderers) r.enabled = false;

        // 5. Visual da Poça
        if (puddlePrefab != null)
        {
            Vector3 spawnPos = transform.position;
            spawnPos.y += 0.05f;
            _puddleInstance = Instantiate(puddlePrefab, spawnPos, Quaternion.Euler(90f, 0f, 0f));
        }

        // 6. Stealth
        ConfundirInimigos(radius * 3f);

        Invoke(nameof(EndDive), duration);
    }

    void ConfundirInimigos(float areaDeEfeito)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, areaDeEfeito);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                NavMeshAgent agent = hit.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                }
                hit.SendMessage("LoseTarget", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void Update()
    {
        if (_puddleInstance != null)
        {
            Vector3 targetPos = transform.position;
            targetPos.y += 0.05f;
            _puddleInstance.transform.position = targetPos;
        }
    }

    void EndDive()
    {
        // 1. Restaurar Identidade (TAG) - Inimigos voltam a te ver/atacar
        gameObject.tag = _originalTag;

        // 2. Restaurar Visuais e Scripts - Você volta a aparecer e atirar
        if (_shootingScript) _shootingScript.enabled = true;
        if (_abilityScript) _abilityScript.enabled = true;
        foreach (var r in _renderers) r.enabled = true;
        if (_puddleInstance != null) Destroy(_puddleInstance);

        // 3. Dano na saída
        CausarDanoEmArea();

        // 4. O TRUQUE DO INTANGÍVEL:
        // NÃO restauramos a Layer ainda. Você continua na Layer 2 (Ghost).
        // Como definimos que Ghost ignora Enemy, você pode andar "dentro" deles sem voar.
        StartCoroutine(SairDoModoIntangivel());
    }

    IEnumerator SairDoModoIntangivel()
    {
        // Opcional: Ainda damos um empurrãozinho leve só pra não ficarem colados visualmente
        EmpurrarInimigosProximos();

        // Fica intangível por 1.5 segundos (pode ajustar esse tempo)
        // Durante esse tempo, a física ignora colisão Player vs Enemy
        yield return new WaitForSeconds(1.5f);

        // 5. AGORA SIM, restaura a Layer física
        SetLayerRecursively(gameObject, _originalLayer);

        // Para de ignorar a colisão global entre as layers (limpeza)
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) Physics.IgnoreLayerCollision(GHOST_LAYER, enemyLayer, false);

        // Fim da habilidade
        Destroy(this);
    }

    void EmpurrarInimigosProximos()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 1.5f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb)
                {
                    Vector3 dir = (hit.transform.position - transform.position).normalized;
                    dir.y = 0;
                    if (dir == Vector3.zero) dir = transform.forward;
                    // Empurrão mais fraco, só para sugerir afastamento
                    rb.AddForce(dir * 3f, ForceMode.VelocityChange);
                }
            }
        }
    }

    void CausarDanoEmArea()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _radius);
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            EnemyHealthSystem enemyHealth = hit.GetComponent<EnemyHealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(_damage, 0f, false);
                if (_playerHealth != null) _playerHealth.TriggerDamageDealt(_damage);
            }
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, newLayer);
    }
}