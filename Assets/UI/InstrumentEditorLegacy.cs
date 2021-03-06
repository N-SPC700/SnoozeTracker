﻿using UnityEngine;
using System.Collections;

public class InstrumentEditorLegacy : MonoBehaviour {
    public Instruments.InstrumentInstance instrument { get { return instruments.presets [ keyboard.currentInstrument ]; } }

    public string volumeEnvelope {
        get {
            return m_VolumeEnvelope;
        }

        set {
            if ( m_VolumeEnvelope == value )
                return;

            Instruments.InstrumentInstance preset = instrument;
            m_VolumeEnvelope = value;
            StringToArray ( m_VolumeEnvelope, ref preset.volumeTable );
            instruments.presets [ keyboard.currentInstrument ] = preset;

            FileManagement.fileModified = true;
        }
    }

    public string arpEnvelope {
        get { return m_Arpeggio; }
        set {
            if ( m_Arpeggio == value )
                return;
            Instruments.InstrumentInstance preset = instrument;
            m_Arpeggio = value;
            StringToArray ( m_Arpeggio, ref preset.arpeggio );
            instruments.presets [ keyboard.currentInstrument ] = preset;

            FileManagement.fileModified = true;
        }
    }

    enum EditorScreen { Volume, Wave, Pitch }

    public FileManagement fileMan;
    public Instruments instruments;
    public VirtualKeyboard keyboard;
    public Vector2 padding;
    public Vector2 size;
    public float volWidth;
    public GUISkin skin;

    private string m_VolumeEnvelope;
    private string m_Arpeggio;
    private bool m_HideFields = false;
    private EditorScreen m_CurrentScreen;
    private Texture2D m_ColorTexture;

    void Start() {
        UpdateAttributes ( );

        m_ColorTexture = new Texture2D ( 1, 1 );
        m_ColorTexture.SetPixel ( 0, 0, Color.green );
        m_ColorTexture.Apply ( );
    }

