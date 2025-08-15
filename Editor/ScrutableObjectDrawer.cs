using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScrutableObjects.UnityEditor
{
	public abstract class ScrutableObjectDrawer : PropertyDrawer
	{
		// 15.0f is the indent width that Unity has always used, but the value is not public.
		// If this changes in the future, platform dependent compilation can be added.
		private const float IndentWidth = 15.0f;

		// This is required for the object field to render with a blank space for the label.
		// The foldout renders the label so that it appears properly even outside of the inspector window.
		// Otherwise, the label and foldout arrow overlap. Manual indentation behavior is inconsistent.
		private static readonly GUIContent BlankLabel = new GUIContent((string)null);

		// Holding the Alt key and toggling the foldout expands or collapses all child properties.
		// These static fields allow for carrying that information across property drawer instances.
		private static bool AltKeyToggle;
		private static bool AltIsExpanded;

		private static ScriptableObject ParentObject;
		private static readonly List<ScriptableObject> ObjectList = new List<ScriptableObject>();

		private SerializedObject SerializedScriptableObject;

		private void UpdateSerializedScriptableObject(SerializedProperty property)
		{
			ScriptableObject scriptableObject = GetScriptableObjectReference(property);
			if(scriptableObject == null)
			{
				SerializedScriptableObject = null;
			}
			else if(SerializedScriptableObject == null || SerializedScriptableObject.targetObject != scriptableObject)
			{
				// Do not Dispose the old SerializedObject. Leave it for garbage collection.
				// Recursive property drawer invocations still need their SerializedObject.
				SerializedScriptableObject = new SerializedObject(scriptableObject);
			}
			else
			{
				SerializedScriptableObject.Update();
			}
		}

		protected static ScriptableObject GetScriptableObjectReference(SerializedProperty property)
		{
			ScriptableObject scriptableObject = null;
			if(property.propertyType == SerializedPropertyType.ObjectReference && !property.hasMultipleDifferentValues)
				scriptableObject = property.objectReferenceValue as ScriptableObject;
			return scriptableObject;
		}

		private void PushObject(SerializedProperty property)
		{
			if(ObjectList.Count == 0)
			{
				SerializedObject serializedObject = property.serializedObject;
				if(serializedObject != null)
					ParentObject = serializedObject.targetObject as ScriptableObject;
			}
			ScriptableObject scriptableObject = (ScriptableObject)SerializedScriptableObject.targetObject;
			ObjectList.Add(scriptableObject);
		}

		private void PopObject()
		{
			ObjectList.RemoveAt(ObjectList.Count - 1);
			if(ObjectList.Count == 0)
				ParentObject = null;
		}

		private bool HasBeenRendered()
		{
			ScriptableObject scriptableObject = (ScriptableObject)SerializedScriptableObject.targetObject;
			bool rendered = (ParentObject == scriptableObject);
			if(!rendered)
				rendered = ObjectList.Contains(scriptableObject);
			return rendered;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float height = EditorGUIUtility.singleLineHeight;
			UpdateSerializedScriptableObject(property);
			if(SerializedScriptableObject == null)
			{
				height = EditorGUI.GetPropertyHeight(property, label);
			}
			else
			{
				// Holding the Alt key and toggling the expandable foldout invokes GetPropertyHeight on other scriptable objects.
				// This allows for setting property.isExpanded from within each property drawer, which is then recognized by the editor.
				if(AltKeyToggle)
					property.isExpanded = AltIsExpanded;

				if(HasBeenRendered())
				{
					if(property.isExpanded)
						height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				}
				else
				{
					PushObject(property);
					try
					{
						bool isExpanded = property.isExpanded;
						SerializedProperty childProperty = SerializedScriptableObject.GetIterator();
						childProperty.NextVisible(true);
						while(childProperty.NextVisible(false))
						{
							if(AltKeyToggle)
							{
								ExpandOrCollapseAll(childProperty);
							}
							else if(isExpanded)
							{
								float childPropertyHeight = EditorGUI.GetPropertyHeight(childProperty);
								height += childPropertyHeight + EditorGUIUtility.standardVerticalSpacing;
							}
						}
					}
					finally
					{
						PopObject();
					}
				}
			}
			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			UpdateSerializedScriptableObject(property);
			if(SerializedScriptableObject == null)
			{
				EditorGUI.PropertyField(position, property, label, true);
			}
			else
			{
				// Object Field
				position.height = EditorGUIUtility.singleLineHeight;
				bool lockedAtRuntime = false;
				ShowPropertiesAttribute showPropertiesAttribute = attribute as ShowPropertiesAttribute;
				if(showPropertiesAttribute != null)
					lockedAtRuntime = showPropertiesAttribute.LockObjectAtRuntime;
				bool disabled = (lockedAtRuntime && EditorApplication.isPlayingOrWillChangePlaymode);
				using(new EditorGUI.DisabledGroupScope(disabled))
				{
					EditorGUI.ObjectField(position, property, BlankLabel);
				}
				UpdateSerializedScriptableObject(property);
				// Foldout
				label = GetTruncatedLabel(label);
				EditorGUI.BeginProperty(position, label, property);
				bool isExpanded = property.isExpanded;
				bool altKeyToggle = false;
				isExpanded = EditorGUI.Foldout(position, isExpanded, label, true, EditorStyles.foldout);
				if(property.isExpanded != isExpanded)
				{
					property.isExpanded = isExpanded;
					altKeyToggle = Event.current.alt;
				}
				EditorGUI.EndProperty();
				// Child Properties
				if(SerializedScriptableObject != null)
				{
					if(HasBeenRendered())
					{
						if(isExpanded)
						{
							// Avoid Inifinite Recursion
							position.x += IndentWidth;
							position.width -= IndentWidth;
							position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
							position.height = EditorGUIUtility.singleLineHeight;
							EditorGUI.LabelField(position, "∞", "∞");
						}
					}
					else
					{
						PushObject(property);
						try
						{
							// Alt Key
							if(altKeyToggle)
							{
								AltKeyToggle = true;
								AltIsExpanded = isExpanded;
								SerializedProperty childProperty = SerializedScriptableObject.GetIterator();
								childProperty.NextVisible(true);
								while(childProperty.NextVisible(false))
								{
									ExpandOrCollapseAll(childProperty);
								}
								AltKeyToggle = false;
							}
							// Is Expanded
							if(isExpanded)
							{
								// Indent
								position.x += IndentWidth;
								position.width -= IndentWidth;
								// Child Properties
								SerializedProperty childProperty = SerializedScriptableObject.GetIterator();
								childProperty.NextVisible(true);
								EditorGUI.BeginChangeCheck();
								while(childProperty.NextVisible(false))
								{
									position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
									position.height = EditorGUI.GetPropertyHeight(childProperty);
									EditorGUI.PropertyField(position, childProperty, true);
								}
								if(EditorGUI.EndChangeCheck())
									SerializedScriptableObject.ApplyModifiedProperties();
							}
						}
						finally
						{
							PopObject();
						}
					}
				}
			}
		}

		// Using GUI.BeginClip to truncate the label doesn't always work properly on older versions of Unity.
		private static GUIContent GetTruncatedLabel(GUIContent label)
		{
			// Some versions of Unity need a little extra space under certain conditions.
			float maxWidth = EditorGUIUtility.labelWidth - IndentWidth * EditorGUI.indentLevel;
			GUIStyle style = EditorStyles.label;
			string text = label.text;
			float width = style.CalcSize(label).x;
			if(width > maxWidth && text.Length > 0)
			{
				label = new GUIContent(label);
				while(width > maxWidth && text.Length > 0)
				{
					text = text.Substring(0, text.Length - 1);
					label.text = text;
					width = style.CalcSize(label).x;
				}
			}
			return label;
		}

		private static void ExpandOrCollapseAll(SerializedProperty property)
		{
			ScriptableObject scriptableObject = GetScriptableObjectReference(property);
			if(scriptableObject == null)
			{
				// Standard Expandable Types
				if(property.hasVisibleChildren)
				{
					property.isExpanded = AltIsExpanded;
					SerializedProperty childProperty = property.Copy();
					childProperty.NextVisible(true);
					while(childProperty.NextVisible(false) && childProperty.depth > property.depth)
					{
						ExpandOrCollapseAll(childProperty);
					}
				}
			}
			else
			{
				// Setting property.isExpandable on other scriptable object properties from this property drawer has no effect.
				// Hijacking GetPropertyHeight to set property.isExpandable from within each property drawer gets the job done.
				EditorGUI.GetPropertyHeight(property);
			}
		}
	}
}
