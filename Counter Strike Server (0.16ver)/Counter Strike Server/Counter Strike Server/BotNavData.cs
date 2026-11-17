// SPDX-License-Identifier: MIT
//
// This file is part of the server of Counter Strike Nintendo DS Multiplayer Edition (CS:DS)

using System;
using System.Collections.Generic;

namespace Counter_Strike_Server
{
    /// <summary>
    /// Navigation data of the bots: the waypoints of the game (made with the
    /// Unity project of CS:DS), the bomb sites and the path finding.
    /// The data is in BotNavData.Generated.cs.
    /// </summary>
    public static partial class BotNavData
    {
        public class Waypoint
        {
            public float x, y, z;
            public int[] edges;

            public Waypoint(float x, float y, float z, int[] edges)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.edges = edges;
            }
        }

        public class Site
        {
            public float x, z, xSize, zSize;
            public int waypoint; // Waypoint where the bomb is planted (-1: none)
            public int[] nearWaypoints; // Waypoints around the site (bots walk there)

            public Site(float x, float z, float xSize, float zSize, int waypoint, int[] nearWaypoints)
            {
                this.x = x;
                this.z = z;
                this.xSize = xSize;
                this.zSize = zSize;
                this.waypoint = waypoint;
                this.nearWaypoints = nearWaypoints;
            }
        }

        public class MapNav
        {
            public Waypoint[] waypoints;
            public int siteAPoint = -1;
            public int siteBPoint = -1;
            public Site[] sites;
        }

        private static readonly MapNav[] maps = new MapNav[7];

        static BotNavData()
        {
            LoadGenerated();
        }

        /// <summary>
        /// Navigation data of a map (null if the map has no data)
        /// </summary>
        public static MapNav Get(mapEnum map)
        {
            int index = (int)map;
            if (index < 0 || index >= maps.Length)
                return null;
            return maps[index];
        }

        /// <summary>
        /// Nearest waypoint of a position
        /// </summary>
        public static int GetNearestWaypoint(MapNav nav, float x, float y, float z)
        {
            int nearest = -1;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < nav.waypoints.Length; i++)
            {
                Waypoint waypoint = nav.waypoints[i];
                float distance = (waypoint.x - x) * (waypoint.x - x) + (waypoint.y - y) * (waypoint.y - y) + (waypoint.z - z) * (waypoint.z - z);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = i;
                }
            }
            return nearest;
        }

        private static float Distance(Waypoint a, Waypoint b)
        {
            float x = a.x - b.x, y = a.y - b.y, z = a.z - b.z;
            return MathF.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// Shortest path between two waypoints (Dijkstra, the graphs are small)
        /// </summary>
        /// <returns>Waypoints from start to end (both included), null if no path</returns>
        public static List<int> FindPath(MapNav nav, int start, int end)
        {
            int count = nav.waypoints.Length;
            if (start < 0 || end < 0 || start >= count || end >= count)
                return null;

            float[] distance = new float[count];
            int[] previous = new int[count];
            bool[] done = new bool[count];
            for (int i = 0; i < count; i++)
            {
                distance[i] = float.MaxValue;
                previous[i] = -1;
            }
            distance[start] = 0;

            for (int step = 0; step < count; step++)
            {
                int current = -1;
                for (int i = 0; i < count; i++)
                    if (!done[i] && distance[i] != float.MaxValue && (current == -1 || distance[i] < distance[current]))
                        current = i;

                if (current == -1 || current == end)
                    break;
                done[current] = true;

                foreach (int next in nav.waypoints[current].edges)
                {
                    if (next < 0 || next >= count || done[next])
                        continue;
                    float newDistance = distance[current] + Distance(nav.waypoints[current], nav.waypoints[next]);
                    if (newDistance < distance[next])
                    {
                        distance[next] = newDistance;
                        previous[next] = current;
                    }
                }
            }

            if (distance[end] == float.MaxValue)
                return null;

            List<int> path = new();
            for (int i = end; i != -1; i = previous[i])
                path.Add(i);
            path.Reverse();
            return path;
        }
    }
}
