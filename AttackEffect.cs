using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    //Called from the animator when the animation finished playing
    private void GoInactive()
    {
        gameObject.SetActive(false);
    }
}
