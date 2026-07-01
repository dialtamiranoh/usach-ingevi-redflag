using UnityEngine;
using UnityEngine.InputSystem;

public class CajonInteractivo : MonoBehaviour
{
    [Header("Animaci�n apertura")]
    public Transform parteCajon;
    public float distanciaApertura = 0.3f;
    public float velocidadApertura = 5f;

    [Header("Brillo hover")]
    public Color colorHover = Color.cyan;
    public float intensidadHover = 0.8f;
    public Color colorHoverConObjeto = Color.green;
    public float intensidadHoverConObjeto = 1.2f;

    private Vector3 posicionCerrado;
    private Vector3 posicionAbierto;
    private bool estaAbierto = false;
    private bool mouseEncima = false;

    private Material[] materiales;
    private Camera cam;

    void Start()
    {
        posicionCerrado = parteCajon.localPosition;
        posicionAbierto = posicionCerrado + Vector3.forward * distanciaApertura;

        // Obtener c�mara correcta del CameraController
        cam = FindObjectOfType<CameraController>().GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;

        Renderer[] renderers = parteCajon.GetComponentsInChildren<Renderer>();
        System.Collections.Generic.List<Material> mats = new();
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                m.EnableKeyword("_EMISSION");
                mats.Add(m);
            }
        }
        materiales = mats.ToArray();
    }

    void Update()
    {


        // Raycast desde la c�mara al mouse
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        int layerMask = ~LayerMask.GetMask("ZonaSegura");
        bool mouseEncimaNow = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)
            && hit.collider.gameObject == gameObject;

        // Detectar entrada y salida manualmente
        if (mouseEncimaNow && !mouseEncima)
        {
            mouseEncima = true;
            Debug.Log("HOVER CAJON");
        }
        else if (!mouseEncimaNow && mouseEncima)
        {
            mouseEncima = false;
        }

        bool hayObjetoArrastrado = HayObjetoArrastrado();
        // Debug.Log($\"mouseEncima...\");

        if (mouseEncima)
        {
            if (hayObjetoArrastrado)
            {
                estaAbierto = true;
                SetBrillo(colorHoverConObjeto * intensidadHoverConObjeto);
            }
            else
            {
                estaAbierto = false;
                SetBrillo(colorHover * intensidadHover);
            }
        }
        else
        {
            if (!hayObjetoArrastrado)
                estaAbierto = false;
            SetBrillo(Color.black);
        }

        // Mover caj�n
        Vector3 objetivo = estaAbierto ? posicionAbierto : posicionCerrado;
        parteCajon.localPosition = Vector3.Lerp(
            parteCajon.localPosition, objetivo,
            velocidadApertura * Time.deltaTime);
    }

    //void OnMouseEnter()
    //{
    //    Debug.Log("MOUSE ENTER CAJON");
    //    mouseEncima = true;
    //    SetBrillo(colorHover * intensidadHover);
    //}

    //void OnMouseExit()
    //{
    //    Debug.Log("MOUSE EXIT CAJON");
    //    mouseEncima = false;
    //    if (!estaAbierto)
    //        SetBrillo(Color.black);
    //}

    void SetBrillo(Color color)
    {
        foreach (Material mat in materiales)
            mat.SetColor("_EmissionColor", color);
    }

    bool HayObjetoArrastrado()
    {
        foreach (ObjetoSospechoso obj in FindObjectsOfType<ObjetoSospechoso>())
            if (obj.estaArrastrado) return true;
        return false;
    }
}
