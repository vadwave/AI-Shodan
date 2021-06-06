using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shodan : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] Text textType;
    [SerializeField] Text textDiffucalty;
    [SerializeField] Text textTime;
    [SerializeField] GameObject canvas;
    [SerializeField] ProceduralGenerationLevel level;
    [SerializeField] Text getTime;


    [Header("Settings")]
    [Range(1, 15)] 
    [SerializeField] int countActions;
    [Range(0f, 15f)] 
    [SerializeField] float timeDelay;
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

    const string textCurTime = "Время: ";
    const string textCurType = "Тип ИП: ";
    const string textCurDiff = "Сложность ИП: ";

    private void Start()
    {
        canvas.SetActive(false);
    }

    public void StartEscaping(Transform transform, Transform transform1)
    {

    }

    public void CollectInfo(float values)
    {

    }
    public void FindTarget()
    {

    }
    public void LostTarget()
    {

    }
    public void Result()
    {
        Time.timeScale = 0;
        textTime.text = textCurTime+getTime.text;
        string textTypes = "Оборонный";
        if (level.CurrentTypeLevel == "Speed")
            textTypes = "Быстрый";
        else if (level.CurrentTypeLevel == "Filled")
            textTypes = " Наполненный";
        else if (level.CurrentTypeLevel == "Defence")
            textTypes = "Оборонный";
        textType.text = textCurType + textTypes;
        textDiffucalty.text = textCurDiff + level.CurrentDiffLevel;
        canvas.SetActive(true);
    }
    public void Restart()
    {
        canvas.SetActive(false);
        Time.timeScale = 1;
    }
}
