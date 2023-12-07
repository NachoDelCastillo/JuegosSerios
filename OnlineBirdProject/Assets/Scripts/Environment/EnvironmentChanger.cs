using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentChanger : MonoBehaviour
{
    static public EnvironmentChanger Instance;
    public EnvironmentChanger GetInstance()
    { return Instance;  }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }



    [SerializeField]
    Transform[] waterFalls;

    [SerializeField]
    Transform water;
    float waterInitialY;

    [SerializeField]
    Transform greenTerrain;

    [SerializeField]
    Transform bigRocks;

    [SerializeField]
    Material greenRocks;
    [SerializeField]
    Material greyRocks;



    private void Start()
    {
        waterInitialY = water.position.y;

        SetFirstLevel();

        // SetSecondLevel();
        //SetThirdLevel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetFirstLevel();
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SetSecondLevel();
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SetThirdLevel();
    }

    void SetFirstLevel()
    {
        // Cascadas
        foreach (Transform t in waterFalls)
            t.gameObject.SetActive(true);

        water.gameObject.SetActive(true);
        water.position = new Vector3(water.position.x, waterInitialY, water.position.z);

        MeshRenderer[] rocksMeshRenderers = bigRocks.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < rocksMeshRenderers.Length; i++)
            rocksMeshRenderers[i].material = greenRocks;
    }

    void SetSecondLevel()
    {
        // Cascadas
        foreach (Transform t in waterFalls)
            t.gameObject.SetActive(false);

        water.gameObject.SetActive(true);
        water.position = new Vector3(water.position.x, waterInitialY - 30, water.position.z);

        MeshRenderer[] rocksMeshRenderers = bigRocks.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < rocksMeshRenderers.Length; i++)
            rocksMeshRenderers[i].material = greyRocks;
    }

    void SetThirdLevel()
    {
        foreach (Transform t in waterFalls)
            t.gameObject.SetActive(false);

        water.gameObject.SetActive(false);

        MeshRenderer[] rocksMeshRenderers = bigRocks.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < rocksMeshRenderers.Length; i++)
            rocksMeshRenderers[i].material = greyRocks;
    }
}
