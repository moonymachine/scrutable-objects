using UnityEngine;

namespace ScrutableObjects
{
	/// <summary> Enables direct access to the properties of ScriptableObject references in the editor. </summary>
	public sealed class ShowPropertiesAttribute : PropertyAttribute
	{
		/// <summary> Disables changing the object reference in the editor at runtime. </summary>
		public bool LockObjectAtRuntime { get; set; }
	}
}
