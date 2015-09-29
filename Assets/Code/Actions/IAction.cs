using UnityEngine;
using System.Collections;

public interface IAction{

    void Execute();
    void ActiveUpdate();
    void QueueNextStep();
    void CancelNextStep();
    bool IsMultiStep();
    bool IsActive();
    bool CanTurn();
    bool UseRootMotion();
    bool AllowPlayerMotion();
    bool IsDisabled();
    bool IsQueueable();
}
