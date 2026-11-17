import re,sys
src=open(sys.argv[1]).read()
src=src[src.index('void initGuns'):] if 'void initGuns' in src else src
blocks=src.split('gunIndex++;')
fields=['gunCategory','team','Price','fireRate','ReloadTime','MagazineCapacity','maxBotShootDisance','inaccuracyReductionForBot','bulletCountPerShoot','isKnife','reloadOneByOne','name']
rows=[]
for b in blocks:
    if 'AllGuns[gunIndex].name' not in b: continue
    d={}
    for f in fields:
        m=re.search(r'AllGuns\[gunIndex\]\.'+f+r'\s*=\s*([^;]+);',b)
        d[f]=m.group(1).strip() if m else None
    rows.append(d)
CAT={'PISTOLS':0,'HEAVY':1,'SGM':2,'SMG':2,'RIFLES':3,'EQUIPMENT':4,'GRENADES':5}
TEAM={'BOTH':-1,'TERRORISTS':0,'COUNTERTERRORISTS':1}
def num(v,default='0'):
    if v in CAT: return str(CAT[v])
    if v in TEAM: return str(TEAM[v])
    if v is None: return default
    v=v.replace('f','') if not v.startswith('"') else v
    return str(eval(v)) if re.fullmatch(r'[-\d.\s/*+()]+',v) else v
out=['''// SPDX-License-Identifier: MIT
//
// This file is part of the server of Counter Strike Nintendo DS Multiplayer Edition (CS:DS)
//
// GENERATED from the game sources (weapons/data/gun_data.c) by gen_guns.py.
// Do not edit by hand. Same order as the shop (ShopManager.allShopElements).

namespace Counter_Strike_Server
{
    public static partial class BotGuns
    {
        private static void LoadGenerated()
        {
            guns = new GunInfo[]
            {''']
for d in rows:
    out.append('                new GunInfo { name = %s, category = %s, team = %s, price = %s, fireRate = %s, reloadTime = %s, magazineCapacity = %s, maxBotShootDistance = %s, inaccuracyReductionForBot = %sf, bulletCountPerShoot = %s, isKnife = %s, reloadOneByOne = %s },' % (
        d['name'], num(d['gunCategory'],'-1'), num(d['team'],'-1'), num(d['Price']), num(d['fireRate']), num(d['ReloadTime']), num(d['MagazineCapacity']), num(d['maxBotShootDisance'],'20'),
        num(d['inaccuracyReductionForBot']), num(d['bulletCountPerShoot'],'1'), (d['isKnife'] or 'false'), (d['reloadOneByOne'] or 'false')))
out.append('            };\n        }\n    }\n}\n')
open(sys.argv[2],'w').write('\n'.join(out))
print(len(rows))
