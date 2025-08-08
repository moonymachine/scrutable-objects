# Scrutable Objects

Enables direct access to the properties of ScriptableObject references in the editor.

### Installation

This package can be installed via Unity's Package Manager.

- Open the Package Manager window.
- Open the Add (+) menu in the toolbar.
- Select the "Install package from git URL" button.
- Enter this URL: https://github.com/moonymachine/scrutable-objects.git
- Select Install.

To install without using the Package Manager, add the contents of this repository to the Assets directory.

### Usage

#### ShowProperties Attribute

Apply the `[ShowProperties]` attribute to any ScriptableObject property to display the properties of whatever object is assigned.

```csharp
using ScrutableObjects;
using UnityEngine;

public class ExampleMonoBehaviour : MonoBehaviour
{
	[ShowProperties]
	public ScriptableObject ScriptableObjectProperty;
}
```

#### LockObjectAtRuntime Argument

Use the `LockObjectAtRuntime` property to disable changing the ScriptableObject reference at runtime in the editor.

```csharp
using ScrutableObjects;
using UnityEngine;

public class ExampleMonoBehaviour : MonoBehaviour
{
	[ShowProperties(LockObjectAtRuntime = true)]
	public ScriptableObject ScriptableObjectProperty;
}
```

Note that this does not prevent all ways changing the object reference at runtime, so write your code defensively.

#### Lists and Arrays

The `[ShowProperties]` attribute can be applied to lists and arrays as well.

```csharp
using ScrutableObjects;
using UnityEngine;

public class ExampleMonoBehaviour : MonoBehaviour
{
	[ShowProperties]
	public List<ScriptableObject> ScriptableObjectList;
	
	[ShowProperties]
	public ScriptableObject[] ScriptableObjectArray;
}
```

#### Apply to Types

To apply to specific types of ScriptableObject, derive a new property drawer from `ScrutableObjectDrawer` for the type.

```csharp
using ScrutableObjects.UnityEditor;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ExampleAsset))]
public class ExampleAssetDrawer : ScrutableObjectDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return base.GetPropertyHeight(property, label);
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		base.OnGUI(position, property, label);
	}
}
```
