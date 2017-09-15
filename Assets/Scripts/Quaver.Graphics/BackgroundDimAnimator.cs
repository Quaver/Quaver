// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Main;

public class BackgroundDimAnimator : MonoBehaviour
{
    public SpriteRenderer dimObject;
    public GameStateManager Manager;
    private Color _color = new Color(1, 1, 1, 0.8f);
    private float _tween;
    public float dim = 0f;

    private void Update()
    {
        if (_tween != dim)
        {
            _tween += (dim - _tween) * Mathf.Min(Time.deltaTime * 10f, 0.2f);
            _color.a = _tween;
            dimObject.color = _color;
        }
    }
}
