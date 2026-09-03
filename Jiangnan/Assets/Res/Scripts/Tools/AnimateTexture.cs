using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.Tools
{
    public class AnimateTexture : MonoBehaviour
    {
        public float framesPerSecond = 10.0f;
        public Sprite[] sprites;
        public bool autoPlay;
        public bool loop = true;

        private Image image;
        private Coroutine playRoutine;

        private void Awake()
        {
            image = GetComponent<Image>();
            ResetToFirstFrame();
        }

        private void OnEnable()
        {
            if (autoPlay)
            {
                PlayLoop();
            }
            else
            {
                ResetToFirstFrame();
            }
        }

        private void OnDisable()
        {
            StopPlayback();
        }

        public void ResetToFirstFrame()
        {
            StopPlayback();
            if (image == null)
            {
                image = GetComponent<Image>();
            }

            if (image != null && sprites != null && sprites.Length > 0)
            {
                image.sprite = sprites[0];
            }
        }

        public void PlayLoop()
        {
            StopPlayback();
            playRoutine = StartCoroutine(PlayLoopRoutine());
        }

        public void PlayOnce(Action onComplete = null)
        {
            StopPlayback();
            playRoutine = StartCoroutine(PlayOnceRoutine(onComplete));
        }

        public void StopPlayback()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }
        }

        private IEnumerator PlayLoopRoutine()
        {
            if (sprites == null || sprites.Length == 0)
            {
                yield break;
            }

            while (true)
            {
                for (var index = 0; index < sprites.Length; index++)
                {
                    if (image != null)
                    {
                        image.sprite = sprites[index];
                    }

                    yield return new WaitForSeconds(1f / framesPerSecond);
                }

                if (!loop)
                {
                    break;
                }
            }
        }

        private IEnumerator PlayOnceRoutine(Action onComplete)
        {
            if (image == null)
            {
                image = GetComponent<Image>();
            }

            if (sprites == null || sprites.Length == 0)
            {
                onComplete?.Invoke();
                yield break;
            }

            for (var index = 0; index < sprites.Length; index++)
            {
                if (image != null)
                {
                    image.sprite = sprites[index];
                }

                yield return new WaitForSeconds(1f / framesPerSecond);
            }

            playRoutine = null;
            onComplete?.Invoke();
        }
    }
}
