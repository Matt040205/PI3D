using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;

public class CuttingBladeLogic : MonoBehaviour
{
    private CharacterController controller;
    private Transform modelPivot;
    private float dashDistance;
    private float damage;
    private string eventoDash;
    private CommanderAbilityController abilityController;
    private Ability sourceAbility;
    private bool resetCooldownOnKill;
    private PlayerMovement playerMovement;

    public void StartDash(GameObject quemUsou, CharacterController cont, Transform pivot, float dist, float dmg, string som, CommanderAbilityController abCont, Ability ability, bool resetOnKill)
    {
        controller = cont;
        modelPivot = pivot;
        dashDistance = dist;
        damage = dmg;
        eventoDash = som;
        abilityController = abCont;
        sourceAbility = ability;
        resetCooldownOnKill = resetOnKill;
        playerMovement = quemUsou.GetComponent<PlayerMovement>();

        if (abilityController != null)
        {
            abilityController.SetAbilityUsage(sourceAbility, true);
        }

        StartCoroutine(DashCoroutine(quemUsou));
    }

    private IEnumerator DashCoroutine(GameObject quemUsou)
    {
        if (playerMovement != null)
        {
            playerMovement.isDashing = true;
        }

        if (!string.IsNullOrEmpty(eventoDash))
        {
            RuntimeManager.PlayOneShot(eventoDash, transform.position);
        }

        Vector3 startPosition = transform.position;
        Vector3 dashDirection = modelPivot.forward;

        Vector3 targetPosition = startPosition + (dashDirection * dashDistance);
        int obstacleMask = LayerMask.GetMask("Default", "Ground", "Terrain");

        RaycastHit wallHit;
        if (Physics.Raycast(startPosition + Vector3.up, dashDirection, out wallHit, dashDistance, obstacleMask))
        {
            targetPosition = wallHit.point - (dashDirection * 0.5f);
        }

        controller.enabled = false;

        Vector3 finalPosition = targetPosition;
        RaycastHit groundHit;
        int groundMask = LayerMask.GetMask("Default", "Ground");

        if (Physics.Raycast(targetPosition + Vector3.up * 0.5f, Vector3.down, out groundHit, 5f, groundMask))
        {
            finalPosition = groundHit.point + (Vector3.up * (controller.height / 2f));
        }

        transform.position = finalPosition;

        yield return null;
        controller.enabled = true;

        float dashRadius = 2f;
        float actualDistance = Vector3.Distance(startPosition, finalPosition);

        RaycastHit[] hits = Physics.SphereCastAll(startPosition, dashRadius, dashDirection, actualDistance);

        List<EnemyHealthSystem> enemiesHit = new List<EnemyHealthSystem>();
        bool matouAlguem = false;

        foreach (var hit in hits)
        {
            EnemyHealthSystem vidaInimigo = hit.collider.GetComponent<EnemyHealthSystem>();

            if (vidaInimigo != null && !enemiesHit.Contains(vidaInimigo))
            {
                enemiesHit.Add(vidaInimigo);
                bool inimigoMorreu = vidaInimigo.TakeDamage(damage);

                if (inimigoMorreu)
                {
                    matouAlguem = true;
                }
            }
        }

        if (resetCooldownOnKill && matouAlguem && abilityController != null)
        {
            abilityController.ResetCooldown(sourceAbility);
        }

        if (playerMovement != null)
        {
            playerMovement.isDashing = false;
        }
        Destroy(this);
    }
}