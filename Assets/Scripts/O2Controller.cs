using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class O2Controller : MonoBehaviour
{
    [SerializeField] private UnityEvent endEvent;

    [SerializeField] private MeshRenderer[] meshRenderers;
    private Material notFixedMaterial;
    [SerializeField] private Material fixedMaterial;

    private bool isFixed = false;
    private int blinkCount = 0;

    private void Start()
    {
        notFixedMaterial = meshRenderers[0].material;
    }

    public void FixO2()
    {
        isFixed = true;
    }

    private void Update()
    {
        if (isFixed)
        {
            // Make the material blink before changing it
            StartCoroutine(BlinkMaterial());
            isFixed = false;
        }
    }

    private IEnumerator BlinkMaterial()
    {
        while (blinkCount < 3)
        {
            ChangeMaterials(fixedMaterial);
            yield return new WaitForSeconds(0.25f);
            ChangeMaterials(notFixedMaterial);
            yield return new WaitForSeconds(0.25f);
            blinkCount++;
        }
        ChangeMaterials(fixedMaterial);
        endEvent.Invoke();
    }

    private void ChangeMaterials(Material m)
    {
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.material = m;
        }
    }

}
