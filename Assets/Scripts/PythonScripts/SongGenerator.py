import json

def dict_to_json(o, level=0):
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
            ret += dict_to_json(v, level + 1)

        ret += NEWLINE + SPACE * INDENT * level + "}"
    elif isinstance(o, str):
        ret += '"' + o + '"'
    elif isinstance(o, list):
        ret += "[" + ",".join([dict_to_json(e, level+1) for e in o]) + "]"
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

def readJSON(path):
    with open(path, 'r', encoding='utf-8') as reader:
        jf = json.load(reader)
    return jf

def writeJSON(JSON_path, data):
    ret = dict_to_json(data, level=0)
    with open(JSON_path, 'w', encoding='utf-8') as writer:
        writer.write(ret)
    return None


if __name__ == "__main__":
    song = {}
    song["name"] = "LittleStar"
    song["speed"] = 3.0
    song["score"] = 0
    
    notes = []
    note = {}
    note["note_type"] = "big"
    note["timing"] = 1.0
    note["position"] = 4
    notes.append(note)
    notes.append({"note_type":"small","timing":1.4,"position":8})
    note = {}
    note["note_type"] = "osu"
    note["timing"] = 1.8
    note["position"] = 12
    notes.append(note)
    
    song["notes"] = notes
    
    writeJSON("test.json", song)