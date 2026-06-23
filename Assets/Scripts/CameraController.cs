/// <summary>
/// CameraController gestiona las 3 vistas de la escena usando puntos de cámara:
/// - Vista Cliente (por defecto): el analista ve al cliente frente a él
/// - Vista Monitor (tecla E): zoom al monitor para revisar documentos
/// - Vista Notepad (tecla Q): zoom al notepad para revisar el checklist
/// 
/// El movimiento usa Time.deltaTime para ser independiente del framerate,
/// garantizando la misma velocidad de transición en cualquier equipo.
/// </summary>
/// 
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // ===================== PUNTOS DE CÁMARA =====================

    [Header("Puntos de cámara por Rol")]
    public Transform puntoClienteKYC;
    public Transform puntoMonitorKYC;
    public Transform puntoNotepadKYC;
    [Space]
    public Transform puntoClienteAML;
    public Transform puntoMonitorAML;
    public Transform puntoNotepadAML;
    [Space]
    public Transform puntoSupervisorCliente;
    public Transform puntoSupervisorMonitor;
    public Transform puntoSupervisorNotepad;

    // ===================== CONFIGURACIÓN =====================

    [Header("Velocidad de transición")]
    [Tooltip("Velocidad con que la cámara se mueve entre vistas usando delta time")]
    public float velocidadTransicion = 5f;

    // ===================== ESTADO =====================

    public enum VistaActiva { Cliente, Monitor, Notepad }
    private VistaActiva vistaActual = VistaActiva.Cliente;

    // Puntos activos actuales (se asocian dinámicamente, públicos para conservar inspector)
    [HideInInspector] public Transform puntoCliente;
    [HideInInspector] public Transform puntoMonitor;
    [HideInInspector] public Transform puntoNotepad;

    // Punto objetivo actual
    private Transform puntoObjetivo;

    // Referencia al UIManager
    private UIManager uiManager;

    // Acumulador de rotacion
    private float rotX = 0f;
    private float rotY = 0f;

    void Awake()
    {
        // Inicializar respaldos si no se han asignado en inspector usando los valores anteriores
        if (puntoClienteKYC == null) puntoClienteKYC = puntoCliente;
        if (puntoMonitorKYC == null) puntoMonitorKYC = puntoMonitor;
        if (puntoNotepadKYC == null) puntoNotepadKYC = puntoNotepad;

        // Auto-descubrimiento para Analista 2 (AML) si están vacíos
        if (puntoClienteAML == null)
        {
            GameObject go = GameObject.Find("CameraCliente2");
            if (go != null) puntoClienteAML = go.transform;
        }
        if (puntoMonitorAML == null)
        {
            GameObject go = GameObject.Find("CameraMonitor2");
            if (go != null) puntoMonitorAML = go.transform;
        }
        if (puntoNotepadAML == null)
        {
            GameObject go = GameObject.Find("CameraNotepad2");
            if (go != null) puntoNotepadAML = go.transform;
        }

        // Auto-descubrimiento para Supervisor si están vacíos
        if (puntoSupervisorCliente == null)
        {
            GameObject go = GameObject.Find("CameraCliente3");
            if (go != null) puntoSupervisorCliente = go.transform;
        }
        if (puntoSupervisorMonitor == null)
        {
            GameObject go = GameObject.Find("CameraMonitor3");
            if (go != null) puntoSupervisorMonitor = go.transform;
        }
        if (puntoSupervisorNotepad == null)
        {
            GameObject go = GameObject.Find("CameraNotepad3");
            if (go != null) puntoSupervisorNotepad = go.transform;
        }

        // Por defecto, apuntar a KYC
        puntoCliente = puntoClienteKYC;
        puntoMonitor = puntoMonitorKYC;
        puntoNotepad = puntoNotepadKYC;

        if (puntoCliente != null)
        {
            transform.SetPositionAndRotation(puntoCliente.position, puntoCliente.rotation);
            puntoObjetivo = puntoCliente;
            vistaActual = VistaActiva.Cliente;
        }
    }

    void OnEnable()
    {
        RoleManager.OnRolCambiado += AlCambiarRol;
        RoleManager.OnNivelCambiado += AlCambiarNivel;
    }

    void OnDisable()
    {
        RoleManager.OnRolCambiado -= AlCambiarRol;
        RoleManager.OnNivelCambiado -= AlCambiarNivel;
    }

    private void AlCambiarNivel(RoleManager.NivelJuego nuevoNivel)
    {
        switch (nuevoNivel)
        {
            case RoleManager.NivelJuego.Nivel1:
            case RoleManager.NivelJuego.Nivel2:
                puntoCliente = puntoClienteKYC;
                puntoMonitor = puntoMonitorKYC;
                puntoNotepad = puntoNotepadKYC;
                break;
            case RoleManager.NivelJuego.Nivel3:
                puntoCliente = puntoSupervisorCliente != null ? puntoSupervisorCliente : puntoClienteKYC;
                puntoMonitor = puntoSupervisorMonitor != null ? puntoSupervisorMonitor : puntoMonitorKYC;
                puntoNotepad = puntoSupervisorNotepad != null ? puntoSupervisorNotepad : puntoNotepadKYC;
                break;
        }
        CambiarVista(VistaActiva.Cliente, true);
    }

    private void AlCambiarRol(RoleManager.RolAnalista nuevoRol)
    {
        if (RoleManager.Instance != null && RoleManager.Instance.nivelActual != RoleManager.NivelJuego.Nivel2) 
            return;

        switch (nuevoRol)
        {
            case RoleManager.RolAnalista.Analista_KYC:
                puntoCliente = puntoClienteKYC;
                puntoMonitor = puntoMonitorKYC;
                puntoNotepad = puntoNotepadKYC;
                break;
            case RoleManager.RolAnalista.Analista_AML:
                puntoCliente = puntoClienteAML != null ? puntoClienteAML : puntoClienteKYC;
                puntoMonitor = puntoMonitorAML != null ? puntoMonitorAML : puntoMonitorKYC;
                puntoNotepad = puntoNotepadAML != null ? puntoNotepadAML : puntoNotepadKYC;
                break;
        }
        CambiarVista(VistaActiva.Cliente);
    }

    void Start()
    {
        uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager != null)
            uiManager.ActualizarVista(VistaActiva.Cliente);
    }

    void Update()
    {
        ManejarInput();
        MoverCamara();
    }

    /// <summary>
    /// Detecta input del teclado para cambiar entre vistas.
    /// E → Vista Monitor | Q → Vista Notepad | Escape → Vista Cliente
    /// </summary>
    //void ManejarInput()
    //{
    //    if (Keyboard.current.eKey.wasPressedThisFrame)
    //    {
    //        if (vistaActual == VistaActiva.Monitor)
    //            CambiarVista(VistaActiva.Cliente);
    //        else
    //            CambiarVista(VistaActiva.Monitor);
    //    }

    //    if (Keyboard.current.qKey.wasPressedThisFrame)
    //    {
    //        if (vistaActual == VistaActiva.Notepad)
    //            CambiarVista(VistaActiva.Cliente);
    //        else
    //            CambiarVista(VistaActiva.Notepad);
    //    }

    //    if (Keyboard.current.escapeKey.wasPressedThisFrame)
    //    {
    //        CambiarVista(VistaActiva.Cliente);
    //    }
    //}

    /// <summary>
    /// Mueve la cámara suavemente hacia el punto objetivo.
    /// Usa Time.deltaTime para que la velocidad sea independiente del framerate.
    /// Sin delta time, la cámara se movería más rápido en equipos con más FPS.
    /// </summary>
    void MoverCamara()
    {
        if (puntoObjetivo == null) return;
        if (puntoObjetivo == puntoCliente && vistaActual == VistaActiva.Cliente &&
            Vector3.Distance(transform.position, puntoCliente.position) < 0.001f) return;


        // Interpolación suave de posición usando delta time
        transform.position = Vector3.Lerp(
            transform.position,
            puntoObjetivo.position,
            velocidadTransicion * Time.deltaTime
        );

        // Interpolación suave de rotación usando delta time
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            puntoObjetivo.rotation,
            velocidadTransicion * Time.deltaTime
        );
    }

    /// <summary>
    /// Cambia la vista activa y actualiza el punto objetivo.
    /// </summary>
    public void CambiarVista(VistaActiva nuevaVista, bool instantaneo = false)
    {
        StopAllCoroutines();
        rotX = 0f;
        rotY = 0f;
        vistaActual = nuevaVista;

        switch (nuevaVista)
        {
            case VistaActiva.Cliente:
                puntoObjetivo = puntoCliente;
                break;
            case VistaActiva.Monitor:
                puntoObjetivo = puntoMonitor;
                break;
            case VistaActiva.Notepad:
                puntoObjetivo = puntoNotepad;
                break;
        }

        // Si es instantáneo, teletransportar sin Lerp
        if (instantaneo && puntoObjetivo != null)
        {
            transform.SetPositionAndRotation(puntoObjetivo.position, puntoObjetivo.rotation);
        }

        if (uiManager != null)
            uiManager.ActualizarVista(nuevaVista);

        Debug.Log($"CameraController: Vista cambiada a {nuevaVista}");
    }

    public VistaActiva ObtenerVistaActual() => vistaActual;

    void ManejarInput()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            Debug.Log("Tecla Q detectada");
            if (vistaActual == VistaActiva.Monitor)
                CambiarVista(VistaActiva.Cliente);
            else
                CambiarVista(VistaActiva.Monitor);
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("Tecla E detectada");
            if (vistaActual == VistaActiva.Notepad)
                CambiarVista(VistaActiva.Cliente);
            else
                CambiarVista(VistaActiva.Notepad);
        }

        // Rotación libre con mouse en vista cliente
        if (vistaActual == VistaActiva.Cliente)
        {
            if (Mouse.current.rightButton.isPressed)
            {
                float mouseX = Mouse.current.delta.x.ReadValue() * 0.15f;
                float mouseY = Mouse.current.delta.y.ReadValue() * 0.15f;

                // Acumular rotación en variables propias
                rotY += mouseX;
                rotX -= mouseY;

                // Limitar como campo visual natural sentado
                rotX = Mathf.Clamp(rotX, -20f, 20f);
                rotY = Mathf.Clamp(rotY, -45f, 45f);

                // Aplicar desde la rotación base del punto cliente
                transform.rotation = puntoCliente.rotation *
                    Quaternion.Euler(rotX, rotY, 0);
            }

            //if (Mouse.current.rightButton.wasReleasedThisFrame)
            //{
            //    StartCoroutine(VolverRotacionOriginal());
            //}
        }

        System.Collections.IEnumerator VolverRotacionOriginal()
        {
            while (Quaternion.Angle(transform.rotation, puntoCliente.rotation) > 0.1f)
            {
                rotX = Mathf.Lerp(rotX, 0f, 3f * Time.deltaTime);
                rotY = Mathf.Lerp(rotY, 0f, 3f * Time.deltaTime);
                transform.rotation = puntoCliente.rotation * Quaternion.Euler(rotX, rotY, 0);
                yield return null;
            }
            rotX = 0f;
            rotY = 0f;
            transform.rotation = puntoCliente.rotation;
        }
    }
}
