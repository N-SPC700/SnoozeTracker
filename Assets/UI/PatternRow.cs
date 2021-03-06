﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PatternRow : MonoBehaviour {
    public static int numDataEntries;

    public int line { get { return transform.GetSiblingIndex ( ); } }
    public int channel;
    public PatternView view;
    public Color selectColor;
    public Color normalColor;
    public Color effectColor;
    public Color validInstrument;
    public Color invalidInstrument;
    public Gradient transposeGradient;
    public Gradient volumeGradient;

    [System.NonSerialized]
    public Button[] dataEntries;

    private int m_SelectedEntry;
    private bool m_Selected;

    void Awake() {
        dataEntries = GetComponentsInChildren<Button> ( );
        numDataEntries = dataEntries.Length;
    }

    void Start() {
        for ( int i = 0 ; i < dataEntries.Length ; i++ ) {
            int index = i;
            dataEntries [ i ].onClick.AddListener ( () => {
                if(!view.playback.isPlaying)
                    view.SetSelection ( line, channel, index, false );
            } );
            dataEntries[i].GetComponent<BoxSelectable>().row = this;
        }
    }

    public BoxSelectable GetSelectable(int index) {
        return dataEntries [ index ].GetComponent<BoxSelectable> ( );
    }

    public void Select(int button) {
        m_Selected = true;
        m_SelectedEntry = button;
        dataEntries [ button ].image.color = selectColor;
    }

    public void Deselect() {
        m_Selected = false;
        UpdateHighlight ( m_SelectedEntry );
    }

    public void UpdateData() {
        var colEntry = view.data.GetPatternColumn ( view.data.currentPattern, channel );
        for ( int i = 0 ; i < dataEntries.Length ; i++ ) {
            UpdateHighlight ( i );
            Text label = dataEntries [ i ].GetComponentInChildren<Text> ( );
            if (colEntry == null ) {
                label.text = "-";
                label.color = Color.white;
                continue;
            }
            int val = colEntry.data [ line, i ];
            if ( val < 0 ) {
                label.text = "-";
                label.color = Color.white;
                continue;
            }

            string text = System.String.Empty;
            Color color = Color.white;
            switch ( i ) {
                case 0:
                    int off = view.data.GetTransposeOffset ( channel );
                    off = Mathf.Clamp ( ( off + 12 ) / 2, 0, 12 );
                    color = transposeGradient.Evaluate(off / 12f);
                    
                    text = VirtualKeyboard.FormatNote ( val );
                    break;
                case 1:
                    color = val < view.instruments.presets.Length ? validInstrument : invalidInstrument;
                    text = val.ToString ( "X2" );
                    break;
                case 2:
                    color = volumeGradient.Evaluate ( val / 15f );
                    text = val.ToString ( "X" );
                    break;
                case 3:
                case 4:
                    color = effectColor;
                    text = val.ToString ( "X2" );
                    break;
            }

            label.color = color;
            label.text = text;
        }
    }

    private void UpdateHighlight(int button) {
        if ( m_Selected )
            return;

        dataEntries [ button ].image.color = ( line % view.highlightInterval ) == 0 ? view.highlight : Color.white;
    }
}
