using UnityEngine;

namespace FastWorkOrderMover
{
    public class State
    {
        public readonly bool IsMoving;
        public readonly bool IsSorting;
        public readonly bool IsNothing;

        public static State GetState()
        {
            return new State();
        }

        private State()
        {
            var shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (ctrlHeld && shiftHeld) IsNothing = true;
            if (!ctrlHeld && !shiftHeld) IsNothing = true;
            if (ctrlHeld) IsSorting = true;
            if (shiftHeld) IsMoving = true;
        }
    }
}