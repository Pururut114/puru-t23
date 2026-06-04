#if LTCGI_INCLUDED
using UdonSharp;
using UnityEngine;

namespace Trigger2to3
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class T23_SetLtcgiState : T23_ActionBase
    {
        public LTCGI_UdonAdapter adapter;

        public bool global = true;
        public bool toggle;
        public bool operation = true;
        public T23_PropertyBox propertyBox;
        public bool usePropertyBox;

        public GameObject[] screens;

        private int[]   _indices;
        private Color[] _onColors;
        private bool[]  _states;
        private bool    _globalState = true;

        protected override void PostStart()
        {
            if (global || adapter == null || screens == null) return;

            _indices  = new int[screens.Length];
            _onColors = new Color[screens.Length];
            _states   = new bool[screens.Length];

            for (int i = 0; i < screens.Length; i++)
            {
                if (screens[i] == null) { _indices[i] = -1; continue; }
                _indices[i]  = adapter._GetIndex(screens[i]);
                _onColors[i] = adapter._GetColor(_indices[i]);
                _states[i]   = true;
            }
        }

        protected override void OnAction()
        {
            if (adapter == null) return;

            if (usePropertyBox && propertyBox)
                operation = propertyBox.value_b;

            if (global)
            {
                if (toggle) _globalState = !_globalState;
                else        _globalState = operation;
                adapter._SetGlobalState(_globalState);
            }
            else
            {
                if (_indices == null) return;
                for (int i = 0; i < _indices.Length; i++)
                {
                    if (_indices[i] < 0) continue;
                    if (toggle) _states[i] = !_states[i];
                    else        _states[i] = operation;
                    adapter._SetColor(_indices[i], _states[i] ? _onColors[i] : Color.black);
                }
            }
        }
    }
}
#endif
