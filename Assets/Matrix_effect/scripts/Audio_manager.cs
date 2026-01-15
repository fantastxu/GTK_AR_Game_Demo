using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zhengsege.matrix
{
    public class Audio_manager : MonoBehaviour
    {
        public static Audio_manager instance;

        [Header("Audio Source")]
        public AudioSource audio_source;

        [Header("button audioclip")]
        public AudioClip audio_clip_btn;

        void Awake()
        {
            Audio_manager.instance = this;
        }

        public void play_btn()
        {
            this.audio_source.PlayOneShot(this.audio_clip_btn, 2f);
        }
    }
}
