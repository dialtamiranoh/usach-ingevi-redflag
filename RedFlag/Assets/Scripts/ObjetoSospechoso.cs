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
        // Sobornos — categoría especial
        SobreEfectivo,
        TarjetaPrepagada,
        CajaRegalo
    }
    public TipoObjeto tipo;

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

    private Rigidbody rb;
    private AudioSource audioSource;
    private Camera camaraActual;
    private Vector3 offsetArrastre;
    private float distanciaCamara;

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
    }

    void Start()
    {
        manager = FindObjectOfType<ObjetosManager>();
        camaraActual = Camera.main;

        // Animación de spawn: escala de 0 a tamaño real
        Vector3 escalaFinal = transform.localScale;
        transform.localScale = Vector3.zero;
        StartCoroutine(AnimarSpawn(escalaFinal));
    }

    void Update()
    {
        // Contar tiempo si no fue guardado ni penalizado
        if (!fueGuardado && !penalizado)
        {
            timerIgnorado += Time.deltaTime;
            if (timerIgnorado >= tiempoLimite)
                AplicarPenalizacion();
        }
    }

    // ── Arrastre con mouse ──────────────────────────────────────

    void OnMouseDown()
    {
        if (fueGuardado || penalizado) return;

        estaArrastrado = true;
        // Congelar física durante el arrastre
        rb.isKinematic = true;

        distanciaCamara = Vector3.Distance(
            camaraActual.transform.position, transform.position);
        offsetArrastre = transform.position -
            ObtenerPosicionMundo();
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
        // Reactivar física al soltar — el objeto cae y rebota
        rb.isKinematic = false;
    }

    Vector3 ObtenerPosicionMundo()
    {
        // Convierte posición del mouse a coordenadas 3D
        // manteniendo la distancia original a la cámara
        Vector3 posMouse = Mouse.current.position.ReadValue();
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

        // Detener brillo al ser recogido
        GetComponent<BrilloSospechoso>()?.DetenerBrillo();

        // Feedback visual: destello verde
        GetComponent<Renderer>().material.color = Color.green;

        // Feedback sonoro
        if (sfxRecogido != null)
            audioSource.PlayOneShot(sfxRecogido);

        manager?.OnObjetoGuardado(this);
    }

    void AplicarPenalizacion()
    {
        penalizado = true;

        // Feedback sonoro de penalización
        if (sfxPenalizacion != null)
            audioSource.PlayOneShot(sfxPenalizacion);

        // Notificar al manager
        manager?.OnObjetoIgnorado(this);

        // Fade out y destruir
        StartCoroutine(FadeYDestruir());
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
        Renderer rend = GetComponent<Renderer>();
        Color colorBase = rend.material.color;

        while (t < duracion)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / duracion);
            rend.material.color = new Color(
                colorBase.r, colorBase.g, colorBase.b, alpha);
            yield return null;
        }
        Destroy(gameObject);
    }

    // Getter para el log
    public string ObtenerNombreTipo() => tipo.ToString();


}