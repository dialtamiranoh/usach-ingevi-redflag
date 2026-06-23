using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class RoleManager : MonoBehaviour
{
    public static RoleManager Instance { get; private set; }

    public enum NivelJuego { Nivel1, Nivel2, Nivel3 }
    public enum RolAnalista { Analista_KYC, Analista_AML }

    [Header("Configuración de Progresión")]
    public NivelJuego nivelActual = NivelJuego.Nivel2; // Por defecto en Nivel 2 para pruebas
    public RolAnalista rolActivo = RolAnalista.Analista_KYC;

    [Header("Guía Visual Implícita (Nivel 2)")]
    [Tooltip("Objeto visual que brilla en el suelo para indicar el escritorio del Analista 2")]
    public GameObject senalPisoAnalista2;

    // Eventos del patrón Observer para notificar cambios de rol
    public static event Action<RolAnalista> OnRolCambiado;
    public static event Action<NivelJuego> OnNivelCambiado;

    private bool haCambiadoDeRolPorPrimeraVez = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        if (FindFirstObjectByType<RoleManager>() == null)
        {
            GameObject go = new GameObject("RoleManager_AutoCreated");
            go.AddComponent<RoleManager>();
            // No llamar a DontDestroyOnLoad para que se limpie al cambiar de escena si es necesario, 
            // pero lo dejamos persistente si GameManager también lo es.
            DontDestroyOnLoad(go);
            Debug.Log("[RoleManager] Spawneado automáticamente en la escena.");
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        ConfigurarNivel(nivelActual);
    }

    void Update()
    {
        ManejarInputCambioRol();
    }

    /// <summary>
    /// Configura el nivel de juego activo y activa/desactiva los elementos visuales correspondientes.
    /// </summary>
    public void ConfigurarNivel(NivelJuego nuevoNivel)
    {
        nivelActual = nuevoNivel;
        rolActivo = RolAnalista.Analista_KYC;
        haCambiadoDeRolPorPrimeraVez = false;

        // Configurar la guía visual implícita en Nivel 2 (Standby por ahora)
        /*
        if (senalPisoAnalista2 != null)
        {
            senalPisoAnalista2.SetActive(nivelActual == NivelJuego.Nivel2);
        }
        */

        OnNivelCambiado?.Invoke(nivelActual);
        OnRolCambiado?.Invoke(rolActivo);

        Debug.Log($"[RoleManager] Configurado Nivel: {nivelActual}");
    }

    /// <summary>
    /// Escucha la tecla W para cambiar entre los dos analistas durante el Nivel 2.
    /// </summary>
    private void ManejarInputCambioRol()
    {
        if (nivelActual != NivelJuego.Nivel2) return;

        if (Keyboard.current != null && Keyboard.current.wKey.wasPressedThisFrame)
        {
            AlternarRol();
        }
    }

    /// <summary>
    /// Alterna el rol activo entre KYC (Analista 1) y AML (Analista 2).
    /// </summary>
    public void AlternarRol()
    {
        rolActivo = (rolActivo == RolAnalista.Analista_KYC) 
            ? RolAnalista.Analista_AML 
            : RolAnalista.Analista_KYC;

        // Desactivar la guía visual implícita tras el primer cambio exitoso (Standby por ahora)
        /*
        if (!haCambiadoDeRolPorPrimeraVez)
        {
            haCambiadoDeRolPorPrimeraVez = true;
            if (senalPisoAnalista2 != null)
            {
                senalPisoAnalista2.SetActive(false);
                Debug.Log("[RoleManager] Guía visual implícita desactivada por interacción.");
            }
        }
        */

        OnRolCambiado?.Invoke(rolActivo);
        Debug.Log($"[RoleManager] Rol Cambiado a: {rolActivo}");
    }
}
