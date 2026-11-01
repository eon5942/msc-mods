using System;
using System.Collections.Generic;
using System.Reflection;
using MSCLoader;
using UnityEngine;
using HutongGames.PlayMaker;

public class QuickSave : Mod
{
    private const string HoldState = "(QuickSave)";

    private List<KeyValuePair<PlayMakerFSM, string>> restore;
    private Dictionary<PlayMakerFSM, string> afterSave;
    private float restoreAt;
    private MethodInfo switchState;

    public override string ID { get { return "QuickSave"; } }
    public override string Name { get { return "Quick Save (Y)"; } }
    public override string Author { get { return "eon"; } }
    public override string Version { get { return "2.3"; } }
    public override string Description { get { return "Press Y to save"; } }
    public override Game SupportedGames { get { return Game.MySummerCar; } }

    public override void ModSetup()
    {
        SetupFunction(Setup.Update, Update);
    }

    private void Update()
    {
        if (restore != null && Time.realtimeSinceStartup >= restoreAt)
        {
            Restore();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.Z))
            Save();
    }

    private void Save()
    {
        if (restore != null)
            return;
        if (Application.loadedLevelName != "GAME")
            return;

        List<KeyValuePair<PlayMakerFSM, string>> states = new List<KeyValuePair<PlayMakerFSM, string>>();
        PlayMakerFSM setup = null;

        foreach (PlayMakerFSM f in UnityEngine.Object.FindObjectsOfType<PlayMakerFSM>())
        {
            if (f == null || f.Fsm == null)
                continue;
            string state = f.ActiveStateName;
            if (string.IsNullOrEmpty(state))
                continue;

            bool listens = false;
            FsmTransition[] transitions = f.Fsm.GlobalTransitions;
            if (transitions != null)
                foreach (FsmTransition t in transitions)
                    if (t != null && t.EventName == "SAVEGAME") { listens = true; break; }
            if (!listens)
                continue;

            if (setup == null && f.FsmName == "WaitPlayer" && f.gameObject != null &&
                f.gameObject.name == "Setup Game" && (state == "Activate game" || state == "Save game"))
                setup = f;
            else
                states.Add(new KeyValuePair<PlayMakerFSM, string>(f, state));
        }

        FsmTransition held = null;
        string heldTo = null;
        if (setup != null)
        {
            FsmState sg = setup.Fsm.GetState("Save game");
            if (sg != null && sg.Transitions != null)
                foreach (FsmTransition t in sg.Transitions)
                    if (t != null && t.EventName == "FINISHED") { held = t; heldTo = t.ToState; break; }
            if (held == null)
            {
                states.Add(new KeyValuePair<PlayMakerFSM, string>(setup, setup.ActiveStateName));
                setup = null;
            }
            else
            {
                held.ToState = HoldState;
            }
        }

        try
        {
            PlayMakerFSM.BroadcastEvent("SAVEGAME");
        }
        finally
        {
            if (held != null)
                held.ToState = heldTo;
        }

        afterSave = new Dictionary<PlayMakerFSM, string>();
        foreach (KeyValuePair<PlayMakerFSM, string> kv in states)
            if (kv.Key != null)
                afterSave[kv.Key] = kv.Key.ActiveStateName;

        restore = states;
        restoreAt = Time.realtimeSinceStartup + 2.5f;
    }

    private void Restore()
    {
        foreach (KeyValuePair<PlayMakerFSM, string> kv in restore)
        {
            PlayMakerFSM f = kv.Key;
            if (f == null || f.Fsm == null)
                continue;
            if (f.ActiveStateName == kv.Value)
                continue;
            string saved;
            if (afterSave != null && afterSave.TryGetValue(f, out saved) && f.ActiveStateName != saved)
                continue;
            SetState(f, kv.Value);
        }
        restore = null;
        afterSave = null;
    }

    private void SetState(PlayMakerFSM f, string state)
    {
        FsmState s = f.Fsm.GetState(state);
        if (s == null)
            return;
        if (switchState == null)
            switchState = typeof(Fsm).GetMethod("SwitchState", BindingFlags.NonPublic | BindingFlags.Instance);
        if (switchState != null)
            switchState.Invoke(f.Fsm, new object[] { s });
    }
}
