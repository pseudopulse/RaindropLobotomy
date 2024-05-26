using System;

namespace RaindropLobotomy.Ordeals.Midnight.Green {
    public static class LastHelixLaserPatterns {
        public static Vector3[][] AllPatterns = {
            CBT
        };

        public static Vector3[] CBT = {
            new(-2f, 0f, 1f),
            new(-2f, 0f, 8f),
            new(-1f, 0f, 9f),
            new(1f, 0f, 9f),
            new(2f, 0f, 8f),
            new(2f, 0f, 1f),
            new(4f, 0f, 2f),
            new(6f, 0f, 1f),
            new(5f, 0f, -2f),
            new(2f, 0f, -3f),
            new(-2f, 0f, -3f),
            new(-5f, 0f, -2f),
            new(-6f, 0f, 1f),
            new(-4f, 0f, 2),
            new(-2f, 0f, 1f)
        };

        public static Vector3[] GetPointSet(Vector3 origin, float scalar) {
            Vector3[] copy = CBT;
            Vector3[] nodes = new Vector3[copy.Length];

            for (int i = 0; i < nodes.Length; i++) {
                nodes[i] = (copy[i] * scalar) + origin;
            }

            return nodes;
        }
    }
}