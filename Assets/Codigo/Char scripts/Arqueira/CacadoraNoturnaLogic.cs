using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static Unity.VisualScripting.Member;

public class CacadoraNoturnaLogic : MonoBehaviour
{
    public ParticleSystem effectParticles;
    public GameObject beamVisualPrefab;
    public float visualDuration = 0.5f;

    // !! MUDANÇA 1 !!
    // Adicionamos um delay para esperar a animação
    [Header("Sincronia da Animação")]
    public float animationDelay = 0.5f; // Ajuste este valor no Inspector!

    private float damage;
    private float range;
    private float width;
    private GameObject caster;
    private LayerMask visualRaycastMask;

    // !! MUDANÇA 2 !!
    // Referência para o Animator
    private Animator anim;

    public void StartUltimateEffect(GameObject caster, float damage, float range, float width)
    {
        this.caster = caster;
        this.damage = damage;
        this.range = range;
        this.width = width;

        LayerMask enemyLayer = LayerMask.GetMask("Enemy");
        LayerMask playerLayer = LayerMask.GetMask("Player");
        visualRaycastMask = ~(enemyLayer | playerLayer);

        // Pega o Animator do seu personagem ("caster")
        if (caster != null)
        {
            this.anim = caster.GetComponentInChildren<Animator>();
        }

        if (effectParticles != null)
        {
            effectParticles.Play();
        }

        // !! MUDANÇA 3 !!
        // Disparamos o "gatilho" da animação.
        if (this.anim != null)
        {
            // (Você ainda precisa criar este Trigger no seu Animator)
            this.anim.SetTrigger("CacadoraUltimate");
        }

        // Em vez de chamar o dano direto, iniciamos a Coroutine com delay
        StartCoroutine(FireBeamAfterDelay());

        // O código original de dano foi removido daqui
        // ApplyBeamDamage();  // <-- REMOVIDO
        // if (beamVisualPrefab != null) // <-- REMOVIDO
        // {
        //     StartCoroutine(ShowBeamVisual()); // <-- REMOVIDO
        // }

        // Aumentei o tempo de destruição para dar tempo de tudo acontecer
        Destroy(gameObject, visualDuration + animationDelay + 1.0f);
    }

    // !! MUDANÇA 4 !!
    // Esta Coroutine espera o delay da animação e DEPOIS executa seu código
    private IEnumerator FireBeamAfterDelay()
    {
        // 1. Espera o tempo do "animationDelay"
        yield return new WaitForSeconds(animationDelay);

        // 2. Agora, executa o SEU código original que funciona
        Debug.Log("Delay terminado. Disparando raio!");
        ApplyBeamDamage();

        if (beamVisualPrefab != null)
        {
            StartCoroutine(ShowBeamVisual());
        }
    }


    private IEnumerator ShowBeamVisual()
    {
        Vector3 startPoint = transform.position;
        Vector3 direction = transform.forward;
        float beamDistance = range;

        RaycastHit groundHit;
        if (Physics.Raycast(startPoint, direction, out groundHit, range, visualRaycastMask))
        {
            beamDistance = groundHit.distance;
        }

        GameObject visual = Instantiate(beamVisualPrefab, startPoint, transform.rotation);
        visual.transform.SetParent(this.transform);

        LineRenderer line = visual.GetComponent<LineRenderer>();
        if (line != null)
        {
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, Vector3.forward * beamDistance);
            line.startWidth = width;
            line.endWidth = width;
        }

        float elapsedTime = 0f;
        Material lineMaterial = line?.material;
        Color originalColor = Color.white;
        if (lineMaterial != null && lineMaterial.HasColor("_Color"))
        {
            originalColor = lineMaterial.color;
        }

        while (elapsedTime < visualDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / visualDuration;

            if (lineMaterial != null)
            {
                originalColor.a = 1f - progress;
                lineMaterial.color = originalColor;
            }
            yield return null;
        }

        Destroy(visual);
    }

    private void ApplyBeamDamage()
    {
        LayerMask enemyLayer = LayerMask.GetMask("Enemy");
        Vector3 startPoint = transform.position;
        Vector3 direction = transform.forward;

        RaycastHit[] inimigosAcertados = Physics.SphereCastAll(startPoint, width, direction, range, enemyLayer);
        Debug.Log($"[HabilidadeCacadoraNoturna] Raio disparado. Acertou {inimigosAcertados.Length} inimigos.");

        foreach (var hit in inimigosAcertados)
        {
            EnemyHealthSystem vidaInimigo = hit.collider.GetComponent<EnemyHealthSystem>();
            if (vidaInimigo != null)
            {
                vidaInimigo.TakeDamage(damage, 0f, false);
            }
        }
    }
}