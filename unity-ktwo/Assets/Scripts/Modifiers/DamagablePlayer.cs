﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(InvinicibilityFlashModifier))]
public class DamagablePlayer : Damagable
{
    [Range(1, 10)]
    public int knockbackFactor; // There's not really any sense of units here sorry...
    private float calculatedKnockBackFactor;
    public float invincibilityDuration = 2; // 2 by default;
    public Rigidbody rbd;
    public bool isInvincible = false;
    public PlayerBehaviour player;

    [Tooltip("These are populated at runtime")]
    public new UnityEventFloat OnHit; // `new` Overrides original OnHit field.

    InvinicibilityFlashModifier invinicibilityComponent;

    void Awake()
    {
        rbd = GetComponent<Rigidbody>();
        if (OnHit == null)
        {
            OnHit = new UnityEventFloat();
        }
        OnHit.AddListener(GetComponent<PlayerBehaviour>().UpdateHealthBar);
    }

    new void Start()
    {
        invinicibilityComponent = GetComponent<InvinicibilityFlashModifier>();
        base.Start();
    }

    override public void Heal(float healAmount)
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, startingHealth);

        RpcHeal(healAmount);
    }

    [ClientRpc]
    public void RpcHeal(float healAmount)
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, startingHealth);
        OnHit.Invoke(currentHealth / startingHealth);
    }

    public void Hit(float damage, Vector3 direction, bool iFrames)
    {
        if (isInvincible) return;

        currentHealth -= damage;

        if (currentHealth <= 0) GetComponent<PlayerBehaviour>().Die();

        if (iFrames)
        {
            KnockbackPlayer(direction);
            StartCoroutine(BeginInvincibility());
        }

        RpcHit(damage, direction, iFrames);
    }

    [ClientRpc]
    public void RpcHit(float damage, Vector3 direction, bool iFrames)
    {
        currentHealth -= damage;

        if (currentHealth <= 0) GetComponent<PlayerBehaviour>().Die();

        OnHit.Invoke(currentHealth / startingHealth);

        if (iFrames)
        {
            KnockbackPlayer(direction);
            StartCoroutine(BeginInvincibility());
        }
    }

    public IEnumerator DamageOverTime(float damageAmount, float duration)
    {
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            Hit(damageAmount, Vector3.zero, false);
            yield return new WaitForSeconds(1.0f);
            elapsedTime++;
        }
    }

    void KnockbackPlayer(Vector3 direction)
    {
        calculatedKnockBackFactor = knockbackFactor * 10; // Adjusted from 1 - 10 -> 10 - 100
        rbd.AddForce(direction.normalized * calculatedKnockBackFactor, ForceMode.VelocityChange);
    }

    IEnumerator BeginInvincibility()
    {
        isInvincible = true;
        invinicibilityComponent.enabled = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
        invinicibilityComponent.enabled = false;
    }

    [Command]
    void CmdServerRegisterHit(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            RpcPlayerOutOfHealth();
        }
        RpcTriggerClientInvincibility();
    }

    [ClientRpc]
    void RpcPlayerOutOfHealth()
    {
        if (GetComponent<Animator>().GetBool("IsDead")) return;
        GetComponent<PlayerBehaviour>().Die();
    }

    [ClientRpc]
    void RpcTriggerClientInvincibility()
    {
        StartCoroutine(BeginInvincibility());
    }
}
