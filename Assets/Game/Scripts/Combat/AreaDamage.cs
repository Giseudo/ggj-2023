using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Combat;

public class AreaDamage : MonoBehaviour
{
    [SerializeField]
    private int _damageAmount = 1;

    [SerializeField]
    private float _timeInterval = 2f;

    [SerializeField]
    private float _walkSpeedMultiplier = 1f;

    [SerializeField]
    private float _radius;

    public void OnEnable()
    {
        StartCoroutine(IntervalDamage());
        StartCoroutine(SlowDown());
    }

    public void OnDisable()
    {
        StopCoroutine(IntervalDamage());
        StopCoroutine(SlowDown());
    }

    private IEnumerator IntervalDamage()
    {
        while (gameObject.activeInHierarchy)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _radius, 1 << LayerMask.NameToLayer("Creep"));

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];

                if (collider.TryGetComponent<Damageable>(out Damageable damageable))
                    damageable.Hurt(_damageAmount);
            }

            yield return new WaitForSeconds(_timeInterval);
        }
    }

    private IEnumerator SlowDown()
    {
        while (gameObject.activeInHierarchy)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _radius, 1 << LayerMask.NameToLayer("Creep"));

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];

                if (collider.TryGetComponent<Creep>(out Creep creep))
                    creep.SlowDown(_walkSpeedMultiplier, 2f);
            }

            yield return new WaitForSeconds(.5f);
        }
    }
}
