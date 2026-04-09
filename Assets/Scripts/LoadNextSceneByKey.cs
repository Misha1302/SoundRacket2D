using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextSceneByKey : MonoBehaviour
{
    [SerializeField] private KeyCode key;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
