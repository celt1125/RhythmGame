import os
import json
import numpy as np
import sklearn
import librosa
from scipy import signal
import scipy.stats
import math
import random

import matplotlib.pyplot as plt
import librosa.display

#### parameters ####
n_fft = 1024
hop_length = int(librosa.time_to_samples(1./200, sr=44100))
lag = 2
n_mels = 138
fmin = 27.5
fmax = 16000.
max_size = 3
note_list = ['A', 'A♯', 'B', 'C', 'C♯', 'D', 'D♯', 'E', 'F', 'F♯', 'G', 'G♯']
#             0    2     4    6    8     10   12    14   16   18    20   22

# 把dict格式優化，以便寫入JSON檔
def Dict2Json(o, level=0):
    INDENT = 3
    SPACE = " "
    NEWLINE = "\n"
    ret = ""
    if isinstance(o, dict):
        ret += "{" + NEWLINE
        comma = ""
        for k,v in o.items():
            ret += comma
            comma = ",\n"
            ret += SPACE * INDENT * (level+1)
            ret += '"' + str(k) + '":' + SPACE
            ret += Dict2Json(v, level + 1)

        ret += NEWLINE + SPACE * INDENT * level + "}"
    elif isinstance(o, str):
        ret += '"' + o + '"'
    elif isinstance(o, list):
        ret += "[" + ",".join([Dict2Json(e, level+1) for e in o]) + "]"
    elif isinstance(o, bool):
        ret += "true" if o else "false"
    elif isinstance(o, int):
        ret += str(o)
    elif isinstance(o, float):
        ret += '%.7g' % o
    elif isinstance(o, np.ndarray) and np.issubdtype(o.dtype, np.integer):
        ret += "[" + ','.join(map(str, o.flatten().tolist())) + "]"
    elif isinstance(o, np.ndarray) and np.issubdtype(o.dtype, np.inexact):
        ret += "[" + ','.join(map(lambda x: '%.7g' % x, o.flatten().tolist())) + "]"
    elif o is None:
        ret += 'null'
    else:
        raise TypeError("Unknown type '%s' for json serialization" % str(type(o)))
    
    return ret

# 讀取JSON檔
def ReadJSON(path):
    with open(path, 'r') as reader:
        jf = json.loads(reader)
    return jf

def ReadJSONArray(path):
    text = open(path, 'r')
    x = text.read()
    x = x.split(',')
    x[0] = x[0].replace('[', '')
    x[-1] = x[-1].replace(']', '')
    arr = []
    for e in x:
        arr.append(float(e))
    return arr

# 寫入JSON檔
def WriteJSON(JSON_path, data):
    ret = Dict2Json(data, level=0)
    with open(JSON_path, 'w') as writer:
        writer.write(ret)

def Mean(arr):
    sum_ = 0
    n = 0
    for num in arr:
        if (not math.isnan(num)):
            sum_ += num
            n += 1
    if (n == 0):
        return 110
    else:
        return sum_ / n

