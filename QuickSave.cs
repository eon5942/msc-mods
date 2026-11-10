using System;
using System.Collections.Generic;
using System.Reflection;
using MSCLoader;
using UnityEngine;
using HutongGames.PlayMaker;

public class QuickSave : Mod
{
    private const string HoldState = "(QuickSave)";
    private const float RestoreDelay = 2.5f;

    private List<KeyValuePair<PlayMakerFSM, string>> restore;
    private Dictionary<PlayMakerFSM, string> afterSave;
    private List<FsmTransition> heldTransitions;
    private List<string> heldTargets;
    private float restoreAt;
    private MethodInfo switchState;
    private FieldInfo baseVerticesField;
    private FieldInfo baseColorsField;

    public override string ID { get { return "QuickSave"; } }
    public override string Name { get { return "Quick Save (Y)"; } }
    public override string Author { get { return "eon"; } }
    public override string Version { get { return "2.7"; } }
    public override string Description { get { return "Press Y to save, B to fix body"; } }
    public override Game SupportedGames { get { return Game.MySummerCar; } }

    public override void ModSetup()
    {
        SetupFunction(Setup.Update, Update);
    }

    private void Update()
    {
        if (restore != null)
        {
            if (Time.realtimeSinceStartup >= restoreAt)
                Restore();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.Z))
            Save();

        if (Input.GetKeyDown(KeyCode.B))
            RepairBody();
    }

    private void RepairBody()
    {
        foreach (Deformable d in UnityEngine.Object.FindObjectsOfType<Deformable>())
        {
            if (d == null || d.meshFilter == null)
                continue;

            Mesh m = d.meshFilter.mesh;
            if (m == null)
                continue;

            if (baseVerticesField == null)
                baseVerticesField = typeof(Deformable).GetField("baseVertices", BindingFlags.NonPublic | BindingFlags.Instance);
            Vector3[] baseVerts = baseVerticesField == null ? null : (Vector3[])baseVerticesField.GetValue(d);
            if (baseVerts == null || baseVerts.Length == 0)
                continue;

            m.vertices = baseVerts;

            if (baseColorsField == null)
                baseColorsField = typeof(Deformable).GetField("baseColors", BindingFlags.NonPublic | BindingFlags.Instance);
            Color32[] baseCols = baseColorsField == null ? null : (Color32[])baseColorsField.GetValue(d);
            if (baseCols != null && baseCols.Length > 0)
                m.colors32 = baseCols;

            m.RecalculateNormals();
            m.RecalculateBounds();
        }
    }

    private void Save()
    {
        if (Application.loadedLevelName != "GAME")
            return;

        List<KeyValuePair<PlayMakerFSM, string>> states = new List<KeyValuePair<PlayMakerFSM, string>>();
        List<FsmTransition> held = new List<FsmTransition>();
        List<string> heldTo = new List<string>();

        foreach (PlayMakerFSM f in UnityEngine.Object.FindObjectsOfType<PlayMakerFSM>())
        {
            if (f == null || f.Fsm == null)
                continue;
            if (f.Fsm.States == null || f.Fsm.States.Length == 0)
                continue;
            string state = f.ActiveStateName;
            if (string.IsNullOrEmpty(state))
                continue;

            if (!ListensForSave(f))
                continue;

            states.Add(new KeyValuePair<PlayMakerFSM, string>(f, state));

            FsmState sg = f.Fsm.GetState("Save game");
            if (sg != null && sg.Transitions != null)
                foreach (FsmTransition t in sg.Transitions)
                    if (t != null && t.EventName == "FINISHED")
                    {
                        held.Add(t);
                        heldTo.Add(t.ToState);
                        t.ToState = HoldState;
                        break;
                    }
        }

        PlayMakerFSM.BroadcastEvent("SAVEGAME");

        afterSave = new Dictionary<PlayMakerFSM, string>(states.Count);
        foreach (KeyValuePair<PlayMakerFSM, string> kv in states)
            if (kv.Key != null)
                afterSave[kv.Key] = kv.Key.ActiveStateName;

        heldTransitions = held;
        heldTargets = heldTo;
        restore = states;
        restoreAt = Time.realtimeSinceStartup + RestoreDelay;
    }

    private static bool ListensForSave(PlayMakerFSM f)
    {
        FsmTransition[] transitions = f.Fsm.GlobalTransitions;
        if (transitions == null)
            return false;
        for (int i = 0; i < transitions.Length; i++)
        {
            FsmTransition t = transitions[i];
            if (t != null && t.EventName == "SAVEGAME")
                return true;
        }
        return false;
    }

    private void Restore()
    {
        if (restore != null)
        {
            foreach (KeyValuePair<PlayMakerFSM, string> kv in restore)
            {
                PlayMakerFSM f = kv.Key;
                if (f == null || f.Fsm == null)
                    continue;
                if (f.Fsm.States == null || f.Fsm.States.Length == 0)
                    continue;
                if (f.ActiveStateName == kv.Value)
                    continue;
                string saved;
                if (afterSave != null && afterSave.TryGetValue(f, out saved) && f.ActiveStateName != saved)
                    continue;
                SetState(f, kv.Value);
            }
        }

        if (heldTransitions != null)
        {
            for (int i = 0; i < heldTransitions.Count; i++)
                if (heldTransitions[i] != null)
                    heldTransitions[i].ToState = heldTargets[i];
        }

        restore = null;
        afterSave = null;
        heldTransitions = null;
        heldTargets = null;
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
