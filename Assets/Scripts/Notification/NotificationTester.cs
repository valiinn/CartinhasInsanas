using UnityEngine;

public class NotificationTester : MonoBehaviour
{
    void Update()
    {
        // Quando apertar a tecla N, mostra uma notificação
        if (Input.GetKeyDown(KeyCode.N))
        {
            NotificationManager.Instance.Show(
                "Nova Notificação!",
                "Você apertou a tecla N.",
                3f // duração de 3 segundos
            );
        }
    }
}
