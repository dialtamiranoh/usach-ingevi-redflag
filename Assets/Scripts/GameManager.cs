using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Datos que persisten entre escenas
    public string nombreJugador { get; private set; }
    public int puntajeFinal { get; private set; }
    public int recordPersonal { get; private set; }

    private const string KEY_RANKING = "Ranking";

    /// <summary>
    /// Normaliza un nombre de jugador para usarlo como parte de una clave de
    /// PlayerPrefs: "Diego" y " diego " son el mismo perfil.
    /// </summary>
    public static string NormalizarNombre(string nombre)
        => string.IsNullOrWhiteSpace(nombre) ? "" : nombre.Trim().ToLowerInvariant();

    /// <summary>
    /// Clave del récord personal PARA un nombre dado. El récord es por usuario:
    /// antes se usaba la clave global "RecordPersonal" compartida entre nombres.
    /// </summary>
    public static string ClaveRecord(string nombre)
    {
        string n = NormalizarNombre(nombre);
        return string.IsNullOrEmpty(n) ? "RecordPersonal" : $"RecordPersonal_{n}";
    }

    void Awake()
    {
        Debug.Log($"[GM] Awake - Instance es null: {Instance == null}");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GM] GameManager creado y persistente");
        }
        else
        {
            Debug.Log("[GM] Duplicado destruido");
            Destroy(gameObject);
            return;
        }
        // El récord se carga al conocer el nombre del jugador (SetNombreJugador)
        recordPersonal = 0;
    }

    public void SetNombreJugador(string nombre)
    {
        nombreJugador = nombre?.Trim();

        // Cargar el récord de ESTE usuario
        recordPersonal = PlayerPrefs.GetInt(ClaveRecord(nombreJugador), 0);

        // Recargar los logros de ESTE usuario si el manager ya existe en la escena
        // (comparación explícita: el operador ?. ignora el "null" de objetos destruidos de Unity)
        if (AchievementManager.Instance != null) AchievementManager.Instance.RecargarLogros();
    }

    public void SetPuntajeFinal(int puntaje)
    {
        puntajeFinal = puntaje;

        // Actualizar récord del usuario activo si se superó
        if (puntaje > recordPersonal)
        {
            recordPersonal = puntaje;
            PlayerPrefs.SetInt(ClaveRecord(nombreJugador), recordPersonal);
            PlayerPrefs.Save();
        }

        // Guardar en ranking
        GuardarEnRanking(nombreJugador, puntaje);
    }

    void GuardarEnRanking(string nombre, int puntaje)
    {
        // Cargar ranking existente
        string json = PlayerPrefs.GetString(KEY_RANKING, "{}");
        RankingData ranking = JsonUtility.FromJson<RankingData>(json)
            ?? new RankingData();

        // Agregar nueva entrada
        ranking.entradas.Add(new RankingEntrada
        {
            nombre = nombre,
            puntaje = puntaje
        });

        // Ordenar y mantener top 10
        ranking.entradas.Sort((a, b) => b.puntaje.CompareTo(a.puntaje));
        if (ranking.entradas.Count > 10)
            ranking.entradas.RemoveRange(10, ranking.entradas.Count - 10);

        PlayerPrefs.SetString(KEY_RANKING, JsonUtility.ToJson(ranking));
        PlayerPrefs.Save();
    }

    public RankingData ObtenerRanking()
    {
        string json = PlayerPrefs.GetString(KEY_RANKING, "{}");
        return JsonUtility.FromJson<RankingData>(json) ?? new RankingData();
    }

    // Navegaci�n
    public void IrATutorial() => SceneManager.LoadScene("SceneTutorial");
    public void IrAlJuego() => SceneManager.LoadScene("MainScene");
    public void IrAResultados() => SceneManager.LoadScene("SceneResultados");
    public void Reiniciar() => SceneManager.LoadScene("MainScene");
    public void IrAInicio() => SceneManager.LoadScene("SceneInicio");
}

[System.Serializable]
public class RankingData
{
    public System.Collections.Generic.List<RankingEntrada> entradas
        = new System.Collections.Generic.List<RankingEntrada>();
}

[System.Serializable]
public class RankingEntrada
{
    public string nombre;
    public int puntaje;
}