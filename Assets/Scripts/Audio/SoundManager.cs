using UnityEngine;

namespace DesertTreasureHunt
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private AudioSource sfxSource;
        private AudioSource musicSource;

        private AudioClip coinClip;
        private AudioClip gemClip;
        private AudioClip keyClip;
        private AudioClip jumpClip;
        private AudioClip slideClip;
        private AudioClip hurtClip;
        private AudioClip chestClip;
        private AudioClip powerUpClip;
        private AudioClip gameOverClip;
        private AudioClip ambientDesertLoop;

        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        [Range(0f, 1f)] public float musicVolume = 0.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;

            GenerateProceduralAudioClips();
        }

        private void Start()
        {
            PlayDesertAtmosphere();
        }

        private void GenerateProceduralAudioClips()
        {
            coinClip = CreateSynthClip("CoinPing", 0.16f, (t) =>
            {
                float freq = Mathf.Lerp(987.77f, 1318.51f, t * 2f); // B5 to E6
                float env = Mathf.Exp(-t * 22f);
                return Mathf.Sin(2f * Mathf.PI * freq * t) * env;
            });

            gemClip = CreateSynthClip("GemSparkle", 0.35f, (t) =>
            {
                float freq1 = 1046.50f; // C6
                float freq2 = 1318.51f; // E6
                float freq3 = 1567.98f; // G6
                float env = Mathf.Exp(-t * 8f);
                float s = (Mathf.Sin(2f * Mathf.PI * freq1 * t) +
                           Mathf.Sin(2f * Mathf.PI * freq2 * t * 1.5f) +
                           Mathf.Sin(2f * Mathf.PI * freq3 * t * 2f)) / 3f;
                return s * env;
            });

            keyClip = CreateSynthClip("KeyChime", 0.45f, (t) =>
            {
                float freq = 1760f; // A6
                float vibrato = Mathf.Sin(2f * Mathf.PI * 15f * t) * 40f;
                float env = Mathf.Exp(-t * 6f);
                return Mathf.Sin(2f * Mathf.PI * (freq + vibrato) * t) * env;
            });

            jumpClip = CreateSynthClip("JumpWhoosh", 0.22f, (t) =>
            {
                float freq = Mathf.Lerp(150f, 600f, t);
                float env = Mathf.Sin(t / 0.22f * Mathf.PI);
                float whiteNoise = (Random.value * 2f - 1f) * 0.3f;
                return (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f + whiteNoise) * env;
            });

            slideClip = CreateSynthClip("SlideSwoosh", 0.32f, (t) =>
            {
                float env = Mathf.Sin(t / 0.32f * Mathf.PI);
                float noise = (Random.value * 2f - 1f) * 0.8f;
                float lowSine = Mathf.Sin(2f * Mathf.PI * 120f * t) * 0.4f;
                return (noise + lowSine) * env * 0.6f;
            });

            hurtClip = CreateSynthClip("PlayerHurt", 0.35f, (t) =>
            {
                float freq = Mathf.Lerp(220f, 60f, t);
                float env = Mathf.Exp(-t * 10f);
                float dist = Mathf.Clamp(Mathf.Sin(2f * Mathf.PI * freq * t) * 2.5f, -1f, 1f);
                return dist * env;
            });

            chestClip = CreateSynthClip("ChestOpen", 0.65f, (t) =>
            {
                float tone1 = Mathf.Sin(2f * Mathf.PI * 523.25f * t); // C5
                float tone2 = Mathf.Sin(2f * Mathf.PI * 659.25f * t); // E5
                float tone3 = Mathf.Sin(2f * Mathf.PI * 783.99f * t); // G5
                float tone4 = Mathf.Sin(2f * Mathf.PI * 1046.50f * t); // C6
                float blend = (tone1 + tone2 + tone3 + tone4) * 0.25f;
                float env = Mathf.Clamp01(t / 0.1f) * Mathf.Exp(-t * 3f);
                return blend * env;
            });

            powerUpClip = CreateSynthClip("PowerUp", 0.5f, (t) =>
            {
                float freq = Mathf.Lerp(300f, 1200f, t * t);
                float pulse = (Mathf.Sin(2f * Mathf.PI * 25f * t) > 0f ? 1f : -1f) * 0.2f;
                float env = Mathf.Exp(-t * 2.5f);
                return (Mathf.Sin(2f * Mathf.PI * freq * t) + pulse) * env * 0.8f;
            });

            gameOverClip = CreateSynthClip("GameOver", 1.2f, (t) =>
            {
                float f = Mathf.Lerp(380f, 80f, t);
                float env = Mathf.Exp(-t * 2.5f);
                return Mathf.Sin(2f * Mathf.PI * f * t) * env;
            });

            // Ambient Desert Breeze & Darbuka rhythm generator
            ambientDesertLoop = CreateSynthClip("DesertAmbience", 4.0f, (t) =>
            {
                // Soft blowing wind
                float windPhase = Mathf.Sin(2f * Mathf.PI * 0.25f * t);
                float windNoise = (Random.value * 2f - 1f) * (0.15f + 0.1f * windPhase);

                // Desert drum groove (beats at 0.0, 0.5, 1.0, 1.5, 2.0, 2.5, 3.0, 3.5s)
                float beatFraction = (t % 0.5f) / 0.5f;
                float drumEnv = Mathf.Exp(-beatFraction * 20f);
                float drumTone = Mathf.Sin(2f * Mathf.PI * 75f * beatFraction) * drumEnv * 0.35f;

                // Syncopated percussion rim
                float subFraction = (t % 0.25f) / 0.25f;
                float rimEnv = Mathf.Exp(-subFraction * 40f);
                float rimNoise = (Random.value * 2f - 1f) * rimEnv * 0.15f;

                return (windNoise + drumTone + rimNoise) * 0.8f;
            });
        }

        private AudioClip CreateSynthClip(string name, float lengthSec, System.Func<float, float> waveFunc)
        {
            int sampleRate = 44100;
            int totalSamples = Mathf.FloorToInt(sampleRate * lengthSec);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                samples[i] = Mathf.Clamp(waveFunc(t), -1f, 1f);
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public void PlayCoin() => PlaySfx(coinClip, 0.8f, Random.Range(0.95f, 1.15f));
        public void PlayGem() => PlaySfx(gemClip, 0.9f, Random.Range(0.95f, 1.05f));
        public void PlayKey() => PlaySfx(keyClip, 1.0f);
        public void PlayJump() => PlaySfx(jumpClip, 0.7f, Random.Range(0.9f, 1.1f));
        public void PlaySlide() => PlaySfx(slideClip, 0.6f);
        public void PlayHurt() => PlaySfx(hurtClip, 1.0f);
        public void PlayChest() => PlaySfx(chestClip, 1.0f);
        public void PlayPowerUp() => PlaySfx(powerUpClip, 0.9f);
        public void PlayGameOver() => PlaySfx(gameOverClip, 1.0f);

        private void PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, volume * sfxVolume);
        }

        public void PlayDesertAtmosphere()
        {
            if (ambientDesertLoop == null || musicSource == null) return;
            musicSource.clip = ambientDesertLoop;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }
}
