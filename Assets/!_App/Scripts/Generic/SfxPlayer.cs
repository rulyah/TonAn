using UnityEngine;
using System.Collections;

namespace CookingStar
{
    public class SfxPlayer : MonoBehaviour
    {
        public static SfxPlayer instance { get; private set; }

        public AudioClip[] availableAudioclips;
        private AudioSource aso;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                aso = GetComponent<AudioSource>();
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
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