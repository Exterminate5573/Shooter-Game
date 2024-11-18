using Unity.Entities.UniversalDelegates;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class InventoryController : NetworkBehaviour
{

    //public GameObject[] inventory = new GameObject[5];
    public GameObject activeItem;

    public GameObject hoveredItem;

    private PlayerController playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*if (activeItem != null) {
            RotateItem();
        }*/
    }

    void RotateItem() {
        NetworkTransform networkTransform = activeItem.GetComponent<NetworkTransform>();
        networkTransform.transform.position = playerController.playerCam.position + playerController.playerCam.forward * 2 + playerController.playerCam.right * 1.1f - playerController.playerCam.up * 0.5f;
        networkTransform.transform.rotation = playerController.playerCam.rotation;
    }

    public void DropItem() {
        if (activeItem != null) {
            var itemComponent = activeItem.GetComponent<Item>();
            
            DropItemRpc(activeItem.GetComponent<NetworkObject>().NetworkObjectId);

            itemComponent.Drop();
            activeItem = null;
        }
    }

    public void PickupItem(GameObject item) {
        if (activeItem != null) {
            DropItem();
        }
        activeItem = item;

        var itemComponent = activeItem.GetComponent<Item>();
        
        PickupItemRpc(this.NetworkObjectId, activeItem.GetComponent<NetworkObject>().NetworkObjectId);

        itemComponent.Pickup();
    }

    public void UseItem() {
        if (activeItem != null) {
            activeItem.GetComponent<Item>().Use();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void DropItemRpc(ulong itemNetworkObjectId) {
        var item = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemNetworkObjectId].gameObject;
        var itemComponent = item.GetComponent<Item>();
        itemComponent.parent = null;
    }

    [Rpc(SendTo.Everyone)]
    private void PickupItemRpc(ulong playerNetworkObjectId, ulong itemNetworkObjectId) {
        var item = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemNetworkObjectId].gameObject;
        var player = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
        var itemComponent = item.GetComponent<Item>();
        itemComponent.parent = player;
    }

}
