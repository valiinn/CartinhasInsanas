using UnityEngine;

[System.Serializable]
public class BuffDefinition
{
    public BuffType buffType;
    public string buffName;
    public Sprite icon;
    public int cost = 10;
    public GameObject buffPrefab; 
}

