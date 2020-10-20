public class MenuActiveController : UnityEngine.MonoBehaviour
{
    //TODO : throw this out, its garbage

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }
}