    void OnGUI() {

        GUI.skin = skin;

        if ( Event.current.keyCode == KeyCode.Tab ) {
            if ( Event.current.type == EventType.KeyDown )
                m_HideFields = true;
            else
                m_HideFields = false;
        }
        Rect rect = new Rect ( new Vector2 ( padding.x, padding.y ), size );

        GUILayout.BeginArea ( rect );

        GUILayout.BeginHorizontal ( );

        GUILayout.BeginVertical (GUILayout.Width(size.x - volWidth - 32));
        GUILayout.BeginHorizontal ( );
        GUILayout.Box ( "Ins " + keyboard.currentInstrument.ToString("X2") );
        if ( GUILayout.Button ( "<" ) )
            IncInstrument ( -1 );
        if ( GUILayout.Button ( ">" ) )
            IncInstrument ( 1 );
        GUILayout.EndHorizontal ( );
        GUILayout.EndVertical ( );
        GUILayout.Space ( 16 );
        GUILayout.BeginVertical ( GUILayout.Width ( volWidth ) );

        GUILayout.BeginHorizontal ( );
        if ( GUILayout.Button ( "Volume" ) )
            m_CurrentScreen = EditorScreen.Volume;
        if ( GUILayout.Button ( "Note" ) )
            m_CurrentScreen = EditorScreen.Pitch;
        if ( GUILayout.Button ( "Wave" ) )
            m_CurrentScreen = EditorScreen.Wave;
        GUILayout.EndHorizontal ( );

        switch ( m_CurrentScreen ) {
            case EditorScreen.Volume:
                ArraySlider ( instrument.volumeTable, 0, 0xF );

                GUILayout.FlexibleSpace ( );
                GUILayout.BeginHorizontal ( );
                GUILayout.Box ( instrument.volumeTable.Length.ToString ( "X2" ) );
                if ( GUILayout.Button ( "-" ) )
                    ChangeVolTableSize ( -1 );
                if ( GUILayout.Button ( "+" ) )
                    ChangeVolTableSize ( 1 );
                GUILayout.EndHorizontal ( );
                break;
            case EditorScreen.Pitch:
                GUILayout.Box ( "Arpeggio" );
                arpEnvelope = TabSafeTextField ( arpEnvelope );
                break;
            case EditorScreen.Wave:
                bool samp = instruments.presets [ keyboard.currentInstrument ].samplePlayback;
                samp = GUILayout.Toggle ( samp, "Custom waves" );

                if ( samp != instruments.presets [ keyboard.currentInstrument ].samplePlayback ) {
                    Instruments.InstrumentInstance ins = instruments.presets [ keyboard.currentInstrument ];
                    ins.samplePlayback = samp;
                    instruments.presets [ keyboard.currentInstrument ] = ins;
                }

                if ( samp ) {
                    GUILayout.BeginHorizontal ( );
                    WaveButton ( Instruments.InstrumentInstance.Wave.Pulse );
                    WaveButton ( Instruments.InstrumentInstance.Wave.Saw );
                    WaveButton ( Instruments.InstrumentInstance.Wave.Triangle );
                    WaveButton ( Instruments.InstrumentInstance.Wave.Sine );
                    WaveButton ( Instruments.InstrumentInstance.Wave.Sample );
                    GUILayout.EndHorizontal ( );

                    switch ( instrument.customWaveform ) {
                        case Instruments.InstrumentInstance.Wave.Pulse:
                            int pwmStart, pwmEnd, pwmSpeed;
                            pwmStart = instrument.pulseWidthMin;
                            pwmEnd = instrument.pulseWidthMax;
                            pwmSpeed = instrument.pulseWidthPanSpeed;

                            GUILayout.BeginHorizontal ( );
                            GUILayout.Box ( "PWM min", GUILayout.Width(96) );
                            pwmStart = (int)GUILayout.HorizontalSlider ( pwmStart, 0, 100 );
                            GUILayout.Box ( pwmStart.ToString(), GUILayout.Width ( 32 ) );
                            GUILayout.EndHorizontal ( );

                            GUILayout.BeginHorizontal ( );
                            GUILayout.Box ( "PWM max", GUILayout.Width ( 96 ) );
                            pwmEnd = ( int ) GUILayout.HorizontalSlider ( pwmEnd, 0, 100 );
                            GUILayout.Box ( pwmEnd.ToString ( ), GUILayout.Width ( 32 ) );
                            GUILayout.EndHorizontal ( );

                            GUILayout.BeginHorizontal ( );
                            GUILayout.Box ( "PWM spd", GUILayout.Width ( 96 ) );
                            pwmSpeed = ( int ) GUILayout.HorizontalSlider ( pwmSpeed, 0, Instruments.InstrumentInstance.PWMSPEED_MAX - 1 );
                            GUILayout.Box ( pwmSpeed.ToString ( ), GUILayout.Width ( 32 ) );
                            GUILayout.EndHorizontal ( );

                            if(pwmStart != instrument.pulseWidthMin ||
                                pwmEnd != instrument.pulseWidthMax ||
                                pwmSpeed != instrument.pulseWidthPanSpeed ) {
                                Instruments.InstrumentInstance ins = instrument;
                                ins.pulseWidthMin = pwmStart;
                                ins.pulseWidthMax = pwmEnd;
                                ins.pulseWidthPanSpeed = pwmSpeed;
                                instruments.presets [ keyboard.currentInstrument ] = ins;
                                FileManagement.fileModified = true;
                            }
                            break;

                        case Instruments.InstrumentInstance.Wave.Sample:
                            if ( GUILayout.Button ( "Load sample" ) ) {
                                Instruments.InstrumentInstance ins = instrument;
                                if(fileMan.LoadSample ( ref ins.waveTable, ref ins.waveTableSampleRate ) ) {
                                    instruments.presets [ keyboard.currentInstrument ] = ins;
                                    FileManagement.fileModified = true;
                                }
                            }

                            if ( instrument.waveTable != null && instrument.waveTable.Length > 0 ) {
                                bool loopSamp = instrument.loopSample;

                                GUILayout.BeginHorizontal ( );
                                GUILayout.Box ( instrument.waveTable.Length + " samples (" + instrument.waveTableSampleRate + "Hz)" );
                                loopSamp = GUILayout.Toggle ( loopSamp, "Loop" );
                                GUILayout.EndHorizontal ( );

                                GUILayout.BeginHorizontal ( );

                                int relNote = instrument.sampleRelNote;

                                if ( GUILayout.Button ( "--" ) )
                                    relNote -= 12;
                                if ( GUILayout.Button ( "-" ) )
                                    relNote--;

                                VirtualKeyboard.Note currNote = ( VirtualKeyboard.Note ) ( relNote % 12 + 1 );
                                GUILayout.Box ( currNote.ToString().Replace('s', '#') + ( relNote / 12 ).ToString() );

                                if ( GUILayout.Button ( "+" ) )
                                    relNote++;
                                if ( GUILayout.Button ( "++" ) )
                                    relNote += 12;

                                if(instrument.sampleRelNote != relNote || loopSamp ) {
                                    Instruments.InstrumentInstance ins = instrument;
                                    ins.sampleRelNote = relNote;
                                    ins.loopSample = loopSamp;

                                    instruments.presets[keyboard.currentInstrument] = ins;
                                    FileManagement.fileModified = true;
                                }
                                //GUILayout.Toggle()
                                GUILayout.EndHorizontal ( );
                            }

                            break;
                    }
                }
                break;
        }

        GUILayout.EndVertical ( );

        GUILayout.EndHorizontal ( );
        GUILayout.EndArea ( );
    }

