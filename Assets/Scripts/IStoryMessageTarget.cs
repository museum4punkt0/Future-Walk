using UnityEngine.EventSystems;

public interface IStoryMessageTarget : IEventSystemHandler
{
    // functions that can be called via the messaging system
    void Reset();
}