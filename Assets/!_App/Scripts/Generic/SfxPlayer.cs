using UnityEngine;
using System.Collections;

namespace CookingStar
{
    public class SfxPlayer : MonoBehaviour
    {
        public static SfxPlayer instance;

        public AudioClip[] availableAudioclips;
        private AudioSource aso;

        private void Awake()
        {
            instance = this;
            aso = GetComponent<AudioSource>();
            DontDestroyOnLoad(this.gameObject);
        }

        /// <summary>
        /// Play the given audioclip
        /// </summary>
        /// <param name="sfxID"></param>
        public void PlaySfx(int sfxID)
        {
            if (!FbMusicPlayer.globalSoundState)
                return;

            aso.PlayOneShot(availableAudioclips[sfxID]);
        }
    }
}