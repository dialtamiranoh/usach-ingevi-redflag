using UnityEngine;
using System.Collections.Generic;

// ===================== MODELOS DE DATOS =====================

[System.Serializable]
public class DocumentosData
{
    public bool cedulaVigente;
    public bool fotoCoincide;
    public bool rutValido;
    public bool domicilioVerificado;
    public bool actividadConcuerda;
}

[System.Serializable]
public class RespuestasClienteData
{
    public string actividad;
    public string origen_fondos;
    public string esPEP;
    public string cuentasExtranjero;
}

[System.Serializable]
public class ClienteData
{
    public string nombre;
    public string rut;
    public string nacionalidad;
    public string actividad;
    public bool esPEP;
}

[System.Serializable]
public class CasoData
{
    public string id;
    public string tipo;
    public string prioridad;
    public ClienteData cliente;
    public DocumentosData documentos;
    public RespuestasClienteData respuestasCliente;
    public List<string> discrepancias;
    public string decisionCorrecta;
    public string normativaAplicable;
    public string explicacion;
}

[System.Serializable]
public class CasosWrapper
{
    public List<CasoData> casos;
}

// ===================== GAME MANAGER =====================

public class CaseManager : MonoBehaviour
{
    [Header("Archivo de Casos")]
    public TextAsset archivoJSON;

    [Header("Referencias")]
    public UIManager uiManager;

    private List<CasoData> listaCasos;
    private int indiceCasoActual = 0;

    public CasoData CasoActual => listaCasos != null && indiceCasoActual < listaCasos.Count
        ? listaCasos[indiceCasoActual]
        : null;

    void Start()
    {
        CargarCasos();
        MostrarCasoActual();
    }

    void CargarCasos()
    {
        if (archivoJSON == null)
        {
            Debug.LogError("CaseManager: No se asignó el archivo JSON.");
            return;
        }

        CasosWrapper wrapper = JsonUtility.FromJson<CasosWrapper>(archivoJSON.text);
        listaCasos = wrapper.casos;
        Debug.Log($"CaseManager: {listaCasos.Count} casos cargados.");
    }

    public void MostrarCasoActual()
    {
        if (CasoActual == null) return;
        uiManager.CargarCaso(CasoActual);
    }

    public void SiguienteCaso()
    {
        indiceCasoActual++;
        if (indiceCasoActual >= listaCasos.Count)
        {
            Debug.Log("CaseManager: Todos los casos completados.");
            indiceCasoActual = listaCasos.Count - 1;
            return;
        }
        MostrarCasoActual();
    }

    public bool ValidarDecision(string decision)
    {
        if (CasoActual == null) return false;
        bool correcto = decision == CasoActual.decisionCorrecta;
        Debug.Log($"Decisión: {decision} | Correcta: {CasoActual.decisionCorrecta} | Resultado: {(correcto ? "✔ CORRECTO" : "✘ INCORRECTO")}");
        return correcto;
    }

    public string ObtenerRespuesta(string campo)
    {
        if (CasoActual == null) return "";
        return campo switch
        {
            "actividad"         => CasoActual.respuestasCliente.actividad,
            "origen_fondos"     => CasoActual.respuestasCliente.origen_fondos,
            "esPEP"             => CasoActual.respuestasCliente.esPEP,
            "cuentasExtranjero" => CasoActual.respuestasCliente.cuentasExtranjero,
            _                   => "El cliente no responde."
        };
    }
}
