using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private int targetFPS = 60;
    public  List<GameObject> listOfUndestroyables; //Stores a list of the objects that are set to do not destroy on load
    public static GameManager instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            Application.targetFrameRate = targetFPS;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        if(instance == this)
        {
            instance = null;
        }
    }

    private void Start()
    {
         listOfUndestroyables = new List<GameObject>();   
    }

    private void Update()
    {
        if (Application.targetFrameRate != targetFPS)
        {
            Application.targetFrameRate = targetFPS;
        }
    }

    public void DestoryUndestroyables() //Destry the undestroyables. Called when we go back to main menu
    {
        foreach(GameObject g in listOfUndestroyables)
        {
            Destroy(g);
        }

        listOfUndestroyables.Clear();
        Destroy(this.gameObject);
    }
}
