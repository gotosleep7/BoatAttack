using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetScreenResolution : MonoBehaviour
{
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        Screen.SetResolution(Screen.width, Screen.height, true);
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
