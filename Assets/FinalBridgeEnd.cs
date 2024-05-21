using System.Collections;
using UnityEngine;

public class FinalBridgeEnd : MonoBehaviour
{
    [SerializeField] Color fixedColor;
    [SerializeField] MeshRenderer[] bridgeMeshes;
    [SerializeField] Light[] bridgeLights;
    [SerializeField] Material fixedMaterial;

    public void FixBridge()
    {
        StartCoroutine(FIXE());
    }

    private IEnumerator FIXE()
    {
        yield return new WaitForSeconds(10f);
        foreach (MeshRenderer mesh in bridgeMeshes)
        {
            mesh.material = fixedMaterial;
        }
        foreach (Light light in bridgeLights)
        {
            // Change light filter to green
            light.color = fixedColor;
        }
    }

}