    string TabSafeTextField(string value) {
        GUI.enabled = !m_HideFields;
        string res = GUILayout.TextField ( value );
        GUI.enabled = true;
        return res;
    }

    void ArraySlider(int[] array, int min, int max) {
        GUILayout.BeginHorizontal ( );
        Vector2 size = new Vector2 ( 1024, 100 );

        for ( int i = 0 ; i < array.Length ; i++ ) {
            Rect layout = GUILayoutUtility.GetRect ( Mathf.Min(16, size.x / array.Length), size.y );
            layout.width += 1;

            if(Input.GetMouseButton(0)) {
                Vector2 mPos = Event.current.mousePosition;
                if ( layout.Contains ( mPos ) ) {
                    int val = ( int ) ( ( ( layout.yMax - mPos.y ) / size.y ) * 16 );
                    array [ i ] = val;
                    FileManagement.fileModified = true;
                }
            }

            layout.height = size.y * ( array [ i ] / 16f );
            layout.y += size.y * ( 1 - ( array [ i ] / 16f ) );
            GUI.DrawTexture ( layout, m_ColorTexture );
        }

        GUILayout.EndHorizontal ( );
    }

    void WaveButton(Instruments.InstrumentInstance.Wave wave)
    {
        Instruments.InstrumentInstance ins = instruments.presets[keyboard.currentInstrument];
        bool sel = ins.customWaveform == wave;
        sel = GUILayout.Toggle(sel, wave.ToString());

        if(sel && ins.customWaveform != wave)
        {
            ins.customWaveform = wave;
            instruments.presets[keyboard.currentInstrument] = ins;
            FileManagement.fileModified = true;
        }
    }

    public void UpdateAttributes() {
        m_VolumeEnvelope = ArrayToString ( instrument.volumeTable );
        m_Arpeggio = ArrayToString ( instrument.arpeggio );
    }

    private void IncInstrument(int dir) {
        keyboard.currentInstrument += dir;
        if ( keyboard.currentInstrument < 0 )
            keyboard.currentInstrument = 0;
        if ( keyboard.currentInstrument >= instruments.presets.Length )
            instruments.CreateInstrument ( );

        UpdateAttributes ( );
    }

    private void ChangeVolTableSize(int inc) {
        if ( inc < 0 && instrument.volumeTable.Length <= 1 )
            return;

        Instruments.InstrumentInstance ins = instrument;
        System.Array.Resize ( ref ins.volumeTable, ins.volumeTable.Length + inc );

        if ( inc > 0 )
            ins.volumeTable [ ins.volumeTable.Length - 1 ] = ins.volumeTable [ ins.volumeTable.Length - 2 ];

        instruments.presets [ keyboard.currentInstrument ] = ins;

        FileManagement.fileModified = true;
    }

    private string ArrayToString(int[] array) {
        if ( array == null )
            return "";
        string res = "";
        for ( int i = 0 ; i < array.Length ; i++ ) {
            res += array[i].ToString ( "X" ) + " ";
        }

        return res;
    }

    private void StringToArray(string str, ref int[] array) {
        string [ ] data = str.Split ( new char[]{ ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries );
        array = new int [ data.Length ];
        for ( int i = 0 ; i < data.Length ; i++ ) {
            int val;
            if ( int.TryParse ( data [ i ], System.Globalization.NumberStyles.HexNumber, null, out val ) )
                array [ i ] = val;
        }
    }
}
