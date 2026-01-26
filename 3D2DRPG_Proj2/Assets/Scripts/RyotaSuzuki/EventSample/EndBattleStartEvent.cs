using UnityEngine;

public class EndBattleStartEvent : MonoBehaviour
{
    [SerializeField] string TestEventID = ""; 
    // Start is called before the first frame update
    void Start()
    {
        string EventID = GameManager.Instance.SetEventIDFlag();

        //シーン遷移時に一度だけイベントを発火するか決める
        if (string.IsNullOrEmpty(EventID)) return;

        SimpleEventTrigger[] triggers =
        FindObjectsOfType<SimpleEventTrigger>();

        foreach (var trigger in triggers)
        {
            Debug.Log(trigger.eventId);
            if (trigger.eventId == EventID)
            {
                trigger.StartEvent(EventID);
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("イベントを発火（テスト）")]
    void EventTest()
    {
        SimpleEventTrigger[] triggers =
        FindObjectsOfType<SimpleEventTrigger>();

        foreach (var trigger in triggers)
        {
            if (trigger.eventId == TestEventID)
            {
                trigger.StartEvent(TestEventID);
                break;
            }
        }
    }
}
