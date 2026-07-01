using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// FSM del Gerente de Sucursal (NPC).
/// Estados: Patrol → Approach → Interact → Patrol
/// Frecuencia de aparición escalonada por nivel (Nivel1 < Nivel2 < Nivel3).
/// </summary>
public class GerenteNPC_FSM : MonoBehaviour
{
    // ──────────────────────────────────────────────
    // ESTADOS
    // ──────────────────────────────────────────────
    public enum NPCState { Patrol, Approach, Interact }
    public NPCState EstadoActual { get; private set; } = NPCState.Patrol;

    // ──────────────────────────────────────────────
    // INSPECTOR
    // ──────────────────────────────────────────────
    [Header("Navegación")]
    [Tooltip("Puntos de patrullaje aleatorios en la oficina")]
    public Transform[] waypointsPatrullaje;

    [Header("Escritorios (uno por nivel)")]
    [Tooltip("Escritorio del Analista KYC — Nivel 1")]
    public Transform escritorioNivel1;
    [Tooltip("Escritorio del Analista AML — Nivel 2 (rol AML)")]
    public Transform escritorioNivel2_AML;
    [Tooltip("Escritorio del Supervisor — Nivel 3")]
    public Transform escritorioNivel3;

    [Header("Detección")]
    [Tooltip("Distancia desde el escritorio a la que se activa Interact")]
    [SerializeField] private float rangoInteraccion = 1.5f;

    [Header("Frecuencia de aparición por nivel (segundos entre visitas)")]
    [SerializeField] private float intervaloNivel1 = 40f;
    [SerializeField] private float intervaloNivel2 = 25f;
    [SerializeField] private float intervaloNivel3 = 15f;

    [Header("Referencias")]
    public GerenteInteractUI interactUI;
    public GerenteNPC_Health health;

    // ──────────────────────────────────────────────
    // PRIVADAS
    // ──────────────────────────────────────────────
    private NavMeshAgent agent;
    private Animator animator;
    private int waypointActual = 0;
    private float timerSiguienteVisita = 0f;
    private float intervaloActual = 40f;
    private bool interaccionActiva = false;

    // ──────────────────────────────────────────────
    // LIFECYCLE
    // ──────────────────────────────────────────────
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        ActualizarIntervaloPorNivel();
        timerSiguienteVisita = intervaloActual;

