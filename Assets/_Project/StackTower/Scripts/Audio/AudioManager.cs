using UnityEngine;
using StackTower.Core;
using StackTower.Gameplay;

namespace StackTower.Audio
{
    /// <summary>
    /// Generates and plays procedural sound effects for placement, perfect stack, and game over.
    /// Completely standalone with zero external audio assets required.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _audioSource;
        private AudioClip _buttonClickClip;
        private AudioClip _gameOverClip;

        // Base musical scale frequencies (Pentatonic C major)
        private readonly float[] _scaleNotes = new float[]
        {
            261.63f, // C4
            293.66f, // D4
            329.63f, // E4
            392.00f, // G4
            440.00f, // A4
            523.25f, // C5
            587.33f, // D5
            659.25f, // E5
            783.99f, // G5
            880.00f, // A5
            1046.50f // C6
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f; // 2D sound

            GenerateClips();
        }

        private void Start()
        {
            if (TowerController.Instance != null)
            {
                TowerController.Instance.OnBlockPlaced += HandleBlockPlaced;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameOver += HandleGameOver;
            }
        }

        private void HandleBlockPlaced(Vector3 pos, bool isPerfect, int combo)
        {
            if (isPerfect)
            {
                PlayPerfectChime(combo);
            }
            else
            {
                PlayPlacementPluck(combo);
            }
        }

        private void HandleGameOver(int finalScore, int bestScore, bool isNewBest)
        {
            PlayGameOverSound();
        }

        public void PlayButtonSound()
        {
            if (_audioSource != null && _buttonClickClip != null)
            {
                _audioSource.PlayOneShot(_buttonClickClip, 0.4f);
            }
        }

        private void PlayPlacementPluck(int combo)
        {
            int noteIndex = Mathf.Clamp(combo, 0, _scaleNotes.Length - 1);
            float freq = _scaleNotes[noteIndex];
            AudioClip clip = GenerateSineChime(freq, 0.12f, 0.005f, 0.08f);
            _audioSource.PlayOneShot(clip, 0.5f);
        }

        private void PlayPerfectChime(int combo)
        {
            int baseIndex = Mathf.Clamp(combo + 2, 0, _scaleNotes.Length - 3);
            float f1 = _scaleNotes[baseIndex];
            float f2 = _scaleNotes[baseIndex + 2];
            AudioClip clip = GenerateHarmonicChord(f1, f2, 0.28f);
            _audioSource.PlayOneShot(clip, 0.65f);
        }

        private void PlayGameOverSound()
        {
            if (_audioSource != null && _gameOverClip != null)
            {
                _audioSource.PlayOneShot(_gameOverClip, 0.5f);
            }
        }

        private void GenerateClips()
        {
            _buttonClickClip = GenerateSineChime(600f, 0.04f, 0.002f, 0.03f);
            _gameOverClip = GenerateDescendingTone(440f, 220f, 0.35f);
        }

        private AudioClip GenerateSineChime(float frequency, float duration, float attack, float decay)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope;
                if (t < attack)
                    envelope = t / attack;
                else
                    envelope = Mathf.Exp(-((t - attack) / decay) * 3.0f);

                // Add slight warm second harmonic
                float tone = Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.8f
                           + Mathf.Sin(2f * Mathf.PI * (frequency * 2f) * t) * 0.2f;

                samples[i] = tone * envelope;
            }

            AudioClip clip = AudioClip.Create("Chime_" + frequency, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateHarmonicChord(float freq1, float freq2, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t / (duration * 0.35f));

                float tone = (Mathf.Sin(2f * Mathf.PI * freq1 * t) * 0.5f)
                           + (Mathf.Sin(2f * Mathf.PI * freq2 * t) * 0.4f)
                           + (Mathf.Sin(2f * Mathf.PI * (freq1 * 2f) * t) * 0.1f);

                samples[i] = tone * envelope;
            }

            AudioClip clip = AudioClip.Create("Chord_" + freq1, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateDescendingTone(float startFreq, float endFreq, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / duration;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, t);
                float envelope = Mathf.Sin(t * Mathf.PI);

                samples[i] = Mathf.Sin(2f * Mathf.PI * currentFreq * ((float)i / sampleRate)) * envelope * 0.4f;
            }

            AudioClip clip = AudioClip.Create("GameOverTone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (TowerController.Instance != null)
            {
                TowerController.Instance.OnBlockPlaced -= HandleBlockPlaced;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameOver -= HandleGameOver;
            }
        }
    }
}

