/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */


using UnityEngine;


namespace Live2D.Cubism.Framework.MouthMovement
{
    /// <summary>
    /// Real-time <see cref="CubismMouthController"/> input from <see cref="AudioSource"/>s.
    /// Modified to support dual AudioSource input for crossfade.
    /// </summary>
    [RequireComponent(typeof(CubismMouthController))]
    public sealed class CubismAudioMouthInput : MonoBehaviour
    {
        /// <summary>
        /// First audio source to sample.
        /// </summary>
        [SerializeField]
        public AudioSource AudioInput1;

        /// <summary>
        /// Second audio source to sample (for crossfade).
        /// </summary>
        [SerializeField]
        public AudioSource AudioInput2;

        /// <summary>
        /// Sampling quality.
        /// </summary>
        [SerializeField]
        public CubismAudioSamplingQuality SamplingQuality;

        /// <summary>
        /// Audio gain.
        /// </summary>
        [Range(1.0f, 20.0f)]
        public float Gain = 1.0f;

        /// <summary>
        /// Smoothing.
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float Smoothing;

        /// <summary>
        /// How to combine audio from both sources.
        /// </summary>
        public enum CombineMode
        {
            Maximum,    // Use the maximum value from both sources
            Average,    // Use the average of both sources
            Sum        // Sum both sources (clamped to 1.0)
        }

        /// <summary>
        /// Method to combine audio from both sources.
        /// </summary>
        [SerializeField]
        public CombineMode AudioCombineMode = CombineMode.Maximum;

        /// <summary>
        /// Current samples for first audio source.
        /// </summary>
        private float[] Samples1 { get; set; }

        /// <summary>
        /// Current samples for second audio source.
        /// </summary>
        private float[] Samples2 { get; set; }

        /// <summary>
        /// Last root mean square.
        /// </summary>
        private float LastRms { get; set; }

        /// <summary>
        /// Buffer for <see cref="Mathf.SmoothDamp(float, float, ref float, float)"/> velocity.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private float VelocityBuffer;

        /// <summary>
        /// Targeted <see cref="CubismMouthController"/>.
        /// </summary>
        private CubismMouthController Target { get; set; }

        /// <summary>
        /// True if instance is initialized.
        /// </summary>
        private bool IsInitialized
        {
            get { return Samples1 != null && Samples2 != null; }
        }

        /// <summary>
        /// Makes sure instance is initialized.
        /// </summary>
        private void TryInitialize()
        {
            // Return early if already initialized.
            if (IsInitialized)
            {
                return;
            }

            // Initialize samples buffers.
            int sampleSize;
            switch (SamplingQuality)
            {
                case (CubismAudioSamplingQuality.VeryHigh):
                    {
                        sampleSize = 256;
                        break;
                    }
                case (CubismAudioSamplingQuality.Maximum):
                    {
                        sampleSize = 512;
                        break;
                    }
                default:
                    {
                        sampleSize = 256;
                        break;
                    }
            }

            Samples1 = new float[sampleSize];
            Samples2 = new float[sampleSize];

            // Cache target.
            Target = GetComponent<CubismMouthController>();
        }

        /// <summary>
        /// Processes audio data from a single source.
        /// </summary>
        private float ProcessAudioSource(AudioSource audioSource, float[] samples)
        {
            if (audioSource == null || !audioSource.isPlaying)
            {
                return 0f;
            }

            // Sample audio.
            var total = 0f;
            audioSource.GetOutputData(samples, 0);

            for (var i = 0; i < samples.Length; ++i)
            {
                var sample = samples[i];
                total += (sample * sample);
            }

            // Compute root mean square over samples.
            return Mathf.Sqrt(total / samples.Length);
        }

        #region Unity Event Handling

        /// <summary>
        /// Samples audio input and applies it to mouth controller.
        /// </summary>
        private void Update()
        {
            // Process both audio sources
            var rms1 = ProcessAudioSource(AudioInput1, Samples1);
            var rms2 = ProcessAudioSource(AudioInput2, Samples2);

            // Combine RMS values based on selected mode
            float combinedRms;
            switch (AudioCombineMode)
            {
                case CombineMode.Maximum:
                    combinedRms = Mathf.Max(rms1, rms2);
                    break;
                case CombineMode.Average:
                    combinedRms = (rms1 + rms2) * 0.5f;
                    break;
                case CombineMode.Sum:
                    combinedRms = rms1 + rms2;
                    break;
                default:
                    combinedRms = Mathf.Max(rms1, rms2);
                    break;
            }

            // Apply gain
            combinedRms *= Gain;

            // Clamp root mean square.
            combinedRms = Mathf.Clamp(combinedRms, 0.0f, 1.0f);

            // Smooth rms.
            combinedRms = Mathf.SmoothDamp(LastRms, combinedRms, ref VelocityBuffer, Smoothing * 0.1f);

            // Set rms as mouth opening and store it for next evaluation.
            Target.MouthOpening = combinedRms;

            LastRms = combinedRms;
        }

        /// <summary>
        /// Initializes instance.
        /// </summary>
        private void OnEnable()
        {
            TryInitialize();
        }

        #endregion

        #region Backward Compatibility

        /// <summary>
        /// Legacy property for backward compatibility.
        /// Sets AudioInput1 when assigned.
        /// </summary>
        [System.Obsolete("Use AudioInput1 instead")]
        public AudioSource AudioInput
        {
            get { return AudioInput1; }
            set { AudioInput1 = value; }
        }

        #endregion
    }
}