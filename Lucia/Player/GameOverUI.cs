using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance
    {
        get { return s_Instance; }
    }

    protected static GameOverUI s_Instance;

    static CanvasGroup canversgroup;

    public bool m_IsFading;
    public CanvasGroup gameOver;

    public CanvasGroup swordSkillLock01;
    public CanvasGroup swordSkillLock02;
    public CanvasGroup bowLock;
    public CanvasGroup explodeLock;
    public CanvasGroup portalScreen;

    public enum FadeType
    {
        GameOver=1,
        swordSkill01 = 2,
        bow = 3,
        eplodeArrow = 4,
        Teleport = 5,
        swordSkill02 = 6,
    }
    private void Awake()
    {
        s_Instance = this;
    }
    private void Start()
    {
        
    }
    /// <summary>
    /// Select canvers which to use
    /// </summary>
    /// <param name="fadeType"></param>
    /// <returns></returns>
    static CanvasGroup SelectCanvers(FadeType fadeType)
    {        
        switch (fadeType)
        {
            case FadeType.GameOver:
                canversgroup = Instance.gameOver;
                break;
            case FadeType.swordSkill01:
                canversgroup = Instance.swordSkillLock01;
                break;
            case FadeType.bow:
                canversgroup = Instance.bowLock;
                break;
            case FadeType.eplodeArrow:
                canversgroup = Instance.explodeLock;
                break;
            case FadeType.Teleport:
                canversgroup = Instance.portalScreen;
                break;
            case FadeType.swordSkill02:
                canversgroup = Instance.swordSkillLock02;
                break;
        }
        return canversgroup;
    }
    /// <summary>
    /// The FadeOut process
    /// </summary>
    /// <param name="fadeType"></param>
    /// <returns></returns>
    public static IEnumerator ScreenFadeOut(FadeType fadeType)
    {
        CanvasGroup canvers;
        canvers=SelectCanvers(fadeType);

        canvers.gameObject.SetActive(true);
        yield return Instance.StartCoroutine(Instance.Fade(1f, canvers));
        if (fadeType == FadeType.GameOver)
            Cursor.visible = true;
    }
    /// <summary>
    /// The FadeIn Process
    /// </summary>
    /// <param name="fadeType"></param>
    /// <returns></returns>
    public static IEnumerator ScreenFadeIn(FadeType fadeType)
    {
        CanvasGroup canvers;
        canvers = SelectCanvers(fadeType);

        yield return Instance.StartCoroutine(Instance.Fade(0f, canvers));
        canvers.gameObject.SetActive(false);

        if(fadeType==FadeType.GameOver)
            Cursor.visible = false;
    }
    /// <summary>
    /// Fade Process�Afade speed adjust here
    /// </summary>
    /// <param name="finalAlpha"></param>
    /// <param name="canvers"></param>
    /// <returns></returns>
    protected IEnumerator Fade(float finalAlpha, CanvasGroup canvers)
    {
        canvers.blocksRaycasts = false;
        m_IsFading = true;

        while(!Mathf.Approximately(canvers.alpha,finalAlpha))
        {
            canvers.alpha = Mathf.MoveTowards(canvers.alpha, finalAlpha, 1f * Time.deltaTime);
            yield return null;
        }
        canvers.alpha = finalAlpha;

        canvers.blocksRaycasts = true;
        m_IsFading = false;
    }
}
