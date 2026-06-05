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

    // Botones de acción
    private Button btnAprobar, btnEscalar, btnRechazar;

    // Tabs
    private Button tabIdentidad, tabTransacciones, tabHistorial, tabSanciones;

    // Estado
    private int turnoActual = 1;
    private int turnoTotal = 10;
    private int puntaje = 0;
    private float tiempoRestante = 300f;
    private bool juegoActivo = true;
    private bool dialogoAbierto = false;

    void Awake()
    {
        var root = uiDocument.rootVisualElement;
        InicializarElementos(root);
        ConfigurarBotones();
        ConfigurarTabs();
        ConfigurarDialogo();
        ConfigurarFieldsClickables(root);
        ActualizarUI();
        Invoke(nameof(IniciarVistaCliente), 0.1f);

    }

    void IniciarVistaCliente()
    {
        ActualizarVista(CameraController.VistaActiva.Cliente);
    }

    void Update()
    {
        if (!juegoActivo) return;
        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0) { tiempoRestante = 0; juegoActivo = false; }
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

    }

    void ConfigurarDialogo()
    {
        pregunta1.clicked += () => ResponderPregunta("Mi actividad es el comercio de productos importados al por mayor.");
        pregunta2.clicked += () => ResponderPregunta("Los fondos provienen de mi negocio de importación registrado en Chile.");
        pregunta3.clicked += () => ResponderPregunta("No, no tengo ninguna vinculación con cargos políticos.");
        pregunta4.clicked += () => ResponderPregunta("No tengo cuentas en el extranjero.");
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

    void ResponderPregunta(string respuesta)
    {
        dialogoRespuesta.text = $"💬 {respuesta}";
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
        if (decision == "APROBADO") puntaje += 10;
        else if (decision == "RECHAZADO") puntaje += 15;
        else if (decision == "ESCALADO") puntaje += 5;

        labelEstado.text = $"Caso resuelto: {decision}";
        ActualizarUI();
        Invoke(nameof(SiguienteTurno), 0.5f);
    }

    void SiguienteTurno()
    {
        if (turnoActual >= turnoTotal) { juegoActivo = false; labelEstado.text = "Jornada finalizada"; return; }
        turnoActual++;
        tiempoRestante = 300f;
        CerrarDialogo();
        CambiarPersonaje(); // <- agrega esta línea
        if (cameraController != null)
            cameraController.CambiarVista(CameraController.VistaActiva.Cliente, true);
        labelEstado.text = "Pendiente de resolución";
        ActualizarUI();
        caseManager.SiguienteCaso();
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
        clientePep.text = caso.cliente.esPEP ? "Sí — PEP confirmado" : "No declarado";

        // Actualizar preguntas con respuestas reales del caso
        pregunta1.text = "¿Cuál es su actividad económica?";
        pregunta2.text = "¿Cuál es el origen de sus fondos?";
        pregunta3.text = "¿Es usted PEP o familiar de PEP?";
        pregunta4.text = "¿Tiene cuentas en el extranjero?";

        // Resetear notas y estado
        notasAnalista.value = "";
        labelEstado.text = "Pendiente de resolución";
        CerrarDialogo();
    }

    public void MostrarBotonReporte(ObjetoSospechoso obj)
    {
        // Por ahora solo log — implementación completa después
        Debug.Log($"[UI] Objeto listo para reportar: {obj.ObtenerNombreTipo()}");
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
}
