// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using UnityEngine;

namespace realvirtual
{
	//! Sound provides audio feedback for automation components.
	//! It can play sounds based on drive movement speed or triggered events (like picks).
	//! Supports various industrial sound types and automatically adjusts pitch based on drive speed.
	public class Sound : MonoBehaviour
	{

		public enum Soundmode {Drive,Pick}
		public enum Soundtype {Robot1,Robot2,SmallElectric1,SmallElectric2,AirPressure}
	
		[Header("Settings")]
		[Tooltip("Sound playback mode: Drive for speed-based continuous sound, Pick for one-shot events")]
		public Soundmode Mode = Soundmode.Drive; //!< Sound playback mode: Drive for continuous speed-based sound, Pick for one-shot events
		[Tooltip("Type of industrial sound to play")]
		public Soundtype SoundType; //!< Type of industrial sound to play (robot, electric motor, air pressure)
		[Tooltip("Maximum drive speed in millimeters per second for pitch calculation")]
		public float SpeedMax = 200; //!< Maximum drive speed in millimeters per second for pitch calculation
		[Tooltip("Minimum pitch value when drive is at low speed")]
		public float PitchMin = 0.9f; //!< Minimum pitch value when drive is at low speed
		[Tooltip("Maximum pitch value when drive is at maximum speed")]
		public float PitchMax = 1.1f; //!< Maximum pitch value when drive is at maximum speed

		[Header("Sound IOs")]
		[Tooltip("Start or stop audio playback")]
		public bool PlayAudio; //!< Controls audio playback (start/stop)
		[Tooltip("Audio volume level (0 to 1)")]
		public float Volume = 1; //!< Audio volume level (0 to 1)
		[Tooltip("Audio pitch multiplier (automatically adjusted in Drive mode)")]
		public float Pitch = 1; //!< Audio pitch multiplier (automatically adjusted in Drive mode)

		[Tooltip("Indicates if audio is currently playing")]
		public bool IsPlaying; //!< Indicates if audio is currently playing
	
		private bool _isplayingbefore = false;
		private AudioSource AudioSource;
		private AudioClip AudioClip;
		private Drive _drive;
		private Soundtype _soundtypebefore;


		void SetSound()
		{
			AudioSource = gameObject.GetComponent<AudioSource>();
			if (AudioSource == null)
			{
				AudioSource = gameObject.AddComponent<AudioSource>();
			}
			IsPlaying = false;
			switch (SoundType)
			{
				case Soundtype.Robot1:
					AudioClip = UnityEngine.Resources.Load("Sounds/robot1", typeof(AudioClip)) as AudioClip;
					break;
				case Soundtype.Robot2:
					AudioClip =  UnityEngine.Resources.Load("Sounds/robot2", typeof(AudioClip)) as AudioClip;
					break;
				case Soundtype.SmallElectric1:
					AudioClip =  UnityEngine.Resources.Load("Sounds/smallelectrical1", typeof(AudioClip)) as AudioClip;
					break;
				case Soundtype.SmallElectric2:
					AudioClip =  UnityEngine.Resources.Load("Sounds/smallelectrical2", typeof(AudioClip)) as AudioClip;
					break;
				case Soundtype.AirPressure:
					AudioClip =  UnityEngine.Resources.Load("Sounds/airpressure", typeof(AudioClip)) as AudioClip;
					break;
			}

			switch (Mode)
			{
				case Soundmode.Drive :
					AudioSource.loop = true;
					_drive = GetComponent<Drive>();
					break;
				case Soundmode.Pick :
					AudioSource.loop = false;
					break;
			}
			AudioSource.clip = AudioClip;	
		}

		// Use this for initialization
		void Start ()
		{
			SetSound();
		}

		// Update is called once per frame
		void Update ()
		{
			if (_soundtypebefore != SoundType)
			{
				SetSound();
				_soundtypebefore = SoundType;
			}
			switch (Mode)
			{
				case Soundmode.Drive :
					var speed = Math.Abs(_drive.CurrentSpeed);
					if (speed > SpeedMax)
					{
						Pitch = PitchMax;
					}
					else
					{
						Pitch = PitchMin + (PitchMax - PitchMin) * speed/ SpeedMax;
					}
					if (speed > 0)
					{
						PlayAudio = true;
					}
					else
					{
						PlayAudio = false;
					}
					break;
				case Soundmode.Pick :
					break;
			}
		
			AudioSource.volume = Volume;
			AudioSource.pitch = Pitch;

		
			if (PlayAudio == true && _isplayingbefore == false)
			{
				AudioSource.Play();
				IsPlaying = true;
				_isplayingbefore = true;
			}

			if (PlayAudio == false)
			{
				IsPlaying = false;
				_isplayingbefore = false;
				AudioSource.Stop();
			}
		}
	}
}
