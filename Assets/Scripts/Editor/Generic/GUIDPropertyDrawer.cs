using UnityEditor;
using UnityEngine;
using GUID = UnityProgrammerTask.GUID;

namespace UnityProgrammerTaskEditor
{
   [CustomPropertyDrawer(typeof(GUID))]
   public class GUIDPropertyDrawer : PropertyDrawer
   {
      public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
      {
         // Find the serialized fields
         SerializedProperty highProp = property.FindPropertyRelative("m_High");
         SerializedProperty lowProp = property.FindPropertyRelative("m_Low");

         GUID guid = new(highProp.longValue, lowProp.longValue);


         EditorGUI.BeginProperty(position, label, property);
         position = EditorGUI.PrefixLabel(position, label);

         bool enabled = GUI.enabled;
         GUI.enabled = false; // Disable the GUI to prevent editing

         // Draw the GUID as a text field
         string guidString = guid.ToString();
         EditorGUI.TextField(position, guidString);

         GUI.enabled = enabled; // Restore the GUI state

         EditorGUI.EndProperty();
      }

      public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
      {
         return EditorGUIUtility.singleLineHeight;
      }
   }
}