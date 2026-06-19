using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjetoSospechoso : MonoBehaviour
{
    public enum TipoObjeto
    {
        Pendrive, Celular, PostIt, Llaves, Credencial,
        Carpeta, Documento, Tarjeta, SobreEfectivo, CajaRegalo
    }

    public enum CategoriaObjeto { Seguridad, Soborno }

    [Header("Configuración")]
    public TipoObjeto tipo;
    public CategoriaObjeto categoria;
    public float tiempoLimite = 20f;

    [Header("Audio")]
    public AudioClip sfxRecogido;
    public AudioClip sfxPenalizacion;

    // Estado
    public bool estaArrastrado = false;
    private bool fueGuardado = false;
    private bool penalizado = false;
    private float timerIgnorado = 0f;

    // Componentes
    private Rigidbody rb;
    private AudioSource audioSource;
    private BrilloSospechoso brillo;
    private Camera camaraActual;
    private Vector3 offsetArrastre;
    private float distanciaCamara;
    private ObjetosManager manager;
    //private ScoreManager scoreManager;

    private UIManager uiManager;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        brillo = GetComponent<BrilloSospechoso>();
        camaraActual = Camera.main;
        manager = FindObjectOfType<ObjetosManager>();
        //scoreManager = FindObjectOfType<ScoreManager>();
        uiManager = FindObjectOfType<UIManager>();
    }

    void Start()
    {
        // Todos los objetos tienen 20 segundos
        tiempoLimite = 20f;

        Vector3 escalaFinal = transform.localScale;
        transform.localScale = Vector3.zero;
        StartCoroutine(AnimarSpawn(escalaFinal));
    }

    void Update()
    {
        if (!fueGuardado && !penalizado)
        {
            timerIgnorado += Time.deltaTime;
            if (timerIgnorado >= tiempoLimite)
            {
                if (categoria == CategoriaObjeto.Soborno)
                    SobornoIgnorado();
                else
                    ObjetoIgnorado();
            }
        }
    }

    // ── Mouse ────────────────────────────────────────────────────

    void OnMouseDown()
    {
        if (fueGuardado || penalizado) return;

        if (categoria == CategoriaObjeto.Soborno)
        {
            SobornoAceptado();
            return;
        }

        estaArrastrado = true;
        rb.isKinematic = true;
        distanciaCamara = Vector3.Distance(
            camaraActual.transform.position, transform.position);
        offsetArrastre = transform.position - ObtenerPosicionMundo();
    }

    void OnMouseDrag()
    {
        if (!estaArrastrado) return;
        transform.position = ObtenerPosicionMundo() + offsetArrastre;
    }

    void OnMouseUp()
    {
        if (!estaArrastrado) return;
        estaArrastrado = false;
        rb.isKinematic = false;
    }

    Vector3 ObtenerPosicionMundo()
    {
        Vector3 posMouse = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        posMouse.z = distanciaCamara;
        return camaraActual.ScreenToWorldPoint(posMouse);
    }

    // ── Zona segura ──────────────────────────────────────────────

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZonaSegura") && !fueGuardado)
            GuardarObjeto();
    }

    void GuardarObjeto()
    {
        fueGuardado = true;
        rb.isKinematic = true;
        brillo?.DetenerBrillo();

        uiManager?.AgregarPuntaje(150);
        Debug.Log($"[LOG] Objeto guardado: {ObtenerNombreTipo()} +150 pts");

        if (sfxRecogido != null)
            audioSource.PlayOneShot(sfxRecogido);

        manager?.OnObjetoGuardado(this);
        StartCoroutine(FadeYDestruir());
    }

    // ── Lógica de seguridad ──────────────────────────────────────

    void ObjetoIgnorado()
    {
        penalizado = true;
        brillo?.DetenerBrillo();

        uiManager?.AgregarPuntaje(-200);
        Debug.Log($"[LOG] Objeto ignorado: {ObtenerNombreTipo()} -200 pts");

        if (sfxPenalizacion != null)
            audioSource.PlayOneShot(sfxPenalizacion);

        manager?.OnObjetoIgnorado(this);
        StartCoroutine(FadeYDestruir());
    }

    // ── Lógica de soborno ────────────────────────────────────────

    void SobornoAceptado()
    {
        penalizado = true;
        brillo?.DetenerBrillo();

        uiManager?.AgregarPuntaje(-300);
        Debug.Log("[LOG] Soborno aceptado — penalización -300");

        manager?.OnObjetoIgnorado(this);
        StartCoroutine(FadeYDestruir());
    }

    void SobornoIgnorado()
    {
        penalizado = true;
        brillo?.DetenerBrillo();

        uiManager?.AgregarPuntaje(100);
        Debug.Log("[LOG] Soborno ignorado correctamente +100");

        manager?.OnObjetoIgnorado(this);
        StartCoroutine(FadeYDestruir());
    }

    // ── Coroutines ────────────────────────────────────────────────

    IEnumerator AnimarSpawn(Vector3 escalaFinal)
    {
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, escalaFinal, t / 0.3f);
            yield return null;
        }
        transform.localScale = escalaFinal;
    }

    void RestaurarOpacidad()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            foreach (Material m in r.materials)
            {
                Color c = m.color;
                m.color = new Color(c.r, c.g, c.b, 1f);
            }
    }

    IEnumerator FadeOut()
    {
        float t = 0f;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        var colores = new List<Color>();
        foreach (Renderer r in renderers)
            foreach (Material m in r.materials)
                colores.Add(m.color);

        while (t < 0.5f)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / 0.5f);
            int idx = 0;
            foreach (Renderer r in renderers)
                foreach (Material m in r.materials)
                {
                    Color c = colores[idx++];
                    m.color = new Color(c.r, c.g, c.b, alpha);
                }
            yield return null;
        }
    }

    IEnumerator FadeYDestruir()
    {
        yield return StartCoroutine(FadeOut());
        Destroy(gameObject);
    }

    public string ObtenerNombreTipo() => tipo.ToString();
}
