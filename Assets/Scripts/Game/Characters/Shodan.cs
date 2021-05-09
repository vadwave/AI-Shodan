using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shodan : MonoBehaviour
{
    [SerializeField] Text text;
    private Coroutine corWait;
    private float counter;
    const string textAIShodan = "AI-Shodan: ";
    const string textCollectInfo = textAIShodan + "Ты украл важные данные? \n Я не позволю этому повторится!";
    const string textFindTarget = textAIShodan + "Я вижу тебя! \n Тебе не скрыться от взоров моих камер!";
    const string textCancelTarget = textAIShodan + "Куда ты пропал? \n Я настрою камеры активно искать тебя!";


    const string textResultSpeedRun = textAIShodan + "Ты слишком быстр! У тебя есть карта базы?";
    const string textResultInvisible = textAIShodan + "Как ты выбрался? \n У тебя явно был цифровой камуфляж!";
    const string textResultJourney = textAIShodan + "Ты смог украсть все данные \n Но тебе не скрыться не волнуйся!";

    const string textStart = textAIShodan + "Воришка тебе не скрыться с этими данными через 3 минуты тебя поймают!";

    private void Start()
    {
        
    }

    public void StartEscaping(Transform transform,Transform transform1)
    {
        if (corWait == null) corWait = StartCoroutine(CoroutineWait(textStart));
    }

    public void CollectInfo(float values)
    {
        if (corWait == null) corWait = StartCoroutine(CoroutineWait(textCollectInfo));
    }
    public void FindTarget()
    {
        if (corWait == null) corWait = StartCoroutine(CoroutineWait(textFindTarget));
    }
    public void LostTarget()
    {
        if (corWait == null) corWait = StartCoroutine(CoroutineWait(textCancelTarget));
        else
        {
            StopCoroutine(corWait); 
            corWait = StartCoroutine(CoroutineWait(textCancelTarget));
        }
    }
    public void Result()
    {
        if (corWait == null) corWait = StartCoroutine(CoroutineWait(textResultJourney, 6f));
        else
        {
            StopCoroutine(corWait);
            corWait = StartCoroutine(CoroutineWait(textResultJourney, 6f));
        }
    }
    IEnumerator CoroutineWait(string textArea, float waitTime = 3f)
    {
        text.text = textArea;
        counter = 0;
        while (counter < waitTime)
        {
            counter += Time.deltaTime;
            yield return null;
        }
        text.text = textAIShodan;
        corWait = null;
    }
}
