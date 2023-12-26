using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
Положение солнца, время рассвета/заката, освещение заданы в анимации.
Длительность анимации должна быть 240 кадров(10 секунд)
Во время сохранения/загрузки, стоит  лишь записывать и менять GlobalVar.GlobalTime. Все остальное будет рассчитано автоматически.
*/
public class AnimatedSun : MonoBehaviour
{
    [Range(0f, 23f)] public float GameTime = 0f;
	[SerializeField, Tooltip("Скорость течения игрового времени")] [Range(1, 100)] public float SpeedMultiplier = 1;
	
	private float CurrentLeight;
	private float MaxLeight = 10f; //длина анимации в секундах
	
	[SerializeField, Tooltip("Текущий час (только чтение)")] public float Hours = 0f;
	[SerializeField, Tooltip("Текущая минута (только чтение)")] public float Minutes = 0f;
	
	public GameObject LightSource;
	public Material Skybox;
	public Transform DawnSlider;
	public Transform DaySlider;
	public Transform NightSlider;
	public Transform LightSlider;

	public AudioSource DaySounds;
	public AudioSource NightSounds;   
	
	public float AmbientBoost = 0.2f;
	
	void Start()
    {		
		GameTime = GlobalVar.GlobalTime;
		
		GetComponent<Animation>()["anim_Sun"].time = GameTime/24*10; //Задаем время. Время конвертируем в из 24 часов 10.0f.
		GetComponent<Animation>().Play("anim_Sun"); //Запуск анимации
    }

    void Update()
    {
        SpeedMultiplier = GlobalVar.TimeSpeed;
		
		GetComponent<Animation>()["anim_Sun"].speed = 0.005f*SpeedMultiplier;
		
		CurrentLeight = GetComponent<Animation>()["anim_Sun"].time; 

		Hours = Mathf.Floor(CurrentLeight/(MaxLeight/24f));
        Minutes = Mathf.Floor(CurrentLeight/(MaxLeight/1440f) - Hours*60f);
		
		if (Hours == 0f && Minutes == 0f)
			Reset();
		
		Skybox.SetFloat("_DawnPower", DawnSlider.localPosition.z);
		Skybox.SetFloat("_DayPower", DaySlider.localPosition.z);
		Skybox.SetFloat("_NightPower", NightSlider.localPosition.z);
		
		RenderSettings.ambientLight=new Color(LightSlider.localPosition.z/2f+AmbientBoost, LightSlider.localPosition.z/2f+AmbientBoost, LightSlider.localPosition.z/2f+AmbientBoost, 1f);
	    RenderSettings.fogColor=new Color(LightSlider.localPosition.z/2f+AmbientBoost, LightSlider.localPosition.z/2f+AmbientBoost, LightSlider.localPosition.z/2f+AmbientBoost, 1f);
		LightSource.GetComponent<Light>().intensity = LightSlider.localPosition.z - GlobalVar.RainPower;

		GlobalVar.Hours = Mathf.RoundToInt(Hours);
		GlobalVar.Minutes = Mathf.RoundToInt(Minutes);

		DaySounds.volume = DaySlider.localPosition.z;
		NightSounds.volume = NightSlider.localPosition.z;
    }
	
	void Reset()
	{
		GetComponent<Animation>().Play("anim_Sun");		
	}
}
