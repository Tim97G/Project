using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Transform cam;
    [HideInInspector] public Transform Hips;
    [HideInInspector] public Transform Spine;
    [HideInInspector] public float AimingAngle;
    bool isGrounded;

    public AudioClip WalkSound;
    public AudioClip RunSound;
    public AudioClip SneakSound;

    Rigidbody rBody;
    public float speed = 6f;
    float speedReduce = 1;
    public float jumpHeight = 2f;
    float FallingTime;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;
    float targetAngle;
    float angle;

    Animator anim;
    AudioSource AudioSource;

    void Start()
    {
        anim = GetComponent<Animator>();
        rBody = GetComponent<Rigidbody>();

        AudioSource = gameObject.AddComponent<AudioSource>();
        AudioSource.clip = WalkSound;
        AudioSource.loop = true;
        AudioSource.spatialBlend = 1f;
        AudioSource.maxDistance = 64;
        AudioSource.volume = 0f;
        AudioSource.Play();

        Hips = transform.Find("Armature/Hips");
        Spine = transform.Find("Armature/Hips/Spine");
    }
    
    void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical);
      
        if(direction.magnitude >= 0.1f)
        {
            anim.speed = 1f;          
            
            if (Input.GetButton("Run") && !GlobalVar.Aiming && !anim.GetBool("Crouching"))
            {
                //Звук бега
                if(isGrounded)
                {
                    if(AudioSource.clip != RunSound)
                    {
                        AudioSource.clip = RunSound;
                        AudioSource.Play();
                    }
                    AudioSource.volume = 1f;
                }
                else
                {
                    AudioSource.volume = 0f;
                }

                //Задание переменной скорости для анимации и ускорение передвижения игрока
                anim.SetFloat("WalkParam", 2f, 0.1f, Time.deltaTime);
                speedReduce = 2f;
            }
            else
            {              
                //Звук ходьбы
                if(isGrounded)
                {
                    if(AudioSource.clip != WalkSound)
                    {
                        AudioSource.clip = WalkSound;
                        AudioSource.Play();
                    }
                    AudioSource.volume = 1f;
                }
                else
                {
                    AudioSource.volume = 0f;
                }

                //Задание переменной скорости для анимации
                anim.SetFloat("WalkParam", 1f, 0.1f, Time.deltaTime);
            }

            //Передвижение
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);           
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            rBody.MovePosition(transform.position+transform.forward * (speed*speedReduce) * Time.deltaTime);
        }
        else 
        {

            AudioSource.volume = 0f;
            anim.SetFloat("WalkParam", 0f, 0.1f, Time.deltaTime);
        } 

        if(GlobalVar.Aiming) 
        {                
            speedReduce = 0.5f; //уменьшение скорости ходьбы при прицеливании
            anim.speed = speedReduce;
                
            anim.SetBool("Aiming", true);

            //Доворот туловища при прицеливании, во избежание скручивания позвонка
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            if(Mathf.Abs(targetAngle - transform.eulerAngles.y) > 45)
            {
                angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);           
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                anim.SetFloat("WalkParam", 1f, 0.1f, Time.deltaTime);
            }

            //Не поворачивать ноги, если нажата кнопка "идти назад" при прицеливании
            if(vertical == -1) 
            {
                anim.SetFloat("WalkParam", -1f, -0.1f, Time.deltaTime);
                targetAngle = Mathf.Atan2(-direction.x, -direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;          
                angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
                rBody.MovePosition(transform.position+transform.forward * -(speed*speedReduce) * Time.deltaTime);
            }
        }
        else
        {
            speedReduce = 1f;
            anim.speed = speedReduce;
            anim.SetBool("Aiming", false);
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
		{			
			anim.SetBool("Grounded",false);
            rBody.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
			isGrounded = false;
		}

        if (Input.GetButton("Crouching"))
		{			
			anim.SetBool("Crouching", true);
            speedReduce = 0.5f;
            AudioSource.volume = 0f;
		}
        else
        {
            anim.SetBool("Crouching", false);
        }

        //Просчет падения
        if(!isGrounded)
            FallingTime += 1*Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
	{
        isGrounded = true;
        FallingTime = 0f;
        anim.SetBool("Grounded",true);
	}  
}