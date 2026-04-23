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
        [SerializeField] private int inputSize = 64;
        [SerializeField] private int inputChannels = 3;

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

            using Tensor<float> inputTensor = new Tensor<float>(
                new TensorShape(1, inputSize, inputSize, inputChannels));

            TextureTransform textureTransform = new TextureTransform()
                .SetTensorLayout(TensorLayout.NHWC);
            TextureConverter.ToTensor(drawingTexture, inputTensor, textureTransform);

            worker.Schedule(inputTensor);

            if (worker.PeekOutput() is not Tensor<float> outputTensor)
            {
                Debug.LogWarning("ONNX symbol recognition failed because model output is not a float tensor.", this);
                return false;
            }

            using Tensor<float> outputCpu = outputTensor.ReadbackAndClone();
            Debug.Log($"ONNX output tensor shape: {outputCpu.shape}.", this);
            float[] probabilities = outputCpu.DownloadToArray();
            if (probabilities.Length == 0)
            {
                Debug.LogWarning("ONNX symbol recognition failed because model output is empty.", this);
                return false;
            }

            int bestClassIndex = GetBestClassIndex(probabilities, out float confidence);
            Debug.Log($"ONNX raw output: [{string.Join(", ", probabilities)}]", this);
            Debug.Log($"ONNX best class: {bestClassIndex}, confidence: {confidence}.", this);

            if (confidence < minimumConfidence)
            {
                Debug.Log($"ONNX symbol confidence too low. Class: {bestClassIndex}, Confidence: {confidence}.", this);
                return false;
            }

            result = new SymbolRecognitionResult(bestClassIndex, confidence);
            return true;
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

            Model model = ModelLoader.Load(modelAsset);
            worker = new Worker(model, backendType);
            return true;
        }

        private int GetBestClassIndex(float[] probabilities, out float confidence)
        {
            int bestClassIndex = 0;
            confidence = probabilities[0];

            for (int i = 1; i < probabilities.Length; i++)
            {
                if (probabilities[i] <= confidence)
                {
                    continue;
                }

                bestClassIndex = i;
                confidence = probabilities[i];
            }

            return bestClassIndex;
        }

        private void OnDisable()
        {
            worker?.Dispose();
            worker = null;
        }

        private void OnValidate()
        {
            minimumConfidence = Mathf.Clamp01(minimumConfidence);
            inputSize = Mathf.Max(1, inputSize);
            inputChannels = Mathf.Clamp(inputChannels, 1, 4);
        }
    }
}
