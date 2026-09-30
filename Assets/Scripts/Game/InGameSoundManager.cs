using UnityEngine;

namespace Game
{
    public class InGameSoundManager : MonoBehaviour
    {
        public static InGameSoundManager Instance { get; private set; }

        [Header("SFX")]
        public AudioSource audioSource;
        public AudioClip putGoodsClip;
        public AudioClip matchClip;
        
        [Header("Background Music")]
        public AudioSource bgmSource;
        public AudioClip bgmClip;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (bgmSource != null && bgmClip != null)
            {
                bgmSource.clip = bgmClip;
                bgmSource.loop = true;
                bgmSource.Play();
            }
        }

        public void PlaySound(AudioEnum audioEnum)
        {
            if (audioSource == null) return;

            if (audioEnum == AudioEnum.PutGoods && putGoodsClip != null)
            {
                audioSource.PlayOneShot(putGoodsClip);
            }
            else if (audioEnum == AudioEnum.Match && matchClip != null)
            {
                audioSource.PlayOneShot(matchClip);
            }
        }
    }
}
