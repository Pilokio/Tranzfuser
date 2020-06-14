using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Bullet Cleanup Parameters")]
    [SerializeField] private float TimeForBulletCheck = 1.0f;
    [SerializeField] private int MaxBulletsInSceneCount = 10;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CheckForBullets());
    }

    // Update is called once per frame
    void Update()
    {
        
    }


   


    //Coroutine for optimisation by removing bullets if there are too many in the scene
    //Currently just for debug as bullets will typically destroy themselves and spawn an impact decal or something?
    //Could be adapted for shell casings later on though?
    IEnumerator CheckForBullets()
    {
        while (true)
        {
            yield return new WaitForSeconds(TimeForBulletCheck);

            // Debug.Log("Checking Bullets");

            //Find all the bullets in the scene
            GameObject[] AllBulletsInScene = GameObject.FindGameObjectsWithTag("Bullet");

            //If there are more than the set number of bullets in the scene
            if (AllBulletsInScene.Length > MaxBulletsInSceneCount)
            {
                // Debug.Log("Too many Bullets. Removing some.");
                //Delete the bullets found up to the desired amount
                for (int i = 0; i < AllBulletsInScene.Length - MaxBulletsInSceneCount; i++)
                {
                    Destroy(AllBulletsInScene[i]);
                }
            }
        }
    }
}