        // Ajustar al Gerente al NavMesh más cercano al iniciar
        if (agent != null && agent.isActiveAndEnabled)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
            {
                bool warped = agent.Warp(hit.position);
                Debug.Log($"[GerenteNPC] Warp inicial en {hit.position}. Éxito: {warped}");
            }
            else
            {
                Debug.Log("[GerenteNPC] Error: No se encontró NavMesh cercano para Warp inicial.");
            }
        }

        EnterPatrol();

        // Suscribirse a cambios de nivel
        RoleManager.OnNivelCambiado += OnNivelCambiado;
    }

    void OnDestroy()
    {
        RoleManager.OnNivelCambiado -= OnNivelCambiado;
    }

    void Update()
    {
        if (health != null && health.EstaMuerto) return;

        // Auto-corrección por si físicas u otros elementos empujan al NPC fuera del NavMesh
        if (agent != null && agent.isActiveAndEnabled && !agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
            {
                bool warped = agent.Warp(hit.position);
                Debug.Log($"[GerenteNPC] Auto-Warp en update: {warped}");
            }
        }

        switch (EstadoActual)
        {
            case NPCState.Patrol:   UpdatePatrol();   break;
            case NPCState.Approach: UpdateApproach(); break;
            case NPCState.Interact: UpdateInteract(); break;
        }
    }

    // ──────────────────────────────────────────────
    // ESTADO: PATROL
    // ──────────────────────────────────────────────
    void EnterPatrol()
    {
        if (health != null && health.EstaMuerto) return;

        EstadoActual = NPCState.Patrol;
        interaccionActiva = false;

        if (agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        SetAnimatorBoolSafe("IsWalking", true);
        SetAnimatorBoolSafe("IsInteracting", false);

        // Moverse al siguiente waypoint de patrullaje (proyectando al NavMesh para evitar fallos de altura/posición)
        if (waypointsPatrullaje.Length > 0 && agent.isActiveAndEnabled)
        {
            Vector3 targetPos = waypointsPatrullaje[waypointActual].position;
            bool sampleExit = NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 3.0f, NavMesh.AllAreas);
            if (sampleExit)
            {
                targetPos = hit.position;
            }
            
            bool rutaSeteada = agent.SetDestination(targetPos);
            Debug.Log($"[GerenteNPC] Intentando ir a Waypoint {waypointActual} ({waypointsPatrullaje[waypointActual].name}). Éxito SetDestination: {rutaSeteada} | SamplePosition: {sampleExit} | targetPos: {targetPos}");

            // FALLBACK: Si no pudo ir al waypoint (ej: está a (0,0,0)), buscar un punto aleatorio transitable cerca
            if (!rutaSeteada)
            {
                Vector3 puntoAleatorio = ObtenerPuntoAleatorioNavMesh(transform.position, 6f);
                if (puntoAleatorio != Vector3.zero)
                {
                    rutaSeteada = agent.SetDestination(puntoAleatorio);
                    Debug.Log($"[GerenteNPC] Fallback: Yendo a punto aleatorio de patrullaje {puntoAleatorio}. Éxito: {rutaSeteada}");
                }
            }
        }
        else
        {
            // FALLBACK DIRECTO: Si no hay waypoints asignados, patrullar aleatoriamente
            if (agent.isActiveAndEnabled)
            {
                Vector3 puntoAleatorio = ObtenerPuntoAleatorioNavMesh(transform.position, 6f);
                if (puntoAleatorio != Vector3.zero)
                {
                    bool rutaSeteada = agent.SetDestination(puntoAleatorio);
                    Debug.Log($"[GerenteNPC] No hay waypoints. Fallback aleatorio: {puntoAleatorio}. Éxito: {rutaSeteada}");
                }
            }
        }

        timerSiguienteVisita = intervaloActual;
        
        if (agent.isActiveAndEnabled)
        {
            Debug.Log($"[GerenteNPC] → Estado: PATROL | isStopped: {agent.isStopped} | pathStatus: {agent.pathStatus} | speed: {agent.speed} | isOnNavMesh: {agent.isOnNavMesh}");
        }
    }

    void UpdatePatrol()
    {
        // Avanzar al siguiente waypoint si llegó al actual (asegurando que tiene una ruta activa)
        if (!agent.pathPending && agent.hasPath && agent.remainingDistance < 0.5f)
        {
            waypointActual = (waypointActual + 1) % waypointsPatrullaje.Length;
            
            Vector3 targetPos = waypointsPatrullaje[waypointActual].position;
            bool sampleExit = NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 3.0f, NavMesh.AllAreas);
            if (sampleExit)
            {
                targetPos = hit.position;
            }
            
            bool rutaSeteada = agent.SetDestination(targetPos);
            
            // Fallback si el siguiente waypoint también falla
            if (!rutaSeteada)
            {
                Vector3 puntoAleatorio = ObtenerPuntoAleatorioNavMesh(transform.position, 6f);
                if (puntoAleatorio != Vector3.zero)
                {
                    agent.SetDestination(puntoAleatorio);
                }
            }
        }

        // Cuenta regresiva para próxima visita al escritorio
        timerSiguienteVisita -= Time.deltaTime;
        if (timerSiguienteVisita <= 0f)
        {
            ExitPatrol();
            EnterApproach();
        }
    }

    void ExitPatrol()
    {
        // Nada que limpiar específicamente
    }

    // ──────────────────────────────────────────────
    // ESTADO: APPROACH
    // ──────────────────────────────────────────────
    void EnterApproach()
    {
        EstadoActual = NPCState.Approach;
        SetAnimatorBoolSafe("IsWalking", true);
        agent.isStopped = false;

        // Navegar al escritorio activo según nivel y rol actuales
        Transform destino = GetEscritorioActivo();
        if (destino != null)
        {
            Vector3 targetPos = destino.position;
            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 3.0f, NavMesh.AllAreas))
            {
                targetPos = hit.position;
            }
            agent.SetDestination(targetPos);
        }

        Debug.Log($"[GerenteNPC] → Estado: APPROACH → {destino?.name}");
    }

    void UpdateApproach()
    {
        Transform destino = GetEscritorioActivo();
        if (destino == null) return;

        float distancia = Vector3.Distance(transform.position, destino.position);

        // Transición a Interact cuando llega al escritorio
        if (!agent.pathPending && distancia <= rangoInteraccion)
        {
            ExitApproach();
            EnterInteract();
        }
    }

    void ExitApproach()
    {
        agent.isStopped = true;
        // Girar hacia el escritorio activo
        Transform destino = GetEscritorioActivo();
        if (destino != null)
        {
            Vector3 dir = (destino.position - transform.position).normalized;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    // ──────────────────────────────────────────────
    // ESTADO: INTERACT
    // ──────────────────────────────────────────────
    void EnterInteract()
    {
        EstadoActual = NPCState.Interact;
        interaccionActiva = true;
        SetAnimatorBoolSafe("IsWalking", false);
        SetAnimatorBoolSafe("IsInteracting", true);

        // Activar UI de pregunta/presión
        if (interactUI != null)
            interactUI.MostrarPregunta(OnRespuestaCorrecta, OnRespuestaIncorrecta);

        Debug.Log("[GerenteNPC] → Estado: INTERACT");
    }

    void UpdateInteract()
    {
        // La lógica de timer y respuesta se maneja en GerenteInteractUI
    }

    void ExitInteract()
    {
        interaccionActiva = false;
        SetAnimatorBoolSafe("IsInteracting", false);

        // Ocultar UI si sigue visible
        if (interactUI != null)
            interactUI.OcultarUI();
    }

    // ──────────────────────────────────────────────
    // CALLBACKS DE INTERACCIÓN
    // ──────────────────────────────────────────────

    /// <summary>Llamado por GerenteInteractUI cuando el jugador responde correctamente.</summary>
    public void OnRespuestaCorrecta()
    {
        Debug.Log("[GerenteNPC] OnRespuestaCorrecta ejecutada.");
        if (health != null)
            health.TakeDamage(1); // Respuesta correcta = el gerente "retrocede"

        ExitInteract();
        EnterPatrol();
    }

    /// <summary>Llamado por GerenteInteractUI cuando falla o se agota el tiempo.</summary>
    public void OnRespuestaIncorrecta()
    {
        Debug.Log("[GerenteNPC] OnRespuestaIncorrecta ejecutada.");
        // Penalización al jugador (puntos + racha)
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.AgregarPuntaje(-100);
            uiManager.ResetRacha();
        }

        ExitInteract();
        EnterPatrol();
    }

    // ──────────────────────────────────────────────
    // ESCRITORIO ACTIVO
    // ──────────────────────────────────────────────

    /// <summary>
    /// Devuelve el escritorio correspondiente al nivel y rol activos.
    /// Nivel1 → escritorio KYC
    /// Nivel2 rol KYC → escritorio KYC | rol AML → escritorio AML
    /// Nivel3 → escritorio Supervisor
    /// </summary>
    private Transform GetEscritorioActivo()
    {
        if (RoleManager.Instance == null) return escritorioNivel1;

        return RoleManager.Instance.nivelActual switch
        {
            RoleManager.NivelJuego.Nivel1 => escritorioNivel1,
            RoleManager.NivelJuego.Nivel2 =>
                RoleManager.Instance.rolActivo == RoleManager.RolAnalista.Analista_AML
                    ? escritorioNivel2_AML
                    : escritorioNivel1,
            RoleManager.NivelJuego.Nivel3 => escritorioNivel3,
            _ => escritorioNivel1
        };
    }

    // ──────────────────────────────────────────────
    // NIVEL
    // ──────────────────────────────────────────────
    private void OnNivelCambiado(RoleManager.NivelJuego nuevoNivel)
    {
        ActualizarIntervaloPorNivel();
    }

    private void ActualizarIntervaloPorNivel()
    {
        if (RoleManager.Instance == null) return;

        intervaloActual = RoleManager.Instance.nivelActual switch
        {
            RoleManager.NivelJuego.Nivel1 => intervaloNivel1,
            RoleManager.NivelJuego.Nivel2 => intervaloNivel2,
            RoleManager.NivelJuego.Nivel3 => intervaloNivel3,
            _ => intervaloNivel1
        };

        Debug.Log($"[GerenteNPC] Intervalo de visita actualizado: {intervaloActual}s");
    }

    // ──────────────────────────────────────────────
    // GIZMO (visualizar rango en editor)
    // ──────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        // Mostrar rango de interacción para cada escritorio
        Transform[] escritorios = { escritorioNivel1, escritorioNivel2_AML, escritorioNivel3 };
        Color[] colores = { Color.yellow, Color.cyan, Color.magenta };
        for (int i = 0; i < escritorios.Length; i++)
        {
            if (escritorios[i] != null)
            {
                Gizmos.color = colores[i];
                Gizmos.DrawWireSphere(escritorios[i].position, rangoInteraccion);
            }
        }
    }

    /// <summary>
    /// Evita advertencias de parámetros de Animator inexistentes.
    /// </summary>
    private void SetAnimatorBoolSafe(string paramName, bool value)
    {
        if (animator == null) return;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
            {
                animator.SetBool(paramName, value);
                return;
            }
        }
    }

    /// <summary>
    /// Obtiene un punto transitable aleatorio en el NavMesh dentro de un radio determinado.
    /// </summary>
    private Vector3 ObtenerPuntoAleatorioNavMesh(Vector3 center, float range)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return Vector3.zero;
    }
}
