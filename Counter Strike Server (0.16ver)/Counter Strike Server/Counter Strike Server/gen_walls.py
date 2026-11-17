import re,sys
col=open(sys.argv[1]).read()
part=col[col.index('void LoadAllCollisions'):col.index('void LoadAllStairs')]
maps=['DUST2','TUTORIAL','DUST2_2x2','AIM_MAP','B2000','FYSNOW','MIRAGE']
pos=[(m.start(),m.group(1)) for m in re.finditer(r'mapToLoad == (\w+)\)',part)]
out=['''// SPDX-License-Identifier: MIT
//
// This file is part of the server of Counter Strike Nintendo DS Multiplayer Edition (CS:DS)
//
// GENERATED from the game sources (collisions/collisions.c, LoadAllCollisions)
// by gen_walls.py. Do not edit by hand.
// Each wall: center x, y, z, size x, y, z (world units, like the game).

namespace Counter_Strike_Server
{
    public static partial class BotWalls
    {
        private static void LoadGenerated()
        {''']
for i,(p,name) in enumerate(pos):
    end=pos[i+1][0] if i+1<len(pos) else len(part)
    blk=part[p:end]
    walls=[]
    for line in blk.split('\n'):
        l=line.strip()
        if l.startswith('//'): continue
        m=re.match(r'CreateWall\(([-\d.]+),\s*([-\d.]+),\s*([-\d.]+),\s*([-\d.]+),\s*([-\d.]+),\s*([-\d.]+),',l)
        if m: walls.append(m.groups())
    cnt=re.search(r'CollisionsCount = (\d+)',blk)
    print(name,len(walls),cnt.group(1) if cnt else '?',file=sys.stderr)
    out.append(f'            walls[(int)mapEnum.{name}] = new float[,]')
    out.append('            {')
    for w in walls:
        out.append('                { '+', '.join(v+'f' for v in w)+' },')
    out.append('            };')
out.append('        }\n    }\n}\n')
open(sys.argv[2],'w').write('\n'.join(out))
