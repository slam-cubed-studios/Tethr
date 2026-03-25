using System;
using UnityEngine;

namespace EditorAttributes
{
    /// <summary>
    /// Attribute to make a dropdown of type paths
    /// </summary>
    public class TypeDropdownAttribute : PropertyAttribute
    {
        public string AssemblyName { get; private set; }
        public Type BaseTypeFilter { get; private set; }

        /// <summary>
        /// Attribute to make a dropdown of type paths
        /// </summary>
        /// <param name="assemblyName">Filter which types are displayed by the assembly name</param>
        /// <param name="baseTypeFilter">Filter which types are displayed by their base type. The base type itself will not be included</param>
        public TypeDropdownAttribute(string assemblyName = "", Type baseTypeFilter = null)
        {
            AssemblyName = assemblyName;
            BaseTypeFilter = baseTypeFilter;
        }
    }
}