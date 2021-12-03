using System;
using UnityEngine;
using UnityEngine.UI;

public class Controls : MonoBehaviour
{
    private GameObject ball;
    private Rigidbody ball_rigid_body;
    private Vector3 ball_start_pos;
    Quaternion ball_start_rotation;
    private bool is_ball_moving;

    private GameObject main_camera;
    private Vector3 camera_start_pos;
    private Vector3 camera_offset;

    private GameObject arrow;
    private GameObject arrow_pivot;

    private GameObject ForceIndicator;
    private Image ForceValue; // ForceIndicator - image
    private float force_factor = 40000f;

    //private int count_set;
    private int count_shot;
    //private int count_try;
    private int count_kegels_down;

    void Start()
    {
        ball = GameObject.Find("Ball");
        ball_rigid_body = ball.GetComponent<Rigidbody>();
        ball_start_pos = ball.transform.position;
        ball_start_rotation = ball.transform.rotation;
        is_ball_moving = false;

        arrow = GameObject.Find("Arrow");
        arrow_pivot = GameObject.Find("PivotPoint");

        ForceIndicator = GameObject.Find("ForceIndicator");
        ForceValue = GameObject.Find("ForceValue").GetComponent<Image>();

        main_camera = GameObject.Find("MainCamera");
        camera_start_pos = main_camera.transform.position;
        camera_offset = main_camera.transform.position - ball.transform.position;

        count_shot = 0;
        count_kegels_down = 0;
    }

    void Update()
    {
        //count_set = (int) Math.Round((double)count_shot / 2);

        #region управление шаром

        if (is_ball_moving)
        {
            if(ball_rigid_body.velocity.magnitude < 0.1f)
            {
                is_ball_moving = false;

                // Сбрасываем остаточную скорость
                ball_rigid_body.velocity = Vector3.zero;        // поступательная скорость
                ball_rigid_body.angularVelocity = Vector3.zero; // вращательная скорость

                ball.transform.position = ball_start_pos; // возврат в начальную позицию
                ball.transform.rotation = ball_start_rotation;

                arrow.SetActive(true); // Показываем стрелку

                ForceIndicator.SetActive(true); // показываем инликатор силы

                main_camera.transform.position = camera_start_pos; // возвращаем камеру на начальную позицию

                //анализируем кегли
                // находим их по тегу
                GameObject[] kegels = GameObject.FindGameObjectsWithTag("Kegel");

                //обход циклом
                foreach(GameObject kegel in kegels)
                {
                    //Debug.Log(kegel.name + " " + kegel.transform.position.y + " " + (kegel.transform.position.y > 0.1 ? "Down" : "Up"));

                    if(kegel.transform.position.y > 0.1 || kegel.transform.position.y < - 0.1)
                    {
                        // кегля лежит - убираем ее
                        kegel.SetActive(false);
                        
                        count_kegels_down++;
                    }

                    //Debug.Log("shot - " + count_shot + "; set - (" + Math.Round((double)count_shot / 2) + ") " + count_set + "; k_down - " + count_kegels_down);
                }

                // ЗАДАНИЕ: подсчет статистики:
                // номер попытки - сбито - осталось - *время
                // на первом этапе выводить в консоль, потом на экран

                Debug.Log("Номер попытки: " + count_shot + ". Сбито: " + count_kegels_down + ". Осталось: " + (10 - count_kegels_down));

                if(count_kegels_down >= 10)
                {
                    Debug.Log("Все кегли сбиты. Количество ударов: " + count_shot);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && !is_ball_moving)
        {
            // определяем направление стрелки
            // оно совпадает с вектором forward объекта arrow

            Vector3 force_direction = arrow.transform.forward;

            // определяем силу по индикатору
            float force_value = ForceValue.fillAmount * force_factor;

            ball_rigid_body.AddForce(force_direction * force_value);
            ball_rigid_body.velocity = force_direction * 0.1f;
            is_ball_moving = true;
            arrow.SetActive(false); // скрываем стрелку
            ForceIndicator.SetActive(false); // скрываем индикатор силы

            count_shot++;
        }

        // Шар влево, вправо

        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKey(KeyCode.LeftArrow))
        {
            ball.transform.Translate(-Vector3.right * 1f * Time.deltaTime);
        }
        else if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKey(KeyCode.RightArrow))
        {
            ball.transform.Translate(Vector3.right * 1f * Time.deltaTime);
        }

        #endregion

        #region движение камеры за шаром

        if(is_ball_moving && main_camera.transform.position.z <= 70)
            main_camera.transform.position = ball.transform.position + camera_offset;

        #endregion

        #region управление стрелкой

        if (Input.GetKey(KeyCode.LeftArrow) && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            if (arrow.transform.rotation.y > -10 * Mathf.PI / 360)
            {
                // поворот стрелки влево

                arrow.transform.RotateAround(       // вращение стрелки
                    arrow_pivot.transform.position, // центр вращения (неподвижная точка)
                    Vector3.up,                     // ось вращения (Y)
                    -0.05f);                         // угол (1 градус)
            }
        }

        if (Input.GetKey(KeyCode.RightArrow) && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            if (arrow.transform.rotation.y < 10 * Mathf.PI / 360)
                arrow.transform.RotateAround(arrow_pivot.transform.position, Vector3.up, 0.05f);
        }

        #endregion

        #region управление силой

        if(Input.GetKey(KeyCode.UpArrow))
            ForceValue.fillAmount += .005f;

        if (Input.GetKey(KeyCode.DownArrow))
            if(ForceValue.fillAmount >= 0.11f)
                ForceValue.fillAmount -= .005f;

        #endregion
    }
}
