using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{

    [DoNotSerialize]
    public static GameObject localPlayer;

    async void Start()
    {
        Debug.Log("UnityAuthManager Start");
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            //TODO: Add account management
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /*void Start()
    {

    }*/

    // Update is called once per frame
    void Update()
    {

        ClientUpdate();

        if (NetworkManager.Singleton.IsServer)
        {
            ServerUpdate();
        }
        
    }

    void ClientUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None; //Unlock the cursor
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Cursor.lockState = CursorLockMode.Locked; //Lock the cursor
        }
    }

    void ServerUpdate()
    {
        
    }
}
