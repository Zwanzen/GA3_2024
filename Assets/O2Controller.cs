using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O2Controller : MonoBehaviour
{

    [SerializeField] private MeshRenderer[] meshRenderers;
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material fixedMaterial;

    private void Start()
    {
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.material = redMaterial;

        }
    }

    public void FixO2()
    {
        ChangeMaterials();
    }

    private void ChangeMaterials()
    {
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.material = fixedMaterial;
        }
    }

}
