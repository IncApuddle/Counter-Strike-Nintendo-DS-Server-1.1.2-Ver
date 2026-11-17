// SPDX-License-Identifier: MIT
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
        {
            // DUST2
            maps[(int)mapEnum.DUST2] = new MapNav
            {
                waypoints = new Waypoint[]
                {
                    new Waypoint(-0.8f, 0.0727f, 5.94f, new int[] { 55, 4, 20 }), // 0
                    new Waypoint(1.588759f, 0.0727f, -13.34f, new int[] { 56, 2, 32 }), // 1
                    new Waypoint(24.565f, 0.0727f, -13.38f, new int[] { 1, 3, 57 }), // 2
                    new Waypoint(37.474f, 3.3017f, -13.38f, new int[] { 2, 9 }), // 3
                    new Waypoint(-10.82f, 0.4764f, 4.41f, new int[] { 0, 5 }), // 4
                    new Waypoint(-15.714f, 0.4764f, 8.416f, new int[] { 4, 6 }), // 5
                    new Waypoint(-15.714f, 2.099f, 11.648f, new int[] { 5, 7 }), // 6
                    new Waypoint(-17.208f, 2.103f, 13.19f, new int[] { 6, 8 }), // 7
                    new Waypoint(-20.2982f, 4.1082f, 13.15f, new int[] { 7, 21 }), // 8
                    new Waypoint(47.92f, 3.3017f, -17.4f, new int[] { 3, 10, 54 }), // 9
                    new Waypoint(47.92f, 6.502f, -30.298f, new int[] { 9, 11 }), // 10
                    new Waypoint(43.91f, 6.502f, -34.19f, new int[] { 10, 12 }), // 11
                    new Waypoint(40.44f, 6.502f, -30.322f, new int[] { 11, 13 }), // 12
                    new Waypoint(40.44f, 5.71f, -28.29f, new int[] { 12, 14, 15 }), // 13
                    new Waypoint(41.02f, 5.7171f, -20.18f, new int[] { 13 }), // 14
                    new Waypoint(21.45f, 5.71f, -24.71f, new int[] { 13, 16 }), // 15
                    new Waypoint(19.26f, 5.71f, -4.494f, new int[] { 15, 17 }), // 16
                    new Waypoint(21.03f, 3.3012f, 1.58f, new int[] { 16, 18 }), // 17
                    new Waypoint(6.953f, 3.3012f, 3.8f, new int[] { 17, 19 }), // 18
                    new Waypoint(6.953f, 3.3012f, 30.92f, new int[] { 42, 20, 18, 46 }), // 19
                    new Waypoint(1.65f, 3.3012f, 21.326f, new int[] { 0, 19, 46 }), // 20
                    new Waypoint(-30.27f, 4.1082f, 13.15f, new int[] { 8, 22, 53 }), // 21
                    new Waypoint(-38.8f, 4.1082f, 5.94f, new int[] { 21, 23 }), // 22
                    new Waypoint(-38.275f, 4.1082f, -4.496f, new int[] { 22, 24 }), // 23
                    new Waypoint(-38.275f, 3.3068f, -6.1201f, new int[] { 23, 25, 28 }), // 24
                    new Waypoint(-36.513f, 3.3068f, -18.07f, new int[] { 24, 26, 28 }), // 25
                    new Waypoint(-36.513f, 4.1038f, -20.6286f, new int[] { 25, 27 }), // 26
                    new Waypoint(-36.513f, 4.1038f, -26.5f, new int[] { 26 }), // 27
                    new Waypoint(-27.67f, 3.3068f, -13.19f, new int[] { 24, 25, 29, 30 }), // 28
                    new Waypoint(-27.17f, 3.3068f, -26.79f, new int[] { 28 }), // 29
                    new Waypoint(-22.02f, 3.3068f, -13.19f, new int[] { 28, 31 }), // 30
                    new Waypoint(-17.385f, 3.3068f, -16.984f, new int[] { 30, 32 }), // 31
                    new Waypoint(-7.695f, -0.234f, -16.36f, new int[] { 1, 31 }), // 32
                    new Waypoint(49.579f, 3.3017f, 18.17f, new int[] { 36, 34, 54, 38 }), // 33
                    new Waypoint(52.7995f, 4.9118f, 18.17f, new int[] { 33, 35 }), // 34
                    new Waypoint(54.39f, 4.9118f, 26.67f, new int[] { 34 }), // 35
                    new Waypoint(47.88f, 3.3017f, 21.315f, new int[] { 33, 37 }), // 36
                    new Waypoint(47.88f, -1.5331f, 34.2316f, new int[] { 36 }), // 37
                    new Waypoint(40.04f, 3.3017f, 17.9f, new int[] { 33, 39 }), // 38
                    new Waypoint(27.17f, 3.3017f, 18.47f, new int[] { 38, 40 }), // 39
                    new Waypoint(29.22f, 3.3017f, 32.01f, new int[] { 39, 41 }), // 40
                    new Waypoint(22.81f, 3.3017f, 38f, new int[] { 40, 42, 43 }), // 41
                    new Waypoint(15.47f, 3.3017f, 30.54f, new int[] { 41, 19 }), // 42
                    new Waypoint(16.63f, 3.3017f, 60.19f, new int[] { 44, 47, 41 }), // 43
                    new Waypoint(12.42f, 3.3017f, 55.01f, new int[] { 45, 43 }), // 44
                    new Waypoint(0.87f, 3.3017f, 55.01f, new int[] { 46, 44 }), // 45
                    new Waypoint(0.83f, 3.3017f, 34.08f, new int[] { 19, 20, 45 }), // 46
                    new Waypoint(11.65f, 3.3017f, 60.19f, new int[] { 43, 48 }), // 47
                    new Waypoint(-1.254f, 6.529f, 60.19f, new int[] { 47, 49 }), // 48
                    new Waypoint(-24.84f, 6.529f, 61.69f, new int[] { 50, 48 }), // 49
                    new Waypoint(-37.03f, 6.529f, 56.829f, new int[] { 49, 51 }), // 50
                    new Waypoint(-37.03f, 3.278f, 43.903f, new int[] { 50, 52 }), // 51
                    new Waypoint(-30.061f, 3.278f, 28.464f, new int[] { 51, 53 }), // 52
                    new Waypoint(-30.061f, 4.1094f, 26.966f, new int[] { 52, 21 }), // 53
                    new Waypoint(49.97f, 3.3017f, 2.61f, new int[] { 33, 9 }), // 54
                    new Waypoint(3.08f, 0.0727f, 0.82f, new int[] { 0, 56 }), // 55
                    new Waypoint(1.01f, 0.0727f, -2.04f, new int[] { 1, 55 }), // 56
                    new Waypoint(19.58f, 0.0727f, -19.42f, new int[] { 2 }), // 57
                },
                siteAPoint = 14,
                siteBPoint = 29,
                sites = new Site[]
                {
                    new Site(40.8f, -20.8f, 5f, 5f, 14, new int[] { 2, 3, 9, 10, 11, 12, 13, 14, 15, 16, 17, 57 }),
                    new Site(-28.03f, -27.07f, 4.46785f, 4.578236f, 29, new int[] { 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 }),
                },
            };
            // TUTORIAL
            maps[(int)mapEnum.TUTORIAL] = new MapNav
            {
                waypoints = new Waypoint[]
                {
                    new Waypoint(9f, 0.55f, -5f, new int[] { 1, 10, 2 }), // 0
                    new Waypoint(9f, 0.55f, -8f, new int[] { 0, 10, 3 }), // 1
                    new Waypoint(4.5f, 0.55f, -5f, new int[] { 10, 0, 3, 11, 4 }), // 2
                    new Waypoint(4.5f, 0.55f, -8f, new int[] { 10, 1, 2, 11, 5 }), // 3
                    new Waypoint(0f, 0.55f, -5f, new int[] { 11, 12, 5, 6, 2 }), // 4
                    new Waypoint(0f, 0.55f, -8f, new int[] { 11, 4, 12, 7, 3 }), // 5
                    new Waypoint(-4.5f, 0.55f, -5f, new int[] { 13, 12, 7, 4 }), // 6
                    new Waypoint(-4.5f, 0.55f, -8f, new int[] { 12, 6, 13, 5 }), // 7
                    new Waypoint(-9f, 0.55f, -5f, new int[] { 9, 6, 13 }), // 8
                    new Waypoint(-9f, 0.55f, -8f, new int[] { 13, 8, 7 }), // 9
                    new Waypoint(7f, 0.55f, -6.5f, new int[] { 0, 1, 2, 3 }), // 10
                    new Waypoint(2f, 0.55f, -6.5f, new int[] { 3, 2, 5, 4 }), // 11
                    new Waypoint(-2f, 0.55f, -6.5f, new int[] { 4, 5, 7, 6 }), // 12
                    new Waypoint(-7f, 0.55f, -6.5f, new int[] { 7, 6, 8, 9 }), // 13
                },
                siteAPoint = 8,
                siteBPoint = -1,
                sites = new Site[]
                {
                    new Site(-8.21f, -1.59f, 4f, 4f, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }),
                },
            };
            // DUST2_2x2
            maps[(int)mapEnum.DUST2_2x2] = new MapNav
            {
                waypoints = new Waypoint[]
                {
                    new Waypoint(19.58f, 0.0727f, -19.42f, new int[] { 1 }), // 0
                    new Waypoint(24.565f, 0.0727f, -13.38f, new int[] { 0, 2 }), // 1
                    new Waypoint(37.474f, 3.3017f, -13.38f, new int[] { 1, 3 }), // 2
                    new Waypoint(47.92f, 3.3017f, -17.4f, new int[] { 2, 4, 30 }), // 3
                    new Waypoint(47.92f, 6.502f, -30.298f, new int[] { 3, 5 }), // 4
                    new Waypoint(43.91f, 6.502f, -34.19f, new int[] { 4, 6 }), // 5
                    new Waypoint(40.44f, 6.502f, -30.322f, new int[] { 5, 7 }), // 6
                    new Waypoint(40.44f, 5.71f, -28.29f, new int[] { 6, 8, 9 }), // 7
                    new Waypoint(41.02f, 5.7171f, -20.18f, new int[] { 7 }), // 8
                    new Waypoint(21.45f, 5.71f, -24.71f, new int[] { 7, 10 }), // 9
                    new Waypoint(19.26f, 5.71f, -4.494f, new int[] { 9, 11 }), // 10
                    new Waypoint(21.03f, 3.3012f, 1.58f, new int[] { 10, 12 }), // 11
                    new Waypoint(6.953f, 3.3012f, 3.8f, new int[] { 11, 13 }), // 12
                    new Waypoint(6.953f, 3.3012f, 30.92f, new int[] { 23, 12, 27 }), // 13
                    new Waypoint(49.579f, 3.3017f, 18.17f, new int[] { 17, 15, 30, 19 }), // 14
                    new Waypoint(52.7995f, 4.9118f, 18.17f, new int[] { 14, 16 }), // 15
                    new Waypoint(54.39f, 4.9118f, 26.67f, new int[] { 15 }), // 16
                    new Waypoint(47.88f, 3.3017f, 21.315f, new int[] { 14, 18 }), // 17
                    new Waypoint(47.88f, -1.5331f, 34.2316f, new int[] { 17 }), // 18
                    new Waypoint(40.04f, 3.3017f, 17.9f, new int[] { 14, 20 }), // 19
                    new Waypoint(27.17f, 3.3017f, 18.47f, new int[] { 19, 21 }), // 20
                    new Waypoint(29.22f, 3.3017f, 32.01f, new int[] { 20, 22 }), // 21
                    new Waypoint(22.81f, 3.3017f, 38f, new int[] { 21, 23, 24 }), // 22
                    new Waypoint(15.47f, 3.3017f, 30.54f, new int[] { 22, 13 }), // 23
                    new Waypoint(16.63f, 3.3017f, 60.19f, new int[] { 25, 28, 22 }), // 24
                    new Waypoint(12.42f, 3.3017f, 55.01f, new int[] { 26, 24 }), // 25
                    new Waypoint(0.87f, 3.3017f, 55.01f, new int[] { 27, 25 }), // 26
                    new Waypoint(0.83f, 3.3017f, 34.08f, new int[] { 13, 26 }), // 27
                    new Waypoint(11.65f, 3.3017f, 60.19f, new int[] { 24, 29 }), // 28
                    new Waypoint(-1.254f, 6.529f, 60.19f, new int[] { 28 }), // 29
                    new Waypoint(49.97f, 3.3017f, 2.61f, new int[] { 14, 3 }), // 30
                },
                siteAPoint = 8,
                siteBPoint = -1,
                sites = new Site[]
                {
                    new Site(40.8f, -20.8f, 5f, 5f, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 14, 30 }),
                },
            };
            // AIM_MAP
            maps[(int)mapEnum.AIM_MAP] = new MapNav
            {
                waypoints = new Waypoint[]
                {
                    new Waypoint(-2.7f, 0.08f, -17.56f, new int[] { 1, 8, 12, 13, 18 }), // 0
                    new Waypoint(-9.17f, 0.08f, -17.44f, new int[] { 2, 13, 0 }), // 1
                    new Waypoint(-15.02f, 0.08f, -15.59f, new int[] { 3, 15, 1 }), // 2
                    new Waypoint(-15.02f, 0.08f, -16.849f, new int[] { 4, 2 }), // 3
                    new Waypoint(-15.02f, 4.129f, -22.92f, new int[] { 5, 3 }), // 4
                    new Waypoint(-13.82f, 4.129f, -26.05f, new int[] { 6, 4 }), // 5
                    new Waypoint(-7.78f, 4.129f, -26.97f, new int[] { 7, 5 }), // 6
                    new Waypoint(12.88f, 4.129f, -26.64f, new int[] { 6 }), // 7
                    new Waypoint(3.765f, 0.08f, -19.461f, new int[] { 9, 12, 0 }), // 8
                    new Waypoint(4.999f, 0.775f, -19.461f, new int[] { 10, 8 }), // 9
                    new Waypoint(8.84f, 0.775f, -20.31f, new int[] { 11, 9 }), // 10
                    new Waypoint(14.15f, 0.775f, -11.53f, new int[] { 10 }), // 11
                    new Waypoint(-0.49f, 0.08f, -14.94f, new int[] { 16, 18, 0, 8, 13 }), // 12
                    new Waypoint(-7.4f, 0.08f, -14.94f, new int[] { 14, 12, 1, 0 }), // 13
                    new Waypoint(-8.22f, 0.08f, -9.2f, new int[] { 21, 20, 15, 13 }), // 14
                    new Waypoint(-14.51f, 0.08f, -10.89f, new int[] { 14, 2 }), // 15
                    new Waypoint(-0.49f, 0.08f, -9.3f, new int[] { 20, 21, 12 }), // 16
                    new Waypoint(6.11f, 0.08f, -9.3f, new int[] { 19, 20, 18 }), // 17
                    new Waypoint(6.11f, 0.08f, -15.66f, new int[] { 17, 12 }), // 18
                    new Waypoint(6.75f, 0.08f, 1.02f, new int[] { 37, 36, 17, 16 }), // 19
                    new Waypoint(0.8099999f, 0.08f, 1.02f, new int[] { 37, 38, 16, 14 }), // 20
                    new Waypoint(-5.33f, 0.08f, 0.61f, new int[] { 31, 38, 14, 15 }), // 21
                    new Waypoint(3.75f, 0.08f, 18.03f, new int[] { 23, 24, 25, 32 }), // 22
                    new Waypoint(0.51f, 0.08f, 15.85f, new int[] { 38, 22, 24, 30, 32 }), // 23
                    new Waypoint(7.12f, 0.08f, 16.45f, new int[] { 37, 25, 22, 23 }), // 24
                    new Waypoint(14.7f, 0.08f, 15.94f, new int[] { 26, 36, 24 }), // 25
                    new Waypoint(14.7f, 0.08f, 17.205f, new int[] { 27, 25 }), // 26
                    new Waypoint(15.1f, 4.129f, 23.268f, new int[] { 28, 26 }), // 27
                    new Waypoint(11.33f, 4.129f, 27.47f, new int[] { 29, 27 }), // 28
                    new Waypoint(-13.75f, 4.129f, 26.76f, new int[] { 28 }), // 29
                    new Waypoint(-6.17f, 0.08f, 16.02f, new int[] { 31, 23 }), // 30
                    new Waypoint(-6.17f, 0.08f, 11.1f, new int[] { 21, 20, 30 }), // 31
                    new Waypoint(-3.95f, 0.08f, 19.546f, new int[] { 33, 23, 22 }), // 32
                    new Waypoint(-4.908f, 0.775f, 19.871f, new int[] { 34, 32 }), // 33
                    new Waypoint(-10.24f, 0.775f, 20.21f, new int[] { 35, 33 }), // 34
                    new Waypoint(-13.93f, 0.775f, 11.86f, new int[] { 34 }), // 35
                    new Waypoint(14.49f, 0.08f, 10.96f, new int[] { 19, 20, 25 }), // 36
                    new Waypoint(8.27f, 0.08f, 9.95f, new int[] { 19, 20, 24 }), // 37
                    new Waypoint(0.59f, 0.08f, 9.92f, new int[] { 20, 21, 23 }), // 38
                },
                siteAPoint = 7,
                siteBPoint = -1,
                sites = new Site[]
                {
                    new Site(22.22f, 0.12f, 4f, 4f, -1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38 }),
                },
            };
            // B2000
            maps[(int)mapEnum.B2000] = new MapNav
            {
                waypoints = new Waypoint[]
                {
                    new Waypoint(-1.142999f, 0.04000021f, -17.56f, new int[] { 1, 2, 19, 14 }), // 0
                    new Waypoint(11.057f, 0.04000021f, -14.36f, new int[] { 6, 0, 19 }), // 1
                    new Waypoint(-11.013f, 0.04000021f, -14.36f, new int[] { 7, 0, 14 }), // 2
                    new Waypoint(-1.142999f, 0.04000021f, 18.18f, new int[] { 4, 5, 21, 20 }), // 3
                    new Waypoint(11.057f, 0.04000021f, 14.05f, new int[] { 8, 3, 20 }), // 4
                    new Waypoint(-11.013f, 0.04000021f, 14.05f, new int[] { 9, 3, 21 }), // 5
                    new Waypoint(12.767f, 0.04000021f, -4.09f, new int[] { 8, 1, 11, 12 }), // 6
                    new Waypoint(-13.563f, 0.04000021f, -4.09f, new int[] { 9, 2, 10, 12 }), // 7
                    new Waypoint(12.767f, 0.04000021f, 4.4f, new int[] { 6, 4, 11, 13 }), // 8
                    new Waypoint(-13.563f, 0.04000021f, 4.4f, new int[] { 7, 5, 10, 13 }), // 9
                    new Waypoint(-17.663f, 0.04000021f, 0.08f, new int[] { 9, 7 }), // 10
                    new Waypoint(17.237f, 0.04000021f, 0.08f, new int[] { 8, 6 }), // 11
                    new Waypoint(-0.2829991f, 0.04000021f, -4.09f, new int[] { 13, 6, 7 }), // 12
                    new Waypoint(-0.2829991f, 0.04000021f, 4.4f, new int[] { 12, 8, 9 }), // 13
                    new Waypoint(-12.536f, 0.04000021f, -20.248f, new int[] { 15, 2, 0 }), // 14
                    new Waypoint(-18.253f, 3.38f, -20.248f, new int[] { 16, 14 }), // 15
                    new Waypoint(-19.389f, 3.387f, -21.984f, new int[] { 17, 15 }), // 16
                    new Waypoint(19.112f, 3.387f, -21.984f, new int[] { 16, 18 }), // 17
                    new Waypoint(18.029f, 3.38f, -20.248f, new int[] { 17, 19 }), // 18
                    new Waypoint(12.319f, 0.04000021f, -20.248f, new int[] { 18, 1, 0 }), // 19
                    new Waypoint(12.319f, 0.04000021f, 20.528f, new int[] { 22, 4, 3 }), // 20
                    new Waypoint(-12.465f, 0.04000021f, 20.478f, new int[] { 25, 5, 3 }), // 21
                    new Waypoint(18.029f, 3.38f, 20.568f, new int[] { 23, 20 }), // 22
                    new Waypoint(19.11f, 3.38f, 22.243f, new int[] { 24, 22 }), // 23
                    new Waypoint(-19.32f, 3.38f, 22.243f, new int[] { 23, 25 }), // 24
                    new Waypoint(-18.25f, 3.38f, 20.494f, new int[] { 24, 21 }), // 25
                },
                siteAPoint = 24,
                siteBPoint = -1,
                sites = new Site[]
                {
                    new Site(-1.76f, -28.5f, 4f, 4f, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }),
                },
            };
            // FYSNOW
            maps[(int)mapEnum.FYSNOW] = new MapNav
            {
                waypoints = new Waypoint[]
                {
                    new Waypoint(0f, 0.01f, -18.25f, new int[] { 1, 2, 8 }), // 0
                    new Waypoint(20f, 0.01f, -14f, new int[] { 3, 0, 9 }), // 1
                    new Waypoint(-20f, 0.01f, -14f, new int[] { 4, 0, 12 }), // 2
                    new Waypoint(16f, 0.01f, 0f, new int[] { 8, 1, 6 }), // 3
                    new Waypoint(-16f, 0.01f, 0f, new int[] { 8, 7, 2 }), // 4
                    new Waypoint(0f, 0.01f, 18.25f, new int[] { 6, 7, 8 }), // 5
                    new Waypoint(20f, 0.01f, 14f, new int[] { 3, 5, 16 }), // 6
                    new Waypoint(-20f, 0.01f, 14f, new int[] { 4, 5, 19 }), // 7
                    new Waypoint(0f, 0.01f, 0f, new int[] { 0, 3, 5, 4 }), // 8
                    new Waypoint(18.2f, 0.01f, -26.6f, new int[] { 10, 1 }), // 9
                    new Waypoint(16.434f, 0.01f, -26.6f, new int[] { 11, 9 }), // 10
                    new Waypoint(13.0964f, 1.875f, -26.6f, new int[] { 15, 10 }), // 11
                    new Waypoint(-18.2f, 0.01f, -26.6f, new int[] { 13, 2 }), // 12
                    new Waypoint(-16.43f, 0.01f, -26.6f, new int[] { 14, 12 }), // 13
                    new Waypoint(-13f, 1.875f, -26.6f, new int[] { 15, 13 }), // 14
                    new Waypoint(0f, 1.875f, -26.6f, new int[] { 11, 14 }), // 15
                    new Waypoint(18.2f, 0.01f, 26.6f, new int[] { 17, 6 }), // 16
                    new Waypoint(16.434f, 0.01f, 26.6f, new int[] { 18, 16 }), // 17
                    new Waypoint(13.0964f, 1.875f, 26.6f, new int[] { 22, 17 }), // 18
                    new Waypoint(-18.2f, 0.01f, 26.6f, new int[] { 20, 7 }), // 19
                    new Waypoint(-16.43f, 0.01f, 26.6f, new int[] { 21, 19 }), // 20
                    new Waypoint(-13f, 1.875f, 26.6f, new int[] { 22, 20 }), // 21
                    new Waypoint(0f, 1.875f, 26.6f, new int[] { 21, 18 }), // 22
                },
                siteAPoint = 15,
                siteBPoint = -1,
                sites = new Site[]
                {
                    new Site(-1.75f, -35f, 4f, 4f, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22 }),
                },
            };
            // MIRAGE
            maps[(int)mapEnum.MIRAGE] = new MapNav
            {
                waypoints = new Waypoint[]
                {
                    new Waypoint(-23.16f, 0.04f, -12.19f, new int[] { 1, 27 }), // 0
                    new Waypoint(-35.15f, 0.0727f, -7.100002f, new int[] { 2, 0 }), // 1
                    new Waypoint(-35.15f, 2.529f, 0.3099976f, new int[] { 3, 1 }), // 2
                    new Waypoint(-33.09f, 2.529f, 8.849998f, new int[] { 4, 2 }), // 3
                    new Waypoint(-31.18f, 2.529f, 17.35f, new int[] { 5, 3 }), // 4
                    new Waypoint(-21.21f, 2.529f, 20.56f, new int[] { 4, 6 }), // 5
                    new Waypoint(-13.26f, 2.529f, 34.07f, new int[] { 5, 7 }), // 6
                    new Waypoint(-16.275f, 0.032f, 41.341f, new int[] { 6, 8 }), // 7
                    new Waypoint(-13.15f, 0.032f, 44.58f, new int[] { 7, 9 }), // 8
                    new Waypoint(-3.809999f, 0.032f, 47.2f, new int[] { 8, 10 }), // 9
                    new Waypoint(-1.43f, 0.032f, 57.93f, new int[] { 9, 11 }), // 10
                    new Waypoint(4.848f, 0.032f, 57.93f, new int[] { 10, 12 }), // 11
                    new Waypoint(14.697f, 2.528f, 57.93f, new int[] { 11, 13, 41 }), // 12
                    new Waypoint(14.697f, 2.528f, 61.76f, new int[] { 14, 41, 12 }), // 13
                    new Waypoint(-11.02f, 2.528f, 61.76f, new int[] { 15, 13 }), // 14
                    new Waypoint(-11.02f, 2.528f, 52.61f, new int[] { 16, 14 }), // 15
                    new Waypoint(-14.375f, 2.528f, 52.61f, new int[] { 17, 15 }), // 16
                    new Waypoint(-19.291f, 4.932f, 52.61f, new int[] { 18, 16 }), // 17
                    new Waypoint(-27.473f, 4.932f, 54.98f, new int[] { 19, 17 }), // 18
                    new Waypoint(-32.72f, 5.762f, 48.76f, new int[] { 20, 18 }), // 19
                    new Waypoint(-34.08f, 5.762f, 29.83f, new int[] { 21, 19 }), // 20
                    new Waypoint(-29.05f, 5.762f, 28.72f, new int[] { 22, 20 }), // 21
                    new Waypoint(-29.59f, 2.529f, 23.34f, new int[] { 4, 5, 21 }), // 22
                    new Waypoint(-13.46f, 2.529f, 10.05f, new int[] { 42, 24, 5, 3 }), // 23
                    new Waypoint(-7.809999f, 2.529f, -2.840002f, new int[] { 25, 23 }), // 24
                    new Waypoint(-0.1100001f, 2.529f, -4.890003f, new int[] { 26, 24 }), // 25
                    new Waypoint(4.054f, 2.5f, -11.94f, new int[] { 27, 28, 25 }), // 26
                    new Waypoint(-4.965f, 0.04f, -12.19f, new int[] { 0, 26 }), // 27
                    new Waypoint(9.77f, 2.5f, -13.94f, new int[] { 29, 26 }), // 28
                    new Waypoint(13.18f, 2.5f, -31.43f, new int[] { 30, 28 }), // 29
                    new Waypoint(20.81f, 2.5f, -31.43f, new int[] { 31, 29 }), // 30
                    new Waypoint(26.16f, 2.5f, -24.61f, new int[] { 32, 30 }), // 31
                    new Waypoint(32.251f, 2.5f, -24.324f, new int[] { 33, 57, 31 }), // 32
                    new Waypoint(30.46f, 2.5f, 7.259997f, new int[] { 34, 32 }), // 33
                    new Waypoint(14.91f, 2.5f, 7.979998f, new int[] { 35, 33 }), // 34
                    new Waypoint(12.2f, 2.5f, 32.236f, new int[] { 45, 36, 34 }), // 35
                    new Waypoint(22.457f, 2.5f, 35.36f, new int[] { 35, 37 }), // 36
                    new Waypoint(31.098f, 0.03f, 34.1f, new int[] { 36, 38 }), // 37
                    new Waypoint(38.44f, 0.03f, 40.17f, new int[] { 37, 39 }), // 38
                    new Waypoint(39.62f, 0.03f, 51.77f, new int[] { 38, 46, 40 }), // 39
                    new Waypoint(34.21f, 0.03f, 61.61f, new int[] { 39, 41 }), // 40
                    new Waypoint(29.403f, 2.528f, 61.76f, new int[] { 40, 12, 13 }), // 41
                    new Waypoint(-6.59f, 2.529f, 11.13f, new int[] { 43, 23 }), // 42
                    new Waypoint(4.056f, 0.04f, 11.1f, new int[] { 44, 42 }), // 43
                    new Waypoint(8.43f, 0.04f, 11.25f, new int[] { 45, 43 }), // 44
                    new Waypoint(8.43f, 0.04f, 21.562f, new int[] { 35, 44 }), // 45
                    new Waypoint(40.985f, 3.28f, 47.645f, new int[] { 47, 39 }), // 46
                    new Waypoint(42.82f, 3.28f, 43.55f, new int[] { 48, 46 }), // 47
                    new Waypoint(45.52f, 3.28f, 42.91f, new int[] { 49, 38 }), // 48
                    new Waypoint(46.448f, 3.28f, 25.971f, new int[] { 50, 48 }), // 49
                    new Waypoint(46.448f, 4.733f, 24.4405f, new int[] { 51, 49 }), // 50
                    new Waypoint(40.27f, 4.733f, 17.8f, new int[] { 52, 50 }), // 51
                    new Waypoint(40.27f, 4.733f, 2.449998f, new int[] { 53, 51 }), // 52
                    new Waypoint(44.5f, 4.733f, -5.735003f, new int[] { 54, 52 }), // 53
                    new Waypoint(44.5f, 5.552f, -8.299002f, new int[] { 55, 53 }), // 54
                    new Waypoint(44.5f, 5.552f, -23.01f, new int[] { 56, 54 }), // 55
                    new Waypoint(42.188f, 6.139f, -23.01f, new int[] { 57, 55 }), // 56
                    new Waypoint(38.47f, 2.5f, -23.91f, new int[] { 32, 56 }), // 57
                },
                siteAPoint = 4,
                siteBPoint = 32,
                sites = new Site[]
                {
                    new Site(-31.82f, 16.7f, 7.4f, 8f, 4, new int[] { 0, 1, 2, 3, 4, 5, 22, 23, 24, 25, 27, 42, 43, 44, 45, 57 }),
                    new Site(32.297f, -24.54f, 9f, 8.5f, 32, new int[] { 26, 29, 30, 31, 32, 33, 34, 35 }),
                },
            };
        }
    }
}
