using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Controls : MonoBehaviour
{
    public GameObject gameOver;  // ссылка на Canvas с финальным меню
    public GameObject history;   // ссылка на Canvas со счетом

    private const string GAME_HISTORY_FILE = "history.bin";

    private List<Info> game_history;

    private Text gameOverStatisctics;
    private Text timer;

    private Text score;

    public static bool is_pause;

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
    private AudioSource sqeek;  // ссылка на звук, привязанный к стрелке

    private GameObject ForceIndicator;
    private Image ForceValue; // ForceIndicator - image
    private float force_factor = 40000f;
    private AudioSource force_sound; // ссылка на звук, привязанный к индикаторы силы

    private int count_shot;         // количество попыток
    private int count_kegels_down;  // количество сбитых кеглей
    private int count_kegels_up;    // количество оставшихся кеглей

    private Text statisctics;       // текст для отображения статистики

    void Start()
    {
        is_pause = false;

        game_history = new List<Info>();

        gameOverStatisctics = GameObject.Find("TextInfo").GetComponent<Text>();
        timer = GameObject.Find("time").GetComponent<Text>();

        score = GameObject.Find("TextScore").GetComponent<Text>();

        gameOver.SetActive(false);
        history.SetActive(false);

        ball = GameObject.Find("Ball");
        ball_rigid_body = ball.GetComponent<Rigidbody>();
        ball_start_pos = ball.transform.position;
        ball_start_rotation = ball.transform.rotation;
        is_ball_moving = false;

        arrow = GameObject.Find("Arrow");
        arrow_pivot = GameObject.Find("PivotPoint");
        sqeek = arrow.GetComponent<AudioSource>();

        ForceIndicator = GameObject.Find("ForceIndicator");
        ForceValue = GameObject.Find("ForceValue").GetComponent<Image>();
        force_sound = ForceValue.GetComponent<AudioSource>();

        main_camera = GameObject.Find("MainCamera");
        camera_start_pos = main_camera.transform.position;
        camera_offset = main_camera.transform.position - ball.transform.position;

        count_shot = 1;
        count_kegels_down = 0;
        count_kegels_up = GameObject.FindGameObjectsWithTag("Kegel").Length;

        statisctics = GameObject.Find("statisctics").GetComponent<Text>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && !is_pause)
        {
            is_pause = true;
            history.SetActive(true);

            LoadHistory();

            if (game_history == null || game_history.Count == 0)
                score.text = "Нет игр!";
            else
                Info.Print(game_history, score);
        }
        else if(Input.GetKeyDown(KeyCode.H) && is_pause)
        {
            is_pause = false;
            history.SetActive(false);
        }

        #region управление шаром

        if (is_ball_moving)
        {
            if(ball_rigid_body.velocity.magnitude < 0.1f)
            {
                is_ball_moving = false;

                // Сбрасываем остаточную скорость
                ball_rigid_body.velocity = Vector3.zero;         // поступательная скорость
                ball_rigid_body.angularVelocity = Vector3.zero;  // вращательная скорость

                ball.transform.position = ball_start_pos;        // возврат в начальную позицию
                ball.transform.rotation = ball_start_rotation;   // возврат в начальную позицию поворота

                arrow.SetActive(true);  // Показываем стрелку

                ForceIndicator.SetActive(true);  // показываем инликатор силы

                main_camera.transform.position = camera_start_pos;  // возвращаем камеру на начальную позицию

                // анализируем кегли
                count_kegels_up = 0;

                // находим их по тегу
                GameObject[] kegels = GameObject.FindGameObjectsWithTag("Kegel");

                //обход циклом
                foreach(GameObject kegel in kegels)
                {
                    if(kegel.transform.position.y > 0.1 || kegel.transform.position.y < - 0.1)
                    {
                        // кегля лежит - убираем ее
                        kegel.SetActive(false);
                        
                        count_kegels_down++;
                    }
                    else
                    {
                        kegel.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        kegel.transform.rotation.SetEulerAngles(Vector3.zero);

                        kegel.transform.position.Set(kegel.transform.position.x, 0, kegel.transform.position.z);

                        count_kegels_up++;
                    }
                }
                count_shot++;

                // Вывод статистики
                statisctics.text = "Попытка: " + count_shot + "\n" +
                                   "Сбито: " + count_kegels_down + "\n" +
                                   "Осталось: " + count_kegels_up;

                // Если все кегли сбиты
                if (count_kegels_up == 0)
                {
                    is_pause = true;
                    gameOver.SetActive(true);

                    gameOverStatisctics.text = "Попыток: " + (count_shot - 1) + "\n" +
                                               "Время: " + timer.text;

                    SaveHistory();  // сохранение результата в файл
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

                // Проигрываем звук
                if(!sqeek.isPlaying)
                    sqeek.Play();
            }
        }

        if (Input.GetKey(KeyCode.RightArrow) && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            if (arrow.transform.rotation.y < 10 * Mathf.PI / 360)
                arrow.transform.RotateAround(arrow_pivot.transform.position, Vector3.up, 0.05f);

            // Проигрываем звук
            if (!sqeek.isPlaying)
                sqeek.Play();
        }

        #endregion

        #region управление силой

        if (Input.GetKey(KeyCode.UpArrow))
        {
            ForceValue.fillAmount += .005f;
            //if (!force_sound.isPlaying)
                force_sound.Play();
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (ForceValue.fillAmount >= 0.11f)
            {
                ForceValue.fillAmount -= .005f;
                //if (!force_sound.isPlaying)
                    force_sound.Play();
            }
        }

        #endregion
    }

    /**
     * Действие при выходе из программы
     */
    private void OnApplicationQuit()
    {
        Debug.Log("Выход");

        if (File.Exists(GAME_HISTORY_FILE))
            File.Delete(GAME_HISTORY_FILE);
    }

    /**
     * Обработчик нажатия кнопки "новая игра"
     * на меню GameOver
     */
    public void RestartClick()
    {
        SceneManager.LoadScene("SampleScene");
        is_pause = false;
    }

    /**
     * Обработчик нажатия кнопки "выход"
     * на меню GameOver
     */
    public void ExitButton()
    {
        Application.Quit();
    }

    /**
     * Загрузка истории при нажатии на кнопку "H"
     */
    private void LoadHistory()
    {
        if (File.Exists(GAME_HISTORY_FILE))
        {
            BinaryFormatter bf = new BinaryFormatter();
            using(Stream reader = new FileStream(GAME_HISTORY_FILE, FileMode.Open))
            {
                game_history = (List<Info>)bf.Deserialize(reader);
            }
        }
    }

    /**
     * Сохранение данных в файл после каждой игры
     */
    private void SaveHistory()
    {
        game_history.Add(new Info
        {
            CountShot = count_shot - 1,
            Timer = timer.text
        });

        BinaryFormatter bf = new BinaryFormatter();
        using (Stream write = new FileStream(GAME_HISTORY_FILE, FileMode.Create))
        {
            bf.Serialize(write, game_history);
        }
    }
}

[Serializable]
class Info
{
    public int CountShot { get; set; }
    public string Timer { get; set; }

    public static void Print(List<Info> list, Text score_list)
    {
        if (list == null || list.Count == 0)
            return;

        score_list.text = "";
        int num = 1;

        var sortedList = (from i in list
                          orderby i.CountShot
                          select i).Take(10);

        foreach(Info info in sortedList)
        {
            score_list.text += (num < 10 ? "  " + num : num.ToString()) +
                " - Бросков: " + info.CountShot.ToString() + " -  Время: " + info.Timer + "\n";
            num++;
        }
    }
}