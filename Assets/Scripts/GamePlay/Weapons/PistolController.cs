using System.Threading;
using Unity.Entities.UniversalDelegates;
using Unity.Netcode;
using UnityEngine;

public class PistolController : Item
{

    private Rigidbody rb;
    private Outline outlineBehaviour;

    public GameObject bulletPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        outlineBehaviour = GetComponent<Outline>();
        outlineBehaviour.enabled = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!GameManager.localPlayer) return;

        var playerController = GameManager.localPlayer.GetComponent<InventoryController>();
        if (GameObject.ReferenceEquals(playerController.hoveredItem, gameObject))
        {
            outlineBehaviour.enabled = true;
        }
        else
        {
            outlineBehaviour.enabled = false;
        }
    }

    public override void Drop()
    {
        //Toss the item
        rb.isKinematic = false;
        rb.AddForce(Camera.main.transform.forward * 5f, ForceMode.Impulse);
    }

    public override void Pickup()
    {
        rb.isKinematic = true;
    }

    public override void Use()
    {
        //Shoot the gun
        ShootRpc();
    }

    [Rpc(SendTo.Server)]
    private void ShootRpc()
    {
        GameObject bullet = Instantiate(bulletPrefab);

        bullet.transform.position = transform.position + transform.forward * 1.5f;
        bullet.transform.rotation = transform.rotation;

        NetworkObject bulletNetworkObject = bullet.GetComponent<NetworkObject>();
        bulletNetworkObject.Spawn();

        
    }
}
