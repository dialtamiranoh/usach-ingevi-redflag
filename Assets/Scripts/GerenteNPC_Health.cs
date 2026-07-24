using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Sistema de vida del Gerente de Sucursal.
/// HP = 3: cada respuesta correcta del jugador aplica TakeDamage(1).
/// Al llegar a 0, el gerente "es retirado" (secuencia Die).
/// La muerte actualiza el contador HUD via UIManager.
/// </summary>
public class GerenteNPC_Health : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHP = 3;
    private int hpActual;

    [Header("UI Barra de Vida Flotante (3D)")]
    [Tooltip("Slider 3D opcional asignado a mano. Si se deja vacío, la barra se crea por código.")]
    [SerializeField] private Slider sliderVida3D;

    [Header("Barra de vida auto-generada")]
    [Tooltip("Crear una barra flotante por código si no hay Slider asignado")]
    [SerializeField] private bool crearBarraAutomatica = true;
    [Tooltip("Altura sobre el origen del Gerente donde flota la barra (metros)")]
    [SerializeField] private float alturaBarra = 2.2f;
    [Tooltip("Tamaño de la barra en metros (ancho x alto)")]
    [SerializeField] private Vector2 tamanoBarra = new Vector2(0.6f, 0.0825f);
    [SerializeField] private Color colorLleno = new Color(0.16f, 0.85f, 0.35f);
    [SerializeField] private Color colorVacio = new Color(0.85f, 0.20f, 0.20f);
    [SerializeField] private Color colorFondo = new Color(0.45f, 0.06f, 0.06f, 0.9f);

    private Image barraFill;      // relleno cuya anchura refleja el HP
    private GameObject barraRoot; // Canvas World-Space hijo del Gerente

    public bool EstaMuerto { get; private set; } = false;

    // Evento observable: la muerte del gerente notifica al HUD
    public static event System.Action OnGerenteMuerto;

    void Start()
    {
        hpActual = maxHP;

        // Si nadie asignó un Slider en el inspector, construimos la barra por código
        if (sliderVida3D == null && crearBarraAutomatica)
            CrearBarraVida();

        ActualizarSliderVida();
    }

    /// <summary>
    /// Recibe daño. Llamado por GerenteNPC_FSM.OnRespuestaCorrecta().
    /// </summary>
    public void TakeDamage(int dmg)
    {
        if (EstaMuerto) return;

        hpActual -= dmg;
        hpActual = Mathf.Max(hpActual, 0);

        Debug.Log($"[GerenteHealth] HP restante: {hpActual}/{maxHP}");
        ActualizarSliderVida();

        if (hpActual <= 0)
            StartCoroutine(SecuenciaMuerte());
    }

    /// <summary>
    /// Secuencia de muerte: animación → desactivación → notificación HUD.
    /// </summary>
    private IEnumerator SecuenciaMuerte()
    {
        EstaMuerto = true;

        // Ocultar la barra de vida al morir (cualquiera de las dos variantes)
        if (sliderVida3D != null)
            sliderVida3D.gameObject.SetActive(false);
        if (barraRoot != null)
            barraRoot.SetActive(false);

        Animator anim = GetComponent<Animator>();
        anim?.SetTrigger("Die");

        // Esperar que termine la animación de muerte (ajustar según clip)
        yield return new WaitForSeconds(1.5f);

        // Notificar al HUD antes de desactivar
        OnGerenteMuerto?.Invoke();

        // Desactivar el NPC
        gameObject.SetActive(false);

        Debug.Log("[GerenteHealth] Gerente de Sucursal retirado.");
    }

    /// <summary>
    /// Devuelve HP normalizado (0-1) para barras de vida opcionales.
    /// </summary>
    public float HPNormalizado() => (float)hpActual / maxHP;

    private void ActualizarSliderVida()
    {
        float hp = HPNormalizado();

        if (sliderVida3D != null)
            sliderVida3D.value = hp;

        if (barraFill != null)
        {
            barraFill.fillAmount = hp;
            // Verde con vida llena → rojo cuando queda poca
            barraFill.color = Color.Lerp(colorVacio, colorLleno, hp);
        }
    }

    // ──────────────────────────────────────────────
    // BARRA DE VIDA FLOTANTE GENERADA POR CÓDIGO
    // ──────────────────────────────────────────────

    /// <summary>
    /// Construye un Canvas World-Space sobre la cabeza del Gerente con un fondo
    /// y un relleno cuyo ancho refleja el HP. Se orienta a la cámara con
    /// GerenteBillboard y sigue al Gerente porque es hijo de su transform.
    /// </summary>
    private void CrearBarraVida()
    {
        // IMPORTANTE: crear el GameObject CON RectTransform desde el constructor.
        // Si se añade Canvas/Image a un Transform normal en runtime, el RectTransform
        // puede no generarse y la UI queda invisible.
        barraRoot = new GameObject("BarraVidaGerente", typeof(RectTransform));
        barraRoot.transform.SetParent(transform, false);

        Canvas canvas = barraRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        barraRoot.AddComponent<GerenteBillboard>(); // mira siempre a la cámara activa

        RectTransform rootRect = (RectTransform)barraRoot.transform;
        rootRect.sizeDelta = tamanoBarra;              // en metros (localScale = 1)
        rootRect.localPosition = new Vector3(0f, alturaBarra, 0f);
        rootRect.localRotation = Quaternion.identity;
        rootRect.localScale = Vector3.one;

        Sprite blanco = SpriteBlanco();

        // Fondo (rojo oscuro): se ve a medida que el relleno se vacía
        Image fondo = CrearImagenHija("Fondo", blanco, colorFondo);
        StretchToParent(fondo.rectTransform);

        // Relleno (verde → rojo) con ancho proporcional al HP
        barraFill = CrearImagenHija("Relleno", blanco, colorLleno);
        barraFill.type = Image.Type.Filled;
        barraFill.fillMethod = Image.FillMethod.Horizontal;
        barraFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        barraFill.fillAmount = 1f;
        StretchToParent(barraFill.rectTransform);

        Debug.Log("[GerenteHealth] Barra de vida flotante creada sobre el Gerente.");
    }

    private Image CrearImagenHija(string nombre, Sprite sprite, Color color)
    {
        // También con RectTransform explícito para que Image se dibuje correctamente.
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(barraRoot.transform, false);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    /// <summary>Sprite blanco 1x1 para dibujar rectángulos de color sólido.</summary>
    private static Sprite SpriteBlanco()
    {
        Texture2D tex = Texture2D.whiteTexture;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
    }
}
