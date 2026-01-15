using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zhengsege.matrix
{
    public class Demo_control : MonoBehaviour
    {

        public int index = 0;


        public GameObject[] array_game_obj_display;
        public GameObject[] array_game_obj_camera;

        void Start()
        {
            for (int i = 0; i < this.array_game_obj_display.Length; i++)
            {
                if (this.index == i)
                {
                    if (this.array_game_obj_display[i].activeSelf == false)
                    {
                        this.array_game_obj_display[i].SetActive(true);
                    }
                    if (this.array_game_obj_camera[i].activeSelf == false)
                    {
                        this.array_game_obj_camera[i].SetActive(true);
                    }

                }
                else
                {
                    if (this.array_game_obj_display[i].activeSelf == true)
                    {
                        this.array_game_obj_display[i].SetActive(false);
                    }
                    if (this.array_game_obj_camera[i].activeSelf == true)
                    {
                        this.array_game_obj_camera[i].SetActive(false);
                    }
                }
            }
        }

        public void on_previous_btn()
        {
            Audio_manager.instance.play_btn();

            this.index--;
            if (this.index <= -1)
                this.index = this.array_game_obj_display.Length - 1;

            for (int i = 0; i < this.array_game_obj_display.Length; i++)
            {
                if (this.index == i)
                {
                    if (this.array_game_obj_display[i].activeSelf == false)
                    {
                        this.array_game_obj_display[i].SetActive(true);
                    }
                    if (this.array_game_obj_camera[i].activeSelf == false)
                    {
                        this.array_game_obj_camera[i].SetActive(true);
                    }

                }
                else
                {
                    if (this.array_game_obj_display[i].activeSelf == true)
                    {
                        this.array_game_obj_display[i].SetActive(false);
                    }
                    if (this.array_game_obj_camera[i].activeSelf == true)
                    {
                        this.array_game_obj_camera[i].SetActive(false);
                    }
                }
            }
        }

        public void on_next_btn()
        {

            Audio_manager.instance.play_btn();

            this.index++;
            if (this.index >= this.array_game_obj_display.Length)
                this.index = 0;

            for (int i = 0; i < this.array_game_obj_display.Length; i++)
            {
                if (this.index == i)
                {
                    if (this.array_game_obj_display[i].activeSelf == false)
                    {
                        this.array_game_obj_display[i].SetActive(true);
                    }
                    if (this.array_game_obj_camera[i].activeSelf == false)
                    {
                        this.array_game_obj_camera[i].SetActive(true);
                    }

                }
                else
                {
                    if (this.array_game_obj_display[i].activeSelf == true)
                    {
                        this.array_game_obj_display[i].SetActive(false);
                    }
                    if (this.array_game_obj_camera[i].activeSelf == true)
                    {
                        this.array_game_obj_camera[i].SetActive(false);
                    }
                }
            }
        }
    }
}
