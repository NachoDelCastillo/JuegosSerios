using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{

    public List<float> timesForPath;
    private int pathIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(activeGroup());
    }

    IEnumerator activeGroup()
    {
        if (pathIndex < timesForPath.Count-1)
        {
            for(int i=pathIndex; i < timesForPath.Count;i++)
            {
                yield return new WaitForSeconds(timesForPath[pathIndex]);
                //Sumamos el path
                //Si todav�a no hemos terminado todos los grupos de comida, activamos el siguiente y esperamos el tiempo
                //indicado en la lista para cada grupo de comida.
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        else StopCoroutine(activeGroup());

    }
}