# 對人聲分離的音檔用librosa偵測onset
def GenerateSong(song_name, song_path, read):
    if(not os.path.isfile(song_path)):
        print("No such file.")
        return None
    y, sr = librosa.load(song_path, 44100)
    
    # normalization
    sos = signal.butter(25, 100, btype="highpass", fs=sr, output="sos")
    wav_data = signal.sosfilt(sos, y)
    wav_data = librosa.util.normalize(wav_data)
    
    # superflux onset
    S = librosa.feature.melspectrogram(y=wav_data, sr=sr, n_fft=n_fft,
                                       hop_length=hop_length,
                                       fmin=fmin,
                                       fmax=fmax,
                                       n_mels=n_mels)
    onset_sf, tempo = GetOnset(S, sr);
    
    # frequency estimate
    if (read):
        f0 = ReadJSONArray(song_name[:-5] + " f0.json")
    else:
        print("get frequency")
        f0, voiced_flag, voiced_probs = librosa.pitch.pyin(wav_data,
                                               fmin=librosa.note_to_hz('C2'),
                                               fmax=librosa.note_to_hz('C7'),
                                               sr=sr,
                                               frame_length=n_fft,
                                               hop_length=hop_length)
        #WriteJSON(song_name[:-5] + " f0.json", f0)
    frequency = [librosa.hz_to_note(Mean(f0[k:k+8]), octave=False) for k in onset_sf]
    #print(frequency)
    
    # amplitude
    amplitude = GetAmplitude(y)
    amplitude = [Mean(amplitude[0][k:k+5]) for k in onset_sf]
    #print(amplitude)
    
    onset_sf = librosa.frames_to_time(onset_sf,
                                      sr=sr,
                                      hop_length=hop_length)
    notes_tmp = GetNotes(onset_sf, frequency, amplitude, round(tempo))
    return notes_tmp, round(tempo)

def GetOnset(S, sr):
    odf_sf = librosa.onset.onset_strength(S=librosa.power_to_db(S, ref=np.max),
                                          sr=sr,
                                          hop_length=hop_length,
                                          lag=lag, max_size=max_size)
    onset_sf = librosa.onset.onset_detect(onset_envelope=odf_sf,
                                          sr=sr,
                                          hop_length=hop_length,
                                          units='frames')
    onset_time = librosa.frames_to_time(onset_sf,
                                        sr=sr,
                                        hop_length=hop_length)
    tempo_from_onset = 15 / min([onset_time[i+1]-onset_time[i] for i in range(len(onset_time) - 1)])
    
    # tempo detect and correction.
    tempo = []
    tempo.append(librosa.beat.tempo(onset_envelope=odf_sf,
                                    sr=sr,
                                    hop_length=hop_length)[0])
    tempo.append(tempo[0] / 2)
    tempo.append(tempo[0] * 2)
    res = tempo[np.argmin([abs(tempo[i] - tempo_from_onset) for i in range(3)])]
    
    # backtracking onset
    #onset_bt = librosa.onset.onset_backtrack(onset_sf, odf_sf)
    
    #PlotOnset(odf_sf, onset_sf, S)
    
    return onset_sf, res

def GetFrequency(S):
    #sheet = librosa.core.mel_frequencies(fmin=fmin, fmax=fmax, n_mels=n_mels)
    sheet = librosa.core.fft_frequencies(sr=44100, n_fft=n_fft)
    freq = []
    for i in range(len(S[0])):
        freq.append(sheet[np.argmax([S[k][i] for k in range(len(S))])])
    return freq

def GetAmplitude(y):
    #S2 = np.abs(librosa.stft(y=y, n_fft=n_fft, hop_length=hop_length))
    rms = librosa.feature.rms(y=y, frame_length=n_fft, hop_length=hop_length)
    
    '''
    fig, ax = plt.subplots(nrows=1)
    frame_time = librosa.frames_to_time(np.arange(len(rms[0])),
                                        sr=44100,
                                        hop_length=hop_length)
    ax.set(xlim=[0, 5.0])
    ax.plot(frame_time, rms[0], label='RMS')
    '''
    return rms

