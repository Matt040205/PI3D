using UnityEngine;
using FMODUnity;

public class MeleeCombatSystem : MonoBehaviour
{
    [Header("Configurações")]
    public CharacterBase characterData;
    public Transform attackPoint;
    public float attackRange = 2f;
    public float attackAngle = 90f;
    public LayerMask hitLayers;

    [Header("Dano (Overrides da Ultimate)")]
    public float damageCombo1 = 13f;
    public float damageCombo2 = 15f;
    public float damageCombo3 = 22f;
    public float damageCombo4 = 30f;

    [Header("FMOD (Combo da Ultimate)")]
    [EventRef]
    public string eventoEspada1 = "event:/SFX/Espada";
    [EventRef]
    public string eventoEspada2 = "event:/SFX/Espada_1";
    [EventRef]
    public string eventoEspada3 = "event:/SFX/Espada_2";
    [EventRef]
    public string eventoEspada4 = "event:/SFX/Espada_3";

    [Header("Overrides da Ultimate (Não mexer)")]
    // Esta variável agora funciona!
    public float? overrideAttackSpeed = null;
    public float? overrideAttackAngle = null;

    private Animator anim;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    // O Update agora tem DUAS responsabilidades
    void Update()
    {
        // 1. Checar o input de ataque
        if (Input.GetButtonDown("Fire1"))
        {
            anim.SetTrigger("Attack");
        }

        // 2. Controlar a velocidade da animação com base no override
        if (overrideAttackSpeed.HasValue)
        {
            // Se o NineTailsDanceLogic definir a velocidade para 5,
            // as animações de ataque tocarão 5x mais rápido.
            anim.speed = overrideAttackSpeed.Value;
        }
        else
        {
            anim.speed = 1.0f; // Velocidade normal
        }
    }

    // Garantir que a velocidade volte ao normal
    void OnDisable()
    {
        // Quando este script for desabilitado (pelo NineTailsDanceLogic),
        // garantimos que a velocidade do animator volte a 1.
        if (anim != null)
        {
            anim.speed = 1.0f;
        }
    }

    private void DetectHits(float damageToApply, string fmodEvent)
    {
        // Tocar o som
        if (!string.IsNullOrEmpty(fmodEvent))
            RuntimeManager.PlayOneShot(fmodEvent, transform.position);

        // Detectar o Dano
        float currentAngle = overrideAttackAngle ?? this.attackAngle;
        Collider[] hitTargets = Physics.OverlapSphere(attackPoint.position, attackRange, hitLayers);

        foreach (Collider target in hitTargets)
        {
            Vector3 directionToTarget = (target.transform.position - attackPoint.position).normalized;
            float angleToTarget = Vector3.Angle(attackPoint.forward, directionToTarget);

            if (angleToTarget < currentAngle / 2)
            {
                EnemyHealthSystem enHealth = target.GetComponent<EnemyHealthSystem>();
                if (enHealth != null)
                {
                    enHealth.TakeDamage(damageToApply);
                    Debug.Log($"<color=cyan>ATAQUE MELEE:</color> Causou {damageToApply} de dano em {target.name}");
                }
            }
        }
    }

    // --- Funções PÚBLICAS para os Eventos de Animação ---
    public void AnimEvent_Hit1()
    {
        DetectHits(damageCombo1, eventoEspada1);
    }

    public void AnimEvent_Hit2()
    {
        DetectHits(damageCombo2, eventoEspada2);
    }

    public void AnimEvent_Hit3()
    {
        DetectHits(damageCombo3, eventoEspada3);
    }

    public void AnimEvent_Hit4()
    {
        DetectHits(damageCombo4, eventoEspada4);
    }


    // Gizmos (Sem omissões)
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        float currentAngle = overrideAttackAngle ?? this.attackAngle;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        Vector3 leftBound = Quaternion.Euler(0, -currentAngle / 2, 0) * attackPoint.forward * attackRange;
        Vector3 rightBound = Quaternion.Euler(0, currentAngle / 2, 0) * attackPoint.forward * attackRange;

        Gizmos.DrawLine(attackPoint.position, attackPoint.position + leftBound);
        Gizmos.DrawLine(attackPoint.position, attackPoint.position + rightBound);
        Gizmos.DrawLine(attackPoint.position + leftBound, attackPoint.position + rightBound);
    }
}