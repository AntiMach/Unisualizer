using UnityEngine;

public class Visualizer : MonoBehaviour {
    public GameObject cubeObject;

    [Range(0, 128)]
    public int samples = 64;

    private float strength = 200f;
    private float size = 0f;

    private DualSpectrum spectrum;
    private GameObject[] lines;
    private Vector3 center = Vector3.zero;

    private GameObject center1;
    private GameObject center2;
    private ParticleSystem pSystem;

    private Display volume;
    private Display intensity;
    private Display easing;

    private Display minimumFreq;
    private Display maximumFreq;

    void Start()
    {
        // Set important initial values
        spectrum = new DualSpectrum(samples);
        lines = new GameObject[samples * 2];

        // Get important objects
        center1 = GameObject.Find("Center1");
        center2 = GameObject.Find("Center2");
        pSystem = GetComponentInChildren<ParticleSystem>();

        // Get volume, intensity and easing objects
        volume = GameObject.Find("Volume").GetComponent<Display>();
        intensity = GameObject.Find("Intensity").GetComponent<Display>();
        easing = GameObject.Find("Easing").GetComponent<Display>();

        minimumFreq = GameObject.Find("Minimum").GetComponent<Display>();
        maximumFreq = GameObject.Find("Maximum").GetComponent<Display>();

        // Create lines from flat cube instances
        for (int i = 0; i < samples * 2; i++)
        {
            GameObject cubeInstance = Instantiate(cubeObject);
            cubeInstance.transform.parent = transform;
            cubeInstance.transform.name = "Cube" + i;
            lines[i] = cubeInstance;
        }
    }
    
    void Update ()
    {
        // Get spectrum data and average out the scales
        if (Menu.source.clip != null)
        {
            DualSpectrum section = new DualSpectrum(8192, Menu.source)
                .BandSection((int)minimumFreq.value, (int)maximumFreq.value, samples);
            spectrum.Interpolate(section, easing.value);
        }

        // Update strength, size and center
        strength = volume.value > 0 ? intensity.value / (volume.value / 100) : 0;
        size = spectrum.GetAverage() * strength + 1;
        center += (new Vector3(Random.value - 0.5f, Random.value - 0.5f) * (size - 1) - center) / 5;

        // Position and scale the centers;
        center1.transform.localScale = new Vector3(size * 2 + 0.1f, size * 2 + 0.1f);
        center2.transform.localScale = new Vector3(size * 2 - 0.2f, size * 2 - 0.2f) * 0.9f;
        center1.transform.position = center;
        center2.transform.position = center + Vector3.back;

        // Update the Particles with a certain speed
        UpdateParticles(pSystem, 9 - 2 / (size - 0.75f));

        // Check if lines exist. This is to prevent any possible errors at the start
        if (lines == null) return;


        // Update lines
        for (int i = 0; i < samples; i++)
        {
            float left = spectrum.LeftChannel[i] * 1.5f * strength;
            float right = spectrum.RightChannel[i] * 1.5f * strength;

            // dist -> Left channel's sample strenght
            // radAngle -> Angle in radians
            // degAngle -> same thing in degrees
            // u -> Line's directional unit vector
            // color -> HSV based color for rainbow effect
            float radAngle = (i + 0.5f) * Mathf.PI / samples - Mathf.PI;
            float degAngle = 180f * radAngle / Mathf.PI;
            Vector3 pos = new Vector3(Mathf.Sin(radAngle), Mathf.Cos(radAngle));
            Color color = Color.HSVToRGB(1 - (i * 100 / samples + Time.time * 25) % 100 / 100f, 0.8f, 1);

            UpdateLine(i, pos, degAngle, left, color);

            pos.x *= -1;
            UpdateLine(i + samples, pos, -degAngle, right, color);
        }
    }

    void UpdateLine(int line, Vector3 pos, float angle, float sample, Color color)
    {
        lines[line].transform.position = pos * (sample / 4 + size) + center;
        lines[line].transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
        lines[line].transform.localScale = new Vector3(6f * size / samples, sample / 2);
        lines[line].GetComponent<Renderer>().material.color = color;
    }

    void UpdateParticles(ParticleSystem ps, float pSpeed)
    {
        // Get list of particles from the particle system object
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.main.maxParticles];
        int pCount = ps.GetParticles(particles);

