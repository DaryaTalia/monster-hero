using UnityEngine;

public class URLComponent : MonoBehaviour
{
    [SerializeField]
    string url;

    public void OpenURL()
    {
        Application.OpenURL($"{url}");
    }
}
