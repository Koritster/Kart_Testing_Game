using UnityEngine;
using static SpecialEffectItemClass;


#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(SpecialEffectItemClass))]
public class SpecialEffectItemEditor : ItemClassEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.LabelField("Special Effect Properties", EditorStyles.boldLabel);

        SpecialEffectItemClass item = (SpecialEffectItemClass)target;

        item.type = (EffectType)EditorGUILayout.EnumPopup("Effect Type", item.type);

        if (item.type == EffectType.Shield)
        {
            item.shieldHits = EditorGUILayout.IntField("Shield Hits", item.shieldHits);
            item.duration = EditorGUILayout.FloatField("Duration", item.duration);
        }

        if (item.type == EffectType.Invulnerability)
        {
            item.duration = EditorGUILayout.FloatField("Duration", item.duration);
        }

        item.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", item.prefab, typeof(GameObject), false);

        if (GUI.changed)
            EditorUtility.SetDirty(item);
    }
}

#endif

[CreateAssetMenu(fileName = "New Special Effect Item", menuName = "Item/Efecto especial")]
public class SpecialEffectItemClass : ItemClass
{
    public enum EffectType
    {
        Shield,
        Invulnerability
    }

    [Header("Special Effect Properties")]
    public EffectType type;
    public int shieldHits;
    public float duration;
    public GameObject prefab;

    public override void UseItem(CarController caller)
    {
        base.UseItem(caller);
    }

    public override SpecialEffectItemClass GetSpecialEffectItem() { return this; }
}

