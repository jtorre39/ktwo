﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggressionPuddleBehaviour : BasePuddleBehaviour
{
    public int duration;

    void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (CannotBeUsed(other.gameObject)) return;

        if (other.gameObject.tag != "Zombie") return;
        affectedEntities.Add(other.gameObject);

        other.GetComponent<EnemyController>()
                .TurnAgainstOwn(duration);
        StartCoroutine(
            RemoveFromHashSet(other.gameObject, duration)
        );
        numberOfUses -= 1;
    }
}
