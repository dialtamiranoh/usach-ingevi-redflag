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
    private Label labelNivel;

    // Alerta de ascenso de nivel
    private VisualElement alertaNivel;
    private Label alertaTitulo;
    private Label alertaSubtitulo;
    private Label alertaInstruccion;
    private Button btnAlertaContinuar;

    // Alerta de coordinación
    private VisualElement alertaCoordinacion;
    private Button btnAlertaCoordinacionEntendido;

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
    private Button btnCerrarLogros;    private VisualElement toastLogro;
    private Label toastLogroNombre;

    // Tabs
    private Button tabIdentidad, tabTransacciones, tabHistorial, tabSanciones;

    // Nuevos Elementos para AML, Supervisor y ROS
    private VisualElement contentIdentidad, contentTransacciones, contentHistorial, contentSanciones;
    private VisualElement panelRosUaf, rosDropzone;
    private Label labelBitacoraJunior, rosDropzoneLabel, rosLeySeleccionada;
    private Button btnLey19913, btnLey20393, btnLey21521, btnRosCancelar, btnRosEnviar;
    private ScrollView transaccionesScroll, sancionesUboContainer, rosTransaccionesScroll;
    private string leySeleccionadaUaf = "";
    private string evidenciaAdjuntaRos = "";
    private Label hintRosDrag;
    private bool haRevisadoAmbosEscritorios = false;

    // Alerta de Ley
    private VisualElement alertaLey;
    private Label alertaLeyTitulo;
    private Label alertaLeySubtitulo;
    private Label alertaLeyDescripcion;
    private Button btnAlertaLeyCancelar;
    private Button btnAlertaLeyConfirmar;
    // Estado
    private int turnoActual = 1;
    private int turnoTotal = 12;
    private int puntaje = 0;
    private float tiempoRestante = 60f;
    private bool juegoActivo = true;
    private bool dialogoAbierto = false;
    
    // Multiplicador y Racha
    private int rachaCorrectas = 0;
    private float multiplicador = 1.0f;
    private int rachaMaxima = 0;

    // Gerente de Sucursal NPC
    private int gerentesRetirados = 0;
    private Label labelGerenteContador; // se enlaza en InicializarElementos si existe en el UXML

    // Eventos para patrón Observer
    public event System.Action<int, int> OnPuntajeChanged;     // (nuevoPuntaje, delta)
    public event System.Action<int, int> OnTurnoChanged;       // (turnoActual, turnoTotal)
    public event System.Action<float> OnTiempoChanged;         // tiempoRestante
    public event System.Action<bool> OnJornadaTerminada;       // true=victoria, false=derrota
    public event System.Action<int> OnMultiplicadorChanged;    // nuevoMultiplicador

    void OnEnable()
    {
        RoleManager.OnRolCambiado += AlCambiarRol;
        RoleManager.OnNivelCambiado += AlCambiarNivel;
        GerenteNPC_Health.OnGerenteMuerto += OnGerenteRetirado;
    }

    void OnDisable()
    {
        RoleManager.OnRolCambiado -= AlCambiarRol;
        RoleManager.OnNivelCambiado -= AlCambiarNivel;
        GerenteNPC_Health.OnGerenteMuerto -= OnGerenteRetirado;
    }

    private void AlCambiarNivel(RoleManager.NivelJuego nuevoNivel)
    {
        ConfigurarUIPorNivelYRol();
    }

    private void AlCambiarRol(RoleManager.RolAnalista nuevoRol)
    {
        haRevisadoAmbosEscritorios = true;
        ConfigurarUIPorNivelYRol();
    }

    private void ConfigurarUIPorNivelYRol()
    {
        if (RoleManager.Instance == null) return;
        
        var nivel = RoleManager.Instance.nivelActual;
        var rol = RoleManager.Instance.rolActivo;

        // Mostrar/ocultar el hint de arrastrar transacción para ROS
        if (hintRosDrag != null)
        {
            if (nivel == RoleManager.NivelJuego.Nivel3)
                hintRosDrag.RemoveFromClassList("hidden");
            else
                hintRosDrag.AddToClassList("hidden");
        }

        // Resetear visibilidad de pestañas
        if (tabIdentidad != null) tabIdentidad.RemoveFromClassList("hidden");
        if (tabTransacciones != null) tabTransacciones.RemoveFromClassList("hidden");
        if (tabHistorial != null) tabHistorial.RemoveFromClassList("hidden");
        if (tabSanciones != null) tabSanciones.RemoveFromClassList("hidden");
        
        if (hintCliente != null) hintCliente.RemoveFromClassList("hidden");

        if (nivel == RoleManager.NivelJuego.Nivel1)
        {
            // Nivel 1: Solo KYC
            ActivarTab("tab-identidad");
            if (tabTransacciones != null) tabTransacciones.AddToClassList("hidden");
            if (tabHistorial != null) tabHistorial.AddToClassList("hidden");
            if (tabSanciones != null) tabSanciones.AddToClassList("hidden");
        }
        else if (nivel == RoleManager.NivelJuego.Nivel2)
        {
            // Nivel 2: Multi-rol
            if (tabHistorial != null) tabHistorial.AddToClassList("hidden");
            if (tabSanciones != null) tabSanciones.AddToClassList("hidden");
            
            if (rol == RoleManager.RolAnalista.Analista_KYC)
            {
                ActivarTab("tab-identidad");
                if (tabTransacciones != null) tabTransacciones.AddToClassList("hidden"); // Ocultar al KYC
            }
            else
            {
                ActivarTab("tab-transacciones");
                if (tabIdentidad != null) tabIdentidad.AddToClassList("hidden"); // Ocultar al AML
                if (hintCliente != null) hintCliente.AddToClassList("hidden"); // AML no interroga directamente
                CerrarDialogo();
            }
        }
        else if (nivel == RoleManager.NivelJuego.Nivel3)
        {
            // Nivel 3: Supervisor
            ActivarTab("tab-historial");
        }
    }

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
        labelNivel    = root.Q<Label>("label-nivel");

        // Alerta de ascenso
        alertaNivel       = root.Q<VisualElement>("alerta-nivel");
        alertaTitulo      = root.Q<Label>("alerta-nivel-titulo");
        alertaSubtitulo   = root.Q<Label>("alerta-nivel-subtitulo");
        alertaInstruccion = root.Q<Label>("alerta-nivel-instruccion");
        btnAlertaContinuar = root.Q<Button>("btn-alerta-continuar");
        if (btnAlertaContinuar != null)
            btnAlertaContinuar.clicked += CerrarAlertaNivel;

        // Alerta de coordinación
        alertaCoordinacion = root.Q<VisualElement>("alerta-coordinacion");
        btnAlertaCoordinacionEntendido = root.Q<Button>("btn-alerta-coordinacion-entendido");
        if (btnAlertaCoordinacionEntendido != null)
            btnAlertaCoordinacionEntendido.clicked += CerrarAlertaCoordinacion;

        ActualizarLabelNivel();

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

        // Inicializar nuevos elementos para AML, Supervisor y ROS
        contentTransacciones = root.Q<VisualElement>("content-transacciones");
        contentHistorial     = root.Q<VisualElement>("content-historial");
        contentSanciones     = root.Q<VisualElement>("content-sanciones");
        
        panelRosUaf          = root.Q<VisualElement>("panel-ros-uaf");
        rosDropzone          = root.Q<VisualElement>("ros-dropzone");
        rosDropzoneLabel     = root.Q<Label>("ros-dropzone-label");
        rosLeySeleccionada   = root.Q<Label>("ros-ley-seleccionada");
        
        btnLey19913          = root.Q<Button>("btn-ley-19913");
        btnLey20393          = root.Q<Button>("btn-ley-20393");
        btnLey21521          = root.Q<Button>("btn-ley-21521");
        btnRosCancelar       = root.Q<Button>("btn-ros-cancelar");
        btnRosEnviar         = root.Q<Button>("btn-ros-enviar");
        
        transaccionesScroll  = root.Q<ScrollView>("transacciones-scroll");
        sancionesUboContainer = root.Q<ScrollView>("sanciones-ubo-container");
        labelBitacoraJunior  = root.Q<Label>("label-bitacora-junior");
        hintRosDrag          = root.Q<Label>("hint-ros-drag");
        rosTransaccionesScroll = root.Q<ScrollView>("ros-transacciones-scroll");

        // Alerta de Ley
        alertaLey            = root.Q<VisualElement>("alerta-ley");
        alertaLeyTitulo      = root.Q<Label>("alerta-ley-titulo");
        alertaLeySubtitulo   = root.Q<Label>("alerta-ley-subtitulo");
        alertaLeyDescripcion = root.Q<Label>("alerta-ley-descripcion");
        btnAlertaLeyCancelar = root.Q<Button>("btn-alerta-ley-cancelar");
        btnAlertaLeyConfirmar = root.Q<Button>("btn-alerta-ley-confirmar");

        if (btnAlertaLeyCancelar != null)
            btnAlertaLeyCancelar.clicked += CerrarAlertaLey;
        
        ConfigurarBotonLeyes();
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
        {
            if (tab != null) tab.RemoveFromClassList("tab-active");
        }
        
        var selectedTab = root.Q<Button>(tabActivo);
        if (selectedTab != null) selectedTab.AddToClassList("tab-active");

        // Ocultar todas las secciones
        if (contentIdentidad == null) contentIdentidad = root.Q<VisualElement>("content-identidad");
        
        if (contentIdentidad != null) contentIdentidad.AddToClassList("hidden");
        if (contentTransacciones != null) contentTransacciones.AddToClassList("hidden");
        if (contentHistorial != null) contentHistorial.AddToClassList("hidden");
        if (contentSanciones != null) contentSanciones.AddToClassList("hidden");

        // Mostrar la seleccionada
        string contentName = tabActivo switch
        {
            "tab-identidad" => "content-identidad",
            "tab-transacciones" => "content-transacciones",
            "tab-historial" => "content-historial",
            "tab-sanciones" => "content-sanciones",
            _ => "content-identidad"
        };

        var targetContent = root.Q<VisualElement>(contentName);
        if (targetContent != null) targetContent.RemoveFromClassList("hidden");
    }

    void TomarDecision(string decision)
    {
        if (!juegoActivo) return;

        // En Nivel 2, es obligatorio coordinar consultando ambos escritorios (presionando W) antes de decidir
        if (RoleManager.Instance != null && RoleManager.Instance.nivelActual == RoleManager.NivelJuego.Nivel2 && !haRevisadoAmbosEscritorios)
        {
            if (alertaCoordinacion != null)
            {
                alertaCoordinacion.RemoveFromClassList("hidden");
                alertaCoordinacion.style.opacity = 1f;
            }
            AudioManager.Instance?.SFXRechazar();
            return;
        }

        // Interceptar si es Nivel 3 y es un rechazo para exigir el formulario de ROS
        if (RoleManager.Instance != null && RoleManager.Instance.nivelActual == RoleManager.NivelJuego.Nivel3 && decision == "RECHAZADO")
        {
            AbrirFormularioROS();
            return;
        }

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

            // Verificar ascenso de nivel si se logra racha de 3
            ChequearProgresoNivel();
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
        
        // El Nivel 3 de supervisor otorga 90 segundos por caso debido a la mayor complejidad de auditar UBO y transacciones
        if (RoleManager.Instance != null && RoleManager.Instance.nivelActual == RoleManager.NivelJuego.Nivel3)
        {
            tiempoRestante = 90f;
        }
        else
        {
            tiempoRestante = 60f;
        }
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
        
        // Guardar puntaje antes de mostrar panel
        GameManager.Instance?.SetPuntajeFinal(puntaje);
        
        // Mostrar panel de resultado
        var root = uiDocument.rootVisualElement;
        var panelResultado = root.Q<VisualElement>("panel-resultado-jornada");
        
        if (panelResultado != null) {
            // Configurar contenido
            var titulo = panelResultado.Q<Label>("resultado-titulo");
            titulo.text = victoria ? "🌟 JORNADA EXITOSA" : "💔 JORNADA FALLIDA";
            titulo.style.color = victoria 
                ? new Color(0.2f, 0.9f, 0.4f) 
                : new Color(0.9f, 0.3f, 0.3f);
            
            panelResultado.Q<Label>("resultado-mensaje").text = mensaje;
            panelResultado.Q<Label>("resultado-puntaje").text = $"{puntaje} pts";
            panelResultado.Q<Label>("resultado-racha-max").text = $"Racha máxima: {rachaMaxima}";
            
            // Botones
            panelResultado.Q<Button>("btn-reintentar").clicked += () => GameManager.Instance?.Reiniciar();
            panelResultado.Q<Button>("btn-menu").clicked += () => GameManager.Instance?.IrAResultados();
            
            // Mostrar
            panelResultado.RemoveFromClassList("hidden");
        }
        
        OnJornadaTerminada?.Invoke(victoria);
        
        // Auto-transición a SceneResultados después de 8 segundos
        StartCoroutine(TransicionAResultados(8f));
    }

    System.Collections.IEnumerator TransicionAResultados(float espera)
    {
        yield return new WaitForSeconds(espera);
        GameManager.Instance?.IrAResultados();
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
        haRevisadoAmbosEscritorios = false;
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

        // Poblar las transacciones y socios del nivel/caso
        PoblarTransaccionesYSocios(caso);

        // Actualizar visibilidad de pestañas y botones según nivel/rol
        ConfigurarUIPorNivelYRol();
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

    // ===================== GERENTE DE SUCURSAL NPC =====================

    /// <summary>
    /// Reinicia la racha y el multiplicador. Llamado por GerenteNPC_FSM cuando el jugador falla.
    /// </summary>
    public void ResetRacha()
    {
        rachaCorrectas = 0;
        multiplicador = 1.0f;
        OnMultiplicadorChanged?.Invoke(0);
        Debug.Log("[UIManager] Racha reseteada por el Gerente de Sucursal.");
    }

    /// <summary>
    /// Actualiza el label HUD de gerentes retirados (si existe en el UXML).
    /// </summary>
    public void ActualizarContadorGerente(int retirados)
    {
        gerentesRetirados = retirados;
        if (labelGerenteContador != null)
            labelGerenteContador.text = $"Gerente retirado: {gerentesRetirados}";
    }

    /// <summary>
    /// Callback del evento GerenteNPC_Health.OnGerenteMuerto.
    /// Muestra toast en HUD y actualiza contador.
    /// </summary>
    private void OnGerenteRetirado()
    {
        gerentesRetirados++;
        ActualizarContadorGerente(gerentesRetirados);
        MostrarToastTexto($"🚩 Gerente retirado ({gerentesRetirados})");
        Debug.Log($"[UIManager] Gerente de Sucursal retirado. Total: {gerentesRetirados}");
    }

    /// <summary>
    /// Muestra un toast con cualquier texto. Reutiliza los elementos visuales del toast de logros.
    /// </summary>
    public void MostrarToastTexto(string mensaje)
    {
        if (toastLogro == null || toastLogroNombre == null) return;
        toastLogroNombre.text = mensaje;
        toastLogro.RemoveFromClassList("hidden");
        StartCoroutine(OcultarToastLogro(3f));
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

    // ===================== AML / SUPERVISOR / ROS HELPERS =====================
    void PoblarTransaccionesYSocios(CasoData caso)
    {
        if (transaccionesScroll != null)
        {
            transaccionesScroll.Clear();
            if (caso.transacciones != null)
            {
                foreach (var tx in caso.transacciones)
                {
                    VisualElement row = new VisualElement();
                    row.AddToClassList("transaccion-row");
                    
                    Label lblFecha = new Label(tx.fecha);
                    lblFecha.style.fontSize = 11;
                    lblFecha.style.color = new Color(0.7f, 0.7f, 0.7f);
                    row.Add(lblFecha);
                    
                    Label lblDesc = new Label(tx.descripcion);
                    lblDesc.style.fontSize = 12;
                    lblDesc.style.color = Color.white;
                    row.Add(lblDesc);
                    
                    Label lblMonto = new Label((tx.monto > 0 ? "+" : "") + tx.monto.ToString("N0") + " CLP");
                    lblMonto.AddToClassList("transaccion-monto");
                    lblMonto.AddToClassList(tx.monto < 0 ? "debit" : "credit");
                    row.Add(lblMonto);

                    // Hacer clickeable para adjuntar evidencia al ROS en Nivel 3
                    row.RegisterCallback<ClickEvent>(evt => {
                        if (RoleManager.Instance != null && RoleManager.Instance.nivelActual == RoleManager.NivelJuego.Nivel3)
                        {
                            evidenciaAdjuntaRos = $"{tx.descripcion} ({tx.monto:N0} CLP)";
                            if (rosDropzoneLabel != null)
                            {
                                rosDropzoneLabel.text = $"✓ Evidencia: {evidenciaAdjuntaRos}";
                                rosDropzoneLabel.style.color = new Color(0.2f, 0.9f, 0.4f);
                            }
                            Debug.Log($"[ROS] Evidencia adjuntada: {evidenciaAdjuntaRos}");
                        }
                    });

                    transaccionesScroll.Add(row);
                }
            }
        }

        if (sancionesUboContainer != null)
        {
            sancionesUboContainer.Clear();
            if (caso.estructuraSocietaria != null)
            {
                foreach (var socio in caso.estructuraSocietaria)
                {
                    VisualElement row = new VisualElement();
                    row.AddToClassList("ubo-row");
                    
                    Label lblNombre = new Label($"{socio.nombre} (RUT: {socio.rut})");
                    lblNombre.style.fontSize = 12;
                    lblNombre.style.color = Color.white;
                    row.Add(lblNombre);
                    
                    Label lblPorcentaje = new Label($"{socio.porcentajeParticipacion:F1}%");
                    lblPorcentaje.style.fontSize = 12;
                    lblPorcentaje.style.color = new Color(0f, 0.8f, 0.6f);
                    row.Add(lblPorcentaje);
                    
                    if (socio.esPEP)
                    {
                        Label alertPep = new Label("PEP");
                        alertPep.AddToClassList("ubo-pep-alert");
                        row.Add(alertPep);
                    }
                    
                    sancionesUboContainer.Add(row);
                }
            }
        }

        if (labelBitacoraJunior != null)
        {
            labelBitacoraJunior.text = string.IsNullOrEmpty(caso.notasJuniorEscalado) 
                ? "No hay registros previos de analistas." 
                : caso.notasJuniorEscalado;
        }
    }

    void ConfigurarBotonLeyes()
    {
        if (btnLey19913 != null) btnLey19913.clicked += () => MostrarDetalleLey("Ley 19.913", btnLey19913);
        if (btnLey20393 != null) btnLey20393.clicked += () => MostrarDetalleLey("Ley 20.393", btnLey20393);
        if (btnLey21521 != null) btnLey21521.clicked += () => MostrarDetalleLey("Ley 21.521", btnLey21521);
        
        if (btnRosCancelar != null) btnRosCancelar.clicked += CerrarFormularioROS;
        if (btnRosEnviar != null) btnRosEnviar.clicked += EnviarROS;
    }
    
    void SeleccionarLey(string ley, Button boton)
    {
        leySeleccionadaUaf = ley;
        if (rosLeySeleccionada != null)
        {
            rosLeySeleccionada.text = $"Fundamento: {ley}";
            rosLeySeleccionada.style.color = new Color(1f, 0.8f, 0f);
        }
        
        foreach (var btn in new[] { btnLey19913, btnLey20393, btnLey21521 })
        {
            if (btn != null) btn.RemoveFromClassList("btn-ley-selected");
        }
        if (boton != null) boton.AddToClassList("btn-ley-selected");
    }

    void AbrirFormularioROS()
    {
        if (panelRosUaf != null)
        {
            panelRosUaf.RemoveFromClassList("hidden");
            leySeleccionadaUaf = "";
            evidenciaAdjuntaRos = "";
            
            if (rosDropzoneLabel != null)
            {
                rosDropzoneLabel.text = "Selecciona una transacción de la lista anterior";
                rosDropzoneLabel.style.color = new Color(1f, 0.5f, 0.5f);
            }
            if (rosLeySeleccionada != null)
            {
                rosLeySeleccionada.text = "Selecciona una ley aplicable";
                rosLeySeleccionada.style.color = Color.white;
            }
            
            foreach (var btn in new[] { btnLey19913, btnLey20393, btnLey21521 })
            {
                if (btn != null) btn.RemoveFromClassList("btn-ley-selected");
            }

            PoblarTransaccionesInternasROS();
        }
    }

    void CerrarFormularioROS()
    {
        if (panelRosUaf != null)
        {
            panelRosUaf.AddToClassList("hidden");
        }
    }

    void EnviarROS()
    {
        if (string.IsNullOrEmpty(leySeleccionadaUaf))
        {
            Debug.LogError("[ROS] Debes seleccionar una ley aplicable.");
            if (rosLeySeleccionada != null)
            {
                rosLeySeleccionada.text = "⚠️ SELECCIONA UNA LEY";
                rosLeySeleccionada.style.color = Color.red;
            }
            return;
        }

        if (string.IsNullOrEmpty(evidenciaAdjuntaRos))
        {
            Debug.LogError("[ROS] Debes adjuntar evidencia transaccional.");
            if (rosDropzoneLabel != null)
            {
                rosDropzoneLabel.text = "⚠️ DEBES SELECCIONAR UNA TRANSACCIÓN";
                rosDropzoneLabel.style.color = Color.red;
            }
            return;
        }

        CerrarFormularioROS();
        FinalizarDecisionConROS();
    }

    void FinalizarDecisionConROS()
    {
        CasoData caso = caseManager?.CasoActual;
        bool decisionCorrecta = caseManager?.ValidarDecision("RECHAZADO") ?? false;
        
        bool leyCorrecta = false;
        if (caso != null)
        {
            if (caso.normativaAplicable.Contains(leySeleccionadaUaf) || 
                (leySeleccionadaUaf == "Ley 19.913" && caso.normativaAplicable.Contains("19.913")) ||
                (leySeleccionadaUaf == "Ley 20.393" && caso.normativaAplicable.Contains("20.393")) ||
                (leySeleccionadaUaf == "Ley 21.521" && caso.normativaAplicable.Contains("21.521")))
            {
                leyCorrecta = true;
            }
        }

        // Validar si seleccionó la transacción sospechosa correcta del caso
        bool evidenciaCorrecta = false;
        if (caso != null)
        {
            if (string.IsNullOrEmpty(caso.transaccionSospechosaCorrecta) || 
                evidenciaAdjuntaRos.ToLower().Contains(caso.transaccionSospechosaCorrecta.ToLower()))
            {
                evidenciaCorrecta = true;
            }
        }

        bool todoCorrecto = decisionCorrecta && leyCorrecta && evidenciaCorrecta;

        if (todoCorrecto)
        {
            rachaCorrectas++;
            if (rachaCorrectas > rachaMaxima) rachaMaxima = rachaCorrectas;
            
            multiplicador = 1.0f + (rachaCorrectas - 1) * 0.5f;
            multiplicador = Mathf.Min(multiplicador, 3.0f);
            
            int puntosConMulti = Mathf.RoundToInt(250 * multiplicador);
            AgregarPuntaje(puntosConMulti);
            OnMultiplicadorChanged?.Invoke(rachaCorrectas);
            
            AudioManager.Instance?.SFXAprobar();

            // Verificar ascenso de nivel si se logra racha de 3
            ChequearProgresoNivel();
        }
        else
        {
            rachaCorrectas = 0;
            multiplicador = 1.0f;
            OnMultiplicadorChanged?.Invoke(0);
            
            int penalizacion = -150; 
            AgregarPuntaje(penalizacion);
            
            AudioManager.Instance?.SFXRechazar();
        }

        MostrarFeedbackDecision("RECHAZADO");
        
        if (caso != null)
        {
            if (!decisionCorrecta)
            {
                MostrarPanelFeedback(false, "RECHAZAR (con ROS)", caso);
            }
            else if (!leyCorrecta)
            {
                feedbackTitulo.text = "FUNDAMENTO ROS INCORRECTO";
                feedbackTitulo.style.color = new UnityEngine.Color(0.9f, 0.3f, 0.3f);
                feedbackDecision.text = $"Fundamento seleccionado: {leySeleccionadaUaf} | Requerido: {caso.normativaAplicable}";
                feedbackNormativa.text = caso.normativaAplicable;
                feedbackExplicacion.text = "El rechazo del caso era correcto, pero fundamentaste el ROS con una ley inaplicable. Debes estudiar la competencia de la Ley 19.913 (UAF), Ley 20.393 (Personas Jurídicas) y Ley 21.521 (Fintech).";
                panelFeedback.RemoveFromClassList("hidden");
            }
            else if (!evidenciaCorrecta)
            {
                feedbackTitulo.text = "EVIDENCIA ROS INCORRECTA";
                feedbackTitulo.style.color = new UnityEngine.Color(0.9f, 0.3f, 0.3f);
                feedbackDecision.text = $"Evidencia seleccionada no corresponde a la operación inusual del caso.";
                feedbackNormativa.text = caso.normativaAplicable;
                feedbackExplicacion.text = $"El rechazo y fundamento legal eran correctos, pero adjuntaste una transacción que no justifica el reporte.\n\nLa transacción sospechosa correcta del caso debe relacionarse con: \"{caso.transaccionSospechosaCorrecta}\".";
                panelFeedback.RemoveFromClassList("hidden");
            }
            else
            {
                feedbackTitulo.text = "ROS PERFECTO (CUMPLIMIENTO EXITOSO)";
                feedbackTitulo.style.color = new UnityEngine.Color(0.2f, 0.8f, 0.4f);
                feedbackDecision.text = $"Fundamento: {leySeleccionadaUaf} | Evidencia: {evidenciaAdjuntaRos}";
                feedbackNormativa.text = caso.normativaAplicable;
                feedbackExplicacion.text = $"¡Excelente! Rechazaste el caso y fundamentaste correctamente el ROS bajo la {leySeleccionadaUaf} adjuntando la transacción sospechosa correcta. Evidencia adjunta registrada en el expediente de la UAF.";
                panelFeedback.RemoveFromClassList("hidden");
            }
        }
    }

    void ChequearProgresoNivel()
    {
        if (RoleManager.Instance == null) return;

        var nivelActual = RoleManager.Instance.nivelActual;

        // Si el analista logra una racha de 3 casos correctos seguidos en Nivel 1, asciende a Nivel 2
        if (nivelActual == RoleManager.NivelJuego.Nivel1 && rachaCorrectas >= 3)
        {
            Debug.Log("[Progreso] ¡Racha de 3 correcta en Nivel 1! Ascendiendo a Nivel 2.");
            
            RoleManager.Instance.ConfigurarNivel(RoleManager.NivelJuego.Nivel2);
            
            rachaCorrectas = 0;
            multiplicador = 1.0f;
            OnMultiplicadorChanged?.Invoke(0);
            
            ActualizarLabelNivel();
            MostrarAlertaNivel(
                "⬆ ASCENSO",
                "Nivel 2 — Analista KYC / AML",
                "Los próximos casos requieren información de tu compañero (Analista 2).\nPresiona la tecla W para cambiar a su vista y consultar los datos de Back-Office."
            );
        }
        // Si logra una racha de 3 en Nivel 2, asciende a Nivel 3 (Supervisor)
        else if (nivelActual == RoleManager.NivelJuego.Nivel2 && rachaCorrectas >= 3)
        {
            Debug.Log("[Progreso] ¡Racha de 3 correcta en Nivel 2! Ascendiendo a Supervisor (Nivel 3).");
            
            RoleManager.Instance.ConfigurarNivel(RoleManager.NivelJuego.Nivel3);
            
            rachaCorrectas = 0;
            multiplicador = 1.0f;
            OnMultiplicadorChanged?.Invoke(0);
            
            ActualizarLabelNivel();
            MostrarAlertaNivel(
                "⭐ SUPERVISOR",
                "Nivel 3 — Supervisor Corporativo",
                "Como supervisor debes evaluar casos escalados por los analistas.\nTienes 90 segundos por caso y debes fundamentar los rechazos con un Reporte de Operación Sospechosa (ROS)."
            );
        }
    }

    void ActualizarLabelNivel()
    {
        if (labelNivel == null || RoleManager.Instance == null) return;

        string texto = RoleManager.Instance.nivelActual switch
        {
            RoleManager.NivelJuego.Nivel1 => "NIVEL 1 — ANALISTA KYC",
            RoleManager.NivelJuego.Nivel2 => "NIVEL 2 — KYC / AML",
            RoleManager.NivelJuego.Nivel3 => "NIVEL 3 — SUPERVISOR",
            _ => "NIVEL 1 — ANALISTA KYC"
        };
        labelNivel.text = texto;
    }

    void MostrarAlertaNivel(string titulo, string subtitulo, string instruccion = "")
    {
        if (alertaNivel == null) return;

        if (alertaTitulo != null) alertaTitulo.text = titulo;
        if (alertaSubtitulo != null) alertaSubtitulo.text = subtitulo;
        if (alertaInstruccion != null) alertaInstruccion.text = instruccion;

        alertaNivel.RemoveFromClassList("hidden");
        alertaNivel.style.opacity = 1f;
    }

    void CerrarAlertaNivel()
    {
        if (alertaNivel == null) return;
        StartCoroutine(OcultarAlertaNivel());
    }

    System.Collections.IEnumerator OcultarAlertaNivel()
    {
        float fadeTime = 0.4f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            if (alertaNivel != null)
                alertaNivel.style.opacity = 1f - (elapsed / fadeTime);
            yield return null;
        }

        if (alertaNivel != null)
        {
            alertaNivel.AddToClassList("hidden");
            alertaNivel.style.opacity = 1f;
        }
    }

    void CerrarAlertaCoordinacion()
    {
        if (alertaCoordinacion == null) return;
        StartCoroutine(OcultarAlertaCoordinacion());
    }

    System.Collections.IEnumerator OcultarAlertaCoordinacion()
    {
        float fadeTime = 0.3f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            if (alertaCoordinacion != null)
                alertaCoordinacion.style.opacity = 1f - (elapsed / fadeTime);
            yield return null;
        }

        if (alertaCoordinacion != null)
        {
            alertaCoordinacion.AddToClassList("hidden");
            alertaCoordinacion.style.opacity = 1f;
        }
    }

    void MostrarDetalleLey(string ley, Button boton)
    {
        if (alertaLey == null) return;

        if (alertaLeyTitulo != null) alertaLeyTitulo.text = $"⚠️ {ley.ToUpper()}";
        
        string desc = ley switch
        {
            "Ley 19.913" => "Regula la prevención y sanción del lavado de activos y financiamiento del terrorismo. Obliga a reportar operaciones sospechosas (ROS) a la UAF.\n\nCompetente para transacciones sospechosas sin justificación económica coherente (smurfing, fondos de origen desconocido, transferencias offshore).",
            "Ley 20.393" => "Establece la responsabilidad penal de las personas jurídicas (empresas) en los delitos de lavado de activos, financiamiento del terrorismo y cohecho.\n\nCompetente cuando el cliente es una empresa (SpA, Ltda., S.A.) que actúa como fachada o instrumento para encubrir fondos de origen ilícito.",
            "Ley 21.521" => "Regula los servicios de tecnología financiera (Ley Fintech), incluyendo plataformas de financiamiento colectivo, custodia y transacciones con criptomonedas.\n\nCompetente cuando las operaciones sospechosas involucran transacciones con criptoactivos o billeteras digitales reguladas por esta ley.",
            _ => "Descripción de la ley seleccionada."
        };

        if (alertaLeyDescripcion != null) alertaLeyDescripcion.text = desc;

        // Registrar acción de confirmar
        if (btnAlertaLeyConfirmar != null)
        {
            btnAlertaLeyConfirmar.clickable = new Clickable(() => {
                SeleccionarLey(ley, boton);
                CerrarAlertaLey();
            });
        }

        alertaLey.RemoveFromClassList("hidden");
        alertaLey.style.opacity = 1f;
    }

    void CerrarAlertaLey()
    {
        if (alertaLey == null) return;
        StartCoroutine(OcultarAlertaLey());
    }

    System.Collections.IEnumerator OcultarAlertaLey()
    {
        float fadeTime = 0.3f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            if (alertaLey != null)
                alertaLey.style.opacity = 1f - (elapsed / fadeTime);
            yield return null;
        }

        if (alertaLey != null)
        {
            alertaLey.AddToClassList("hidden");
            alertaLey.style.opacity = 1f;
        }
    }

    void PoblarTransaccionesInternasROS()
    {
        if (rosTransaccionesScroll == null) return;
        rosTransaccionesScroll.Clear();

        var caso = caseManager?.CasoActual;
        if (caso == null || caso.transacciones == null) return;

        foreach (var tx in caso.transacciones)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.paddingTop = 6;
            row.style.paddingBottom = 6;
            row.style.paddingLeft = 8;
            row.style.paddingRight = 8;
            row.style.borderBottomWidth = 1;
            row.style.borderBottomColor = new Color(1, 1, 1, 0.05f);
            row.style.borderTopLeftRadius = 4f;
            row.style.borderTopRightRadius = 4f;
            row.style.borderBottomLeftRadius = 4f;
            row.style.borderBottomRightRadius = 4f;
            row.style.marginBottom = 2;
            
            Label lblFecha = new Label(tx.fecha);
            lblFecha.style.fontSize = 11;
            lblFecha.style.color = new Color(0.6f, 0.6f, 0.6f);
            row.Add(lblFecha);
            
            Label lblDesc = new Label(tx.descripcion);
            lblDesc.style.fontSize = 11;
            lblDesc.style.color = Color.white;
            lblDesc.style.flexGrow = 1;
            lblDesc.style.marginLeft = 10;
            row.Add(lblDesc);
            
            Label lblMonto = new Label((tx.monto > 0 ? "+" : "") + tx.monto.ToString("N0") + " CLP");
            lblMonto.style.fontSize = 11;
            lblMonto.style.color = tx.monto < 0 ? new Color(1.0f, 0.3f, 0.3f) : new Color(0.2f, 0.8f, 0.4f);
            row.Add(lblMonto);

            // Click listener to select
            row.RegisterCallback<ClickEvent>(evt => {
                evidenciaAdjuntaRos = $"{tx.descripcion} ({tx.monto:N0} CLP)";
                if (rosDropzoneLabel != null)
                {
                    rosDropzoneLabel.text = $"✓ Seleccionado: {tx.descripcion} ({tx.monto:N0} CLP)";
                    rosDropzoneLabel.style.color = new Color(0.2f, 0.9f, 0.4f);
                }
                
                // Limpiar resaltado de otros
                foreach (var child in rosTransaccionesScroll.Children())
                {
                    child.style.backgroundColor = StyleKeyword.Null;
                    child.style.borderLeftWidth = StyleKeyword.Null;
                    child.style.borderRightWidth = StyleKeyword.Null;
                    child.style.borderTopWidth = StyleKeyword.Null;
                    child.style.borderBottomWidth = StyleKeyword.Null;
                    child.style.borderLeftColor = StyleKeyword.Null;
                    child.style.borderRightColor = StyleKeyword.Null;
                    child.style.borderTopColor = StyleKeyword.Null;
                    child.style.borderBottomColor = StyleKeyword.Null;
                }
                
                // Resaltar el seleccionado
                row.style.backgroundColor = new Color(0f, 0.82f, 0.62f, 0.15f);
                row.style.borderLeftWidth = 1f;
                row.style.borderRightWidth = 1f;
                row.style.borderTopWidth = 1f;
                row.style.borderBottomWidth = 1f;
                row.style.borderLeftColor = new Color(0f, 0.82f, 0.62f, 0.5f);
                row.style.borderRightColor = new Color(0f, 0.82f, 0.62f, 0.5f);
                row.style.borderTopColor = new Color(0f, 0.82f, 0.62f, 0.5f);
                row.style.borderBottomColor = new Color(0f, 0.82f, 0.62f, 0.5f);
                
                Debug.Log($"[ROS] Transacción seleccionada como evidencia: {evidenciaAdjuntaRos}");
            });

            rosTransaccionesScroll.Add(row);
        }
    }
}
