using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Datos que persisten entre escenas
    public string nombreJugador { get; private set; }
    public int puntajeFinal { get; private set; }
    public int recordPersonal { get; private set; }

    private const string KEY_RECORD = "RecordPersonal";
    private const string KEY_RANKING = "Ranking";

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
        recordPersonal = PlayerPrefs.GetInt(KEY_RECORD, 0);
    }

    public void SetNombreJugador(string nombre)
    {
        nombreJugador = nombre;
    }

    public void SetPuntajeFinal(int puntaje)
    {
        puntajeFinal = puntaje;

        // Actualizar récord si se superó
        if (puntaje > recordPersonal)
        {
            recordPersonal = puntaje;
            PlayerPrefs.SetInt(KEY_RECORD, recordPersonal);
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

    // Navegación
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