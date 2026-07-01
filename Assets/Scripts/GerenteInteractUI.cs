using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Maneja el popup de pregunta/presión del Gerente de Sucursal.
/// Carga preguntas de npcPreguntas.json, muestra timer de 15s.
/// Correcto → callback a FSM (TakeDamage al gerente).
/// Incorrecto/Timeout → penalización: -100 pts + reset racha.
/// </summary>
public class GerenteInteractUI : MonoBehaviour
{
    // ──────────────────────────────────────────────
    // INSPECTOR
    // ──────────────────────────────────────────────
    [Header("UI Toolkit")]
    public UIDocument uiDocument;

    [Header("Timer")]
    [SerializeField] private float tiempoRespuesta = 15f;

    [Header("Penalización")]
    [SerializeField] private int penalizacionPuntos = 100;

    // ──────────────────────────────────────────────
    // PRIVADAS
    // ──────────────────────────────────────────────
    private VisualElement panelGerente;
    private Label labelPregunta;
    private Label labelNormativa;
    private Label labelTimer;
    private Button[] botonesOpciones;
    private VisualElement timerBar;

    private List<NPCPregunta> preguntas;
    private NPCPregunta preguntaActual;
    private System.Action cbCorrecta;
    private System.Action cbIncorrecta;
    private Coroutine coroutineTimer;
    private bool respondido = false;

    // ──────────────────────────────────────────────
    // LIFECYCLE
    // ──────────────────────────────────────────────
    void Awake()
    {
        CargarPreguntas();
        InicializarUI();
        OcultarUI();
    }

    // ──────────────────────────────────────────────
    // CARGA DE DATOS
    // ──────────────────────────────────────────────
    private void CargarPreguntas()
    {
        string path = Path.Combine(Application.dataPath, "Data", "npcPreguntas.json");
        if (!File.Exists(path))
        {
            Debug.LogError("[GerenteUI] npcPreguntas.json no encontrado en: " + path);
            preguntas = new List<NPCPregunta>();
            return;
        }
        string json = File.ReadAllText(path);
        NPCPreguntasWrapper wrapper = JsonUtility.FromJson<NPCPreguntasWrapper>(json);
        preguntas = wrapper?.preguntas ?? new List<NPCPregunta>();
        Debug.Log($"[GerenteUI] {preguntas.Count} preguntas cargadas.");
    }

