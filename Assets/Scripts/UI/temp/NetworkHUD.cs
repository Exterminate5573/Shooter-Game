using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Multiplayer.Playmode;
using System.Threading.Tasks;
using System.Threading;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using Unity.Collections;

[RequireComponent(typeof(NetworkManager))]
[DisallowMultipleComponent]
public class NetworkHUD : MonoBehaviour
{

    GUIStyle m_LabelTextStyle;

    string m_JoinCode = "";

    public Vector2 DrawOffset = new Vector2(10, 10);

    public Color LabelColor = Color.black;

    void Awake()
    {
        m_LabelTextStyle = new GUIStyle(GUIStyle.none);
    }

    void OnGUI()
    {
        m_LabelTextStyle.normal.textColor = LabelColor;

        GUILayout.BeginArea(new Rect(DrawOffset, new Vector2(200, 200)));

        if (IsRunning(NetworkManager.Singleton))
        {
            DrawStatusGUI();
        }
        else
        {
            DrawConnectGUI();
        }

        GUILayout.EndArea();
    }

    void DrawConnectGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Label("Join Code: ", m_LabelTextStyle);

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        m_JoinCode = GUILayout.TextField(m_JoinCode);

        GUILayout.EndHorizontal();

        if (GUILayout.Button("Host"))
        {

            //Run the host task
            StartHostWithRelay().ContinueWith((task) =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"Failed to start host: {task.Exception}");
                }
                else
                {
                    Debug.Log($"Host started with join code: {task.Result}");
                    m_JoinCode = task.Result;
                }
            });
        }

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Join"))
        {
            //Run the join task
            StartClientWithRelay(m_JoinCode).ContinueWith((task) =>
            {
                if (task.IsFaulted || !task.Result)
                {
                    Debug.LogError($"Failed to start client: {task.Exception}");
                }
                else
                {
                    Debug.Log($"Client started successfully!");
                }
            });
        }

        GUILayout.EndHorizontal();
    }

    void DrawStatusGUI()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var mode = NetworkManager.Singleton.IsHost ? "Hosting" : "Server";
            GUILayout.Label($"{mode} with code: {m_JoinCode}", m_LabelTextStyle);
        }
        else
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                GUILayout.Label($"Client connected to {m_JoinCode}", m_LabelTextStyle);
            }
        }

        if (GUILayout.Button("Shutdown"))
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IsRunning(NetworkManager networkManager) => networkManager.IsServer || networkManager.IsClient;

    /// <summary>
    /// Starts a game host with a relay allocation: it initializes the Unity services, signs in anonymously and starts the host with a new relay allocation.
    /// </summary>
    /// <param name="maxConnections">Maximum number of connections to the created relay.</param>
    /// <returns>The join code that a client can use.</returns>
    /// <exception cref="ServicesInitializationException"> Exception when there's an error during services initialization </exception>
    /// <exception cref="UnityProjectNotLinkedException"> Exception when the project is not linked to a cloud project id </exception>
    /// <exception cref="CircularDependencyException"> Exception when two registered <see cref="IInitializablePackage"/> depend on the other </exception>
    /// <exception cref="AuthenticationException"> The task fails with the exception when the task cannot complete successfully due to Authentication specific errors. </exception>
    /// <exception cref="RequestFailedException"> See <see cref="IAuthenticationService.SignInAnonymouslyAsync"/></exception>
    /// <exception cref="ArgumentException">Thrown when the maxConnections argument fails validation in Relay Service SDK.</exception>
    /// <exception cref="RelayServiceException">Thrown when the request successfully reach the Relay Allocation service but results in an error.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the UnityTransport component cannot be found.</exception>
    public async Task<string> StartHostWithRelay(int maxConnections=5)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    /// <summary>
    /// Joins a game with relay: it will initialize the Unity services, sign in anonymously, join the relay with the given join code and start the client.
    /// </summary>
    /// <param name="joinCode">The join code of the allocation</param>
    /// <returns>True if starting the client was successful</returns>
    /// <exception cref="ServicesInitializationException"> Exception when there's an error during services initialization </exception>
    /// <exception cref="UnityProjectNotLinkedException"> Exception when the project is not linked to a cloud project id </exception>
    /// <exception cref="CircularDependencyException"> Exception when two registered <see cref="IInitializablePackage"/> depend on the other </exception>
    /// <exception cref="AuthenticationException"> The task fails with the exception when the task cannot complete successfully due to Authentication specific errors. </exception>
    /// <exception cref="RequestFailedException">Thrown when the request does not reach the Relay Allocation service.</exception>
    /// <exception cref="ArgumentException">Thrown if the joinCode has the wrong format.</exception>
    /// <exception cref="RelayServiceException">Thrown when the request successfully reach the Relay Allocation service but results in an error.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the UnityTransport component cannot be found.</exception>
    public async Task<bool> StartClientWithRelay(string joinCode)
    {
        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }
}
