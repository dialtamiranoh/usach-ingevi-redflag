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

    [Header("Puntos de cámara")]
    [Tooltip("Transform del punto de vista del cliente")]
    public Transform puntoCliente;

    [Tooltip("Transform del punto de vista del monitor")]
    public Transform puntoMonitor;

    [Tooltip("Transform del punto de vista del notepad")]
    public Transform puntoNotepad;

    // ===================== CONFIGURACIÓN =====================

    [Header("Velocidad de transición")]
    [Tooltip("Velocidad con que la cámara se mueve entre vistas usando delta time")]
    public float velocidadTransicion = 5f;

    // ===================== ESTADO =====================

    public enum VistaActiva { Cliente, Monitor, Notepad }
    private VistaActiva vistaActual = VistaActiva.Cliente;

    // Punto objetivo actual
    private Transform puntoObjetivo;

    // Referencia al UIManager
    private UIManager uiManager;

    // Acumulador de rotacion
    private float rotX = 0f;
    private float rotY = 0f;

    void Awake()
    {
        Debug.Log($"Awake - puntoCliente: {puntoCliente?.name} pos: {puntoCliente?.position}");

        if (puntoCliente != null)
        {
            transform.SetPositionAndRotation(puntoCliente.position, puntoCliente.rotation);
            puntoObjetivo = puntoCliente;
            vistaActual = VistaActiva.Cliente;
        }

        Debug.Log($"Awake - Camera pos después: {transform.position}");
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