    // ──────────────────────────────────────────────
    // INICIALIZAR UI
    // ──────────────────────────────────────────────
    private void InicializarUI()
    {
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;

        panelGerente  = root.Q<VisualElement>("panel-gerente");
        labelPregunta = root.Q<Label>("gerente-pregunta");
        labelNormativa = root.Q<Label>("gerente-normativa");
        labelTimer    = root.Q<Label>("gerente-timer");
        timerBar      = root.Q<VisualElement>("gerente-timer-bar");

        // Botones de opciones (4 opciones A B C D)
        botonesOpciones = new Button[4];
        botonesOpciones[0] = root.Q<Button>("gerente-opcion-0");
        botonesOpciones[1] = root.Q<Button>("gerente-opcion-1");
        botonesOpciones[2] = root.Q<Button>("gerente-opcion-2");
        botonesOpciones[3] = root.Q<Button>("gerente-opcion-3");

        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            int idx = i; // captura para lambda
            if (botonesOpciones[idx] != null)
            {
                botonesOpciones[idx].clicked += () => OnOpcionSeleccionada(idx);
            }
        }
    }

    // ──────────────────────────────────────────────
    // API PÚBLICA — llamada por GerenteNPC_FSM
    // ──────────────────────────────────────────────

    /// <summary>
    /// Muestra el panel con una pregunta aleatoria e inicia el timer.
    /// </summary>
    public void MostrarPregunta(System.Action onCorrecta, System.Action onIncorrecta)
    {
        if (preguntas == null || preguntas.Count == 0) { onIncorrecta?.Invoke(); return; }

        cbCorrecta   = onCorrecta;
        cbIncorrecta = onIncorrecta;
        respondido   = false;

        // Seleccionar pregunta aleatoria
        preguntaActual = preguntas[Random.Range(0, preguntas.Count)];

        PoblarUI(preguntaActual);

        if (panelGerente != null)
            panelGerente.RemoveFromClassList("hidden");

        // Iniciar timer
        if (coroutineTimer != null) StopCoroutine(coroutineTimer);
        coroutineTimer = StartCoroutine(TimerCoroutine());

        AudioManager.Instance?.SFXEscalar(); // reutilizar sonido de alerta
    }

    /// <summary>Oculta el panel del gerente.</summary>
    public void OcultarUI()
    {
        if (panelGerente != null)
            panelGerente.AddToClassList("hidden");
    }

    // ──────────────────────────────────────────────
    // LÓGICA INTERNA
    // ──────────────────────────────────────────────
    private void PoblarUI(NPCPregunta p)
    {
        if (labelPregunta != null)  labelPregunta.text  = $"💼 GERENTE: \"{p.pregunta}\"";
        if (labelNormativa != null) labelNormativa.text = $"Normativa: {p.normativa}";

        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            if (botonesOpciones[i] != null && i < p.opciones.Length)
            {
                botonesOpciones[i].text = p.opciones[i];
                // Reset estilo
                botonesOpciones[i].RemoveFromClassList("opcion-correcta");
                botonesOpciones[i].RemoveFromClassList("opcion-incorrecta");
                botonesOpciones[i].SetEnabled(true);
            }
        }
    }

    private void OnOpcionSeleccionada(int indice)
    {
        if (respondido) return;
        respondido = true;

        Debug.Log($"[GerenteUI] Opción seleccionada: {indice} (Correcta: {preguntaActual.correcta})");

        if (coroutineTimer != null) StopCoroutine(coroutineTimer);

        bool esCorrecta = (indice == preguntaActual.correcta);

        // Feedback visual en botones
        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            if (botonesOpciones[i] == null) continue;
            botonesOpciones[i].SetEnabled(false);
            if (i == preguntaActual.correcta)
                botonesOpciones[i].AddToClassList("opcion-correcta");
            else if (i == indice && !esCorrecta)
                botonesOpciones[i].AddToClassList("opcion-incorrecta");
        }

        StartCoroutine(CerrarConDelay(esCorrecta ? cbCorrecta : cbIncorrecta, 1.5f));
    }

    private IEnumerator TimerCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < tiempoRespuesta)
        {
            elapsed += Time.deltaTime;
            float ratio = 1f - (elapsed / tiempoRespuesta);

            // Actualizar label y barra de progreso
            if (labelTimer != null)
                labelTimer.text = $"{(tiempoRespuesta - elapsed):F0}s";
            if (timerBar != null)
                timerBar.style.width = Length.Percent(ratio * 100f);

            yield return null;
        }

        // Tiempo agotado → incorrecto (Gerente espió el expediente)
        if (!respondido)
        {
            respondido = true;
            Debug.Log("[GerenteUI] Tiempo de respuesta agotado.");
            if (labelTimer != null) labelTimer.text = "¡TIEMPO!";
            StartCoroutine(CerrarConDelay(cbIncorrecta, 1.0f));
        }
    }

    private IEnumerator CerrarConDelay(System.Action callback, float delay)
    {
        Debug.Log($"[GerenteUI] CerrarConDelay iniciado con delay de {delay}s.");
        yield return new WaitForSeconds(delay);
        OcultarUI();
        if (callback != null)
        {
            Debug.Log($"[GerenteUI] Invocando callback de respuesta: {callback.Method.Name}");
            callback.Invoke();
        }
        else
        {
            Debug.Log("[GerenteUI] El callback de retorno a la FSM es NULL.");
        }
    }
}

// ──────────────────────────────────────────────
// DATA CLASSES (deserialización JSON)
// ──────────────────────────────────────────────
[System.Serializable]
public class NPCPregunta
{
    public int id;
    public string pregunta;
    public string[] opciones;
    public int correcta;
    public string normativa;
    public int puntaje;
}

[System.Serializable]
public class NPCPreguntasWrapper
{
    public List<NPCPregunta> preguntas;
}
