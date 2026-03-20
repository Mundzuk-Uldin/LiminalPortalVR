using System.Collections;
using UnityEngine;

public class Fade_Colors : MonoBehaviour
{
    public Material[] material;
    [Range(0f, 1f)]
    public float grayScale = 1.0f;
    [SerializeField] private bool Invertcolor = false;
    [SerializeField] bool fadeOn = false;

    public void DoTheFade(int index) 
    {
        if (fadeOn == true)
        {
            StartCoroutine(DoTheFadeCR(material[index]));
        }
    }

    public void DoGray(int index) 
    {
        if (fadeOn == true) return;
        ConverToGray((material[index]));
    }

    public void DoColor(int index)
    {
        if (fadeOn == true) return;
        ConverToColor((material[index]));
    }


    private IEnumerator DoTheFadeCR(Material material) 
    {
        float time = 0;
        while(time < 1f)
        {
            material.SetFloat("_Blend", time);
            time += Time.deltaTime;
            yield return null;
        }

        material.SetFloat("_Blend", grayScale);
    }

    private void ConverToGray(Material material) 
    {
        int color = 1;
        if (Invertcolor) color = 0;
        material.SetFloat("_Blend", color);
    }
    private void ConverToColor(Material material)
    {
        int color = 0;
        if (Invertcolor) color = 1;
        material.SetFloat("_Blend", color);
    }
}
