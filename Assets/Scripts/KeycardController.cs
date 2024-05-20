using UnityEngine;

public class KeycardController : MonoBehaviour
{
    public bool HasRedKeycard = false;
    public bool HasGreenKeycard = false;
    public bool HasBlueKeycard = false;

    public void SetBlueKeycard()
    {
        HasBlueKeycard = true;
    }
    public void SetGreenKeycard()
    {
        HasGreenKeycard = true;
    }
    public void SetRedKeycard()
    {
        HasRedKeycard = true;
    }
}