        // Change each particle's velocity relative to size
        for (int i = 0; i < pCount; i++)
        {
            particles[i].velocity = particles[i].velocity.normalized * pSpeed;
            particles[i].rotation += pSpeed / 2;
        }
        ps.SetParticles(particles);
        ParticleSystem.EmissionModule psEm = ps.emission;
        psEm.rateOverTime = pSpeed * 16;
    }

    private struct DualSpectrum
    {
        public const int LEFT = 0;
        public const int RIGHT = 1;

        public float[] LeftChannel;
        public float[] RightChannel;
        public readonly int Length;

        public DualSpectrum(int size)
        {
            LeftChannel = new float[size];
            RightChannel = new float[size];
            Length = size;
        }

        public DualSpectrum(int size, AudioSource source)
        {
            LeftChannel = new float[size];
            RightChannel = new float[size];
            Length = size;

            source.GetSpectrumData(LeftChannel, LEFT, FFTWindow.BlackmanHarris);

            if (source.clip.channels > 1)
                source.GetSpectrumData(RightChannel, RIGHT, FFTWindow.BlackmanHarris);
            else
                RightChannel = (float[])LeftChannel.Clone();
        }

        public DualSpectrum BandSection(int startBand, int endingBand, int samples)
        {
            int bands = endingBand - startBand;
            float samplef = (float)samples;
            DualSpectrum section = new DualSpectrum(samples);

            for (int i = startBand; i <= endingBand; i++)
            {
                int min = Mathf.FloorToInt(Mathf.Pow(2, i));

                for (int j = 0; j < samples; j++)
                {
                    int lower = min + (int)(min * (   j    / samplef));
                    int upper = min + (int)(min * ((j + 1) / samplef));

                    float left = 0;
                    float right = 0;

                    /*if (samplef > min)
                    {
                        upper = lower + 1;

                        float sin = Mathf.Sin(2f * Mathf.PI * j * min / samplef);
                        sin *= sin;
                        float cos = 1 - sin;

                        left = LeftChannel[lower] * sin + LeftChannel[upper] * cos;
                        right = RightChannel[lower] * sin + RightChannel[upper] * cos;
                    }
                    else
                    {
                        
                    }*/

                    for (int k = lower; k < upper; k++)
                    {
                        left = Mathf.Max(left, LeftChannel[k]);
                        right = Mathf.Max(right, RightChannel[k]);
                    }
                    
                    float mult = Mathf.Pow(2, i / 3f);
                    left *= mult;
                    right *= mult;
                    section.LeftChannel[j] = Mathf.Max(section.LeftChannel[j], left);
                    section.RightChannel[j] = Mathf.Max(section.RightChannel[j], right);
                }
            }

            return section;
        }

        public DualSpectrum LogSection(int min, int max, int size)
        {
            float sizef = (float)size;
            DualSpectrum section = new DualSpectrum(size);

            for (int i = 0; i < size; i++)
            {
                int diff = max - min;
                int lower = min + Mathf.FloorToInt(diff * (Mathf.Pow(sizef,    i    / sizef) - 1) / (sizef - 1));
                int upper = min + Mathf.FloorToInt(diff * (Mathf.Pow(sizef, (i + 1) / sizef) - 1) / (sizef - 1)) - 1;

                float left = 0;
                float right = 0;

                for (int j = lower; j <= upper; j++)
                {
                    left = Mathf.Max(left, LeftChannel[j]);
                    right = Mathf.Max(right, RightChannel[j]);
                }
                //  * Mathf.Log(mid + Length, Length)
                section.LeftChannel[i] = left;
                section.RightChannel[i] = right;
            }

            return section;
        }

        public void Interpolate(DualSpectrum other, float factor)
        {
            if (Length != other.Length)
                return;

            for (int i = 0; i < Length; i++)
            {
                LeftChannel[i] += (other.LeftChannel[i] - LeftChannel[i]) / factor;
                RightChannel[i] += (other.RightChannel[i] - RightChannel[i]) / factor;
            }
        }

        public float LogSample(int channel, int sample)
        {
            return GetChannel(channel)[sample] * Mathf.Log(sample + 10, Length);
        }

        public float GetAverage()
        {
            float sum = 0f;

            for (int i = 0; i < Length; i++)
            {
                sum += LeftChannel[i] + RightChannel[i];
            }

            return sum / Length / 4f;
        }

        public float[] GetChannel(int channel)
        {
            return channel == LEFT ? LeftChannel : RightChannel;
        }
    }
}
