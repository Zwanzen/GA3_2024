using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExplosionScript : MonoBehaviour
{
    [SerializeField]
    private float explosionForce = 10f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Explode();
        }
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, transform.localScale.z);
        foreach (Collider collider in colliders)
        {
            if(collider.attachedRigidbody != null)
            {
                collider.attachedRigidbody.AddExplosionForce(explosionForce, transform.position, transform.localScale.z);
            }   
        }
    }
}
