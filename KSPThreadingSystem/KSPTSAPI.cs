using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSPThreadingSystem
{
    /// <summary>
    /// Used to interface with KSPTS to prevent interference between plugins and the system
    /// </summary>
    public static class KSPTSAPI
    {
        public static void RegisterNewAction(KSPTSThreadingGroups threadingGroup, Action newAction)
        {
            KSPTSThreadController.KSPTSRegisteredActions registeredActions = KSPTSThreadController.instance.registeredActions;

            switch (threadingGroup)
            {
                case KSPTSThreadingGroups.IN_LOOP_UPDATE:
                registeredActions.inLoop_Update_Actions.Add(newAction);
                return;

                case KSPTSThreadingGroups.IN_LOOP_LATE_UPDATE:
                registeredActions.inLoop_LateUpdate_Actions.Add(newAction);
                return;

                case KSPTSThreadingGroups.IN_LOOP_FIXED_UPDATE:
                registeredActions.inLoop_FixedUpdate_Actions.Add(newAction);
                return;

                case KSPTSThreadingGroups.ACROSS_LOOP_UPDATE:
                registeredActions.acrossLoop_Update_Actions.Add(newAction);
                return;

                case KSPTSThreadingGroups.ACROSS_LOOP_LATE_UPDATE:
                registeredActions.acrossLoop_LateUpdate_Actions.Add(newAction);
                return;

                case KSPTSThreadingGroups.ACROSS_LOOP_FIXED_UPDATE:
                registeredActions.acrossLoop_FixedUpdate_Actions.Add(newAction);
                return;

                default:
                Debug.LogError("KSPTSThreadingGroup Enum Error");
                return;
            }
        }

        public static void UnregisterAction(KSPTSThreadingGroups threadingGroup, Action actionToRemove)
        {
            KSPTSThreadController.KSPTSRegisteredActions registeredActions = KSPTSThreadController.instance.registeredActions;

            switch (threadingGroup)
            {
                case KSPTSThreadingGroups.IN_LOOP_UPDATE:
                    registeredActions.inLoop_Update_Actions.Remove(actionToRemove);
                    return;

                case KSPTSThreadingGroups.IN_LOOP_LATE_UPDATE:
                    registeredActions.inLoop_LateUpdate_Actions.Remove(actionToRemove);
                    return;

                case KSPTSThreadingGroups.IN_LOOP_FIXED_UPDATE:
                    registeredActions.inLoop_FixedUpdate_Actions.Remove(actionToRemove);
                    return;

                case KSPTSThreadingGroups.ACROSS_LOOP_UPDATE:
                    registeredActions.acrossLoop_Update_Actions.Remove(actionToRemove);
                    return;

                case KSPTSThreadingGroups.ACROSS_LOOP_LATE_UPDATE:
                    registeredActions.acrossLoop_LateUpdate_Actions.Remove(actionToRemove);
                    return;

                case KSPTSThreadingGroups.ACROSS_LOOP_FIXED_UPDATE:
                    registeredActions.acrossLoop_FixedUpdate_Actions.Remove(actionToRemove);
                    return;

                default:
                    Debug.LogError("KSPTSThreadingGroup Enum Error");
                    return;
            }
        }
    }

    public enum KSPTSThreadingGroups
    {
        IN_LOOP_UPDATE,
        IN_LOOP_LATE_UPDATE,
        IN_LOOP_FIXED_UPDATE,
        ACROSS_LOOP_UPDATE,
        ACROSS_LOOP_LATE_UPDATE,
        ACROSS_LOOP_FIXED_UPDATE
    }
}
