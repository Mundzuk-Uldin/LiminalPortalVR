using UnityEngine;

public class ObjectWasClick : MonoBehaviour
{
    [SerializeField] private int _index = 0;
    private void OnMouseUpAsButton()
    {
        if (_index >= 0)
            FindObjectOfType<Fade_Colors>().DoTheFade(_index);
    }

    private void OnMouseOver()
    {
       if (_index >= 0)
            FindObjectOfType<Fade_Colors>().DoGray(_index);
       
    }

    private void OnMouseExit()
    {
        if (_index >= 0)
            FindObjectOfType<Fade_Colors>().DoColor(_index);

    }
}
