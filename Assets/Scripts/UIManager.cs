using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument uiDocument;

    [Header("Cliente 3D")]
    public GameObject clienteObject;

    [Header("Case Manager")]
    public CaseManager caseManager;

    [Header("Personajes")]
    public GameObject[] personajes;
    private int indicePersonaje = 0;

    [Header("Camera")]
    public CameraController cameraController;


    // Feedback visual
    private VisualElement flashPantalla;
    private VisualElement selloContainer;
    private Label selloTexto;

    // Top bar
    private Label labelTurno;
    private Label labelTiempo;
    private Label labelPuntaje;
    private Label labelEstado;

    // Expediente
    private Label casoId;
    private Label casoTipo;
    private Label casoPrioridad;
    private Label clienteNombre;
    private Label clienteRut;
    private Label clienteNacionalidad;
    private Label clienteActividad;
    private Label clientePep;
    private TextField notasAnalista;

    // Diálogo
    private VisualElement dialogoPanel;
    private Label dialogoRespuesta;
    private Button pregunta1, pregunta2, pregunta3, pregunta4;
    private Button dialogoCerrar;
    private Label hintCliente;
    private bool mouseEnUI = false;

    // Feedback modal
    private VisualElement panelFeedback;
    private Label feedbackTitulo;
    private Label feedbackDecision;
    private Label feedbackNormativa;
    private Label feedbackExplicacion;
    private Button feedbackCerrar;

    // Botones de acción
    private Button btnAprobar, btnEscalar, btnRechazar;

    // Logros
    private Button btnLogros;
    private VisualElement panelLogros;
    private VisualElement grillaLogros;
    private Button btnCerrarLogros;
    private VisualElement toastLogro;
    private Label toastLogroNombre;

    // Tabs
    private Button tabIdentidad, tabTransacciones, tabHistorial, tabSanciones;

    // Estado
    private int turnoActual = 1;
    private int turnoTotal = 5;
    private int puntaje = 0;
    private float tiempoRestante = 60f;
    private bool juegoActivo = true;
    private bool dialogoAbierto = false;
    
    // Multiplicador y Racha
    private int rachaCorrectas = 0;
    private float multiplicador = 1.0f;
    private int rachaMaxima = 0;

    // Eventos para patrón Observer
    public event System.Action<int, int> OnPuntajeChanged;     // (nuevoPuntaje, delta)
    public event System.Action<int, int> OnTurnoChanged;       // (turnoActual, turnoTotal)
    public event System.Action<float> OnTiempoChanged;         // tiempoRestante
    public event System.Action<bool> OnJornadaTerminada;       // true=victoria, false=derrota
    public event System.Action<int> OnMultiplicadorChanged;    // nuevoMultiplicador

    void Awake()
    {
        var root = uiDocument.rootVisualElement;
        InicializarElementos(root);
        ConfigurarBotones();
        ConfigurarTabs();
        ConfigurarDialogo();
        ConfigurarFieldsClickables(root);
        ConfigurarHUDFeedback();
        ActualizarUI();
        Invoke(nameof(IniciarVistaCliente), 0.1f);
    }

    void Start()
    {
        if (AchievementManager.Instance != null) {
            AchievementManager.Instance.OnLogroDesbloqueado += MostrarToastLogro;
        }
    }

    void IniciarVistaCliente()
    {
        ActualizarVista(CameraController.VistaActiva.Cliente);
    }

    void Update()
    {
        if (!juegoActivo) return;
        tiempoRestante -= Time.deltaTime;
        
        OnTiempoChanged?.Invoke(tiempoRestante);
        
        if (tiempoRestante <= 0) { 
            tiempoRestante = 0; 
            TerminarJornada(false, "Tiempo agotado — caso no resuelto");
            return;
        }
        ActualizarTiempo();

        // Detectar clic en cliente 3D
        if (Mouse.current.leftButton.wasPressedThisFrame && !dialogoAbierto && !mouseEnUI)
        {
            // Ignorar si el puntero está sobre la UI
            //if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                DetectarClicCliente();
        }
    }

    void InicializarElementos(VisualElement root)
    {
        labelTurno    = root.Q<Label>("label-turno");
        labelTiempo   = root.Q<Label>("label-tiempo");
        labelPuntaje  = root.Q<Label>("label-puntaje");
        labelEstado   = root.Q<Label>("label-estado");

        panelFeedback = root.Q<VisualElement>("panel-feedback");
        panelFeedback.RegisterCallback<MouseEnterEvent>(e => mouseEnUI = true);
        panelFeedback.RegisterCallback<MouseLeaveEvent>(e => mouseEnUI = false);

        feedbackTitulo = root.Q<Label>("feedback-titulo");
        feedbackDecision = root.Q<Label>("feedback-decision");
        feedbackNormativa = root.Q<Label>("feedback-normativa");
        feedbackExplicacion = root.Q<Label>("feedback-explicacion");
        feedbackCerrar = root.Q<Button>("feedback-cerrar");
        feedbackCerrar.clicked += CerrarFeedback;

        flashPantalla       = root.Q<VisualElement>("flash-pantalla");
        selloContainer      = root.Q<VisualElement>("sello-container");
        selloTexto          = root.Q<Label>("sello-texto");

        casoId              = root.Q<Label>("caso-id");
        casoTipo            = root.Q<Label>("caso-tipo");
        casoPrioridad       = root.Q<Label>("caso-prioridad");
        clienteNombre       = root.Q<Label>("cliente-nombre");
        clienteRut          = root.Q<Label>("cliente-rut");
        clienteNacionalidad = root.Q<Label>("cliente-nacionalidad");
        clienteActividad    = root.Q<Label>("cliente-actividad");
        clientePep          = root.Q<Label>("cliente-pep");
        notasAnalista       = root.Q<TextField>("notas-analista");

        dialogoPanel      = root.Q<VisualElement>("dialogo-panel");
        dialogoRespuesta  = root.Q<Label>("dialogo-respuesta");
        hintCliente       = root.Q<Label>("hint-cliente");
        pregunta1         = root.Q<Button>("pregunta-1");
        pregunta2         = root.Q<Button>("pregunta-2");
        pregunta3         = root.Q<Button>("pregunta-3");
        pregunta4         = root.Q<Button>("pregunta-4");
        dialogoCerrar     = root.Q<Button>("dialogo-cerrar");

        btnAprobar  = root.Q<Button>("btn-aprobar");
        btnEscalar  = root.Q<Button>("btn-escalar");
        btnRechazar = root.Q<Button>("btn-rechazar");

        tabIdentidad     = root.Q<Button>("tab-identidad");
        tabTransacciones = root.Q<Button>("tab-transacciones");
        tabHistorial     = root.Q<Button>("tab-historial");
        tabSanciones     = root.Q<Button>("tab-sanciones");

        root.Q<VisualElement>("panel-expediente").RegisterCallback<MouseEnterEvent>(e => mouseEnUI = true);
        root.Q<VisualElement>("panel-expediente").RegisterCallback<MouseLeaveEvent>(e => mouseEnUI = false);
        root.Q<VisualElement>("panel-documentos").RegisterCallback<MouseEnterEvent>(e => mouseEnUI = true);
        root.Q<VisualElement>("panel-documentos").RegisterCallback<MouseLeaveEvent>(e => mouseEnUI = false);

        root.Q<VisualElement>("bottom-bar").RegisterCallback<MouseEnterEvent>(e => mouseEnUI = true);
        root.Q<VisualElement>("bottom-bar").RegisterCallback<MouseLeaveEvent>(e => mouseEnUI = false);

        btnLogros = root.Q<Button>("btn-logros");
        panelLogros = root.Q<VisualElement>("panel-logros");
        grillaLogros = root.Q<VisualElement>("grilla-logros");
        btnCerrarLogros = root.Q<Button>("btn-cerrar-logros");
        toastLogro = root.Q<VisualElement>("toast-logro");
        toastLogroNombre = root.Q<Label>("toast-logro-nombre");

        if (btnLogros != null) btnLogros.clicked += AbrirPanelLogros;
        if (btnCerrarLogros != null) btnCerrarLogros.clicked += CerrarPanelLogros;
    }

    void ConfigurarDialogo()
    {
        pregunta1.clicked += () => ResponderPregunta("actividad",
            caseManager?.ObtenerRespuesta("actividad") ?? "");
        pregunta2.clicked += () => ResponderPregunta("origen_fondos",
            caseManager?.ObtenerRespuesta("origen_fondos") ?? "");
        pregunta3.clicked += () => ResponderPregunta("esPEP",
            caseManager?.ObtenerRespuesta("esPEP") ?? "");
        pregunta4.clicked += () => ResponderPregunta("cuentasExtranjero",
            caseManager?.ObtenerRespuesta("cuentasExtranjero") ?? "");
        dialogoCerrar.clicked += CerrarDialogo;
    }

    void ConfigurarFieldsClickables(VisualElement root)
    {
        // Al hacer clic en un campo del expediente, se genera la pregunta asociada
        root.Q<VisualElement>("field-actividad").RegisterCallback<ClickEvent>(e =>
            AbrirDialogoConPregunta("¿Cuál es su actividad económica?",
            "Mi actividad es el comercio de productos importados al por mayor."));

        root.Q<VisualElement>("field-pep").RegisterCallback<ClickEvent>(e =>
            AbrirDialogoConPregunta("¿Es usted PEP o familiar de PEP?",
            "No, no tengo ninguna vinculación con cargos políticos."));

        root.Q<VisualElement>("field-rut").RegisterCallback<ClickEvent>(e =>
            AbrirDialogoConPregunta("¿Puede confirmar su RUT?",
            "Sí, mi RUT es 12.345.678-9, puede verificarlo en mi cédula."));

        root.Q<VisualElement>("field-nombre").RegisterCallback<ClickEvent>(e =>
            AbrirDialogoConPregunta("¿Puede confirmar su nombre completo?",
            "Sí, soy Juan Pérez Silva."));
    }

    void AbrirDialogoConPregunta(string pregunta, string respuesta)
    {
        dialogoRespuesta.text = $"❓ {pregunta}\n\n💬 {respuesta}";
        dialogoPanel.RemoveFromClassList("hidden");
        hintCliente.AddToClassList("hidden");
        dialogoAbierto = true;
    }

    void ResponderPregunta(string campo, string respuesta)
    {
        dialogoRespuesta.text = $"💬 {respuesta}";

        // Verificar si esta respuesta activa una señal de alerta
        var caso = caseManager?.CasoActual;
        if (caso?.señalesInteraccion != null &&
            caso.señalesInteraccion.Contains(campo))
        {
            // Destacar el campo con alerta
            var root = uiDocument.rootVisualElement;
            root.Q<VisualElement>($"field-{campo}")?.AddToClassList("field-alerta");

            // Agregar al panel de discrepancias
            AgregarDiscrepanciaInteraccion(caso, campo);
        }
    }

    void AgregarDiscrepanciaInteraccion(CasoData caso, string campo)
    {
        var root = uiDocument.rootVisualElement;
        var contenedor = root.Q<VisualElement>("discrepancias-container");
        if (contenedor == null) return;

        string mensaje = campo switch
        {
            "actividad" => "⚠ Actividad declarada no concuerda con perfil",
            "origen_fondos" => "⚠ Origen de fondos inconsistente",
            "esPEP" => "⚠ Cliente confirma ser PEP — EDD obligatorio",
            "cuentasExtranjero" => "⚠ Cuenta en extranjero no declarada previamente",
            _ => $"⚠ Alerta detectada: {campo}"
        };

        var label = new Label(mensaje);
        label.AddToClassList("discrepancia-item");
        contenedor.Add(label);
    }

    void CerrarDialogo()
    {
        dialogoPanel.AddToClassList("hidden");
        hintCliente.RemoveFromClassList("hidden");
        dialogoAbierto = false;
    }

    void DetectarClicCliente()
    {
        if (clienteObject == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.IsChildOf(clienteObject.transform) || hit.transform == clienteObject.transform)
            {
                AbrirDialogo();
            }
        }
    }

    void AbrirDialogo()
    {
        dialogoRespuesta.text = "El cliente te mira expectante. ¿Qué deseas preguntar?";
        dialogoPanel.RemoveFromClassList("hidden");
        hintCliente.AddToClassList("hidden");
        dialogoAbierto = true;
    }

    void ConfigurarBotones()
    {
        btnAprobar.clicked  += () => TomarDecision("APROBADO");
        btnEscalar.clicked  += () => TomarDecision("ESCALADO");
        btnRechazar.clicked += () => TomarDecision("RECHAZADO");
    }

    void ConfigurarTabs()
    {
        tabIdentidad.clicked     += () => ActivarTab("tab-identidad");
        tabTransacciones.clicked += () => ActivarTab("tab-transacciones");
        tabHistorial.clicked     += () => ActivarTab("tab-historial");
        tabSanciones.clicked     += () => ActivarTab("tab-sanciones");
    }

    void ActivarTab(string tabActivo)
    {
        var root = uiDocument.rootVisualElement;
        foreach (var tab in new[] { tabIdentidad, tabTransacciones, tabHistorial, tabSanciones })
            tab.RemoveFromClassList("tab-active");
        root.Q<Button>(tabActivo).AddToClassList("tab-active");
    }

    void TomarDecision(string decision)
    {
        if (!juegoActivo) return;

        bool correcto = caseManager?.ValidarDecision(decision) ?? false;
        CasoData caso = caseManager?.CasoActual;

        // Puntaje y Multiplicador
        if (correcto)
        {
            rachaCorrectas++;
            if (rachaCorrectas > rachaMaxima) rachaMaxima = rachaCorrectas;
            
            multiplicador = 1.0f + (rachaCorrectas - 1) * 0.5f; // ×1, ×1.5, ×2, ×2.5...
            multiplicador = Mathf.Min(multiplicador, 3.0f);       // Cap en ×3
            
            int puntos = decision switch
            {
                "APROBADO" => 200,
                "RECHAZADO" => 200,
                "ESCALADO" => 150,
                _ => 100
            };
            
            int puntosConMulti = Mathf.RoundToInt(puntos * multiplicador);
            AgregarPuntaje(puntosConMulti);
            OnMultiplicadorChanged?.Invoke(rachaCorrectas);
            
            AudioManager.Instance?.SFXAprobar();
        }
        else
        {
            rachaCorrectas = 0;
            multiplicador = 1.0f;
            OnMultiplicadorChanged?.Invoke(0);
            
            int penalizacion = decision switch
            {
                "APROBADO" when caso?.decisionCorrecta == "RECHAZADO" => -300,
                "APROBADO" when caso?.decisionCorrecta == "ESCALADO" => -150,
                "RECHAZADO" when caso?.decisionCorrecta == "APROBAR" => -100,
                _ => -100
            };
            AgregarPuntaje(penalizacion);
            AudioManager.Instance?.SFXRechazar();
        }

        // Feedback visual
        MostrarFeedbackDecision(decision);

        // Mostrar panel modal
        MostrarPanelFeedback(correcto, decision, caso);
    }

    void MostrarPanelFeedback(bool correcto, string decision, CasoData caso)
    {
        if (caso == null) return;

        if (correcto)
        {
            feedbackTitulo.text = "DECISION CORRECTA";
            feedbackTitulo.style.color = new UnityEngine.Color(0.2f, 0.8f, 0.4f);
            feedbackDecision.text = $"Decidiste: {decision}";
        }
        else
        {
            feedbackTitulo.text = "DECISION INCORRECTA";
            feedbackTitulo.style.color = new UnityEngine.Color(0.9f, 0.3f, 0.3f);
            feedbackDecision.text = $"Decidiste: {decision}  |  Correcto: {caso.decisionCorrecta}";
        }

        feedbackNormativa.text = caso.normativaAplicable;
        feedbackExplicacion.text = caso.explicacion;

        panelFeedback.RemoveFromClassList("hidden");
    }

    void CerrarFeedback()
    {
        panelFeedback.AddToClassList("hidden");
        mouseEnUI = false; // resetear explícitamente
        Invoke(nameof(SiguienteTurno), 0.1f);
    }

    void SiguienteTurno()
    {
        if (turnoActual >= turnoTotal)
        {
            if (puntaje >= 500)
                TerminarJornada(true, "Jornada exitosa");
            else
                TerminarJornada(false, "Puntaje insuficiente");
            return;
        }
        turnoActual++;
        OnTurnoChanged?.Invoke(turnoActual, turnoTotal);
        tiempoRestante = 60f;
        CerrarDialogo();
        CambiarPersonaje();
        if (cameraController != null)
            cameraController.CambiarVista(CameraController.VistaActiva.Cliente, true);
        labelEstado.text = "Pendiente de resolución";
        ActualizarUI();
        caseManager.SiguienteCaso();
    }

    void TerminarJornada(bool victoria, string mensaje)
    {
        juegoActivo = false;
        
        // Mostrar panel de resultado
        var root = uiDocument.rootVisualElement;
        var panelResultado = root.Q<VisualElement>("panel-resultado-jornada");
        
        if (panelResultado != null) {
            // Configurar contenido
            var titulo = panelResultado.Q<Label>("resultado-titulo");
            titulo.text = victoria ? "✓ JORNADA EXITOSA" : "✗ JORNADA FALLIDA";
            titulo.style.color = victoria 
                ? new Color(0.2f, 0.9f, 0.4f) 
                : new Color(0.9f, 0.3f, 0.3f);
            
            panelResultado.Q<Label>("resultado-mensaje").text = mensaje;
            panelResultado.Q<Label>("resultado-puntaje").text = $"{puntaje} pts";
            panelResultado.Q<Label>("resultado-racha-max").text = $"Racha máxima: {rachaMaxima}";
            
            // Botones
            panelResultado.Q<Button>("btn-reintentar").clicked += () => GameManager.Instance?.Reiniciar();
            panelResultado.Q<Button>("btn-menu").clicked += () => GameManager.Instance?.IrAInicio();
            
            // Mostrar
            panelResultado.RemoveFromClassList("hidden");
        }
        
        // Guardar puntaje
        GameManager.Instance?.SetPuntajeFinal(puntaje);
        
        OnJornadaTerminada?.Invoke(victoria);
    }

    void IrAResultados()
    {
        GameManager.Instance?.IrAResultados();
    }

    void ActualizarUI()
    {
        labelTurno.text   = $"TURNO {turnoActual} / {turnoTotal}";
        labelPuntaje.text = $"★ {puntaje} pts";
    }

    void ActualizarTiempo()
    {
        if (labelTiempo == null) return;
        int min = Mathf.FloorToInt(tiempoRestante / 60);
        int seg = Mathf.FloorToInt(tiempoRestante % 60);
        labelTiempo.text = $"⏱ {min:00}:{seg:00}";
    }




    public void CargarCaso(CasoData caso)
    {
        casoId.text = caso.id;
        casoTipo.text = $"{caso.tipo} — Onboarding";
        casoPrioridad.text = $"⚠ {caso.prioridad}";
        clienteNombre.text = caso.cliente.nombre;
        clienteRut.text = caso.cliente.rut;
        clienteNacionalidad.text = caso.cliente.nacionalidad;
        clienteActividad.text = caso.cliente.actividad;
        clientePep.text = caso.cliente.esPEP ? "⚠ PEP CONFIRMADO — EDD obligatorio" : "No declarado";

        pregunta1.text = "¿Cuál es su actividad económica?";
        pregunta2.text = "¿Cuál es el origen de sus fondos?";
        pregunta3.text = "¿Es usted PEP o familiar de PEP?";
        pregunta4.text = "¿Tiene cuentas en el extranjero?";

        notasAnalista.value = "";
        labelEstado.text = "Pendiente de resolución";
        CerrarDialogo();

        // Señales automáticas y checkboxes
        MostrarSeñalesAutomaticas(caso);
    }

    public void MostrarBotonReporte(ObjetoSospechoso obj)
    {
        // Por ahora solo log — implementación completa después
        Debug.Log($"[UI] Objeto listo para reportar: {obj.ObtenerNombreTipo()}");
    }

    public void MostrarDecisionSoborno(ObjetoSospechoso obj)
    {
        // Por ahora solo log — implementación del panel después
        Debug.Log($"[UI] Panel soborno: {obj.ObtenerNombreTipo()}");
    }

    void CambiarPersonaje()
    {
        Debug.Log($"CambiarPersonaje - indice actual: {indicePersonaje}, total: {personajes.Length}");

        if (personajes == null || personajes.Length == 0) return;

        if (clienteObject != null)
            clienteObject.SetActive(false);

        indicePersonaje = (indicePersonaje + 1) % personajes.Length;
        clienteObject = personajes[indicePersonaje];
        clienteObject.SetActive(true);

        Debug.Log($"CambiarPersonaje - nuevo personaje: {clienteObject.name}");
    }

    public void ActualizarVista(CameraController.VistaActiva vista)
    {
        var root = uiDocument.rootVisualElement;
        var panelExpediente = root.Q<VisualElement>("panel-expediente");
        var panelDocumentos = root.Q<VisualElement>("panel-documentos");
        var panelCentro = root.Q<VisualElement>("panel-centro");
        var topBar = root.Q<VisualElement>("top-bar");
        var bottomBar = root.Q<VisualElement>("bottom-bar");

        switch (vista)
        {
            case CameraController.VistaActiva.Cliente:
                // Vista general — solo barras visibles
                panelExpediente.AddToClassList("hidden");
                panelDocumentos.AddToClassList("hidden");
                panelCentro.RemoveFromClassList("hidden");
                topBar.RemoveFromClassList("hidden");
                bottomBar.RemoveFromClassList("hidden");
                break;

            case CameraController.VistaActiva.Monitor:
                // Vista monitor — mostrar documentos
                panelExpediente.AddToClassList("hidden");
                panelDocumentos.RemoveFromClassList("hidden");
                panelCentro.AddToClassList("hidden");
                topBar.RemoveFromClassList("hidden");
                bottomBar.RemoveFromClassList("hidden");
                break;

            case CameraController.VistaActiva.Notepad:
                // Vista notepad — mostrar expediente
                panelExpediente.RemoveFromClassList("hidden");
                panelDocumentos.AddToClassList("hidden");
                panelCentro.AddToClassList("hidden");
                topBar.RemoveFromClassList("hidden");
                bottomBar.RemoveFromClassList("hidden");
                break;
        }
    }

    void MostrarFeedbackDecision(string decision)
    {
        // Configurar colores y texto según decisión
        Color colorFlash;
        string textoSello;
        string claseSello;

        switch (decision)
        {
            case "APROBADO":
                colorFlash = new Color(0f, 0.8f, 0.3f, 0.3f);
                textoSello = "✓ APROBADO";
                claseSello = "sello-aprobado";
                break;
            case "RECHAZADO":
                colorFlash = new Color(0.9f, 0.2f, 0.2f, 0.3f);
                textoSello = "✗ RECHAZADO";
                claseSello = "sello-rechazado";
                break;
            default: // ESCALADO
                colorFlash = new Color(0.2f, 0.5f, 1f, 0.3f);
                textoSello = "↑ ESCALADO";
                claseSello = "sello-escalado";
                break;
        }

        // Aplicar sello
        selloTexto.text = textoSello;
        selloTexto.RemoveFromClassList("sello-aprobado");
        selloTexto.RemoveFromClassList("sello-escalado");
        selloTexto.RemoveFromClassList("sello-rechazado");
        selloTexto.AddToClassList(claseSello);

        // Mostrar flash y sello
        flashPantalla.style.backgroundColor = colorFlash;
        flashPantalla.RemoveFromClassList("hidden");
        selloContainer.RemoveFromClassList("hidden");

        StartCoroutine(OcultarFeedback());
    }

    System.Collections.IEnumerator OcultarFeedback()
    {
        yield return new WaitForSeconds(0.8f);

        // Ocultar sello
        selloContainer.AddToClassList("hidden");

        // Fade out del flash
        float t = 0f;
        Color colorBase = flashPantalla.style.backgroundColor.value;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0.3f, 0f, t / 0.3f);
            flashPantalla.style.backgroundColor = new Color(
                colorBase.r, colorBase.g, colorBase.b, alpha);
            yield return null;
        }
        flashPantalla.AddToClassList("hidden");
    }


    void MostrarSeñalesAutomaticas(CasoData caso)
    {
        var root = uiDocument.rootVisualElement;

        // Resetear estilos de campos
        root.Q<VisualElement>("field-pep")?.RemoveFromClassList("field-alerta");
        root.Q<VisualElement>("field-actividad")?.RemoveFromClassList("field-alerta");
        root.Q<VisualElement>("field-nombre")?.RemoveFromClassList("field-alerta");

        // PEP automático
        if (caso.cliente.esPEP)
        {
            root.Q<VisualElement>("field-pep")?.AddToClassList("field-alerta");
            clientePep.text = "⚠ PEP CONFIRMADO — EDD obligatorio";
        }

        // Discrepancias automáticas en documentos
        if (caso.señalesAutomaticas != null)
        {
            foreach (string señal in caso.señalesAutomaticas)
            {
                switch (señal)
                {
                    case "fotoCoincide":
                        root.Q<VisualElement>("field-nombre")?.AddToClassList("field-alerta");
                        break;
                    case "domicilioVerificado":
                        // marca en checklist — se hace en los toggles
                        break;
                    case "actividadConcuerda":
                        root.Q<VisualElement>("field-actividad")?.AddToClassList("field-alerta");
                        break;
                }
            }
        }

        // Mostrar señales en panel de discrepancias
        var contenedor = root.Q<VisualElement>("discrepancias-container");
        if (contenedor != null)
        {
            contenedor.Clear();
            if (caso.señalesAlerta != null && caso.señalesAlerta.Count > 0)
            {
                foreach (string alerta in caso.señalesAlerta)
                {
                    var label = new Label($"⚠ {alerta}");
                    label.AddToClassList("discrepancia-item");
                    contenedor.Add(label);
                }
            }
            else
            {
                var label = new Label("Sin discrepancias detectadas");
                label.AddToClassList("discrepancia-hint");
                contenedor.Add(label);
            }
        }

        // Marcar checkboxes según documentos
        ActualizarCheckboxes(caso);
    }

    void ActualizarCheckboxes(CasoData caso)
    {
        var root = uiDocument.rootVisualElement;
        root.Q<Toggle>("check-ci")?.SetValueWithoutNotify(caso.documentos.cedulaVigente);
        root.Q<Toggle>("check-foto")?.SetValueWithoutNotify(caso.documentos.fotoCoincide);
        root.Q<Toggle>("check-rut")?.SetValueWithoutNotify(caso.documentos.rutValido);
        root.Q<Toggle>("check-domicilio")?.SetValueWithoutNotify(caso.documentos.domicilioVerificado);
        root.Q<Toggle>("check-actividad")?.SetValueWithoutNotify(caso.documentos.actividadConcuerda);
    }



    void ConfigurarHUDFeedback() {
        // Suscribirse a eventos propios (Observer)
        OnPuntajeChanged += AnimarCambioPuntaje;
        OnMultiplicadorChanged += AnimarMultiplicador;
        OnTiempoChanged += AnimarTiempoUrgente;
        OnTurnoChanged += ActualizarProgreso;
    }

    void ActualizarProgreso(int turnoAct, int turnoTot) {
        var root = uiDocument.rootVisualElement;
        var progresoFill = root.Q<VisualElement>("progreso-fill");
        if (progresoFill != null) {
            float porcentaje = (float)(turnoAct - 1) / turnoTot * 100f;
            progresoFill.style.width = new StyleLength(Length.Percent(porcentaje));
        }
    }

    void AnimarCambioPuntaje(int nuevoPuntaje, int delta) {
        if (delta > 0) {
            labelPuntaje.AddToClassList("puntaje-subio");
            StartCoroutine(RemoverClaseDespues(labelPuntaje, "puntaje-subio", 0.5f));
        } else if (delta < 0) {
            labelPuntaje.AddToClassList("puntaje-bajo");
            StartCoroutine(RemoverClaseDespues(labelPuntaje, "puntaje-bajo", 0.5f));
        }
    }

    System.Collections.IEnumerator RemoverClaseDespues(VisualElement element, string className, float delay) {
        yield return new WaitForSeconds(delay);
        element.RemoveFromClassList(className);
    }

    void AnimarMultiplicador(int racha) {
        var root = uiDocument.rootVisualElement;
        var multiplicadorContainer = root.Q<VisualElement>("multiplicador-container");
        var labelMulti = root.Q<Label>("label-multiplicador");
        var labelR = root.Q<Label>("label-racha");

        if (multiplicadorContainer == null) return;

        if (racha >= 2) {
            multiplicadorContainer.RemoveFromClassList("hidden");
            labelMulti.text = $"×{multiplicador:F1}";
            labelR.text = $"RACHA: {racha}";
            multiplicadorContainer.AddToClassList("multiplicador-activo");
            StartCoroutine(RemoverClaseDespues(multiplicadorContainer, "multiplicador-activo", 0.3f));
        } else {
            multiplicadorContainer.AddToClassList("hidden");
        }
    }

    void AnimarTiempoUrgente(float tiempo) {
        if (labelTiempo == null) return;
        if (tiempo <= 15f && tiempo > 0)
            labelTiempo.AddToClassList("tiempo-urgente");
        else
            labelTiempo.RemoveFromClassList("tiempo-urgente");
    }

    public void AgregarPuntaje(int puntos)
    {
        int anterior = puntaje;
        puntaje += puntos;
        if (puntaje < 0) puntaje = 0;
        
        OnPuntajeChanged?.Invoke(puntaje, puntos);
        ActualizarUI();
        
        // Condición de derrota por puntaje en turnos avanzados o si llegó a 0 con errores
        if (puntaje <= 0 && turnoActual > 1 && puntos < 0) {
            TerminarJornada(false, "Demasiados errores de compliance");
        }
    }

    // ===================== LOGROS =====================
    void AbrirPanelLogros()
    {
        if (panelLogros == null || grillaLogros == null) return;
        panelLogros.RemoveFromClassList("hidden");
        mouseEnUI = true;
        ActualizarGrillaLogros();
    }

    void CerrarPanelLogros()
    {
        if (panelLogros != null) panelLogros.AddToClassList("hidden");
        mouseEnUI = false;
    }

    void ActualizarGrillaLogros()
    {
        grillaLogros.Clear();

        foreach (ObjetoSospechoso.TipoObjeto tipo in System.Enum.GetValues(typeof(ObjetoSospechoso.TipoObjeto)))
        {
            var itemContainer = new VisualElement();
            itemContainer.AddToClassList("item-logro");

            bool desbloqueado = AchievementManager.Instance != null && AchievementManager.Instance.TieneLogro(tipo);

            if (desbloqueado)
            {
                itemContainer.AddToClassList("logro-desbloqueado");
            }
            else
            {
                itemContainer.AddToClassList("logro-bloqueado");
            }

            var icono = new Label(ObtenerIconoPorTipo(tipo));
            icono.AddToClassList("logro-icono");

            var texto = new Label(desbloqueado ? tipo.ToString() : "???");
            texto.AddToClassList("logro-texto");

            itemContainer.Add(icono);
            itemContainer.Add(texto);
            grillaLogros.Add(itemContainer);
        }
    }

    string ObtenerIconoPorTipo(ObjetoSospechoso.TipoObjeto tipo)
    {
        return tipo switch
        {
            ObjetoSospechoso.TipoObjeto.Pendrive => "💾",
            ObjetoSospechoso.TipoObjeto.Celular => "📱",
            ObjetoSospechoso.TipoObjeto.PostIt => "📝",
            ObjetoSospechoso.TipoObjeto.Llaves => "🔑",
            ObjetoSospechoso.TipoObjeto.Credencial => "🪪",
            ObjetoSospechoso.TipoObjeto.Carpeta => "📁",
            ObjetoSospechoso.TipoObjeto.Documento => "📄",
            ObjetoSospechoso.TipoObjeto.Tarjeta => "💳",
            ObjetoSospechoso.TipoObjeto.SobreEfectivo => "✉",
            ObjetoSospechoso.TipoObjeto.CajaRegalo => "🎁",
            _ => "❓"
        };
    }

    void MostrarToastLogro(ObjetoSospechoso.TipoObjeto tipo)
    {
        if (toastLogro == null || toastLogroNombre == null) return;
        
        toastLogroNombre.text = $"{ObtenerIconoPorTipo(tipo)} {tipo.ToString()}";
        toastLogro.RemoveFromClassList("hidden");
        
        // Ocultar después de 3 segundos
        StartCoroutine(RemoverClaseDespues(toastLogro, "hidden", 3f));
        // Pero RemoverClaseDespues remueve. Necesito una para añadir.
        StartCoroutine(OcultarToastLogro(3f));
    }

    System.Collections.IEnumerator OcultarToastLogro(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (toastLogro != null) toastLogro.AddToClassList("hidden");
    }
}
