using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.Runtime.Services
{
    // Original soft bell composition and effects. No external audio or runtime downloads.
    public sealed class GameAudio : MonoBehaviour
    {
        private static GameAudio instance;
        private AudioSource music;
        private readonly List<AudioSource> voices = new List<AudioSource>();
        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private readonly Dictionary<string,double> lastPlayed=new Dictionary<string,double>();
        private double outcomeUntil, duckUntil;
        public const int VoiceLimit=3;
        public int ActiveVoices { get {int count=0;foreach(var voice in voices)if(voice.isPlaying)count++;return count;} }
        public int SuppressedCues { get; private set; }
        public string LastCue { get; private set; }
        public AudioClip MusicClip=>music.clip;
        public AudioClip EffectClip(string cue)=>clips[cue];
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => instance = null;
        public static GameAudio Ensure()
        {
            if (!instance)
            {
                instance = new GameObject("GameAudio").AddComponent<GameAudio>();
                DontDestroyOnLoad(instance.gameObject);
                instance.Initialize();
            }
            return instance;
        }
        private void Initialize()
        {
            music = gameObject.AddComponent<AudioSource>(); music.loop = true;
            music.clip = Compose(); music.playOnAwake = false;
            for (int i=0;i<VoiceLimit;i++) { var voice=gameObject.AddComponent<AudioSource>(); voice.playOnAwake=false; voices.Add(voice); }
            clips["swap"] = Tone("Swap", new[] { 72, 76 }, .07f);
            clips["invalid"] = Tone("Return", new[] { 60, 55 }, .10f);
            clips["match"] = Tone("Match", new[] { 76, 79 }, .10f);
            clips["cascade"] = Tone("Cascade", new[] { 79, 84, 88 }, .09f);
            clips["special"] = Tone("Special", new[] { 72, 79, 84, 88 }, .08f);
            clips["win"] = Tone("Clear", new[] { 72, 76, 79, 84 }, .18f);
            clips["lose"] = Tone("Retry", new[] { 67, 64, 60 }, .20f);
            clips["complete"] = Tone("Kingdom", new[] { 72, 76, 79, 84, 88, 91, 96 }, .20f);
            Apply(); GamePreferences.Changed += Apply; music.Play();
        }
        private void Apply()
        {
            var p=GamePreferences.Current;
            music.volume=p.musicMuted ? 0 : p.musicVolume*(AudioSettings.dspTime<duckUntil?.45f:1);
            foreach (var voice in voices) voice.volume=p.effectsMuted ? 0 : p.effectsVolume;
        }
        private void Update()
        {
            if(!music)return;
            var p=GamePreferences.Current;
            float target=p.musicMuted?0:p.musicVolume*(AudioSettings.dspTime<duckUntil?.45f:1);
            music.volume=Mathf.MoveTowards(music.volume,target,Time.unscaledDeltaTime*2);
        }
        public static void Play(string cue)
        {
            var audio=Ensure();
            if (GamePreferences.Current.effectsMuted || !audio.clips.TryGetValue(cue, out var clip)) return;
            double now=AudioSettings.dspTime;
            bool outcome=cue=="win" || cue=="lose" || cue=="complete";
            if(audio.lastPlayed.TryGetValue(cue,out var last) && now-last<(outcome?.5:.12) || !outcome && now<audio.outcomeUntil)
            {audio.SuppressedCues++;return;}
            if(outcome)
            {
                foreach(var source in audio.voices)source.Stop();
                audio.outcomeUntil=now+clip.length;audio.duckUntil=audio.outcomeUntil;
            }
            else if(cue=="special")audio.duckUntil=now+.7;
            AudioSource available=null;
            foreach(var source in audio.voices)if(!source.isPlaying){available=source;break;}
            if(!available && cue=="special")available=audio.voices[0];
            if(!available){audio.SuppressedCues++;return;}
            audio.lastPlayed[cue]=now;audio.LastCue=cue;available.clip=clip;available.Play();
        }
        private static AudioClip Tone(string name, int[] notes, float beat)
        {
            const int rate=22050;
            var samples=new float[(int)((notes.Length*beat+.32f)*rate)];
            for(int i=0;i<notes.Length;i++) AddNote(samples,rate,i*beat,beat+.30f,notes[i],.22f);
            LimitPeak(samples,.23f);
            var clip=AudioClip.Create(name,samples.Length,1,rate,false); clip.SetData(samples,0); return clip;
        }
        private static AudioClip Compose()
        {
            const int rate=22050; const float beat=.375f; const int steps=384;
            var samples=new float[(int)(steps*beat*rate)];
            int[][] chords={new[]{60,64,67,72},new[]{57,60,64,69},new[]{53,57,60,65},new[]{55,59,62,67}};
            int[][] melodies={new[]{76,79,81,79,76,74,72,74,76,79,84,81,79,76,74,72},
                new[]{72,76,79,0,81,79,76,0,77,81,84,81,79,74,71,0},
                new[]{84,0,79,76,81,0,76,72,77,0,81,84,79,0,74,71}};
            for(int i=0;i<steps;i++)
            {
                int section=i/128;var chord=chords[((i/8)+(section==1?2:0))%4];
                if(section!=2 || i%2==0)AddNote(samples,rate,i*beat,1.3f,chord[(i+(section==1?1:0))%4],.055f,true);
                int note=melodies[section][(i/2)%16];
                if(i%2==0 && note>0 && i%32<28)AddNote(samples,rate,i*beat,.9f,note,.075f,true);
                if(i%8==0) AddNote(samples,rate,i*beat,2.5f,chord[0]-12,.065f,true);
            }
            LimitPeak(samples,.14f);
            var clip=AudioClip.Create("Sugar Garden - Bell Walk",samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        private static void LimitPeak(float[] samples,float ceiling)
        {float peak=0;foreach(float sample in samples)peak=Mathf.Max(peak,Mathf.Abs(sample));if(peak<=ceiling)return;float gain=ceiling/peak;for(int i=0;i<samples.Length;i++)samples[i]*=gain;}
        private static void AddNote(float[] samples,int rate,float start,float length,int midi,float gain,bool loop=false)
        {
            double frequency=440*Math.Pow(2,(midi-69)/12.0);
            int begin=(int)(start*rate), count=(int)(length*rate);
            for(int j=0;j<count;j++)
            {
                int index=begin+j; if(index>=samples.Length) { if(!loop)break; index%=samples.Length; }
                double t=(double)j/rate, envelope=Math.Min(1,t/.012)*Math.Exp(-5*t/length)*Math.Min(1,(length-t)/.035);
                double phase=2*Math.PI*frequency*t;
                samples[index]+=(float)(gain*envelope*(Math.Sin(phase)+.25*Math.Sin(phase*2)+.08*Math.Sin(phase*3)));
            }
        }
        private void OnDestroy()
        {
            GamePreferences.Changed-=Apply;
            foreach(var clip in clips.Values) Destroy(clip);
            if(music && music.clip) Destroy(music.clip);
            if(instance==this)instance=null;
        }
    }
}
