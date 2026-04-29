using RPGame.Core.Spells.Symbols;
using Unity.InferenceEngine;
using UnityEngine;

namespace RPGame.SymbolRecognition
{
    public sealed class OnnxSymbolRecognizer : SymbolRecognizerBase
    {
        [SerializeField] private ModelAsset modelAsset;
        [SerializeField] private BackendType backendType = BackendType.CPU;
        [SerializeField, Range(0f, 1f)] private float minimumConfidence = 0.5f;

        private Worker worker;

        public override bool TryRecognize(Texture2D drawingTexture, out SymbolRecognitionResult result)
        {
            result = SymbolRecognitionResult.NotRecognized;

            if (drawingTexture == null)
            {
                Debug.LogWarning("ONNX symbol recognition failed because drawing texture is not assigned.", this);
                return false;
            }

            if (!EnsureWorker())
            {
                return false;
            }

            Texture2D preprocessedTexture = SymbolDrawingUtility.CreateWhiteForegroundTexture(drawingTexture);
            if (preprocessedTexture == null)
            {
                return false;
            }

            using Tensor<float> inputTensor = new Tensor<float>(
                new TensorShape(
                    1,
                    SymbolDrawingConstants.NormalizedTextureSize,
                    SymbolDrawingConstants.NormalizedTextureSize,
                    SymbolDrawingConstants.InputChannels));
            try
            {
                TextureConverter.ToTensor(
                    preprocessedTexture,
                    inputTensor,
                    new TextureTransform()
                        .SetTensorLayout(TensorLayout.NHWC)
                        .SetCoordOrigin(CoordOrigin.BottomLeft));

                worker.Schedule(inputTensor);

                if (worker.PeekOutput() is not Tensor<float> outputTensor)
                {
                    Debug.LogWarning("ONNX symbol recognition failed because model output is not a float tensor.", this);
                    return false;
                }

                using Tensor<float> outputCpu = outputTensor.ReadbackAndClone();
                float[] probabilities = outputCpu.DownloadToArray();
                if (probabilities == null || probabilities.Length == 0)
                {
                    Debug.LogWarning("ONNX symbol recognition failed because model output is empty.", this);
                    return false;
                }

                int bestClassIndex = 0;
                float confidence = probabilities[0];

                for (int i = 1; i < probabilities.Length; i++)
                {
                    if (probabilities[i] <= confidence)
                    {
                        continue;
                    }

                    bestClassIndex = i;
                    confidence = probabilities[i];
                }

                if (confidence < minimumConfidence)
                {
                    return false;
                }

                result = new SymbolRecognitionResult(bestClassIndex, confidence);
                return true;
            }
            finally
            {
                Destroy(preprocessedTexture);
            }
        }

        private bool EnsureWorker()
        {
            if (worker != null)
            {
                return true;
            }

            if (modelAsset == null)
            {
                Debug.LogWarning("ONNX symbol recognition failed because modelAsset is not assigned.", this);
                return false;
            }

            worker = new Worker(ModelLoader.Load(modelAsset), backendType);
            return true;
        }

        private void OnDisable()
        {
            worker?.Dispose();
            worker = null;
        }

        private void OnValidate()
        {
            minimumConfidence = Mathf.Clamp01(minimumConfidence);
        }
    }
}
