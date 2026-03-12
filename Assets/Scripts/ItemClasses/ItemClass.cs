using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ItemClass))]
public class ItemClassEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ItemClass item = (ItemClass)target;

        EditorGUILayout.LabelField("Item General Properties", EditorStyles.boldLabel);
        
        item.name = EditorGUILayout.TextField("Name", item.name);
        item.itemId = EditorGUILayout.IntField("ID", item.itemId);
        item.itemIcon = (Sprite)EditorGUILayout.ObjectField("Icon", item.itemIcon, typeof(Sprite), false);

        if (GUI.changed)
            EditorUtility.SetDirty(item);
    }
}

#endif


public class ItemClass : ScriptableObject
{
    public string name;
    public int itemId;
    public Sprite itemIcon;

    public virtual void UseItem(CarController caller)
    {
        Debug.Log("Used: " + name);
    }

    public virtual ItemClass GetItem() { return this; }
    public virtual ShootItemClass GetShootItem() { return null; }
    public virtual SpecialEffectItemClass GetSpecialEffectItem() { return null; }
}
