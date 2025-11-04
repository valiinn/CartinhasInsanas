using UnityEngine;

public static class Notify
{
    public static void Show(string title, string message = "", float duration = 1f, NotificationType type = NotificationType.Info)
    {
        if (NotificationManager.Instance == null)
        {
            Debug.LogWarning($"[Notify] NotificationManager não encontrado na cena.");
            return;
        }

        NotificationManager.Instance.Show(title, message, duration, type);
    }

    // atalhos práticos:
    public static void Info(string message, float duration = 1f)
        => Show("Info", message, duration, NotificationType.Info);

    public static void Success(string message, float duration = 1f)
        => Show("Sucesso", message, duration, NotificationType.Success);

    public static void Warning(string message, float duration = 1f)
        => Show("Atenção", message, duration, NotificationType.Warning);

    public static void Error(string message, float duration = 1f)
        => Show("Erro", message, duration, NotificationType.Error);
}

/// Funções globais para mostrar notificações.
/// Facilita substituir Debug.Log() no projeto.