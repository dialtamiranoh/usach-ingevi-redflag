using UnityEngine;

/// <summary>
/// Hace que la barra de vida en World Space siempre rote mirando de frente a la cámara activa.
/// </summary>
public class GerenteBillboard : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        ActualizarReferenciaCamara();
    }

    void LateUpdate()
    {
        // En RedFlag la cámara activa puede cambiar de vista, por lo que buscamos
        // la cámara principal si la que teníamos ya no es válida o está inactiva
        if (cam == null || !cam.isActiveAndEnabled)
        {
            ActualizarReferenciaCamara();
        }

        if (cam != null)
        {
            // Rota el Canvas para que mire en la misma dirección que la cámara
            transform.rotation = cam.transform.rotation;
        }
    }

    private void ActualizarReferenciaCamara()
    {
        // Busca la cámara principal de la escena
        cam = Camera.main;
        if (cam == null)
        {
            // Fallback si no está taggeada como MainCamera
            cam = FindAnyObjectByType<Camera>();
        }
    }
}
