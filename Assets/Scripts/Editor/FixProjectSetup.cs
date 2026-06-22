#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class FixProjectSetup
{
    static FixProjectSetup()
    {
        EditorApplication.delayCall += DoFix;
    }

    static void DoFix()
    {
        if (EditorPrefs.GetBool("RedFlagFixesApplied_V2", false)) return;
        
        // 1. Fix the Cash prefab
        var cashPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Cash.prefab");
        if (cashPrefab != null)
        {
            var comp = cashPrefab.GetComponent<ObjetoSospechoso>();
            if (comp != null)
            {
                comp.categoria = ObjetoSospechoso.CategoriaObjeto.Soborno;
                EditorUtility.SetDirty(cashPrefab);
            }
        }
        
        // 2. Fix the scene (ObjetosManager references and AchievementManager component)
        var scene = SceneManager.GetActiveScene();
        if (scene.isLoaded)
        {
            // Add AchievementManager
            var gm = GameObject.Find("GameManager") ?? GameObject.Find("Managers") ?? Object.FindObjectOfType<GameManager>()?.gameObject;
            if (gm != null)
            {
                if (gm.GetComponent<AchievementManager>() == null)
                {
                    gm.AddComponent<AchievementManager>();
                    Debug.Log("[FIX] Added AchievementManager to " + gm.name);
                    EditorUtility.SetDirty(gm);
                }
            }

            // Fix ObjetosManager
            var manager = Object.FindObjectOfType<ObjetosManager>();
            if (manager != null)
            {
                manager.prefabPendrive = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/USB.prefab");
                manager.prefabCelular = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/smartphone2.prefab");
                manager.prefabPostIt = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/postit.prefab");
                manager.prefabDocumento = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/document.prefab");
                manager.prefabLlave = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/llave.prefab");
                manager.prefabTarjeta = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/tarjeta.prefab");
                manager.prefabCarpeta = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/carpeta.prefab");
                manager.prefabRegalo = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/regalo.prefab");
                manager.prefabSoborno = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Cash.prefab");
                
                EditorUtility.SetDirty(manager);
                Debug.Log("[FIX] Fixed ObjetosManager prefab references.");
            }
            
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        
        AssetDatabase.SaveAssets();
        EditorPrefs.SetBool("RedFlagFixesApplied_V2", true);
    }
}
#endif
