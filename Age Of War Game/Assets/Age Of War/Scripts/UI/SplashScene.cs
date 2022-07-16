using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AgeOfWar.UI
{
    public class SplashScene : MonoBehaviour
    {
        // Parameters
        [SerializeField]
        private float WaitOnImageTime = 0.75f;
        [SerializeField]
        private float WaitOnBlackTime = 0.75f;
        [SerializeField]
        private float FadeToBlackTime = 0.75f;
        [SerializeField]
        private LeanTweenType FadeToBlackTweenType = LeanTweenType.linear;
        [SerializeField]
        private float FadeToImageTime = 0.75f;
        [SerializeField]
        private LeanTweenType FadeToImageTweenType = LeanTweenType.linear;

        // Objects
        [SerializeField]
        private List<RectTransform> SplashImages = new List<RectTransform>();

        // Variables
        private int CurrentImageIndex = -1;

        // Start is called before the first frame update
        void Start()
        {
            if (SplashImages.Count > 0)
            {
                FadeToImage();
            }
            else
            {
                SceneLoadManager.Instance.LoadScene("Main Menu");
            }
        }

        private void FadeToBlack()
        {
            LeanTween.alpha(SplashImages[CurrentImageIndex], 0, FadeToBlackTime).setEase(FadeToBlackTweenType).setOnComplete(FadeToImage);
        }

        private void FadeToImage()
        {
            CurrentImageIndex++;
            if (CurrentImageIndex == SplashImages.Count)
            {
                SceneLoadManager.Instance.LoadScene("Main Menu");
            }
            else
            {
                LeanTween.alpha(SplashImages[CurrentImageIndex], 1, FadeToImageTime).setEase(FadeToImageTweenType).setOnComplete(WaitOnImage);
            }
        }

        private void WaitOnImage()
        {
            LeanTween.alpha(SplashImages[CurrentImageIndex], 1, WaitOnImageTime).setEase(FadeToImageTweenType).setOnComplete(FadeToBlack);
        }

        private void WaitOnBlack()
        {
            LeanTween.alpha(SplashImages[CurrentImageIndex], 1, WaitOnBlackTime).setEase(FadeToImageTweenType).setOnComplete(FadeToImage);
        }
    }
}
