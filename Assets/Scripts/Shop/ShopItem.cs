using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public int id;
    public bool bought = false;

    private static int nextId = 0;

    void Awake()
    {
        if (id == 0) id = ++nextId; // gera ID simples por instância
    }
}
