using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjetosManager : MonoBehaviour
{
    [Header("Prefabs de objetos seguridad")]
    public GameObject prefabPendrive;
    public GameObject prefabCelular;
    public GameObject prefabPostIt;
    public GameObject prefabDocumento;
    public GameObject prefabLlave;
    public GameObject prefabTarjeta;
    public GameObject prefabCarpeta;

    [Header("Prefabs soborno")]
    public GameObject prefabRegalo;
    public GameObject prefabSoborno;

    [Header("Spawning")]
    public Transform[] puntosSpawn;
    //public float alturaEscritorio = -0.1538271f;
    public int maxObjetosSimultaneos = 2;

    private List<ObjetoSospechoso> objetosActivos = new();
    private ScoreManager scoreManager;

    void Awake()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
    }

    private int turnoActual = 1;

    void Start()
    {
        var uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null) {
            uiManager.OnTurnoChanged += (turnoAct, turnoTot) => {
                turnoActual = turnoAct;
            };
        }
        StartCoroutine(CicloSpawnSeguridad());
        StartCoroutine(CicloSpawnSoborno());
    }

    // ── Ciclo objetos seguridad ─────────────────────────────────
    IEnumerator CicloSpawnSeguridad()
    {
        while (true)
        {
            float intervaloSpawn = Mathf.Max(10f, 25f - (turnoActual * 3f));
            yield return new WaitForSeconds(intervaloSpawn);

            if (objetosActivos.Count < maxObjetosSimultaneos)
                SpawnObjetoSeguridad();
        }
    }

    void SpawnObjetoSeguridad()
    {
        if (puntosSpawn.Length == 0) return;

        GameObject[] prefabs = {
            prefabPendrive, prefabCelular, prefabPostIt,
            prefabDocumento, prefabLlave, prefabTarjeta, prefabCarpeta
        };

        var validos = System.Array.FindAll(prefabs, p => p != null);
        if (validos.Length == 0)
        {
            Debug.LogWarning("[SPAWN] No hay prefabs de seguridad asignados");
            return;
        }

        GameObject prefabElegido = validos[Random.Range(0, validos.Length)];
        //Transform punto = puntosSpawn[Random.Range(0, puntosSpawn.Length)];
        Transform punto = ObtenerPuntoSpawn();
        if (punto == null) return;
        //Vector3 pos = new Vector3(punto.position.x, alturaEscritorio, punto.position.z);
        Vector3 pos = punto.position;

        GameObject obj = Instantiate(prefabElegido, pos, Quaternion.identity);
        ObjetoSospechoso comp = obj.GetComponent<ObjetoSospechoso>();
        if (comp != null)
        {
            objetosActivos.Add(comp);
            Debug.Log($"[SPAWN] Objeto seguridad: {prefabElegido.name}");
        }
    }

    // ── Ciclo sobornos ──────────────────────────────────────────
    // Aparece cada 20 seg, dura 20 seg
    // Si se ignora → suma puntaje
    // Si se clickea → penalización

    IEnumerator CicloSpawnSoborno()
    {
        while (true) {
            float intervaloSoborno = Mathf.Max(15f, 30f - (turnoActual * 2f));
            yield return new WaitForSeconds(intervaloSoborno);
            SpawnSoborno();
        }
    }

    void SpawnSoborno()
    {
        if (puntosSpawn.Length == 0) return;

        GameObject[] prefabsSoborno = { prefabRegalo, prefabSoborno };
        var validos = System.Array.FindAll(prefabsSoborno, p => p != null);
        if (validos.Length == 0) return;

        GameObject prefabElegido = validos[Random.Range(0, validos.Length)];
        //Transform punto = puntosSpawn[Random.Range(0, puntosSpawn.Length)];
        Transform punto = ObtenerPuntoSpawn();
        if (punto == null) return;
        //Vector3 pos = new Vector3(punto.position.x, alturaEscritorio, punto.position.z);
        Vector3 pos = punto.position;

        GameObject obj = Instantiate(prefabElegido, pos, Quaternion.identity);
        ObjetoSospechoso comp = obj.GetComponent<ObjetoSospechoso>();
        if (comp != null)
        {
            objetosActivos.Add(comp);
            Debug.Log($"[SPAWN] Soborno: {prefabElegido.name}");
        }
    }

    // ── Callbacks ───────────────────────────────────────────────

    public void OnObjetoGuardado(ObjetoSospechoso obj)
    {
        objetosActivos.Remove(obj);
    }

    public void OnObjetoIgnorado(ObjetoSospechoso obj)
    {
        objetosActivos.Remove(obj);
    }

    public void ProgramarRespawn(ObjetoSospechoso obj, float tiempo)
    {
        StartCoroutine(RespawnCoroutine(obj, tiempo));
    }

    IEnumerator RespawnCoroutine(ObjetoSospechoso obj, float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        if (obj != null) obj.Respawn();
    }


    private int ultimoPuntoUsado = -1;

    Transform ObtenerPuntoSpawn()
    {
        if (puntosSpawn.Length == 0) return null;

        // Evitar el mismo punto consecutivo
        int indice;
        do
        {
            indice = Random.Range(0, puntosSpawn.Length);
        } while (indice == ultimoPuntoUsado && puntosSpawn.Length > 1);

        ultimoPuntoUsado = indice;
        return puntosSpawn[indice];
    }
}