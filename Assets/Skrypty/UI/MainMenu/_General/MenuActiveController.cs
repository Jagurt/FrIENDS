public class MenuActiveController : UnityEngine.MonoBehaviour
{
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }
}
