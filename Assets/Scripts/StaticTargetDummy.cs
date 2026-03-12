using System.Collections;
using TMPro;
using UnityEngine;

public class StaticTargetDummy : HitteableBehaviour
{
    [SerializeField] private TextMeshProUGUI m_HitText;

    public override void OnHit()
    {
        base.OnHit();

        StartCoroutine(OnHitCoroutine());
    }

    IEnumerator OnHitCoroutine()
    {
        m_HitText.text = "Me has golpeado!";
        yield return new WaitForSeconds(2f);
        m_HitText.text = "";
    }
}
