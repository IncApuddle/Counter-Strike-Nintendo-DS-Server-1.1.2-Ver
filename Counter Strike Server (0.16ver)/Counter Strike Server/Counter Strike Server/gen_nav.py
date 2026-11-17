import re,sys
src=open('/home/claude/port/cs/source/ai/data/ai_data.c').read()
col=open('/home/claude/port/cs/source/collisions/collisions.c').read()
maps=['DUST2','TUTORIAL','DUST2_2x2','AIM_MAP','B2000','FYSNOW','MIRAGE']
wp_part=src[src.index('void CreateWaypoints'):src.index('void createLengthMatrices')]
# split per map
blocks={}
pos=[(m.start(),m.group(1)) for m in re.finditer(r'mapToLoad == (\w+)\)',wp_part)]
for i,(p,name) in enumerate(pos):
    end=pos[i+1][0] if i+1<len(pos) else len(wp_part)
    blocks[name]=wp_part[p:end]
out=[]
out.append('''// SPDX-License-Identifier: MIT
//
// This file is part of the server of Counter Strike Nintendo DS Multiplayer Edition (CS:DS)
//
// GENERATED from the game sources (ai/data/ai_data.c and collisions/collisions.c)
// by gen_nav.py. Do not edit by hand.

namespace Counter_Strike_Server
{
    public static partial class BotNavData
    {
        private static void LoadGenerated()
        {''')
colpart=col[col.index('BombsTriggersCollisionsCount'):]
colpos=[(m.start(),m.group(1)) for m in re.finditer(r'mapToLoad == (\w+)\)',col)]
for name in maps:
    b=blocks[name]
    edges={}
    for m in re.finditer(r'int edge(\d+)\[\d+\] = \{([^}]*)\};',b):
        edges[int(m.group(1))]=[int(x) for x in m.group(2).split(',') if x.strip()]
    wps=[]
    for m in re.finditer(r'CreateWaypoint\((\d+),\s*([-\d.]+),\s*([-\d.]+),\s*([-\d.]+),\s*(\d+),\s*edge(\d+)\)',b):
        i=int(m.group(1)); wps.append((i,m.group(2),m.group(3),m.group(4),edges[int(m.group(6))]))
    size=int(re.search(r'waypointsSize = (\d+)',b).group(1))
    assert len(wps)==size,(name,len(wps),size)
    wps.sort()
    sa=re.search(r'SiteAPoint = (-?\d+)',b); sb=re.search(r'SiteBPoint = (-?\d+)',b)
    out.append(f'            // {name}')
    out.append(f'            maps[(int)mapEnum.{name}] = new MapNav')
    out.append('            {')
    out.append('                waypoints = new Waypoint[]')
    out.append('                {')
    for i,x,y,z,e in wps:
        out.append(f'                    new Waypoint({x}f, {y}f, {z}f, new int[] {{ {", ".join(map(str,e))} }}), // {i}')
    out.append('                },')
    out.append(f'                siteAPoint = {sa.group(1) if sa else -1},')
    out.append(f'                siteBPoint = {sb.group(1) if sb else -1},')
    # sites from collisions.c: find the block for this map that contains SetBombZone
    sites=[]
    for k,(p,n) in enumerate(colpos):
        if n!=name: continue
        end=colpos[k+1][0] if k+1<len(colpos) else len(col)
        blk=col[p:end]
        if 'SetBombZone' not in blk: continue
        arrays={}
        for m in re.finditer(r'int (nearWaypoints\d+)\[\d+\] = \{([^}]*)\};',blk):
            arrays[m.group(1)]=[int(x) for x in m.group(2).split(',') if x.strip()]
        for m in re.finditer(r'SetBombZone\(([-\d.]+),\s*([-\d.]+),\s*([-\d.]+),\s*([-\d.]+),\s*(\d+),\s*(-?\d+),\s*(\d+),\s*(nearWaypoints\d+)\)',blk):
            sites.append((m.group(1),m.group(2),m.group(3),m.group(4),int(m.group(5)),int(m.group(6)),arrays[m.group(8)]))
        break
    assert sites,name
    out.append('                sites = new Site[]')
    out.append('                {')
    for x,z,xs,zs,tid,w,near in sites:
        out.append(f'                    new Site({x}f, {z}f, {xs}f, {zs}f, {w}, new int[] {{ {", ".join(map(str,near))} }}),')
    out.append('                },')
    out.append('            };')
out.append('        }\n    }\n}\n')
open(sys.argv[1],'w').write('\n'.join(out))
