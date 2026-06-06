using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjetoSospechoso : MonoBehaviour
{
    // Tipo de objeto para el log y puntaje
    public enum TipoObjeto
    {
        Pendrive,
        Celular,
        PostIt,
        Llaves,
        Credencial,
        Carpeta,
        Documento,
        Tarjeta,
        // Sobornos — categoría especial
        SobreEfectivo,
        CajaRegalo
    }
    public TipoObjeto tipo;


    [Header("Respawn")]
    public float tiempoRespawnMin = 15f;
    public float tiempoRespawnMax = 45f;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    [Header("Configuración")]
    public float tiempoLimite = 30f;
    public AudioClip sfxRecogido;
    public AudioClip sfxPenalizacion;

    [Header("Feedback visual")]
    public GameObject alertaVFX; // partícula o luz parpadeante opcional

    private float timerIgnorado = 0f;
    public bool estaArrastrado = false;
    private bool fueGuardado = false;
    private bool penalizado = false;
    private bool fueInteractuado = false;

    private Rigidbody rb;
    private AudioSource audioSource;
    private BrilloSospechoso brillo;
    private Camera cam;
    private Camera camaraActual;
    private Vector3 offsetArrastre;
    private float distanciaCamara;
    private Plane planoArrastre;



    public enum CategoriaObjeto { Seguridad, Soborno }
    public CategoriaObjeto categoria;

    // Referencia al manager para reportar eventos
    private ObjetosManager manager;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        brillo = GetComponent<BrilloSospechoso>();
        cam = Camera.main;
        camaraActual = Camera.main;
        manager = FindObjectOfType<ObjetosManager>(); // mover aquí desde Start
    }


    void Start()
    {
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;

        if (categoria == CategoriaObjeto.Soborno)
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
                    SobornoIgnorado();  // timer llegó a 20 seg sin click = correcto
                else
                    AplicarPenalizacion();
            }
        }
    }

    // ── Arrastre con mouse ──────────────────────────────────────


    void OnMouseDown()
    {
        if (fueGuardado || penalizado) return;

        if (categoria == CategoriaObjeto.Soborno)
        {
            // Click en soborno = penalización
            SobornoAceptado();
            return;
        }

        // Objetos de seguridad — drag normal
        estaArrastrado = true;
        rb.isKinematic = true;
        distanciaCamara = Vector3.Distance(
            camaraActual.transform.position, transform.position);
        offsetArrastre = transform.position - ObtenerPosicionMundo();
    }

    void SobornoAceptado()
    {
        penalizado = true;
        brillo?.DetenerBrillo();

        // Penalización por aceptar soborno
        ScoreManager score = FindObjectOfType<ScoreManager>();
        score?.AgregarPuntajeIndividual(-300);
        Debug.Log("[LOG] Soborno aceptado — penalización -300");

        StartCoroutine(FadeYRespawn());
    }

    void SobornoIgnorado()
    {
        penalizado = true;
        brillo?.DetenerBrillo();

        ScoreManager score = FindObjectOfType<ScoreManager>();
        score?.AgregarPuntajeIndividual(100);
        Debug.Log("[LOG] Soborno ignorado correctamente +100");

        StartCoroutine(FadeYRespawn());
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

    // ── Zona segura ─────────────────────────────────────────────

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

        if (sfxRecogido != null)
            audioSource.PlayOneShot(sfxRecogido);

        manager?.OnObjetoGuardado(this);

        // Usar el scoreManager del manager en lugar de buscarlo de nuevo
        ScoreManager score = FindObjectOfType<ScoreManager>();
        if (score != null)
        {
            score.AgregarPuntajeIndividual(50);
            Debug.Log("[LOG] Objeto guardado +50 puntos");
        }
        else
            Debug.LogWarning("[LOG] ScoreManager no encontrado");

        StartCoroutine(FadeYRespawn());
    }

    IEnumerator FadeYRespawn()
    {
        yield return StartCoroutine(FadeOut());
        gameObject.SetActive(false);

        float espera = categoria == CategoriaObjeto.Soborno
            ? 20f
            : Random.Range(tiempoRespawnMin, tiempoRespawnMax);

        Debug.Log($"[LOG] Programando respawn en {espera} seg — manager: {manager}");
        manager?.ProgramarRespawn(this, espera);
    }




    public void Respawn()
    {
        if (manager == null)
            manager = FindObjectOfType<ObjetosManager>();

        fueGuardado = false;
        penalizado = false;
        timerIgnorado = 0f;
        fueInteractuado = false;

        transform.position = posicionInicial;
        transform.rotation = rotacionInicial;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        gameObject.SetActive(true);
        Vector3 escalaFinal = transform.localScale;
        transform.localScale = Vector3.zero;
        if (brillo != null)
        {
            brillo.enabled = true;
            brillo.ReiniciarBrillo();
        }
        StartCoroutine(AnimarSpawn(escalaFinal));
    }

    System.Collections.IEnumerator FadeOut()
    {
        float t = 0f;
        float duracion = 0.5f;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // Guardar colores originales
        var coloresOriginales = new System.Collections.Generic.List<Color>();
        foreach (Renderer r in renderers)
            foreach (Material m in r.materials)
                coloresOriginales.Add(m.color);

        while (t < duracion)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / duracion);
            int idx = 0;
            foreach (Renderer r in renderers)
                foreach (Material m in r.materials)
                {
                    Color c = coloresOriginales[idx++];
                    m.color = new Color(c.r, c.g, c.b, alpha);
                }
            yield return null;
        }
    }

    void AplicarPenalizacion()
    {
        penalizado = true;
        brillo?.DetenerBrillo();

        if (categoria == CategoriaObjeto.Soborno)
        {
            // Soborno ignorado = correcto, suma puntaje
            ScoreManager score = FindObjectOfType<ScoreManager>();
            score?.AgregarPuntajeIndividual(100);
            Debug.Log($"[LOG] Soborno ignorado correctamente +100");
            StartCoroutine(FadeYRespawn());
        }
        else
        {
            // Objeto seguridad ignorado = penalización
            if (sfxPenalizacion != null)
                audioSource.PlayOneShot(sfxPenalizacion);
            manager?.OnObjetoIgnorado(this);
            StartCoroutine(FadeYDestruir());
        }
    }

    // ── Coroutines ───────────────────────────────────────────────

    System.Collections.IEnumerator AnimarSpawn(Vector3 escalaFinal)
    {
        float t = 0f;
        float duracion = 0.3f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(
                Vector3.zero, escalaFinal, t / duracion);
            yield return null;
        }
        transform.localScale = escalaFinal;
    }

    System.Collections.IEnumerator FadeYDestruir()
    {
        float t = 0f;
        float duracion = 0.5f;

        // Buscar renderers en hijos también
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        var coloresOriginales = new System.Collections.Generic.List<Color>();
        foreach (Renderer r in renderers)
            foreach (Material m in r.materials)
                coloresOriginales.Add(m.color);

        while (t < duracion)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / duracion);
            int idx = 0;
            foreach (Renderer r in renderers)
                foreach (Material m in r.materials)
                {
                    Color c = coloresOriginales[idx++];
                    m.color = new Color(c.r, c.g, c.b, alpha);
                }
            yield return null;
        }
        Destroy(gameObject);
    }


    public void ProgramarRespawn(ObjetoSospechoso obj, float tiempo)
    {
        StartCoroutine(RespawnCoroutine(obj, tiempo));
    }

    IEnumerator RespawnCoroutine(ObjetoSospechoso obj, float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        obj.Respawn();
    }

    // Getter para el log
    public string ObtenerNombreTipo() => tipo.ToString();


}