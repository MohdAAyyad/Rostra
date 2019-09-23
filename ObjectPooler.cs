using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectPooler : MonoBehaviour
{
    //Pool class
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int poolSize;
    }

    #region Singleton

    //A singleton is a design pattern that makes sure only one instance of a class is used at all times
    public static ObjectPooler instance;

    public void Awake()
    {
        if(instance==null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    //A list of all the pools we have
    public List<Pool> listOfPools;

    //A Dictionary that stores queues of gameobjects. The queues will be used to transfer the gameobjects into the world
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach(Pool p in listOfPools)
        {
            //Create a queue for each pool
            Queue<GameObject> gameObjectQueue = new Queue<GameObject>();
            
            for(int i =0;i<p.poolSize;i++)
            {
                //Fill the queue with the prefabs
                GameObject obj = Instantiate(p.prefab);
                obj.SetActive(false);
                gameObjectQueue.Enqueue(obj);
            }

            //Add the queue to the dictionary
            poolDictionary.Add(p.tag, gameObjectQueue);
        }

        //SpawnFromPool("Giant", new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
    }

    public GameObject SpawnFromPool(string tag,Vector3 position, Quaternion rotation)
    {
        Debug.Log("Summon demo attack");
        if (poolDictionary.ContainsKey(tag))
        {

            //Get the object from the dictionary
            GameObject objectToSpawn = poolDictionary[tag].Dequeue();

            //Transform the object to the desired position and rotation
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.SetActive(true);

            //Add the objecttospawn to the queue again so that it can be reused later on
            poolDictionary[tag].Enqueue(objectToSpawn);

            //Yes, you can do a getcomponent of interfaces.
            //This is only for test purposes, however, if you can get rid of the getcomponent, it would be better
            IPooledObject pooledObject;
            pooledObject = objectToSpawn.GetComponent<IPooledObject>();

            if(pooledObject!=null)
            {
                pooledObject.OnSpawn();
            }

            return objectToSpawn;
        }
        else
        {
            Debug.LogWarning("Could not find object with tag" + tag);
            return null;
        }

    }
}
