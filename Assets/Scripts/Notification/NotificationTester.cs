using UnityEngine;

public class NotificationTester : MonoBehaviour
{
    void Update()
{
    if (Input.GetKeyDown(KeyCode.N))
        Notify.Info("Você apertou N!");

    if (Input.GetKeyDown(KeyCode.M))
        Notify.Success("Nova carta comprada!");

    if (Input.GetKeyDown(KeyCode.B))
        Notify.Warning("Mana baixa!");

    if (Input.GetKeyDown(KeyCode.V))
        Notify.Error("Você foi atacado!");
}
}
