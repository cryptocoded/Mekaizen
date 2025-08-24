using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace Mechs.UI
{
    public class StatsPanelUI : MonoBehaviour
    {
        [SerializeField] private Mechs.MechRuntime mech;
        [SerializeField] private TMP_Text statsText;

        private void OnEnable()
        {
            if (mech != null) mech.StatsChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (mech != null) mech.StatsChanged -= Refresh;
        }

        public void Refresh()
        {
            if (mech == null || statsText == null) return;

            var sb = new StringBuilder();
            foreach (Mechs.StatType t in Enum.GetValues(typeof(Mechs.StatType)))
            {
                float v = mech.FinalStats.Get(t);
                sb.AppendLine($"{t}: {v:0.##}");
            }
            sb.AppendLine($"NetPower: {mech.NetPower:0.##}");
            statsText.text = sb.ToString();
        }
    }
}
