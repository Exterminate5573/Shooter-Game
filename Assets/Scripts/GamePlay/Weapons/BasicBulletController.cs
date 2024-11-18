using Unity.Entities.UniversalDelegates;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class BasicBulletController : NetworkBehaviour
{

    private Rigidbody rb;
    private NetworkTransform networkTransform;

    private long nanoTime = 0;

    void Awake()
    {
        networkTransform = GetComponent<NetworkTransform>();
        rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            nanoTime = System.DateTime.Now.Ticks;
        }

        rb.AddForce(transform.forward * 10f, ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            int seconds = (int)((System.DateTime.Now.Ticks - nanoTime) / 10000000);
            if (seconds > 5)
            {
                this.NetworkObject.Despawn();
            }
        }
    }
}
