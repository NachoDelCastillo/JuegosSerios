using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Se encarga de actualizar el color de este jugador
// tanto en la escena de seleccion de personajes (desde CharacterSelect)
// como en la escena de juego al principio de la partida (desde PlayerManager)
public class PlayerVisual : MonoBehaviour
{
    [SerializeField] MeshRenderer headMeshRenderer;
    [SerializeField] SkinnedMeshRenderer bodyMeshRenderer;

    TrailRenderer[] trailRenderers;

    private void Awake()
    {
        //material = new Material(headMeshRenderer.material);

        //headMeshRenderer.material = material;
        //bodyMeshRenderer.material = material;

        trailRenderers = GetComponentsInChildren<TrailRenderer>();
    }

    public void SetPlayerMaterial(Material newMaterial)
    {
        //headMeshRenderer.material = material;
        bodyMeshRenderer.material = newMaterial;
    }

    public void SetNeonColorById(int id)
    {
        Material neonMaterial = OnlineMultiplayerManager.Instance.GetNeonMaterial(id);
        for (int i = 0; i < trailRenderers.Length; i++)
            trailRenderers[i].material = neonMaterial;
    }
}
