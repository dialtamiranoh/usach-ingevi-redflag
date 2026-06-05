using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // Claves PlayerPrefs
    private const string KEY_RECORD = "RecordPersonal";
    private const string KEY_RECORD_EQUIPO = "RecordEquipo";

    public int puntajeIndividual { get; private set; }
    public int puntajeEquipo { get; private set; }
    public int recordPersonal { get; private set; }

    void Awake()
    {
        // Cargar récord guardado al iniciar
        recordPersonal = PlayerPrefs.GetInt(KEY_RECORD, 0);
    }

    public void AgregarPuntajeIndividual(int puntos)
    {
        puntajeIndividual += puntos;
        // El puntaje individual también suma al equipo
        AgregarPuntajeEquipo(puntos);
    }

    public void AgregarPuntajeEquipo(int puntos)
    {
        puntajeEquipo += puntos;
    }

    public void FinalizarJornada()
    {
        // Actualizar récord si se superó
        if (puntajeIndividual > recordPersonal)
        {
            recordPersonal = puntajeIndividual;
            PlayerPrefs.SetInt(KEY_RECORD, recordPersonal);
            PlayerPrefs.Save();
            Debug.Log($"¡Nuevo récord personal: {recordPersonal}!");
        }

        int recordEquipo = PlayerPrefs.GetInt(KEY_RECORD_EQUIPO, 0);
        if (puntajeEquipo > recordEquipo)
        {
            PlayerPrefs.SetInt(KEY_RECORD_EQUIPO, puntajeEquipo);
            PlayerPrefs.Save();
        }
    }
}