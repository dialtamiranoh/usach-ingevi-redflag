using UnityEngine;
using System.Collections.Generic;

public class BrilloSospechoso : MonoBehaviour
{
    [Header("Configuración brillo")]
    public Color colorBrillo = Color.yellow;
    public float velocidad = 3f;
    public float intensidadMin = 0f;
    public float intensidadMax = 0.8f;

    private Material[] materiales;
    private bool activo = true;

    void Awake()
    {
        // Busca renderers en el objeto Y en todos sus hijos
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        List<Material> todosMateriales = new List<Material>();
        foreach (Renderer r in renderers)
        {
            Material[] mats = r.materials;
            foreach (Material mat in mats)
                mat.EnableKeyword("_EMISSION");
            todosMateriales.AddRange(mats);
        }
        materiales = todosMateriales.ToArray();
    }

    void Update()
    {
        if (!activo) return;

        float t = Mathf.PingPong(Time.time * velocidad, 1f);
        float intensidad = Mathf.Lerp(intensidadMin, intensidadMax, t);

        foreach (Material mat in materiales)
            mat.SetColor("_EmissionColor", colorBrillo * intensidad);
    }

    public void DetenerBrillo()
    {
        activo = false;
        foreach (Material mat in materiales)
            mat.SetColor("_EmissionColor", Color.black);
    }
}