def GetNotes(onset, freq, amp, tempo):
    tempo = 90
    difficulty = 0
    
    delta = 15 / tempo
    time = onset[0]
    notes = []
    note_position = 0
    for i in range(len(onset)):
        while (abs(onset[i] - time) > delta*0.5):
            time += delta
            note_position += 1
        if (notes):
            if (abs(notes[-1][1] - time) > delta * 0.1):
                if (abs(onset[i] - time) < delta * 0.25):
                    notes.append([amp[i], time, freq[i], note_position])
                    if (note_position % (4 * notes_in_bar) == 0):
                        notes.append([-1, time, freq[i], note_position])
        else:
            notes.append([-1, time, freq[i], note_position])
            notes.append([amp[i], time, freq[i], note_position])
    
    notes_dict = []
    
    i = 0
    t = onset[0]
    has_aux = False
    accumulated_notes = 0
    while (t < notes[-1][1] + delta):
        note_type = str()
        position = 0
        is_end = False
        if (i >= len(notes)):
            break
        if (abs(notes[i][1] - t) < delta * 0.25):
            if (i > 0):
                if (notes[i][3] % (8 * notes_in_bar) < notes[i - 1][3] % (8 * notes_in_bar)):
                    is_end = True
                    notes_dict[-1]["clear"] = True
                    accumulated_notes = 0
                else:
                    if (notes[i][3] % (4 * notes_in_bar) < notes[i - 1][3] % (4 * notes_in_bar)):
                        if (accumulated_notes >= (2 * notes_in_bar)):
                            is_end = True
                            notes_dict[-1]["clear"] = True
                            accumulated_notes = 0
            
            if (notes[i][0] == -1):
                note_type = "osu"
            else:
                note_type = "main"
            
            if (note_type == "osu"):
                position = 22 - note_list.index(notes[i][2]) * 2
            else:
                tmp = len(notes_dict) - 1
                position = note_list.index(notes[i][2]) * 2
                while (tmp >= 0):
                    if (notes_dict[tmp]["note_type"] != "osu"):
                        if (abs(notes_dict[tmp]["timing"] - notes[i][1]) < delta * 1.1):
                            difficulty += 1
                            other_pos = notes_dict[tmp]["position"]
                            if (other_pos < position):
                                if (position - other_pos > 4):
                                    difficulty += 1
                                    position = other_pos + 4
                            else:
                                if (other_pos - position > 4):
                                    difficulty += 1
                                    position = other_pos - 4
                        elif (abs(notes_dict[tmp]["timing"] - notes[i][1]) < delta * 2.2):
                            other_pos = notes_dict[tmp]["position"]
                            if (other_pos < position):
                                if (position - other_pos > 8):
                                    difficulty += 1
                                    position = other_pos + 8
                            else:
                                if (other_pos - position > 8):
                                    difficulty += 1
                                    position = other_pos - 8
                            
                        break
                    else:
                        tmp -= 1
                
                accumulated_notes += 1
            
        else:
            t += delta
            continue
        
        
        if (not has_aux):
            if (not is_end and i > 0):
                if (notes[i][1] - notes[i - 1][1] > delta * 5.6):
                    begin_position = notes_dict[-1]["position"]
                    num_time = round((notes[i][1] - notes[i - 1][1]) / delta)
                    num_pos = abs(position - begin_position)
                    if (num_time > num_pos):
                        if (num_pos != 0):
                            auxiliary_time = notes_dict[-1]["timing"]
                            for k in range(1, num_time):
                                auxiliary_time += delta
                                if (k % 4 == 3):
                                    notes_dict.append({"note_type": "ornament",
                                                       "timing": auxiliary_time,
                                                       "position": begin_position + int((position-begin_position) * k/num_time),
                                                       "clear": False})
                                else:
                                    notes_dict.append({"note_type": "auxiliary",
                                                       "timing": auxiliary_time,
                                                       "position": begin_position + int((position-begin_position) * k/num_time),
                                                       "clear": False})
                        else:
                            auxiliary_time = notes_dict[-1]["timing"]
                            direction = random.randint(-1,1)
                            for k in range(num_time - 1):
                                auxiliary_time += delta
                                if (k % 4 == 3):
                                    notes_dict.append({"note_type": "ornament",
                                                       "timing": auxiliary_time,
                                                       "position": begin_position + direction,
                                                       "clear": False})
                                else:
                                    notes_dict.append({"note_type": "auxiliary",
                                                       "timing": auxiliary_time,
                                                       "position": begin_position + direction,
                                                       "clear": False})
                    else:
                        auxiliary_delta = (notes[i][1] - notes[i - 1][1]) / (num_pos - 1)
                        auxiliary_time = notes_dict[-1]["timing"]
                        if (begin_position < position):
                            k = 0
                            for auxiliary_position in range(begin_position + 1, position):
                                auxiliary_time += auxiliary_delta
                                if (k % 4 == 3):
                                    notes_dict.append({"note_type": "ornament",
                                                       "timing": auxiliary_time,
                                                       "position": auxiliary_position,
                                                       "clear": False})
                                else:
                                    notes_dict.append({"note_type": "auxiliary",
                                                       "timing": auxiliary_time,
                                                       "position": auxiliary_position,
                                                       "clear": False})
                                k += 1
                        else:
                            k = 0
                            for auxiliary_position in range(begin_position - 1, position, -1):
                                auxiliary_time += auxiliary_delta
                                if (k % 4 == 3):
                                    notes_dict.append({"note_type": "ornament",
                                                       "timing": auxiliary_time,
                                                       "position": auxiliary_position,
                                                       "clear": False})
                                else:
                                    notes_dict.append({"note_type": "auxiliary",
                                                       "timing": auxiliary_time,
                                                       "position": auxiliary_position,
                                                       "clear": False})
                                k += 1
                    has_aux = True
        else:
            has_aux = False
        
        notes_dict.append({"note_type": note_type,
                           "timing": notes[i][1],
                           "position": position,
                           "clear": False})
        i += 1
    
    notes_dict[-1]["clear"] = True
    print("difficulty:", difficulty)
    return notes_dict

