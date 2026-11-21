using System.Collections;
using TMPro;
using UnityEngine;

namespace MycoMenuLib;

/// Keeps a generated option's label after Option.OnEnable resolves the
/// label through TextBlocks using the generated id.
internal class OptionLabelOverride : MonoBehaviour
{
    internal TMP_Text Label;
    internal string DisplayName;

    private void OnEnable()
    {
        Apply();
        StartCoroutine(RepairAfterEnable());
    }

    private void Start()
    {
        Apply();
    }

    private IEnumerator RepairAfterEnable()
    {
        yield return null;
        Apply();
        yield return null;
        Apply();
    }

    private void Apply()
    {
        if (Label != null && !string.IsNullOrEmpty(DisplayName))
            Label.text = DisplayName;
    }
}
