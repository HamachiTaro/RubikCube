using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace RubikCube
{
    
    public class GameUI : MonoBehaviour
    {
        
        [SerializeField] private Button undoButton;

        public IObservable<Unit> OnClickUndoAsObservable => undoButton.OnClickAsObservable();

    }
}