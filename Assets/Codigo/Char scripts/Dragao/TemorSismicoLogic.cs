using UnityEngine;

public class TemorSismicoLogic : MonoBehaviour
{
    public void Setup(GameObject owner, float range, float angle, float damage, float duration, float upForce, CommanderAbilityController controller, Ability ability)
    {
        if (controller != null) controller.SetAbilityUsage(ability, true);

        Collider[] hits = Physics.OverlapSphere(transform.position, range);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector3 dirToEnemy = (hit.transform.position - transform.position).normalized;

                // Verifica se está dentro do ângulo (Triângulo/Cone)
                if (Vector3.Angle(transform.forward, dirToEnemy) < angle / 2)
                {
                    EnemyHealthSystem hp = hit.GetComponent<EnemyHealthSystem>();
                    if (hp != null) hp.TakeDamage(damage);

                    EnemyController ai = hit.GetComponent<EnemyController>();
                    if (ai != null)
                    {
                        // Knock-up: Empurra pra CIMA
                        ai.ApplyKnockback(Vector3.up, upForce);

                        // Suspende o inimigo (Stun aéreo)
                        ai.ApplySlow(1f, duration);
                    }
                }
            }
        }

        // Adicione aqui Screenshake ou Partículas de terra rachando
        Destroy(gameObject, 2f);
    }
}