using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode.Components;

public abstract class Item : NetworkBehaviour
{

    [DoNotSerialize]
    public GameObject parent;

    protected virtual void Update() 
    {
        if (parent != null)
        {
            RotateItem();
        }
    }

    private void RotateItem() 
    {
        NetworkTransform networkTransform = GetComponent<NetworkTransform>();
        Transform parentOrientation = parent.transform.GetChild(0).transform;
        networkTransform.transform.position = parentOrientation.position + parentOrientation.forward * 2 + parentOrientation.right * 1.1f - parentOrientation.up * 0.5f;
        networkTransform.transform.rotation = parentOrientation.rotation;
    }

    public abstract void Use();
    public abstract void Drop();
    public abstract void Pickup();

}
