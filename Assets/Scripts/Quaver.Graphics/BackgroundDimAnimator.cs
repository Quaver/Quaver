using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Main;

public class BackgroundDimAnimator : MonoBehaviour {

    public SpriteRenderer dimObject;
    public GameStateManager Manager;
    private Color color = new Color(1,1,1,0.8f);
    private float tween;
    public float dim = 0f;

	void Update () {
		if (tween != dim)
        {
            tween += (dim - tween) * Mathf.Min(Time.deltaTime*10f, 0.2f);
            color.a = tween;
            dimObject.color = color;
        }
	}
}
