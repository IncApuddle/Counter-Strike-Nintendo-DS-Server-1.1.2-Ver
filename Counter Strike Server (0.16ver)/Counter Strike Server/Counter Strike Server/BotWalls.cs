// SPDX-License-Identifier: MIT
//
// This file is part of the server of Counter Strike Nintendo DS Multiplayer Edition (CS:DS)

using System;

namespace Counter_Strike_Server
{
    /// <summary>
    /// Walls of the maps for the bots (line of sight and shots), same boxes as
    /// the raycast of the game (collisions/raycast.c). Data in
    /// BotWalls.Generated.cs.
    /// </summary>
    public static partial class BotWalls
    {
        // Per map: { x, y, z, xSize, ySize, zSize } (center and full size)
        private static readonly float[][,] walls = new float[7][,];

        static BotWalls()
        {
            LoadGenerated();
        }

        /// <summary>
        /// Ray / box intersection (slab method)
        /// </summary>
        /// <returns>Distance along the ray (in ray direction units), or -1</returns>
        public static float RayBox(float ox, float oy, float oz, float dx, float dy, float dz,
                                   float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            float tMin = float.NegativeInfinity, tMax = float.PositiveInfinity;

            if (!Slab(ox, dx, minX, maxX, ref tMin, ref tMax)) return -1;
            if (!Slab(oy, dy, minY, maxY, ref tMin, ref tMax)) return -1;
            if (!Slab(oz, dz, minZ, maxZ, ref tMin, ref tMax)) return -1;

            // Behind, or the start point is inside the box (ignored: a bot
            // standing in a box would never see anything)
            if (tMin < 0)
                return -1;
            return tMin;
        }

        private static bool Slab(float origin, float direction, float min, float max, ref float tMin, ref float tMax)
        {
            if (MathF.Abs(direction) < 1e-8f)
                return origin >= min && origin <= max;

            float t1 = (min - origin) / direction;
            float t2 = (max - origin) / direction;
            if (t1 > t2)
            {
                float tmp = t1;
                t1 = t2;
                t2 = tmp;
            }
            if (t1 > tMin) tMin = t1;
            if (t2 < tMax) tMax = t2;
            return tMin <= tMax;
        }

        /// <summary>
        /// Distance to the nearest wall along a ray
        /// </summary>
        /// <returns>Distance (in ray direction units), float.MaxValue if no wall</returns>
        public static float NearestWall(mapEnum map, float ox, float oy, float oz, float dx, float dy, float dz)
        {
            float[,] mapWalls = walls[(int)map];
            float nearest = float.MaxValue;
            if (mapWalls == null)
                return nearest;

            int count = mapWalls.GetLength(0);
            for (int i = 0; i < count; i++)
            {
                float halfX = mapWalls[i, 3] / 2f, halfY = mapWalls[i, 4] / 2f, halfZ = mapWalls[i, 5] / 2f;
                float t = RayBox(ox, oy, oz, dx, dy, dz,
                                 mapWalls[i, 0] - halfX, mapWalls[i, 1] - halfY, mapWalls[i, 2] - halfZ,
                                 mapWalls[i, 0] + halfX, mapWalls[i, 1] + halfY, mapWalls[i, 2] + halfZ);
                if (t >= 0 && t < nearest)
                    nearest = t;
            }
            return nearest;
        }

        /// <summary>
        /// Is there a wall between two points?
        /// </summary>
        public static bool IsBlocked(mapEnum map, float x1, float y1, float z1, float x2, float y2, float z2)
        {
            // Ray from point 1 to point 2: t = 1 at point 2
            float t = NearestWall(map, x1, y1, z1, x2 - x1, y2 - y1, z2 - z1);
            return t < 1f;
        }
    }
}
