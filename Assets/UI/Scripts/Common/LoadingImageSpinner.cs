using UnityEngine;

namespace RPGame.UI.Common
{
    public sealed class LoadingImageSpinner : MonoBehaviour
    {
        [SerializeField] private RectTransform loadingImage;
        [SerializeField] private float rotationSpeedDegreesPerSecond = 180f;

        private void Update()
        {
            if (loadingImage != null)
            {
                loadingImage.Rotate(
                    Vector3.forward,
                    rotationSpeedDegreesPerSecond * Time.unscaledDeltaTime);
            }
        }
    }
}
