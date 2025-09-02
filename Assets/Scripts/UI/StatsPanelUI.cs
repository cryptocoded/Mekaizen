using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace Mechs.UI
{
    public class StatsPanelUI : MonoBehaviour
    {
        [SerializeField] private MechRuntime mech;     // assign in Inspector, or it will auto-find
        [SerializeField] private TMP_Text statsText;   // assign, or it will auto-find in children

        private void OnEnable()
        {
            TryBind();
            if (mech != null) mech.StatsChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (mech != null) mech.StatsChanged -= Refresh;
        }

        private void TryBind()
        {
            if (mech == null)
            {
#if UNITY_2023_1_OR_NEWER
                mech = FindFirstObjectByType<MechRuntime>();
#else
                mech = FindObjectOfType<MechRuntime>();
#endif
                if (mech == null)
                    Debug.LogWarning($"[StatsPanelUI] No MechRuntime found. Assign one on {name}.");
            }

            if (statsText == null)
            {
                statsText = GetComponentInChildren<TMP_Text>(true);
                if (statsText == null)
                    Debug.LogWarning($"[StatsPanelUI] No TMP_Text assigned/found on {name}.");
            }
        }

        public void Refresh()
        {
            if (mech == null || statsText == null) return;

            var sb = new StringBuilder();
            foreach (StatType t in Enum.GetValues(typeof(StatType)))
                sb.AppendLine($"{t}: {mech.FinalStats.Get(t):0.##}");
            sb.AppendLine($"NetPower: {mech.NetPower:0.##}");
            statsText.text = sb.ToString();
        }

        // Optional: lets other scripts bind the mech explicitly
        public void SetMech(MechRuntime m)
        {
            if (mech == m) { Refresh(); return; }
            if (mech != null) mech.StatsChanged -= Refresh;
            mech = m;
            if (isActiveAndEnabled && mech != null) mech.StatsChanged += Refresh;
            Refresh();
        }

        [ContextMenu("Debug Dump")]
        private void DebugDump()
        {
            Refresh();
            Debug.Log($"[StatsPanelUI] BoundMech={(mech ? mech.name : "null")}\n{statsText?.text}");
        }
    }
}
