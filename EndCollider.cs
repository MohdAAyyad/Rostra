using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndCollider : MonoBehaviour
{
    public Image fadeImage;
    private bool fadeOut;

    void Update()
    {

        if(fadeOut)
        {
            fadeImage.fillAmount += 0.02f;
            if(fadeImage.fillAmount>=1.0f)
            {
                fadeOut = false;
                SceneManager.LoadScene("Credits");                
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.tag.Equals("Player") && UIBTL.conversationAfterBattle)
        {
            fadeOut = true;
        }
    }
}
