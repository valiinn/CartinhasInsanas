using UnityEngine;
using System;
using System.Collections.Generic;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [Header("Setup")]
    public RectTransform container;          // NotificationsContainer (onde as notificações aparecem)
    public GameObject notificationPrefab;    // prefab criado
    public int initialPool = 6;

    List<NotificationUI> pool = new List<NotificationUI>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitPool()
    {
        if (notificationPrefab == null || container == null) return;
        for (int i = 0; i < initialPool; i++)
        {
            var go = Instantiate(notificationPrefab, container);
            go.SetActive(false);
            pool.Add(go.GetComponent<NotificationUI>());
        }
    }

    NotificationUI GetFromPool()
    {
        // procura inativo
        foreach (var n in pool)
            if (!n.gameObject.activeSelf) return n;

        // se nenhum disponível, instancia mais (auto-grow)
        var g = Instantiate(notificationPrefab, container);
        var ui = g.GetComponent<NotificationUI>();
        pool.Add(ui);
        return ui;
    }

    /// <summary>
    /// Chamada pública para mostrar uma notificação
    /// </summary>
    public void Show(string title, string message, float duration = 3f, NotificationType type = NotificationType.Info, Action onClick = null)
    {
        if (notificationPrefab == null || container == null)
        {
            Debug.LogWarning("NotificationManager não configurado (prefab/container faltando).");
            return;
        }

        var ui = GetFromPool();
        ui.gameObject.SetActive(true);
        // newest no topo:
        ui.transform.SetAsFirstSibling();
        ui.Setup(title, message, duration, type, onClick);
    }

    // usado pelo NotificationUI para devolver o item ao pool
    public void ReturnToPool(NotificationUI ui)
    {
        ui.gameObject.SetActive(false);
    }

    public void ClearAll()
    {
        foreach (var n in pool)
            if (n.gameObject.activeSelf)
                n.HideNow();
    }
}
