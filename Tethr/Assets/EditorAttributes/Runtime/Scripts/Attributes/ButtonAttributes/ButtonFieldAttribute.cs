using UnityEngine;

namespace EditorAttributes
{
    /// <summary>
    /// Attribute to add a button in the inspector in place of a field
    /// </summary>
    public class ButtonFieldAttribute : PropertyAttribute, IRepetableButton
    {
        public string FunctionName { get; private set; }
        public string ButtonLabel { get; private set; }
        public float ButtonHeight { get; private set; }

        public bool IsRepetable { get; private set; }
        public long PressDelay { get; private set; }
        public long RepetitionInterval { get; private set; }

        public bool MakeDirty { get; private set; }

        /// <summary>
        /// Attribute to add a button in the inspector in place of a field
        /// </summary>
        /// <param name="functionName">The name of the function to call</param>
        /// <param name="buttonLabel">The label displayed on the button</param>
        /// <param name="buttonHeight">The height of the button in pixels</param>
        /// <param name="makeDirty">Whether to mark the object as dirty after invoking the function</param>
        public ButtonFieldAttribute(string functionName, string buttonLabel = "", float buttonHeight = 18f, bool makeDirty = true)
        {
            FunctionName = functionName;
            ButtonLabel = buttonLabel;
            ButtonHeight = buttonHeight;
            MakeDirty = makeDirty;
        }

        /// <summary>
        /// Attribute to add a button next to a property
        /// </summary>
        /// <param name="functionName">The name of the function the button activates</param>
        /// <param name="isRepetable">Makes the button repeat logic on hold</param>
        /// <param name="pressDelay">How many milliseconds to wait before the logic is repeated</param>
        /// <param name="repetitionInterval">The interval in milliseconds the logic will repeat</param>
        /// <param name="buttonLabel">The label displayed on the button</param>
        /// <param name="buttonHeight">The height of the button in pixels</param>
        /// <param name="makeDirty">Whether to mark the object as dirty after invoking the function</param>
        public ButtonFieldAttribute(string functionName, bool isRepetable, long pressDelay = 60, long repetitionInterval = 100, string buttonLabel = "", float buttonHeight = 18f, bool makeDirty = true) : this(functionName, buttonLabel, buttonHeight, makeDirty)
        {
            IsRepetable = isRepetable;
            PressDelay = pressDelay;
            RepetitionInterval = repetitionInterval;
        }
    }
}
