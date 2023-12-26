using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
Различные составляющие погоды заданы в анимации.
Длительность анимации должна быть 720 кадров(30 секунд)
Во время сохранения/загрузки, стоит  лишь записывать и менять GlobalVar.GlobalTime. Все остальное будет рассчитано автоматически.
*/
public class AnimatedWeather : MonoBehaviour
{
    [Range(0f, 23f)] public float GameTime = 0f;
	[SerializeField, Tooltip("Скорость течения игрового времени")] [Range(1, 100)] public float SpeedMultiplier = 1;

    private float CurrentLeight;
	//private float MaxLeight = 30f; //длина анимации в секундах

    public Material Skybox;
    public Transform NormalCloudsSlider;
	public Transform RainCloudsSlider;

    [Range(500f, 1500f)] public int RainMaxDensity;
    public GameObject RainParticleObject;

    GameObject SunFlare;
    ParticleSystem RainParticleSystem;
    AudioSource RainAudioSource;

    void Start()
    {
        GameTime = GlobalVar.GlobalTime;
		
		GetComponent<Animation>()["anim_WeatherDay01"].time = GameTime/24*10; //Задаем время. Время конвертируем в из 24 часов 30.0f.
		GetComponent<Animation>().Play("anim_WeatherDay01"); //Запуск анимации

        SunFlare = GameObject.FindGameObjectWithTag("Sun");

        RainParticleSystem = RainParticleObject.GetComponent<ParticleSystem>();
        RainAudioSource = RainParticleObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        SpeedMultiplier = GlobalVar.TimeSpeed;
		
		GetComponent<Animation>()["anim_WeatherDay01"].speed = 0.005f*SpeedMultiplier;

        GlobalVar.CloudPower = NormalCloudsSlider.localPosition.z;
        GlobalVar.RainPower = RainCloudsSlider.localPosition.z;

        var emission = RainParticleSystem.emission;
        emission.rateOverTime = RainCloudsSlider.localPosition.z*RainMaxDensity;
        RainAudioSource.volume = RainCloudsSlider.localPosition.z;

        Skybox.SetFloat("_Clouds", GlobalVar.CloudPower);
        Skybox.SetFloat("_Rain", GlobalVar.RainPower);
        Skybox.SetColor("_Tint", RenderSettings.fogColor*2);
    }
}
