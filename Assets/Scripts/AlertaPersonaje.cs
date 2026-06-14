using UnityEngine;
using UnityEngine.UIElements;

public class AlertaPersonaje : MonoBehaviour
{
    [Header("Badge de alerta")]
    public GameObject badgeAlerta;  // objeto 3D flotante sobre el personaje
    public GameObject badgeVerificado; // checkmark verde opcional

    public void MostrarAlerta(bool fotoNoCoincide)
    {
        if (badgeAlerta != null)
            badgeAlerta.SetActive(fotoNoCoincide);

        if (badgeVerificado != null)
            badgeVerificado.SetActive(!fotoNoCoincide);
    }
}