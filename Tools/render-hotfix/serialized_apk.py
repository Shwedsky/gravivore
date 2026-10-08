"""Minimal read-only Unity 6 player SerializedFile reader (format 22, no type tree).

Inspect actual typed objects rather than treating arbitrary strings as assets.
APK split asset chunks are reassembled in numeric order before metadata parsing.
"""
import struct
import zipfile

class Reader:
    def __init__(self, data, pos=0): self.data, self.pos = data, pos
    def unpack(self, fmt):
        size=struct.calcsize(fmt); result=struct.unpack_from(fmt,self.data,self.pos); self.pos+=size
        return result[0] if len(result)==1 else result
    def cstring(self):
        end=self.data.index(0,self.pos); value=self.data[self.pos:end].decode('utf8'); self.pos=end+1; return value
    def string(self):
        size=self.unpack('<i')
        if not 0<=size<=len(self.data)-self.pos: raise ValueError('Invalid serialized string')
        value=self.data[self.pos:self.pos+size].decode('utf8'); self.pos=(self.pos+size+3)&~3; return value
    def align(self): self.pos=(self.pos+3)&~3

def objects(data):
    if len(data)<48 or struct.unpack_from('>I',data,8)[0]!=22: return []
    metadata_size,file_size,data_offset=struct.unpack_from('>IQQ',data,20)
    if file_size!=len(data): raise ValueError('SerializedFile size mismatch')
    r=Reader(data,48); unity=r.cstring(); platform=r.unpack('<i'); tree=r.unpack('<?')
    if tree: raise ValueError('Player inspector expects stripped type trees')
    types=[]
    for _ in range(r.unpack('<i')):
        class_id=r.unpack('<i'); stripped=r.unpack('<?'); script_index=r.unpack('<h')
        if class_id==114: r.pos+=16
        r.pos+=16
        types.append(class_id)
    result=[]
    for _ in range(r.unpack('<i')):
        r.align(); path_id,start,size,type_index=r.unpack('<qqIi')
        result.append({'pathId':path_id,'classId':types[type_index],'data':data[data_offset+start:data_offset+start+size]})
    # Script references precede external file references.
    for _ in range(r.unpack('<i')): r.unpack('<i');r.align();r.unpack('<q')
    externals=[]
    for _ in range(r.unpack('<i')):
        r.cstring();r.pos+=16;r.unpack('<i');externals.append(r.cstring())
    for obj in result: obj['externals']=externals
    return result

def apk_files(path):
    with zipfile.ZipFile(path) as archive:
        entries={e.filename:e for e in archive.infolist() if e.filename.startswith('assets/bin/Data/') and '/Managed/' not in e.filename}
        for name in entries:
            if '.split' in name or name.endswith('.resS'):continue
            yield name,archive.read(name)
        bases={name.rsplit('.split',1)[0] for name in entries if '.split' in name and '.resS' not in name}
        for base in sorted(bases):
            chunks=sorted((n for n in entries if n.startswith(base+'.split')),key=lambda n:int(n.rsplit('.split',1)[1]))
            yield base,b''.join(archive.read(n) for n in chunks)

def named_assets(path, class_ids=(21,48,114)):
    for entry,data in apk_files(path):
        for obj in objects(data):
            if obj['classId'] not in class_ids:continue
            try: name=Reader(obj['data']).string()
            except (ValueError,UnicodeError,struct.error):continue
            yield entry,name,obj

def shader_assets(path,names):
    for entry,data in apk_files(path):
        for obj in objects(data):
            if obj['classId']!=48:continue
            for name in names:
                encoded=name.encode('utf8');needle=struct.pack('<i',len(encoded))+encoded
                if needle in obj['data']:
                    yield {'entry':entry,'name':name,'pathId':obj['pathId'],'bytes':len(obj['data'])}

if __name__=='__main__':
    import argparse,json
    parser=argparse.ArgumentParser();parser.add_argument('apk');parser.add_argument('--shader',action='append',default=[])
    args=parser.parse_args(); found=[]
    found=list(shader_assets(args.apk,args.shader))
    print(json.dumps(found,indent=2))
