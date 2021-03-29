using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;

public class CrashOnClick : MonoBehaviour
{
    private void Awake( )
    {
        GetComponent<Button>( ).onClick.AddListener( ( ) => Utils.ForceCrash( ForcedCrashCategory.AccessViolation ) );
    }
}
