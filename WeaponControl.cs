using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class WeaponControl : MonoBehaviour
{
    public GameObject[] Knifes;
    public GameObject[] Pistols;
    public GameObject[] Rifles;
    public RuntimeAnimatorController AnimatorController;
    public Transform WeaponFirePivot;
    public GameObject BulletPrefab;
    public GameObject Crosshair;
    public GameObject WeaponDisplay;
    
    [HideInInspector] public bool SemiAuto;
    [HideInInspector] public int MagSize;
	[HideInInspector] public float MaxDistance;
	[HideInInspector] public float Spread;
	[HideInInspector] public float Rpm;
    [HideInInspector] public float Flatness;
    [HideInInspector] public int BulletSpeed;
    [HideInInspector] public float ShotFragments;
    [HideInInspector] public int ShootDistance;

    float RestTime;
    bool CanShoot = true;
    int ChangeWeapon;
    float SpreadX;
    float SpreadY;

    Animator anim;

    AudioSource audioSource;
    GameObject Camera;
    [HideInInspector] public AudioClip TakeSound;
	[HideInInspector] public AudioClip FireSound;
	[HideInInspector] public AudioClip EmptySound;
	[HideInInspector] public AudioClip ReloadSound;

    [HideInInspector] public GameObject BulletHole;
	[HideInInspector] public GameObject MuzzleFlash;

   
    void Start()
    {
        anim = GetComponent<Animator>();
        Camera = GameObject.Find("CameraPivot/Dynamic/Camera");
        WeaponSelect();
    }

    void WeaponSelect()
    {
        //Деактивация всего оружия
        for(int i = 0; i < Knifes.Length; i++)
            Knifes[i].SetActive(false);
        
        for(int i = 0; i < Pistols.Length; i++)
            Pistols[i].SetActive(false);
        
        for(int i = 0; i < Rifles.Length; i++)
            Rifles[i].SetActive(false);
        
        //Включение оружия, которое только есть в наличии
        if(GlobalVar.CurrentRifle != 0)
        {
            Rifles[GlobalVar.CurrentRifle-1].SetActive(true);
            audioSource = Rifles[GlobalVar.CurrentRifle-1].GetComponent<AudioSource>();
            FireSound = Rifles[GlobalVar.CurrentRifle-1].GetComponent<WeaponItem>().FireSound;
            TakeSound = Rifles[GlobalVar.CurrentRifle-1].GetComponent<WeaponItem>().TakeSound;
            SemiAuto = Rifles[GlobalVar.CurrentRifle-1].GetComponent<WeaponItem>().SemiAuto;
            Rpm = Rifles[GlobalVar.CurrentRifle-1].GetComponent<WeaponItem>().Rpm;
            Flatness = Rifles[GlobalVar.CurrentRifle-1].GetComponent<WeaponItem>().Flatness;
            Spread = Rifles[GlobalVar.CurrentRifle-1].GetComponent<WeaponItem>().Spread;
            BulletSpeed = Rifles[GlobalVar.CurrentRifle-1].GetComponent<WeaponItem>().BulletSpeed;
            ShotFragments = Rifles[GlobalVar.CurrentRifle-1].GetComponent<WeaponItem>().ShotFragments;
            ShootDistance = Rifles[GlobalVar.CurrentRifle-1].GetComponent<WeaponItem>().ShootDistance;
            Crosshair.GetComponent<Crosshair>().SpreadPower = Spread*3200; 
        }
    }
    
    void Update()
    {           
        if(GlobalVar.Aiming && GlobalVar.WeaponSelected !=0) 
        {           
            if(SemiAuto)
            {
                if (Input.GetButtonDown("Fire1") && CanShoot)
                    Shoot();
            }
            else
            {
                if (Input.GetButton("Fire1") && CanShoot)
                    Shoot();             
            }

        } 

        if (RestTime > 0)
		    RestTime -= Time.deltaTime;
	        else
		    RestTime = 0;
	    
		if (RestTime == 0)
		    CanShoot = true;

        //Выбор оружия(1-4)
        if (Input.GetKeyDown("1")) ChangeWeapon = 1;
        if (Input.GetKeyDown("2")) ChangeWeapon = 2;
        if (Input.GetKeyDown("3")) ChangeWeapon = 3;
        if (Input.GetKeyDown("4")) ChangeWeapon = 4;

        //Выбор оружия(колесо мыши)
        if (Input.GetAxis("Mouse ScrollWheel") !=0)
        {
            GlobalVar.WeaponSelected += Mathf.FloorToInt(Input.GetAxis("Mouse ScrollWheel")*10);
            WeaponDisplay.GetComponent<WeaponDisplay>().Refresh = true;

            if(GlobalVar.WeaponSelected < 0) GlobalVar.WeaponSelected = 0;
            if(GlobalVar.WeaponSelected > 4) GlobalVar.WeaponSelected = 4;
        }   
    }

    void FixedUpdate()
    {
        if(ChangeWeapon > 0)
        {
            if(GlobalVar.WeaponSelected == ChangeWeapon)
            {
                GlobalVar.WeaponSelected = 0;
                anim.SetLayerWeight(1, 0f);
                anim.runtimeAnimatorController = AnimatorController;
            }
            else
            {
                GlobalVar.WeaponSelected = ChangeWeapon;
                anim.SetLayerWeight(1, 1f);
                if(ChangeWeapon == 3)
                anim.runtimeAnimatorController = Rifles[GlobalVar.CurrentRifle-1].GetComponent<WeaponItem>().OverrideController;
            }

            WeaponDisplay.GetComponent<WeaponDisplay>().Refresh = true;
            ChangeWeapon = 0;
            audioSource.PlayOneShot(TakeSound, 1.0f);
        } 
    } 

    void Shoot()
    {
        anim.SetTrigger("Shoot"); 
        audioSource.PlayOneShot(FireSound, 1.0f);
        Instantiate(MuzzleFlash,WeaponFirePivot.transform.position, Quaternion.identity);

        for(int i = 0; i < ShotFragments; i++)
        {
            RaycastHit Hit;
            SpreadX = Random.Range(-Spread,Spread);
            SpreadY = Random.Range(-Spread,Spread);
            if(Physics.Raycast(Camera.transform.position, Camera.transform.forward+new Vector3(SpreadX,SpreadY,0f), out Hit, 512f))
            {                       
                GameObject BulletObject = Instantiate(BulletPrefab,WeaponFirePivot.transform.position, Quaternion.identity);
                BulletObject.GetComponent<Bullet>().Direction = Hit.point;
                BulletObject.GetComponent<Bullet>().BulletSpeed = BulletSpeed;
                BulletObject.GetComponent<Bullet>().Flatness = Flatness;
                BulletObject.GetComponent<Bullet>().ShootDistance = ShootDistance;
            }
        }

        CanShoot = false;
        RestTime = 1/(Rpm/60);
        Crosshair.GetComponent<Crosshair>().Impulse = 2f;   
    }
}