def PlotOnset(odf_sf, onset_sf, S):
    sr = 44100
    
    fig, ax = plt.subplots(nrows=2, sharex=True)
    frame_time = librosa.frames_to_time(np.arange(len(odf_sf)),
                                        sr=sr,
                                        hop_length=hop_length)
    
    librosa.display.specshow(librosa.power_to_db(S, ref=np.max),
                             y_axis='mel', x_axis='time', sr=sr,
                             hop_length=hop_length, fmin=fmin, fmax=fmax, ax=ax[1])
    #ax[1].set(xlim=[0, 10.0])
    
    ax[0].plot(frame_time, odf_sf, label='Spectral flux')
    ax[0].vlines(librosa.frames_to_time(onset_sf, sr=sr, hop_length=hop_length), 0, odf_sf.max(), label='Onsets')
    ax[0].legend()
    ax[0].label_outer()

def main(song_name, read):
    # paths
    program_path = os.path.dirname(__file__)
    song_path = os.path.join(program_path, song_name + ".wav")
    JSON_path = os.path.join(program_path, song_name + '.json')
    
    # generate song
    notes, tempo = GenerateSong(song_name + ".json", song_path, read)
    song = {"name": song_name,
            "speed": -0.01 * tempo + 2.8,
            "difficulty": 0,
            "BPM": tempo,
            "notes": notes}
    print(tempo)
    # write in json
    WriteJSON(JSON_path, song)

def test():
    x = ReadJSONArray("fff.json")
    y = Mean(x)
    print(y)

if __name__ == "__main__":
    notes_in_bar = 3
    song_list = ["Canon in D", "Carol of the Bells", "Gravity Falls Opening",
                 "Greensleeves", "Jojo's Bizarre Adventure Giorno's Theme",
                 "Little Star", "Minuet in G Major Bach", "Passacaglia",
                 "Pirates of the Caribbean He's a Pirate", "Summer",
                 "Theme from Schindler's List", "Wellerman"]
    bar_list = [4, 3, 4, 3, 4, 4, 3, 4, 3, 4, 4, 4]
    notes_in_bar = 4
    main("Summer", False)
    for i in range(len(song_list)):
        print(song_list[i])
        notes_in_bar = bar_list[i]
        #main(song_list[i], False)
    #test()
    
